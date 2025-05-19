using UnityEngine;

// 출혈 효과
[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/Bleed")]
public class StatusEffectBleedData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "020001";
        effectType = StatusEffectType.ContinuousDamage;
        effectName = "출혈";
        description = "매 턴마다 피해를 입습니다.";
        iconPath = "StatusEffect/Bleed";
    }
}

