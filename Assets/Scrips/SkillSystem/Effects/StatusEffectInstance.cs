using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

/// <summary>
/// 상태이상의 개별 인스턴스를 관리하는 컴포넌트
/// 상태이상 프리팹에 부착되어 해당 상태이상의 시각적 표현과 로직을 처리
/// </summary>
public class StatusEffectInstance : MonoBehaviour
{
    [Header("상태이상 기본 정보")]
    [SerializeField] private StatusEffectData effectData;   // 인스펙터에서 연결
    public StatusEffectData EffectData => effectData;
    
    [Tooltip("효과가 지속될 남은 턴 수")]
    public int remainingTurns;        // 남은 턴 수
    
    [Tooltip("상태이상의 수치 (피해량, 회복량, 버프 수치 등)")]
    public int value;                 // 피해량 등
    
    [Tooltip("현재 상태이상이 활성화되어 있는지 여부")]
    public bool isActive = true;       // 효과 활성 여부
    
    [Tooltip("이 상태이상이 적용된 캐릭터")]
    public CharacterStats owner;       // 상태이상 소유자

    [Header("UI 요소")]
    [Tooltip("상태이상 아이콘을 표시하는 이미지")]
    private Image effectIcon;
    
    [Tooltip("남은 턴 수를 표시하는 텍스트")]
    private TextMeshProUGUI durationText;
    
    [Tooltip("중첩된 효과의 수치를 표시하는 텍스트")]
    private TextMeshProUGUI stackText;

    private bool effectApplied = false;

    public int triggerCount; // 인스턴스별로 관리

    [Header("시각적 요소")]
    [SerializeField] private SpriteRenderer iconRenderer; // 월드 스프라이트용

