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
    [Header("기본 정보 (필수)")]
    [Tooltip("상태이상 ID (XML에서 참조하는 ID, 예: 021004)")]
    public string EffectID;
    [Tooltip("상태이상 이름 (표시용)")]
    public string effectName;             // 효과 이름
    [Tooltip("효과 종류")]
    public StatusEffectType effectType;   // 효과 종류(enum)
    [Tooltip("상태이상 설명")]
    public string description;            // 설명
    [Header("아이콘 설정")]
    [Tooltip("기본 아이콘 (양수값 또는 일반 상태이상용)")]
    public Sprite icon;                   // 아이콘
    [Tooltip("대응형 아이콘 (음수값용, 선택적)")]
    public Sprite negativeIcon;           // 음수값용 아이콘 (선택적)
    [Header("색상 설정")]
    [Tooltip("상태이상 피해 시 캐릭터 색상 효과 (Inspector에서 선택 가능)")]
    public Color effectColor = Color.red; // 상태이상 고유 색상
    public GameObject effectPrefab;       // 시각적 이펙트 프리팹
    public int duration;                  // 지속 턴
    public int value;                    // 피해량 
    public string iconPath = "StatusEffect/Bleed";
    public int maxTriggerCount = 0; // 사용 횟수 제한(0이면 횟수제한 없음)
    public StatType statType;

    /// <summary>
    /// ScriptableObject 활성화 시 호출 - EffectID가 비어있으면 경고
    /// </summary>
    private void OnEnable()
    {
        // EffectID가 비어있으면 경고 출력 (Inspector에서 입력 필요)
        if (string.IsNullOrEmpty(EffectID))
        {
            Debug.LogWarning($"[StatusEffectData] '{name}' ScriptableObject의 EffectID가 비어있습니다. " +
                           $"Inspector에서 EffectID를 입력해주세요 (예: 021004). " +
                           $"현재 effectName: '{effectName}'");
        }
    }

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
        // 1순위: ScriptableObject에 직접 할당된 아이콘
        if (icon != null)
            return icon;

        // 2순위: 경로 기반 로드
        string fullPath = iconPath;
        if (!fullPath.StartsWith("UI/"))
        {
            fullPath = "UI/" + fullPath;
        }
        return Resources.Load<Sprite>(fullPath);
    }

    /// <summary>
    /// 능동형 아이콘 시스템: 값에 따라 다른 아이콘을 반환
    /// 양수면 기본 아이콘, 음수면 대응형 아이콘을 반환
    /// </summary>
    /// <param name="effectValue">상태이상 효과 값</param>
    /// <returns>값에 따른 적절한 아이콘</returns>
    public virtual Sprite GetDynamicIcon(int effectValue)
    {
        // 업/다운 개념이 없는 효과(예: 지속피해, 토큰 등)는 기본 아이콘 사용
        if (effectType != StatusEffectType.Buff && effectType != StatusEffectType.Debuff)
        {
            // ScriptableObject에 직접 할당된 아이콘이 있으면 우선 사용
            if (icon != null)
                return icon;
            return GetIcon();
        }

        // 1순위: ScriptableObject에 직접 할당된 아이콘 사용
        if (effectValue < 0 && negativeIcon != null)
        {
            // 음수값이고 negativeIcon이 할당되어 있으면 사용
            Debug.Log($"[StatusEffectData] 대응형 아이콘 사용 (음수값): {effectName} (값: {effectValue})");
            return negativeIcon;
        }
        else if (effectValue >= 0 && icon != null)
        {
            // 양수값이고 icon이 할당되어 있으면 사용
            Debug.Log($"[StatusEffectData] 기본 아이콘 사용 (양수값): {effectName} (값: {effectValue})");
            return icon;
        }

        // 2순위: 경로 기반 동적 로드 (기존 방식, 폴백)
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
        
        if (effectValue > 0)
        {
            // 양수: 버프 아이콘 (Up)
            dynamicPath = basePath + "Up";
            Sprite upIcon = Resources.Load<Sprite>(dynamicPath);
            if (upIcon != null)
            {
                Debug.Log($"[StatusEffectData] 경로 기반 동적 아이콘 로드: {dynamicPath} (값: {effectValue})");
                return upIcon;
            }
        }
        else if (effectValue < 0)
        {
            // 음수: 디버프 아이콘 (Down)
            dynamicPath = basePath + "Down";
            Sprite downIcon = Resources.Load<Sprite>(dynamicPath);
            if (downIcon != null)
            {
                Debug.Log($"[StatusEffectData] 경로 기반 동적 아이콘 로드: {dynamicPath} (값: {effectValue})");
                return downIcon;
            }
        }

        // 3순위: 기본 아이콘 반환
        if (icon != null)
            return icon;
        
        return GetIcon();
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