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

    [Header("팝업 관련")]
    [Tooltip("상태이상 팝업 핸들러")]
    public StatusPopupHandler popupHandler;

    private bool effectApplied = false;

    public int triggerCount; // 인스턴스별로 관리

    [Header("시각적 요소")]
    [SerializeField] private SpriteRenderer iconRenderer; // 월드 스프라이트용

    [SerializeField] private GameObject statusEffectPopupInstance; // Inspector에서 직접 연결

    /// <summary>
    /// 컴포넌트가 활성화될 때 UI 요소들을 찾아서 초기화
    /// </summary>
    private void Awake()
    {
        // UI 요소 참조
        effectIcon = transform.Find("Icon")?.GetComponent<Image>();
        durationText = transform.Find("DurationText")?.GetComponent<TextMeshProUGUI>();
        stackText = transform.Find("StackText")?.GetComponent<TextMeshProUGUI>();

        // 팝업 핸들러 참조
        popupHandler = statusEffectPopupInstance.GetComponent<StatusPopupHandler>();
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

        // 능동형 아이콘 시스템 적용
        Sprite dynamicIcon = effectData.GetDynamicIcon(effectValue);

        // 월드 스프라이트 갱신
        if (iconRenderer != null && dynamicIcon != null)
            iconRenderer.sprite = dynamicIcon;

        // UI 아이콘 갱신
        if (effectIcon != null && dynamicIcon != null)
            effectIcon.sprite = dynamicIcon;

        UpdateUI();

        Debug.Log($"[StatusEffectInstance] Initialize: {effectData.effectName}, {effectData.effectType}, {effectData.description}, 동적 아이콘 적용 (값: {effectValue})");
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
    /// 턴이 끝날 때 호출되는 메서드
    /// 상태이상 효과를 적용하고 지속시간을 감소시킴
    /// </summary>
    public void OnTurnEnd()
    {
        if (!isActive) return;

        remainingTurns--;
        UpdateUI();

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
    /// 턴이 시작할 때 호출되는 메서드
    /// 상태이상 효과를 적용하고 지속시간을 감소시킴
    /// </summary>
    /// <param name="target">효과가 적용될 대상</param>
    public bool OnTurnStart()
    {
        Debug.Log($"[StatusEffectInstance] OnTurnStart: {effectData.effectName}, {effectData.effectType}, {effectData.description}");
        if (!isActive || owner == null) return false;
        if (!owner.IsMyTurn) return false;

        // 지속피해 효과만 적용
        owner.Hp -= value;
        Debug.Log($"[StatusEffect] {owner.Label}: {effectData.effectName} 지속 피해 {value}, 남은 HP: {owner.Hp}");
        owner.HpUI.UpdateHpBar(owner.Hp, owner.MaxHp);

        owner.Deathcheck();
        owner.DeathAction();
        if (owner.IsDead)
        {
            // 사망 시 턴 종료는 외부에서 처리됨 (중복 호출 방지)
            Debug.Log($"[StatusEffect] {owner.Label} 사망으로 인한 턴 종료는 외부에서 처리됨");
            return true; // 더 이상 처리하지 않음
        }

        effectData.OnSpecialEffect(owner, this);

        return true;
    }

    private void ShowPopup()
    {
        statusEffectPopupInstance.SetActive(true);
        // ... 팝업 내용 갱신 등 ...
    }
}
