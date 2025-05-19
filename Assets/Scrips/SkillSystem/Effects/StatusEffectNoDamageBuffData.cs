using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/NoDamageBuff")]
public class StatusEffectNoDamageBuffData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "021002";
        effectType = StatusEffectType.Buff;
        effectName = "피해무시";
        description = "한 번의 피해를 완전히 무시합니다.";
        iconPath = "StatusEffect/IgnoreDamage";
        maxTriggerCount = 1;
    }
}
