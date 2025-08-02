using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/GuardPowerBuff")]
public class StatusEffectGuardPowerBuffData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "021001";
        effectType = StatusEffectType.Buff; // 기본값은 Buff이지만 값에 따라 변경됨
        effectName = "방어력 변화";
        description = "지속 시간 동안 방어력이 변화합니다. (양수: 증가, 음수: 감소)";
        iconPath = "StatusEffect/DEFUp"; // 기본 아이콘 (방어력 증가)
    }

    public override void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // 값이 양수면 방어력 증가, 음수면 방어력 감소
        target.Def += instance.value;
        
        string effectType = instance.value > 0 ? "증가" : "감소";
        Debug.Log($"[StatusEffect] {target.Label}: 방어력 {Mathf.Abs(instance.value)} {effectType}! 현재 방어력: {target.Def}");
    }

    /// <summary>
    /// 능동형 아이콘 시스템을 위한 오버라이드
    /// </summary>
    public override Sprite GetDynamicIcon(int effectValue)
    {
        string dynamicPath;
        
        if (effectValue >= 0)
        {
            // 0 이상: 방어력 증가 아이콘
            dynamicPath = "StatusEffect/DEFUp";
        }
        else
        {
            // 음수: 방어력 감소 아이콘
            dynamicPath = "StatusEffect/DEFDown";
        }

        Sprite dynamicIcon = Resources.Load<Sprite>(dynamicPath);
        
        // 동적 아이콘이 없으면 기본 아이콘 반환
        if (dynamicIcon == null)
        {
            Debug.LogWarning($"[StatusEffectGuardPowerBuffData] 동적 아이콘을 찾을 수 없음: {dynamicPath}, 기본 아이콘 사용");
            return GetIcon();
        }

        Debug.Log($"[StatusEffectGuardPowerBuffData] 동적 아이콘 로드: {dynamicPath} (값: {effectValue})");
        return dynamicIcon;
    }
}