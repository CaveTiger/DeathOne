using UnityEngine;

public enum StatusEffectType
{
    None,
    Buff,
    Debuff,
    ContinuousDamage
}

[CreateAssetMenu(fileName = "StatusEffect", menuName = "Scriptable Objects/StatusEffect")]
public class StatusEffectData : ScriptableObject
{
    public string EffectID;
    public StatusEffectType effectType;   // 효과 종류(enum)
    public string effectName;             // 효과 이름
    public string description;            // 설명
    public Sprite icon;                   // 아이콘
    public GameObject effectPrefab;       // 시각적 이펙트 프리팹
    public int duration;                  // 지속 턴
    public int value;                    // 피해량 
    public string iconPath = "StatusEffect/Bleed";
    public int maxTriggerCount = 0; // 사용 횟수 제한(0이면 횟수제한 없음)

    // 특수 효과가 필요할 때 오버라이드
    public virtual void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // 기본은 아무것도 안 함
    }
    public Sprite GetIcon()
    {
        return Resources.Load<Sprite>(iconPath);
    }
}