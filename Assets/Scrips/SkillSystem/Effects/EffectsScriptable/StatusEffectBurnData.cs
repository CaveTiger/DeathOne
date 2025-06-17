using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/Burn")]
public class StatusEffectBurnData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "020003";
        effectType = StatusEffectType.ContinuousDamage;
        effectName = "화상";
        description = "매 턴마다 피해를 입고, 주변 유닛에게도 피해를 줍니다.";
        iconPath = "StatusEffect/Burn";
    }
    // 화상 효과: 주 타겟과 양 옆 슬롯의 유닛에게 피해 적용
    public override void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // 주 타겟에게 피해
        target.Hp -= instance.value;
        Debug.Log($"화상 피해: {instance.value} ({target.Label})");

        // 슬롯 이름에서 인덱스 추출 (예: Eslot2 → 2)
        string slotName = target.transform.parent.name;
        int slotIndex = 0;
        if (slotName.Length > 5 && int.TryParse(slotName.Substring(slotName.Length - 1), out slotIndex))
        {
            // 왼쪽 슬롯
            if (slotIndex > 1)
            {
                var leftSlot = GameObject.Find(slotName.Substring(0, slotName.Length - 1) + (slotIndex - 1));
                if (leftSlot != null && leftSlot.transform.childCount > 0)
                {
                    var leftUnit = leftSlot.transform.GetChild(0).GetComponent<CharacterStats>();
                    if (leftUnit != null && !leftUnit.IsDead)
                    {
                        int splash = Mathf.RoundToInt(instance.value * 0.5f);
                        leftUnit.Hp -= splash;
                        Debug.Log($"화상 확산 피해(왼쪽): {splash} ({leftUnit.Label})");
                    }
                }
            }
            // 오른쪽 슬롯
            if (slotIndex < 4)
            {
                var rightSlot = GameObject.Find(slotName.Substring(0, slotName.Length - 1) + (slotIndex + 1));
                if (rightSlot != null && rightSlot.transform.childCount > 0)
                {
                    var rightUnit = rightSlot.transform.GetChild(0).GetComponent<CharacterStats>();
                    if (rightUnit != null && !rightUnit.IsDead)
                    {
                        int splash = Mathf.RoundToInt(instance.value * 0.5f);
                        rightUnit.Hp -= splash;
                        Debug.Log($"화상 확산 피해(오른쪽): {splash} ({rightUnit.Label})");
                    }
                }
            }
        }
    }
}
