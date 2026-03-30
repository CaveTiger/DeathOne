using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;
using System.Collections;
using System;

public class TurnManager : MonoBehaviour
{
    private List<SlotHandler> turnQueue = new List<SlotHandler>();
    public List<SlotHandler> allSlots = new(); 
    public CharacterStats currentCaster;
    public static TurnManager Instance { get; private set; }
    public CharacterInfoPlayer playerInfoUI; // 인스펙터에서 PlayerInfo 오브젝트 할당
    public List<SlotHandler> playerSlots = new List<SlotHandler>();
    public List<SlotHandler> enemySlots = new List<SlotHandler>();

    // 안전장치: 짧은 시간에 과도한 턴 진행 호출을 방지
    private int advanceTurnCalls = 0;
    private float advanceTurnWindowStart = 0f;
    private const int ADV_MAX_PER_WINDOW = 100; // 윈도우 내 최대 허용 호출 수
    private const float ADV_TIME_WINDOW_SEC = 2f; // 윈도우 길이(초)

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        playerSlots = allSlots.Where(slot => slot.name.StartsWith("Pslot")).ToList();
        enemySlots = allSlots.Where(slot => slot.name.StartsWith("Eslot")).ToList();

        ResetTurn();
        TurnDecider();
    }

    private void ResetTurn()
    {
        Debug.Log("[AI개선] ResetTurn 시작");
        float startTime = Time.realtimeSinceStartup;
        
        turnQueue.Clear(); // 이전 턴 정보 초기화

        for (int i = 0; i < allSlots.Count; i++)
        {
            var slot = allSlots[i];
            slot.slotIndex = i; // 슬롯 인덱스 설정
            
            var character = slot.SlotCharacterLoad();  // 이 안에서 FindUnit() 실행됨
            if (character == null || character.IsDead)
                continue;

            turnQueue.Add(slot);
            character.TurnChanse = true;
        }

        Debug.Log($"[AI개선] ResetTurn - turnQueue 크기: {turnQueue.Count}");

        // UI 업데이트만 담당 (정렬은 GetNextTurnCharacter에서)
        UpdateTurnIndicatorUI();

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] ResetTurn 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }
    
    private void TurnDecider()
    {
        Debug.Log("[턴관리] TurnDecider 시작");
        float startTime = Time.realtimeSinceStartup;
        
        // 단일 정렬로 다음 턴 캐릭터 결정
        var nextCharacter = GetNextTurnCharacter();
        
        if (nextCharacter != null)
        {
            Debug.Log($"[턴관리] TurnDecider - 선택된 캐릭터: {nextCharacter.Label} (속도: {nextCharacter.Speed})");
            
            // 턴 전환 알림 표시
            if (NextTurnIndicatorUI.Instance != null)
            {
                NextTurnIndicatorUI.Instance.ShowTurnTransition(nextCharacter);
            }
            
            TurnIndicatorHandler.Instance.SetIndicator(nextCharacter.transform, true);
            StartTurn(nextCharacter);
        }
        else
        {
            Debug.Log("[턴관리] TurnDecider - 턴 가능한 캐릭터 없음, ResetTurn 호출");
            // 인디케이터를 숨기고 턴을 리셋
            if (TurnIndicatorHandler.Instance != null)
                TurnIndicatorHandler.Instance.SetIndicator(null, false);
            ResetTurn();
            // 재귀 호출 대신 다음 프레임에서 다시 시도
            StartCoroutine(DelayedTurnDecider());
        }

        // UI 업데이트는 UpdateTurnIndicatorUI에서 처리

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴관리] TurnDecider 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    /// <summary>
    /// 다음 프레임에서 TurnDecider를 호출하는 코루틴
    /// </summary>
    private IEnumerator DelayedTurnDecider()
    {
        yield return null; // 다음 프레임까지 대기
        TurnDecider();
    }

    // 중앙화된 턴 진행 메서드(안전장치 포함)
    private void AdvanceTurn(string reason)
    {
        // 호출 빈도 윈도우 관리
        if (Time.time - advanceTurnWindowStart > ADV_TIME_WINDOW_SEC)
        {
            advanceTurnWindowStart = Time.time;
            advanceTurnCalls = 0;
        }
        advanceTurnCalls++;
        if (advanceTurnCalls > ADV_MAX_PER_WINDOW)
        {
            Debug.LogError($"[TurnManager] AdvanceTurn 과다 호출 감지({advanceTurnCalls}) - 무한 루프 방지. reason={reason}");
            // 안전하게 턴을 리셋하고 다시 결정
            ResetTurn();
            StartCoroutine(DelayedTurnDecider());
            return;
        }

        // 현재 캐릭터 턴 플래그 정리
        if (currentCaster != null)
        {
            currentCaster.TurnChanse = false;
            currentCaster.IsMyTurn = false;
        }

        // 전투 종료 먼저 확인
        if (CheckBattleEnd())
        {
            return;
        }

        // 다음 턴으로 진행
        StartCoroutine(DelayedTurnDecider());
    }

    private void StartTurn(CharacterStats character)
    {
        Debug.Log($"[턴관리] StartTurn 시작 - 캐릭터: {character?.Label}");
        float startTime = Time.realtimeSinceStartup;
        
        // 파괴된 오브젝트 체크 추가
        if (character == null || character.gameObject == null)
        {
            Debug.LogWarning("[TurnManager] StartTurn: 캐릭터가 파괴되어 턴을 건너뜁니다.");
            TurnDecider(); // 다음 턴으로
            return;
        }

        if (CheckBattleEnd()) //시작과 동시에 턴을 한쪽의 전멸을 체크
        {
            Debug.Log("[턴관리] StartTurn - 전투 종료 확인됨");
            return;
        }

        if (!character.IsActive)
        {
            Debug.Log("[턴관리] StartTurn - 캐릭터 비활성, DelayedTurnDecider 호출");
            // 재귀 호출 대신 다음 프레임에서 다시 시도
            StartCoroutine(DelayedTurnDecider());
            return;
        }

        CapturePlayerTurnSnapshotBeforeStart(character);

        character.IsMyTurn = true;
        currentCaster = character;

        // 2. 상태이상 효과 적용 (슬롯 컨테이너를 통해 정산 + 연출)
        Debug.Log("[턴관리] StartTurn - 상태이상 효과 정산 시작 (슬롯)");
        var slotOfCharacter = allSlots.FirstOrDefault(s => s != null && s.currentCharacter == character);
        if (slotOfCharacter != null)
        {
            // 상태이상 정산 + 연출을 코루틴으로 처리
            StartCoroutine(ProcessStatusEffectsWithAnimation(slotOfCharacter, character));
            return; // 연출이 끝날 때까지 대기
        }
        else
        {
            // 슬롯을 못 찾은 경우 기존 방식 백업
            var controller = character.GetComponent<StatusEffectController>();
            if (controller != null)
                controller.ApplyStatusEffectsOnTurnStart();
        }

        character.InvokePassivesOnOwnerTurnStart();

        // 상태이상 정산 중 사망했을 수 있으므로 즉시 검증 후 다음 진행 결정
        if (character == null || character.gameObject == null || character.IsDead)
        {
            Debug.Log("[턴관리] StartTurn - 상태이상 정산 결과 사망/파괴 감지, 다음 턴으로 진행");
            AdvanceTurn("post-settlement death");
            return;
        }

        // 보편 진입점: CharacterInfo 역할(bool) 기반으로 일괄 갱신
        Debug.Log("[턴관리] StartTurn - CharacterInfo 라우팅 갱신");
        UpdateCharacterInfoByRole(character);

        // 스킬 UI 업데이트 - 턴이 온 캐릭터의 스킬만 활성화
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateSkillUIForTurn(character);
        }
    }

    private void CapturePlayerTurnSnapshotBeforeStart(CharacterStats character)
    {
        if (character == null || !character.IsPlayer) return;
        if (BattleSnapshotManager.Instance == null) return;

        var liveUnits = allSlots
            .Where(s => s != null && s.currentCharacter != null)
            .Select(s => s.currentCharacter)
            .ToList();

        var turnOrder = GetSortedTurnList();
        var selectedTarget = TargetSelector.Instance != null ? TargetSelector.Instance.GetCurrentTarget() : null;
        int nextTurnIndex = BattleSnapshotManager.Instance.GetTurnSnapshotCount() + 1;

        BattleSnapshotManager.Instance.CaptureTurnSnapshot(
            liveUnits,
            nextTurnIndex,
            turnOrder,
            selectedTarget);
    }

    /// <summary>
    /// 상태이상 정산 + 연출을 처리하는 코루틴
    /// </summary>
    private IEnumerator ProcessStatusEffectsWithAnimation(SlotHandler slotHandler, CharacterStats character)
    {
        float startTime = Time.realtimeSinceStartup;
        
        // 상태이상 정산 + 연출 실행 (캐릭터 매개변수 전달)
        yield return StartCoroutine(slotHandler.SettleStatusEffectsWithAnimation(character));

        // 연출 완료 후 상태이상 정산 결과 검증
        if (character == null || character.gameObject == null || character.IsDead)
        {
            Debug.Log("[턴관리] ProcessStatusEffectsWithAnimation - 상태이상 정산 결과 사망/파괴 감지, 다음 턴으로 진행");
            AdvanceTurn("post-settlement death");
            yield break;
        }

        character.InvokePassivesOnOwnerTurnStart();

        // 보편 진입점: CharacterInfo 역할(bool) 기반으로 일괄 갱신
        Debug.Log("[턴관리] ProcessStatusEffectsWithAnimation - CharacterInfo 라우팅 갱신");
        UpdateCharacterInfoByRole(character);

        // 스킬 UI 업데이트 - 턴이 온 캐릭터의 스킬만 활성화
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.UpdateSkillUIForTurn(character);
        }

        if (character.IsPlayer)
        {
            Debug.Log("[턴관리] StartTurn - 플레이어 턴 시작");
            TargetSelector.Instance.ShowSelector();
            TargetSelector.Instance.AutoSelectFirstEnemy();
        }
        else
        {
            Debug.Log("[턴관리] StartTurn - 적 턴 시작, EnemyActionRoutine 호출");
            TargetSelector.Instance.HideSelector();
            // 적 턴일 때는 모든 스킬 UI 비활성화
            if (BattleUIManager.Instance != null)
            {
                BattleUIManager.Instance.DisableAllSkillUI();
            }
            StartCoroutine(character.GetComponent<EnemyAIController>().EnemyActionRoutine(character));
        }

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴관리] StartTurn 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }
    


    public void EndTurn()
    {
        Debug.Log("[턴관리] EndTurn 호출됨");
        StartCoroutine(EndTurnCoroutine());
    }

    private IEnumerator EndTurnCoroutine()
    {
        Debug.Log($"[턴관리] EndTurnCoroutine 시작 - 현재 캐릭터: {currentCaster?.Label}");
        float startTime = Time.realtimeSinceStartup;
        
        if (currentCaster != null && currentCaster.gameObject != null)
        {
            // 타임라인에 현재 턴의 행동 기록
            //if (TimelineManager.Instance != null)
            //{
            //    TimelineManager.Instance.CreateNewBlock(currentCaster.IsPlayer);
            //    // TODO: 현재 턴의 행동 로그 추가
            //}

            // 상태이상 턴 종료 처리 (지속시간 감소 및 만료된 상태이상 제거)
            var statusEffectController = currentCaster.GetComponent<StatusEffectController>();
            if (statusEffectController != null)
            {
                Debug.Log($"[턴관리] {currentCaster.Label}의 상태이상 턴 종료 처리 시작");
                statusEffectController.ApplyStatusEffectsOnTurnEnd();
            }

            currentCaster.TurnChanse = false;
            currentCaster.IsMyTurn = false;
            Debug.Log($"[턴관리] 턴 종료 처리 완료: {currentCaster.Label}");
        }

        // 전투 종료 체크를 먼저 수행
        if (CheckBattleEnd())
        {
            Debug.Log("[턴관리] EndTurnCoroutine - 전투 종료 확인됨");
            yield break; // 전투가 끝났으면 더 이상 진행하지 않음
        }

        // 죽는 연출이 진행 중인지 확인하고 대기
        Debug.Log("[턴관리] EndTurnCoroutine - WaitForDeathEffects 시작");
        yield return StartCoroutine(WaitForDeathEffects());

        // 위치 복귀는 SkillManager에서 이미 처리됨
        Debug.Log("[턴관리] EndTurnCoroutine - 위치 복귀는 SkillManager에서 처리됨");

        if (AllTurnChanseUsed())
        {
            Debug.Log("[턴관리] EndTurnCoroutine - 모든 턴 사용됨, ResetTurn 호출");
            ResetTurn();     // 다시 TurnChanse = true로 설정하고 속도순 재정렬
        }
        TurnDecider();

        if (NextTurnIndicatorUI.Instance != null)
            NextTurnIndicatorUI.Instance.RemoveCurrentTurnBlock();

        // 카메라와 UI 리셋은 SkillManager에서 이미 처리됨
        Debug.Log("[턴관리] EndTurnCoroutine - 턴 블록 제거 완료");

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴관리] EndTurnCoroutine 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    private IEnumerator WaitForDeathEffects()
    {
        Debug.Log("[턴관리] WaitForDeathEffects 시작");
        float startTime = Time.realtimeSinceStartup;
        
        // 턴 전환 스킵 시스템 사용
        if (TurnTransitionSkipManager.Instance != null)
        {
            Debug.Log("[턴관리] WaitForDeathEffects - TurnTransitionSkipManager 사용");
            yield return TurnTransitionSkipManager.Instance.WaitForTurnTransition(1.5f, "death");
        }
        else
        {
            Debug.Log("[턴관리] WaitForDeathEffects - 기본 대기 방식 사용 (1.5초)");
            yield return new WaitForSeconds(1.5f); // 기존 방식 (스킵 불가)
        }
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴관리] WaitForDeathEffects 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    private void ResetKnockedBackCharacters()
    {
        Debug.Log("[턴관리] ResetKnockedBackCharacters 시작");
        float startTime = Time.realtimeSinceStartup;
        
        foreach (var slot in allSlots)
        {
            if (slot.currentCharacter == null || slot.currentCharacter.gameObject == null) continue;
            
            var motionController = slot.currentCharacter.GetComponent<CharacterMotionController>();
            if (motionController != null)
            {
                motionController.ResetPosition();
            }
        }
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴관리] ResetKnockedBackCharacters 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    private bool AllTurnChanseUsed()
    {
        return turnQueue.All(slot => slot.currentCharacter != null && !slot.currentCharacter.TurnChanse);
    }

    /// <summary>
    /// 턴 순서대로 정렬된 캐릭터 리스트를 반환합니다 (공통 로직)
    /// </summary>
    private List<CharacterStats> GetSortedTurnList()
    {
        return turnQueue
            .Where(slot => slot.currentCharacter != null && !slot.currentCharacter.IsDead && slot.currentCharacter.TurnChanse)
            .OrderByDescending(slot => slot.currentCharacter.Speed)
            .ThenBy(slot => slot.slotIndex) // 같은 속도일 때 슬롯 순서 우선 (O(1) 연산)
            .Select(slot => slot.currentCharacter)
            .ToList();
    }

    /// <summary>
    /// 다음 턴 캐릭터를 안정적으로 결정합니다 (속도 + 슬롯 순서 기준)
    /// </summary>
    private CharacterStats GetNextTurnCharacter()
    {
        return GetSortedTurnList().FirstOrDefault();
    }

    /// <summary>
    /// 턴 인디케이터 UI를 업데이트합니다
    /// </summary>
    private void UpdateTurnIndicatorUI()
    {
        if (NextTurnIndicatorUI.Instance != null)
        {
            NextTurnIndicatorUI.Instance.CreateTurnBlocks(GetSortedTurnList());
        }
    }

    /// <summary>
    /// CharacterInfo의 역할 플래그(isTurnTargetInfo / isSelectedTargetInfo)를 기준으로
    /// 전투 UI를 한 곳에서 라우팅 갱신합니다.
    /// </summary>
    private void UpdateCharacterInfoByRole(CharacterStats turnCharacter)
    {
        CharacterInfo[] infoUIs = FindObjectsByType<CharacterInfo>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        CharacterStats selectedTarget = TargetSelector.Instance != null ? TargetSelector.Instance.GetCurrentTarget() : null;

        foreach (var infoUI in infoUIs)
        {
            if (infoUI == null) continue;

            if (infoUI.IsTurnTargetInfo())
            {
                if (turnCharacter != null)
                    infoUI.SetCharacterStats(turnCharacter);
                else
                    infoUI.HideInfo();
                continue;
            }

            if (infoUI.IsSelectedTargetInfo())
            {
                if (selectedTarget != null)
                    infoUI.SetCharacterStats(selectedTarget);
                // 선택 대상 UI는 항상 켜둔다.
                // 타겟이 아직 없을 때(턴 시작 직후/자동 타겟팅 전) HideInfo로 꺼지지 않게 유지.
            }
        }
    }

    public List<CharacterStats> GetAliveTurnList()
    {
        return GetSortedTurnList();
    }

    public bool CheckBattleEnd()
    {
        Debug.Log("[턴관리] CheckBattleEnd 시작");
        float startTime = Time.realtimeSinceStartup;
        
        // 1번 슬롯(주인공 슬롯) 체크 - 즉시 게임오버
        if (playerSlots != null && playerSlots.Count > 0)
        {
            var slot1 = playerSlots[0];
            if (slot1 != null && slot1.currentCharacter != null && slot1.currentCharacter.gameObject != null)
            {
                if (slot1.currentCharacter.IsDead)
                {
                    Debug.Log("[턴관리] CheckBattleEnd - 1번 슬롯(주인공) 사망으로 게임오버");
                    EndBattle(false); // 패배 처리
                    return true;
                }
            }
        }
        
        bool allPlayersDead = true;
        bool allEnemiesDead = true;

        foreach (var slot in allSlots)
        {
            if (slot.currentCharacter == null || slot.currentCharacter.gameObject == null) continue;
            
            if (slot.currentCharacter.IsPlayer)
            {
                if (!slot.currentCharacter.IsDead)
                    allPlayersDead = false;
            }
            else
            {
                if (!slot.currentCharacter.IsDead)
                    allEnemiesDead = false;
            }
        }

        if (allPlayersDead)
        {
            Debug.Log("[턴관리] CheckBattleEnd - 플레이어 패배");
            EndBattle(false); // 패배 처리
            return true;
        }
        else if (allEnemiesDead)
        {
            Debug.Log("[턴관리] CheckBattleEnd - 플레이어 승리");
            EndBattle(true); // 승리 처리
            return true;
        }

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴관리] CheckBattleEnd 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
        return false;
    }

    private void EndBattle(bool isPlayerWin)
    {

        
        // ★ BattleManager의 EndBattle 호출 (전투 결과 데이터 수집)
        BattleManager battleManager = FindFirstObjectByType<BattleManager>();
        if (battleManager != null)
        {
            battleManager.EndBattle(isPlayerWin);
            
            // 전투 결과 데이터 확인
            if (BattleManager.LastBattleResult != null)
            {
                // ★ 보상 처리를 즉시 실행 (씬 전환 전에 완료)
                var rewardManager = FindFirstObjectByType<RewardManager>();
                if (rewardManager != null)
                {
                    rewardManager.ProcessBattleReward(BattleManager.LastBattleResult);
                }
                else
                {
                    Debug.LogError("[TurnManager] RewardManager를 찾을 수 없습니다!");
                }
            }
            else
            {
                Debug.LogError("[TurnManager] BattleManager.LastBattleResult가 null입니다!");
            }
        }
        else
        {
            Debug.LogError("[TurnManager] BattleManager를 찾을 수 없습니다!");
        }

        // 2초 후 월드맵으로 복귀 (보상 처리는 이미 완료됨)
        StartCoroutine(ReturnToStageCoroutine());
    }

    private IEnumerator ReturnToStageCoroutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("SampleScene"); // 월드맵으로 복귀
    }
}