    /// <summary>
    /// 컴포넌트가 활성화될 때 UI 요소들을 찾아서 초기화
    /// </summary>
    private void Awake()
    {
        // UI 요소 참조
        effectIcon = transform.Find("Icon")?.GetComponent<Image>();
        durationText = transform.Find("DurationText")?.GetComponent<TextMeshProUGUI>();
        stackText = transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();

        // iconRenderer가 할당되지 않았으면 자동으로 찾기
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
            if (iconRenderer == null)
            {
                iconRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
    }

    /// <summary>
    /// 상태이상 인스턴스를 초기화
    /// </summary>
    /// <param name="data">상태이상 데이터</param>
    /// <param name="duration">지속 턴 수</param>
    /// <param name="effectValue">효과 수치</param>
    /// <param name="target">대상 캐릭터</param>
    public void Initialize(StatusEffectData data, int duration, int effectValue, CharacterStats target)
    {
        effectData = data;
        remainingTurns = duration;
        value = effectValue;
        owner = target;
        triggerCount = data.maxTriggerCount;
        
        // 상태이상 적용 시 지속시간 감소 제거 (정산 시에만 감소)
        // if (remainingTurns > 0)
        // {
        //     remainingTurns--;
        //     Debug.Log($"[StatusEffectInstance] {data.effectName} 적용 즉시 지속시간 감소: {duration} → {remainingTurns}");
        // }
        
        // UI 즉시 업데이트
        UpdateUI();

        // iconRenderer가 없으면 다시 찾기
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
            if (iconRenderer == null)
            {
                iconRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }
        
        // 호버 감지를 위한 Collider2D 자동 추가 (없으면 추가)
        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {
            // BoxCollider2D 추가 (SpriteRenderer 크기에 맞춤)
            BoxCollider2D boxCollider = gameObject.AddComponent<BoxCollider2D>();
            if (iconRenderer != null && iconRenderer.sprite != null)
            {
                // Sprite 크기에 맞춰 Collider 크기 설정
                boxCollider.size = iconRenderer.sprite.bounds.size;
            }
            else
            {
                // 기본 크기 (상태이상 아이콘 크기)
                boxCollider.size = new Vector2(0.5f, 0.5f);
            }
            boxCollider.isTrigger = true; // 트리거로 설정하여 물리 충돌 없이 감지만
        }

        // 아이콘 설정 (버프/디버프는 동적 아이콘 우선, 그 외는 기본 아이콘)
        Sprite icon = null;
        
        // 버프/디버프 타입은 동적 아이콘 우선 사용 (음수값 대응)
        if (effectData.effectType == StatusEffectType.Buff || effectData.effectType == StatusEffectType.Debuff)
        {
            // 동적 아이콘 시도 (음수값일 때 negativeIcon 사용)
            icon = effectData.GetDynamicIcon(effectValue);
            
            // 동적 아이콘이 없으면 기본 아이콘 시도
            if (icon == null)
            {
                icon = effectData.GetIcon();
            }
        }
        else
        {
            // 그 외 타입은 기본 아이콘 우선
            icon = effectData.GetIcon();
            
            // 기본 아이콘이 없으면 동적 아이콘 시도
            if (icon == null)
            {
                icon = effectData.GetDynamicIcon(effectValue);
            }
        }

        // 월드 스프라이트 갱신
        if (iconRenderer != null && icon != null)
        {
            iconRenderer.sprite = icon;
            Debug.Log($"[StatusEffectInstance] 월드 아이콘 설정 완료: {effectData.effectName} - {icon.name}");
        }
        else if (iconRenderer == null)
        {
            Debug.LogWarning("[StatusEffectInstance] iconRenderer를 찾을 수 없습니다.");
        }
        else if (icon == null)
        {
            Debug.LogWarning($"[StatusEffectInstance] 아이콘을 찾을 수 없습니다: {effectData.effectName} (ID: {effectData.EffectID})");
        }

        // UI 아이콘 갱신
        if (effectIcon != null && icon != null)
        {
            effectIcon.sprite = icon;
            Debug.Log($"[StatusEffectInstance] UI 아이콘 설정 완료: {effectData.effectName} - {icon.name}");
        }

        UpdateUI();

        Debug.Log($"[StatusEffectInstance] Initialize: {effectData.effectName}, {effectData.effectType}, {effectData.description}, 아이콘 적용 (값: {effectValue})");
    }

    /// <summary>
    /// 지속피해 중첩(3안): 이미 같은 ID가 있을 때 재적용되면, 들어온 수치의 절반(내림)만 현재 피해량에 합산. 지속 턴은 바꾸지 않음.
    /// </summary>
    public void MergeHalfIncomingDamage(int incomingValue)
    {
        int add = Mathf.FloorToInt(incomingValue / 2f);
        value += add;
        UpdateUI();
    }

    /// <summary>
    /// UI 요소들을 현재 상태에 맞게 업데이트
    /// </summary>
    private void UpdateUI()
    {
        if (durationText != null)
            durationText.text = remainingTurns.ToString();
        
        if (stackText != null && value > 1)
            stackText.text = value.ToString();
    }

    /// <summary>
    /// 지속 턴을 감소시키는 메서드 (특정 타이밍에서 호출)
    /// </summary>
    public void ReduceDuration()
    {
        if (!isActive) return;
        
        remainingTurns--;
        UpdateUI();
        
        Debug.Log($"[StatusEffectInstance] {effectData.effectName} 지속 턴 감소: {remainingTurns + 1} → {remainingTurns}");
        
        if (remainingTurns <= 0)
        {
            // 효과 해제(복구)
            if (effectApplied && owner != null)
            {
                effectApplied = false;
            }
            isActive = false;
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 상태이상 효과를 적용하는 메서드 (수동 호출)
    /// </summary>
    public bool ApplyEffect()
    {
        Debug.Log($"[StatusEffectInstance] ApplyEffect: {effectData.effectName}, {effectData.effectType}, {effectData.description}");
        if (!isActive || owner == null) return false;
        if (!owner.IsMyTurn) return false;

        // 상태이상 피해량 팝업 표시 (체력 감소는 팝업 표시 시 처리됨)
        CreateStatusEffectDamagePopup(owner.transform.position, value);
        
        // 체력 감소 및 사망 체크는 BattleUIManager의 ProcessStatusEffectPopupsCoroutine에서 처리됨
        // 여기서는 팝업만 큐에 추가

        effectData.OnSpecialEffect(owner, this);

        return true;
    }

    private void ShowPopup()
    {
        // 상태이상 팝업 표시는 VirtualMouse를 통해 처리됨
        // 이 메서드는 레거시 코드로 보이며 현재 사용되지 않음
    }

    /// <summary>
    /// 상태이상 피해 팝업을 생성합니다 (BattleUIManager를 통해 처리)
    /// </summary>
    /// <param name="position">월드 좌표 위치</param>
    /// <param name="damage">피해량</param>
    private void CreateStatusEffectDamagePopup(Vector3 position, int damage)
    {
        if (BattleUIManager.Instance != null)
            {
            BattleUIManager.Instance.CreateStatusEffectDamagePopup(position, damage, effectData, value, owner);
        }
        else
        {
            Debug.LogWarning("[StatusEffectInstance] BattleUIManager.Instance를 찾을 수 없습니다.");
        }
    }


    /// <summary>
    /// 버추얼 마우스(또는 외부 시스템)가 팝업 표시용 데이터를 가져갈 수 있도록 제공합니다.
    /// </summary>
    public void GetStatusPopupData(out Sprite icon, out string description, out int displayValue, out int turns)
    {
        icon = null;
        description = string.Empty;
        displayValue = 0;
        turns = 0;

        if (effectData == null) return;

        // 버프/디버프 타입은 동적 아이콘 우선 사용 (음수값 대응)
        if (effectData.effectType == StatusEffectType.Buff || effectData.effectType == StatusEffectType.Debuff)
        {
            icon = effectData.GetDynamicIcon(value);
            if (icon == null)
            {
                icon = effectData.GetIcon();
            }
        }
        else
        {
            icon = effectData.icon;
            if (icon == null)
            {
                icon = effectData.GetIcon();
            }
        }
        
        description = string.IsNullOrEmpty(effectData.description) ? effectData.effectName : effectData.description;
        displayValue = value;
        turns = Mathf.Max(remainingTurns, 0);
    }
}
