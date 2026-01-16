using UnityEngine;
using System.Collections;

public class SlotHandler : MonoBehaviour
{
    public CharacterStats currentCharacter;
    public int slotIndex; // 슬롯 순서 저장 (성능 최적화용)
    

    public void FindUnit()
    {
        currentCharacter = GetComponentInChildren<CharacterStats>();

        if (currentCharacter != null)
        {
            //Debug.Log($"[슬롯 {name}] 캐릭터 로드 완료: {currentCharacter.name}");
        }
        else
        {
            //Debug.LogWarning($"[슬롯 {name}] 캐릭터가 자식에 존재하지 않음!");
        }
    }
    public CharacterStats SlotCharacterLoad()
    {
        FindUnit();
        return currentCharacter;
    }

    // 턴 시작 직전 상태이상 정산 (슬롯 컨테이너 중심 설계 대비용)
    public void SettleStatusEffectsAtTurnStart()
    {
        if (currentCharacter == null)
            FindUnit();

        if (currentCharacter == null)
            return;

        var controller = currentCharacter.GetComponent<StatusEffectController>();
        if (controller != null)
        {
            controller.ApplyStatusEffectsOnTurnStart();
        }
    }

    /// <summary>
    /// 상태이상 정산 + 연출을 함께 처리하는 메서드
    /// </summary>
    public IEnumerator SettleStatusEffectsWithAnimation(CharacterStats character)
    {
        // 매개변수로 받은 캐릭터를 currentCharacter로 설정 (동기화 보장)
        currentCharacter = character;
        
        if (currentCharacter == null)
        {
            Debug.LogWarning($"[SlotHandler] {name}: currentCharacter가 null입니다.");
            yield break;
        }

        Debug.Log($"[SlotHandler] {name}: 상태이상 정산 시작 - 캐릭터: {currentCharacter.Label}");

        // StatusEffectSlot을 통해 상태이상 정산 + 연출 처리
        var statusEffectSlot = currentCharacter.transform.Find("StatusEffectSlot")?.GetComponent<StatusEffectSlot>();
        if (statusEffectSlot != null)
        {
            yield return StartCoroutine(statusEffectSlot.SettleStatusEffectsWithAnimation(currentCharacter));
        }
        else
        {
            Debug.LogWarning($"[SlotHandler] {name}: StatusEffectSlot을 찾을 수 없습니다.");
            // StatusEffectSlot이 없는 경우 기존 방식 백업
            SettleStatusEffectsAtTurnStart();
        }
    }

}
