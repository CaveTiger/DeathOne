using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/NoDamageBuff")]
public class StatusEffectNoDamageBuffData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "024001";
        effectType = StatusEffectType.Token; // Buff에서 Token으로 변경
        effectName = "피해무시";
        description = "한 번의 피해를 완전히 무시합니다.";
        iconPath = "StatusEffect/NoDamage"; // 기존 아이콘 사용
        maxTriggerCount = 1;
    }
}
