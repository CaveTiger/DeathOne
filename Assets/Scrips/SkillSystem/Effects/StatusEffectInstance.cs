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

        // 월드 스프라이트 갱신
        if (iconRenderer != null && effectData.icon != null)
            iconRenderer.sprite = effectData.icon;

        // UI 아이콘 갱신
        if (effectIcon != null && effectData.icon != null)
            effectIcon.sprite = effectData.icon;

        UpdateUI();

        Debug.Log($"[StatusEffectInstance] Initialize: {effectData.effectName}, {effectData.effectType}, {effectData.description}");
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
            if (effectApplied)
            {
                switch (effectData.effectType)
                {
                    case StatusEffectType.Buff:
                        owner.Def -= value;
                        Debug.Log($"[StatusEffect] {owner.Label}: 방어력 버프 해제! 현재 방어력: {owner.Def}");
                        break;
                    case StatusEffectType.Debuff:
                        owner.Def += value;
                        Debug.Log($"[StatusEffect] {owner.Label}: 방어력 디버프 해제! 현재 방어력: {owner.Def}");
                        break;
                }
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
        if (!isActive || owner == null) return false;
        if (!owner.IsMyTurn) return false;

        if (!effectApplied)
        {
            switch (effectData.effectType)
            {
                case StatusEffectType.Buff:
                    owner.Def += value;
                    Debug.Log($"[StatusEffect] {owner.Label}: 방어력 {value} 증가! 현재 방어력: {owner.Def}");
                    break;
                case StatusEffectType.Debuff:
                    owner.Def -= value;
                    Debug.Log($"[StatusEffect] {owner.Label}: 방어력 {value} 감소! 현재 방어력: {owner.Def}");
                    break;
                case StatusEffectType.ContinuousDamage:
                    // 지속피해는 매 턴마다 적용
                    break;
            }
            effectApplied = true;
        }

        if (effectData.effectType == StatusEffectType.ContinuousDamage)
        {
            owner.Hp -= value;
            Debug.Log($"[StatusEffect] {owner.Label}: {effectData.effectName} 지속 피해 {value}, 남은 HP: {owner.Hp}");
            owner.HpUI.UpdateHpBar(owner.Hp, owner.MaxHp);
        }

        effectData.OnSpecialEffect(owner, this);

        return true;
    }

    private void OnMouseEnter()
    {
        Debug.Log("마우스 들어감");
        if (statusEffectPopupInstance != null && effectData != null)
        {
            statusEffectPopupInstance.SetActive(true); //팝업창을 활성
            var popupHandler = statusEffectPopupInstance.GetComponent<StatusPopupHandler>(); //컴포넌트 받아내기
            if (popupHandler != null) //팝업핸들러가 비지 않았을때
            {
                popupHandler.ShowStatusPopup(
                    effectData.icon,
                    effectData.description,
                    value,
                    remainingTurns
                ); //내부 데이터 채워넣기 위한 매개변수
            }
            // 팝업 위치를 상태이상 아이콘 근처로 이동
            statusEffectPopupInstance.transform.position = this.transform.position + new Vector3(1, 1, 0);
        }
    }

    private void OnMouseExit()
    {
        if (statusEffectPopupInstance != null) //이 역시조건은 안전장치
        {
            statusEffectPopupInstance.SetActive(false); //비활성화
        }
    }

    /// <summary>
    /// 피격 시 호출되는 상태이상 효과 처리 메서드
    /// </summary>
    /// <param name="damage">참조로 전달되는 피해량</param>
    /// <returns>피해를 무시하면 true, 아니면 false</returns>
    public virtual bool OnTakeDamage(ref int damage)
    {
        // 피해무시 효과(EffectID == "021002" && triggerCount > 0)일 때만 동작
        if (effectData != null && effectData.EffectID == "021002" && triggerCount > 0)
        {
            triggerCount--;
            Debug.Log($"[피해무시] {owner.Label}가 피해를 무시했습니다! 남은 횟수: {triggerCount}");
            damage = 0;
            if (triggerCount <= 0)
            {
                remainingTurns = 0;
                OnTurnEnd();
            }
            return true;
        }
        // 기본은 아무 효과 없음
        return false;
    }

    private void ShowPopup()
    {
        statusEffectPopupInstance.SetActive(true);
        // ... 팝업 내용 갱신 등 ...
    }
}
