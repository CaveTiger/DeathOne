using UnityEngine;

public class StatusEffectInstanceBuff : StatusEffectInstanceBase
{
    [Header("상태이상 기본 정보")]
    public bool isActive = true;       // 효과 활성 여부

    [Header("시각적 요소")]
    [SerializeField] private SpriteRenderer iconRenderer; // 월드 스프라이트용(선택)

    private void Awake()
    {
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

    public void Start()
    {

    }
    
    /// <summary>
    /// 버프 효과 해제(스탯 원복)
    /// </summary>
    public void RemoveEffect()
    {
        if (owner == null) return;

        var statField = owner.GetType().GetField(effectData.statType.ToString());
        if (statField != null)
        {
            int current = (int)statField.GetValue(owner);
            statField.SetValue(owner, current - value);
        }
        else
        {
            Debug.LogWarning($"[StatusEffectInstanceBuff] {effectData.statType} 필드를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 지속 턴을 감소시키는 메서드 (특정 타이밍에서 호출)
    /// </summary>
    public void ReduceDuration()
    {
        if (!isActive) return;
        
        remainingTurns--;
        UpdateUI();
        
        Debug.Log($"[StatusEffectInstanceBuff] {effectData.effectName} 지속 턴 감소: {remainingTurns + 1} → {remainingTurns}");
        
        if (remainingTurns <= 0)
        {
            // 효과 해제
            RemoveEffect();
            Destroy(this.gameObject);
        }
    }

    public void OnTurnEnd()
    {
        // 턴 종료 시 상태이상 관리 일절 안 함
        // 특수한 로직이 필요한 경우에만 여기에 추가
    }

    public void Initialize(StatusEffectData effectData, int duration, int value, CharacterStats owner)
    {
        InitializeBasic(effectData, duration, value, owner);
        
        // 상태이상 적용 시 지속시간 감소 제거 (정산 시에만 감소)
        // if (remainingTurns > 0)
        // {
        //     remainingTurns--;
        //     Debug.Log($"[StatusEffectInstanceBuff] {effectData.effectName} 적용 즉시 지속시간 감소: {duration} → {remainingTurns}");
        // }
        
        // UI 즉시 업데이트
        UpdateUI();

        // 아이콘 갱신 - iconRenderer가 없으면 다시 찾기
        if (iconRenderer == null)
        {
            iconRenderer = GetComponent<SpriteRenderer>();
            if (iconRenderer == null)
            {
                iconRenderer = GetComponentInChildren<SpriteRenderer>();
            }
        }

        // 아이콘 설정 (버프/디버프는 동적 아이콘 우선, 그 외는 기본 아이콘)
        if (effectData != null && iconRenderer != null)
        {
            Sprite icon = null;
            
            // 버프/디버프 타입은 동적 아이콘 우선 사용 (음수값 대응)
            if (effectData.effectType == StatusEffectType.Buff || effectData.effectType == StatusEffectType.Debuff)
            {
                // 동적 아이콘 시도 (음수값일 때 negativeIcon 사용)
                icon = effectData.GetDynamicIcon(value);
                
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
                    icon = effectData.GetDynamicIcon(value);
                }
            }
            
            // 아이콘 설정
            if (icon != null)
            {
                iconRenderer.sprite = icon;
                Debug.Log($"[StatusEffectInstanceBuff] 아이콘 설정 완료: {effectData.effectName} (값: {value}) - {icon.name}");
            }
            else
            {
                Debug.LogWarning($"[StatusEffectInstanceBuff] 아이콘을 찾을 수 없습니다: {effectData.effectName} (ID: {effectData.EffectID}, 값: {value})");
            }
        }
        else
        {
            if (effectData == null)
                Debug.LogWarning("[StatusEffectInstanceBuff] effectData가 null입니다.");
            if (iconRenderer == null)
                Debug.LogWarning("[StatusEffectInstanceBuff] iconRenderer를 찾을 수 없습니다.");
        }

        // 효과 즉시 적용
        if (this.owner == null || this.owner.Equals(null)) return;
        ApplyEffect();
    }
    
    /// <summary>
    /// UI 업데이트 (지속시간 표시)
    /// </summary>
    private void UpdateUI()
    {
        // UI 업데이트 로직이 필요하면 여기에 추가
        Debug.Log($"[StatusEffectInstanceBuff] {effectData.effectName} UI 업데이트 - 남은 턴: {remainingTurns}");
    }

    public void ApplyEffect()
    {
        if (owner == null) return;

        // statType의 이름과 동일한 필드를 찾아서 값을 증가
        var statField = owner.GetType().GetField(effectData.statType.ToString());
        if (statField != null)
        {
            int current = (int)statField.GetValue(owner);
            statField.SetValue(owner, current + value);
        }
        else
        {
            Debug.LogWarning($"[StatusEffectInstanceBuff] {effectData.statType} 필드를 찾을 수 없습니다.");
        }
    }
}
