using UnityEngine;

public enum StatusEffectType
{
    None,
    Buff,
    Debuff,
    ContinuousDamage,
    Stun,
    Token
}

public enum StatType
{
    Def,
    Atk,
    Speed,
    Evasion,
    Accuracy,
    Hp,
    MaxHp,
    Mana,
    MaxMana
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
    public StatType statType;

    // 특수 효과가 필요할 때 오버라이드
    public virtual void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // 기본은 아무것도 안 함
    }

    // StatusEffect에서 이전된 메서드
    public virtual void Apply(CharacterStats target, int value, int duration)
    {
        // 기본 구현은 비어있음
    }

    /// <summary>
    /// 기본 아이콘을 반환하는 메서드
    /// </summary>
    public Sprite GetIcon()
    {
        // UI/ 접두사 추가 (실제 파일 위치에 맞춤)
        string fullPath = iconPath;
        if (!fullPath.StartsWith("UI/"))
        {
            fullPath = "UI/" + fullPath;
        }
        return Resources.Load<Sprite>(fullPath);
    }

    /// <summary>
    /// 능동형 아이콘 시스템: 값에 따라 다른 아이콘을 반환
    /// 양수면 버프 아이콘, 음수면 디버프 아이콘을 반환
    /// </summary>
    /// <param name="effectValue">상태이상 효과 값</param>
    /// <returns>값에 따른 적절한 아이콘</returns>
    public virtual Sprite GetDynamicIcon(int effectValue)
    {
        // 기본 아이콘 경로에서 확장자 제거
        string basePath = iconPath;
        if (basePath.Contains("."))
        {
            basePath = basePath.Substring(0, basePath.LastIndexOf('.'));
        }

        // UI/ 접두사 추가 (실제 파일 위치에 맞춤)
        if (!basePath.StartsWith("UI/"))
        {
            basePath = "UI/" + basePath;
        }

        string dynamicPath;
        
        if (effectValue >= 0)
        {
            // 0 이상: 기본/버프 아이콘 (Up 접미사)
            dynamicPath = basePath + "Up";
        }
        else
        {
            // 음수: 디버프 아이콘 (Down 접미사)
            dynamicPath = basePath + "Down";
        }

        Sprite dynamicIcon = Resources.Load<Sprite>(dynamicPath);
        
        // 동적 아이콘이 없으면 기본 아이콘 반환
        if (dynamicIcon == null)
        {
            Debug.LogWarning($"[StatusEffectData] 동적 아이콘을 찾을 수 없음: {dynamicPath}, 기본 아이콘 사용");
            return GetIcon();
        }

        Debug.Log($"[StatusEffectData] 동적 아이콘 로드: {dynamicPath} (값: {effectValue})");
        return dynamicIcon;
    }

    /// <summary>
    /// 능동형 아이콘 경로를 반환하는 메서드 (디버깅용)
    /// </summary>
    /// <param name="effectValue">상태이상 효과 값</param>
    /// <returns>예상되는 아이콘 경로</returns>
    public string GetDynamicIconPath(int effectValue)
    {
        string basePath = iconPath;
        if (basePath.Contains("."))
        {
            basePath = basePath.Substring(0, basePath.LastIndexOf('.'));
        }

        if (effectValue > 0)
        {
            return basePath + "Up";
        }
        else if (effectValue < 0)
        {
            return basePath + "Down";
        }
        else
        {
            return basePath;
        }
    }
}