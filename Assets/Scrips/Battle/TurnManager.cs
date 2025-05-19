using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.TextCore.Text;
using static UnityEngine.GraphicsBuffer;
public class TurnManager : MonoBehaviour
{
    private List<SlotHandler> turnQueue = new List<SlotHandler>();
    public List<SlotHandler> allSlots = new(); 
    public CharacterStats currentCaster;
    public static TurnManager Instance { get; private set; }

    void Awake()
    {
        Instance = this;
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
    }
    private void Start()
    {
        ResetTurn();
        TurnDecider();
    }
    private void TurnDecider()
    {
        CharacterStats fastest = null;
        float topSpeed = float.MinValue;
        bool hasTurnable = false;

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
            }
        }

        // 반복문 밖에서 단 한 번만 인디케이터 표시!
        if (fastest != null)
        {
            TurnIndicatorHandler.Instance.SetIndicator(fastest.transform, true);
            StartTurn(fastest);
            //Debug.Log($"[TurnDecider] 가장 빠른 캐릭터: {fastest.name}, 속도: {fastest.Speed}");
        }
        else if (!hasTurnable)
        {
            TurnIndicatorHandler.Instance.SetIndicator(null, false);
            Debug.Log(" 모든 캐릭터 턴 종료됨 새 턴 사이클 시작");
            ResetTurn();
            TurnDecider();
        }
    }

    private void StartTurn(CharacterStats character)
    {
        if (!character.IsActive)
        {
            Debug.Log($"[{character.name}]는 현재 행동 불능. 턴 스킵");
            TurnDecider(); // 다음 턴으로
            return;
        }

        character.IsMyTurn = true;
        currentCaster = character;

        // 2. 상태이상 효과 적용
        character.ApplyStatusEffectsOnTurnStart();

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
    }
    private bool AllTurnChanseUsed()
    {
        return turnQueue.All(slot => slot.currentCharacter != null && !slot.currentCharacter.TurnChanse);
    }
}
