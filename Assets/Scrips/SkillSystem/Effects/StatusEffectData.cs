using UnityEngine;

public enum StatusEffectType
{
    None,
    Buff,
    Debuff,
    ContinuousDamage,
    Stun,
    Token,
    Knockdown
}

public enum ReactionEffectMode
{
    None,
    DamageNullify,
    ReflectFixed,
    ReflectPercentOfReceived
}

public enum ReactionDamageReductionMode
{
    None,
    Fixed,
    PercentOfReceived
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
    public int value;                    // 효과 수치(지속피해는 +, 지속회복은 -)
    public string iconPath = "StatusEffect/Bleed";
    public int maxTriggerCount = 0; // 사용 횟수 제한(0이면 횟수제한 없음)
    public StatType statType;
    [Header("타겟 제한")]
    [Tooltip("체크 시, 이 버프를 가진 시전자는 자기 자신을 제외한 아군을 타겟으로 지정할 수 없습니다. (도발 기믹용)")]
    public bool blockOtherAlliesAsTarget = false;
    [Tooltip("체크 시, 랜덤 타겟 스킬에도 도발 우선 타겟팅을 적용합니다. (일반 도발은 보통 해제)")]
    public bool tauntAffectsRandomTargeting = false;
    [Tooltip("체크 시, 광역/범위 타겟 스킬에도 도발 우선 타겟팅을 적용합니다. (완전 보호형 도발용)")]
    public bool tauntProtectsAgainstAoE = false;
    [Tooltip("체크 시, 지목형 디버프로 간주하여 AI가 이 대상을 우선 고려합니다.")]
    public bool markPriorityTarget = false;
    [Tooltip("지목형 디버프가 걸린 대상이 받는 피해 배율. 기본 1.1 (10% 증가)")]
    public float markDamageTakenMultiplier = 1.1f;
    [Header("지속 효과 해석")]
    [Tooltip("체크 시 XML 양수를 자동으로 음수로 전환해 사용")]
    public bool treatContinuousValueAsHeal = false;

    [Header("리액션(Reaction) 설정")]
    [Tooltip("리액션 동작 선택: 없음 / 피해무시 / 고정 반사 / 받은 피해 비율 반사")]
    public ReactionEffectMode reactionMode = ReactionEffectMode.None;
    [Tooltip("고정 반사 피해량. (useAppliedValueForReaction=true면 XML/스킬 Value를 사용)")]
    public int reactionFixedValue = 0;
    [Tooltip("받은 피해의 반사 비율(%). (useAppliedValueForReaction=true면 XML/스킬 Value를 퍼센트로 사용)")]
    [Range(0f, 100f)] public float reactionPercent = 0f;
    [Tooltip("켜면 반사 수치를 SO 고정값 대신 XML/스킬 Value에서 읽음")]
    public bool useAppliedValueForReaction = true;
    [Tooltip("켜면 반사량 = 이번 피격에서 실제로 줄어든 피해량(감소량)")]
    public bool reflectReducedAmountDirect = false;
    [Tooltip("퍼센트 반사 기준을 '받은 원피해' 대신 '감소량'으로 계산할지")]
    public bool useReducedAmountAsReflectBase = false;

    [Header("리액션 피해 감소 설정 (선택)")]
    [Tooltip("피격 시 먼저 적용할 피해 감소 방식: 없음 / 고정 감소 / 퍼센트 감소")]
    public ReactionDamageReductionMode reductionMode = ReactionDamageReductionMode.None;
    [Tooltip("고정 피해 감소량. (useAppliedValueForReduction=true면 XML/스킬 Value를 사용)")]
    public int reductionFixedValue = 0;
    [Tooltip("피해 감소 비율(%). (useAppliedValueForReduction=true면 XML/스킬 Value를 퍼센트로 사용)")]
    [Range(0f, 100f)] public float reductionPercent = 0f;
    [Tooltip("켜면 감소 수치를 SO 고정값 대신 XML/스킬 Value에서 읽음")]
    public bool useAppliedValueForReduction = false;

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