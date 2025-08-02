using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/HpChange")]
public class StatusEffectHpChangeData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "021004";
        effectType = StatusEffectType.Buff; // 기본값은 Buff이지만 값에 따라 변경됨
        effectName = "체력 변화";
        description = "지속 시간 동안 체력이 변화합니다. (양수: 증가, 음수: 감소)";
        iconPath = "StatusEffect/ATKUp"; // 체력 변화용 아이콘 (임시로 공격력 아이콘 사용, 회복 효과임을 나타냄)
    }

    public override void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // 값이 양수면 체력 증가, 음수면 체력 감소
        target.Hp += instance.value;
        
        // 체력이 최대치를 넘지 않도록 제한
        if (target.Hp > target.MaxHp)
            target.Hp = target.MaxHp;
        
        // 체력이 0 이하로 내려가지 않도록 제한
        if (target.Hp < 0)
            target.Hp = 0;
        
        string effectType = instance.value > 0 ? "증가" : "감소";
        Debug.Log($"[StatusEffect] {target.Label}: 체력 {Mathf.Abs(instance.value)} {effectType}! 현재 체력: {target.Hp}/{target.MaxHp}");
        
        // UI 업데이트
        if (target.HpUI != null)
            target.HpUI.UpdateHpBar(target.Hp, target.MaxHp);
    }

    /// <summary>
    /// 능동형 아이콘 시스템을 위한 오버라이드
    /// </summary>
    public override Sprite GetDynamicIcon(int effectValue)
    {
        string dynamicPath;
        
        if (effectValue >= 0)
        {
            // 0 이상: 방어력 증가 아이콘 (임시)
            dynamicPath = "StatusEffect/DEFUp";
        }
        else
        {
            // 음수: 방어력 감소 아이콘 (임시)
            dynamicPath = "StatusEffect/DEFDown";
        }

        Sprite dynamicIcon = Resources.Load<Sprite>(dynamicPath);
        
        // 동적 아이콘이 없으면 기본 아이콘 반환
        if (dynamicIcon == null)
        {
            Debug.LogWarning($"[StatusEffectHpChangeData] 동적 아이콘을 찾을 수 없음: {dynamicPath}, 기본 아이콘 사용");
            return GetIcon();
        }

        Debug.Log($"[StatusEffectHpChangeData] 동적 아이콘 로드: {dynamicPath} (값: {effectValue})");
        return dynamicIcon;
    }
} 