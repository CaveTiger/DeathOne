using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/GuardPowerBuff")]
public class StatusEffectGuardPowerBuffData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "021001";
        effectType = StatusEffectType.Buff;
        effectName = "방어도 상승";
        description = "지속 시간 동안 방어도 2 증가합니다.";
        iconPath = "StatusEffect/GuardPowerBuff";
    }

    public override void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        target.Def += 2;
        Debug.Log($"[StatusEffect] {target.Label}: 방어력 2 증가! 현재 방어력: {target.Def}");
    }
}