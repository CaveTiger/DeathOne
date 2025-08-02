using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/HpReduction")]
public class StatusEffectHpReductionData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "022001";
        effectType = StatusEffectType.Debuff;
        effectName = "체력 감소";
        description = "체력이 일정 비율 감소합니다.";
        iconPath = "StatusEffect/HpReduction";
    }

    public override void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // 체력의 Value%만큼 감소
        int reductionAmount = Mathf.RoundToInt(target.MaxHp * (instance.value / 100f));
        target.Hp -= reductionAmount;
        
        Debug.Log($"[StatusEffect] {target.Label}: 체력 {instance.value}% 감소! 감소량: {reductionAmount}, 현재 체력: {target.Hp}");
    }
} 