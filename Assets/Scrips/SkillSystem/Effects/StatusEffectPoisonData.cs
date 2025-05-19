using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/Poison")]
public class StatusEffectPoisonData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "020002";
        effectType = StatusEffectType.ContinuousDamage;
        effectName = "중독";
        description = "2턴마다 피해를 입습니다.";
        iconPath = "StatusEffect/Poison";
    }
    // 중독 효과: 2턴마다 한 번씩 피해를 입힘
    public override void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // 지속수치가 홀수일 때만 피해 적용
        if (instance.remainingTurns % 2 == 1)
        {
            target.Hp -= instance.value;
            Debug.Log($"중독 피해: {instance.value} (남은턴: {instance.remainingTurns})");
        }
    }
}
