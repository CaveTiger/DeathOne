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

        foreach (var slot in allSlots)
        {
            var character = slot.SlotCharacterLoad();  // 이 안에서 FindUnit() 실행됨
            if (character == null || character.IsDead)
                continue;

            turnQueue.Add(slot);
            character.TurnChanse = true;
        }

        Debug.Log($"[AI개선] ResetTurn - turnQueue 크기: {turnQueue.Count}");

        // 여기서 블록 생성!
        if (NextTurnIndicatorUI.Instance != null)
        {
            var turnList = turnQueue
                .Select(slot => slot.currentCharacter)
                .Where(c => c != null && !c.IsDead && c.TurnChanse)
                .OrderByDescending(c => c.Speed)
                .ToList();
            NextTurnIndicatorUI.Instance.CreateTurnBlocks(turnList);
        }

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] ResetTurn 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }
    
    private void TurnDecider()
    {
        Debug.Log("[턴관리] TurnDecider 시작");
        float startTime = Time.realtimeSinceStartup;
        
        CharacterStats fastest = null;
        float topSpeed = float.MinValue;
        bool hasTurnable = false;

        // 다음 턴 순서 계산을 위한 리스트
        List<CharacterStats> nextTurns = new List<CharacterStats>();

        foreach (var slot in turnQueue)
        {
            var character = slot.currentCharacter;
            // 파괴된 오브젝트 체크 추가
            if (character == null || character.gameObject == null || character.IsDead)
                continue;

            if (character.TurnChanse)
            {
                hasTurnable = true;
                if (character.Speed > topSpeed)
                {
                    fastest = character;
                    topSpeed = character.Speed;
                }
                nextTurns.Add(character);
            }
        }

        // 속도순으로 정렬
        nextTurns.Sort((a, b) => b.Speed.CompareTo(a.Speed));

        if (fastest != null && fastest.gameObject != null)
        {
            Debug.Log($"[턴관리] TurnDecider - 선택된 캐릭터: {fastest.Label} (속도: {fastest.Speed})");
            
            // 턴 전환 알림 표시
            if (NextTurnIndicatorUI.Instance != null)
            {
                NextTurnIndicatorUI.Instance.ShowTurnTransition(fastest);
            }
            
            TurnIndicatorHandler.Instance.SetIndicator(fastest.transform, true);
            StartTurn(fastest);
        }
        else if (!hasTurnable)
        {
            Debug.Log("[턴관리] TurnDecider - 턴 가능한 캐릭터 없음, ResetTurn 호출");
            // 인디케이터를 숨기고 턴을 리셋
            if (TurnIndicatorHandler.Instance != null)
                TurnIndicatorHandler.Instance.SetIndicator(null, false);
            ResetTurn();
            // 재귀 호출 대신 다음 프레임에서 다시 시도
            StartCoroutine(DelayedTurnDecider());
        }

        if (NextTurnIndicatorUI.Instance != null)
        {
            NextTurnIndicatorUI.Instance.CreateTurnBlocks(nextTurns);
        }

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

        character.IsMyTurn = true;
        currentCaster = character;

        // 2. 상태이상 효과 적용
        Debug.Log("[턴관리] StartTurn - 상태이상 효과 적용 시작");
        character.GetComponent<StatusEffectController>().ApplyStatusEffectsOnTurnStart();

        // 모든 캐릭터의 정보 UI 갱신
        Debug.Log("[턴관리] StartTurn - UI 갱신 시작");
        foreach (var slot in allSlots)
        {
            if (slot.currentCharacter == null || slot.currentCharacter.gameObject == null) continue;
            var infoUI = slot.currentCharacter?.GetComponentInChildren<CharacterInfoPlayer>();
            if (infoUI != null)
            {
                infoUI.UpdateInfo();
                infoUI.ShowInfo();
            }
        }

        if (character.IsPlayer && playerInfoUI != null)
        {
            playerInfoUI.SetCharacterStats(character);
            playerInfoUI.UpdateInfo();
            playerInfoUI.ShowInfo();
        }

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

    public List<CharacterStats> GetAliveTurnList()
    {
        return turnQueue
            .Select(slot => slot.currentCharacter)
            .Where(c => c != null && !c.IsDead && c.TurnChanse)
            .OrderByDescending(c => c.Speed)
            .ToList();
    }

    public bool CheckBattleEnd()
    {
        Debug.Log("[턴관리] CheckBattleEnd 시작");
        float startTime = Time.realtimeSinceStartup;
        
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
        BattleManager battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null)
        {
            battleManager.EndBattle(isPlayerWin);
            
            // 전투 결과 데이터 확인
            if (BattleManager.LastBattleResult != null)
            {
                // ★ 보상 처리를 즉시 실행 (씬 전환 전에 완료)
                var rewardManager = FindObjectOfType<RewardManager>();
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
