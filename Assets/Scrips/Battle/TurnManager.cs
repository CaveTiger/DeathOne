using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;
using UnityEngine.SceneManagement;
using System.Collections;

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

        foreach (var slot in playerSlots)
        {
            var cs = slot.GetComponentInChildren<CharacterStats>();
            Debug.Log($"[디버그] 플레이어 슬롯: {slot.name}, 캐릭터: {cs?.name}, IsDead: {cs?.IsDead}");
        }
        foreach (var slot in enemySlots)
        {
            var cs = slot.GetComponentInChildren<CharacterStats>();
            Debug.Log($"[디버그] 적 슬롯: {slot.name}, 캐릭터: {cs?.name}, IsDead: {cs?.IsDead}");
        }
    }

    private void ResetTurn()
    {
        Debug.Log($"[ResetTurn] 턴 초기화 시작. 슬롯 수: {allSlots.Count}");
        turnQueue.Clear(); // 이전 턴 정보 초기화

        foreach (var slot in allSlots)
        {
            var character = slot.SlotCharacterLoad();  // 이 안에서 FindUnit() 실행됨
            if (character == null || character.IsDead)
                continue;

            turnQueue.Add(slot);
            character.TurnChanse = true;

            //Debug.Log($"[턴 등록] {slot.name} / 속도: {character.Speed}");
        }

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
    }
    private void TurnDecider()
    {
        CharacterStats fastest = null;
        float topSpeed = float.MinValue;
        bool hasTurnable = false;

        // 다음 턴 순서 계산을 위한 리스트
        List<CharacterStats> nextTurns = new List<CharacterStats>();

        foreach (var slot in turnQueue)
        {
            var character = slot.currentCharacter;
            if (character == null || character.IsDead)
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

        if (fastest != null)
        {
            TurnIndicatorHandler.Instance.SetIndicator(fastest.transform, true);
            StartTurn(fastest);
        }
        else if (!hasTurnable)
        {
            TurnIndicatorHandler.Instance.SetIndicator(null, false);
            Debug.Log("모든 캐릭터 턴 종료됨 새 턴 사이클 시작");
            ResetTurn();
            TurnDecider();
        }

        if (NextTurnIndicatorUI.Instance != null)
        {
            NextTurnIndicatorUI.Instance.CreateTurnBlocks(nextTurns);
        }
    }

    private void StartTurn(CharacterStats character)
    {
        if (CheckBattleEnd()) //시작과 동시에 턴을 한쪽의 전멸을 체크
        {
            Debug.Log("전투 종료");
            return;
        }

        if (!character.IsActive)
        {
            Debug.Log($"[{character.name}]는 현재 행동 불능. 턴 스킵");
            TurnDecider(); // 다음 턴으로
            return;
        }

        character.IsMyTurn = true;
        currentCaster = character;

        // 2. 상태이상 효과 적용
        character.GetComponent<StatusEffectController>().ApplyStatusEffectsOnTurnStart();

        // 모든 캐릭터의 정보 UI 갱신
        foreach (var slot in allSlots)
        {
            if (slot.currentCharacter == null || slot.currentCharacter.Equals(null)) continue;
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

        if (character.IsPlayer)
        {
            TargetSelector.Instance.ShowSelector();
            TargetSelector.Instance.AutoSelectFirstEnemy();
        }
        else
        {
            TargetSelector.Instance.HideSelector();
            StartCoroutine(character.GetComponent<EnemyAIController>().EnemyActionRoutine(character));
        }

        //Debug.Log($"[{character.name}]의 턴 시작!");
    }

    public void EndTurn()
    {
        //Debug.Log($"[EndTurn] 현재 캐릭터: {currentCaster?.name}, TurnChanse 종료 처리");
        if (currentCaster != null)
        {
            // 타임라인에 현재 턴의 행동 기록
            //if (TimelineManager.Instance != null)
            //{
            //    TimelineManager.Instance.CreateNewBlock(currentCaster.IsPlayer);
            //    // TODO: 현재 턴의 행동 로그 추가
            //}

            currentCaster.TurnChanse = false;
            currentCaster.IsMyTurn = false;
            //Debug.Log($"[턴 종료] {currentCaster.name}");
        }

        if (AllTurnChanseUsed())
        {
            Debug.Log($"[EndTurn] 모든 캐릭터가 턴을 사용했음. ResetTurn() 시작");
            ResetTurn();     // 다시 TurnChanse = true로 설정하고 속도순 재정렬
        }
        TurnDecider();

        if (NextTurnIndicatorUI.Instance != null)
            NextTurnIndicatorUI.Instance.RemoveCurrentTurnBlock();
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

    private bool CheckBattleEnd()
    {
        // 플레이어 팀 생존자 체크
        int playerAliveCount = playerSlots.Count(slot => {
            var cs = slot.GetComponentInChildren<CharacterStats>();
            return cs != null && !cs.IsDead;
        });

        // 적 팀 생존자 체크
        int enemyAliveCount = enemySlots.Count(slot => {
            var cs = slot.GetComponentInChildren<CharacterStats>();
            return cs != null && !cs.IsDead;
        });

        if (playerAliveCount == 0)
        {
            EndBattle(false);
            return true;
        }
        if (enemyAliveCount == 0)
        {
            EndBattle(true);
            return true;
        }
        return false;
    }

    private void EndBattle(bool isPlayerWin)
    {
        Debug.Log(isPlayerWin ? "플레이어 승리!" : "패배...");

        // ★ 파티원 HP 정보 저장
        BattleManager battleManager = FindObjectOfType<BattleManager>();
        if (battleManager != null)
        {
            battleManager.SavePartyStatusToStageSetting();
        }

        // TODO: 결과 UI, 보상 지급 등 추가

        // 예시: 2초 후 월드맵(혹은 스테이지)로 이동
        StartCoroutine(ReturnToStageCoroutine());
    }

    private IEnumerator ReturnToStageCoroutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("TestStage");
    }
}
