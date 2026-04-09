using UnityEngine;

public class StatusEffectInstanceReaction : StatusEffectInstanceBase
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

    public void Initialize(StatusEffectData data, int duration, int value, CharacterStats owner)
    {
        InitializeBasic(data, duration, value, owner);
        this.triggerCount = data.maxTriggerCount;
        
        // 상태이상 적용 시 지속시간 감소 제거 (정산 시에만 감소)
        // if (remainingTurns > 0)
        // {
        //     remainingTurns--;
        //     Debug.Log($"[StatusEffectInstanceReaction] {data.effectName} 적용 즉시 지속시간 감소: {duration} → {remainingTurns}");
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
                Debug.Log($"[StatusEffectInstanceReaction] 아이콘 설정 완료: {data.effectName} (값: {value}) - {icon.name}");
            }
            else
            {
                Debug.LogWarning($"[StatusEffectInstanceReaction] 아이콘을 찾을 수 없습니다: {data.effectName} (ID: {data.EffectID}, 값: {value})");
            }
        }
        else
        {
            if (effectData == null)
                Debug.LogWarning("[StatusEffectInstanceReaction] effectData가 null입니다.");
            if (iconRenderer == null)
                Debug.LogWarning("[StatusEffectInstanceReaction] iconRenderer를 찾을 수 없습니다.");
        }
    }
    
    /// <summary>
    /// UI 업데이트 (지속시간 표시)
    /// </summary>
    private void UpdateUI()
    {
        // UI 업데이트 로직이 필요하면 여기에 추가
        Debug.Log($"[StatusEffectInstanceReaction] {effectData.effectName} UI 업데이트 - 남은 턴: {remainingTurns}");
    }

    public virtual bool OnTakeDamage(ref int damage, CharacterStats attacker, bool isReflectedDamage)
    {
        if (effectData == null || !isActive)
            return false;

        if (DebugTraceFlags.PassiveStatusEffectFlow)
        {
            Debug.Log($"[ReflectTrace][Reaction] enter owner={owner?.Label}, effect={effectData.EffectID}, mode={effectData.reactionMode}, dmgIn={damage}, attacker={(attacker != null ? attacker.Label : "null")}, reflectedIn={isReflectedDamage}, trigger={triggerCount}, maxTrigger={effectData.maxTriggerCount}");
        }

        // 반사로 들어온 피해는 반응 재트리거를 막아 루프를 차단
        if (isReflectedDamage)
            return false;

        ReactionEffectMode mode = effectData.reactionMode;
        // 기존 데이터 호환: reactionMode를 아직 안 쓴 021002는 피해무시로 동작 유지
        if (mode == ReactionEffectMode.None && effectData.EffectID == "021002")
            mode = ReactionEffectMode.DamageNullify;

        // maxTriggerCount > 0 일 때만 횟수 제한을 적용한다. (0은 무제한)
        bool limitedByTriggerCount = effectData.maxTriggerCount > 0;
        if (limitedByTriggerCount && triggerCount <= 0)
        {
            if (DebugTraceFlags.PassiveStatusEffectFlow)
                Debug.Log($"[ReflectTrace][Reaction] skip trigger 소진: effect={effectData.EffectID}, owner={owner?.Label}");
            return false;
        }

        if (mode == ReactionEffectMode.DamageNullify)
        {
            if (limitedByTriggerCount)
                triggerCount--;
            Debug.Log($"[피해무시] {owner.Label}가 피해를 무시했습니다! 남은 횟수: {(limitedByTriggerCount ? triggerCount : -1)}");
            damage = 0;
            if (limitedByTriggerCount && triggerCount <= 0)
            {
                remainingTurns = 0;
                Destroy(this.gameObject);
            }
            return true;
        }

        int incomingBeforeReduction = damage;
        int reducedAmount = ApplyDamageReduction(ref damage);

        if (mode == ReactionEffectMode.ReflectFixed || mode == ReactionEffectMode.ReflectPercentOfReceived)
        {
            int reflectAmount = CalculateReflectDamage(incomingBeforeReduction, reducedAmount);
            if (DebugTraceFlags.PassiveStatusEffectFlow)
                Debug.Log($"[ReflectTrace][Reaction] reflect 계산: owner={owner?.Label}, incoming={incomingBeforeReduction}, reduced={reducedAmount}, dmgAfterReduction={damage}, reflect={reflectAmount}, attacker={(attacker != null ? attacker.Label : "null")}");
            if (reflectAmount > 0 && attacker != null && !attacker.IsDead)
            {
                if (limitedByTriggerCount)
                    triggerCount--;
                Debug.Log($"[반사] {owner.Label} -> {attacker.Label} 반사 피해 {reflectAmount} (모드: {mode}, 남은 횟수: {(limitedByTriggerCount ? triggerCount : -1)})");
                attacker.TakeDamage(reflectAmount, owner != null ? owner.Accuracy : 1f, null, owner != null ? owner.transform.position : (Vector3?)null, owner, true);

                if (limitedByTriggerCount && triggerCount <= 0)
                {
                    remainingTurns = 0;
                    Destroy(this.gameObject);
                }
            }
            else if (DebugTraceFlags.PassiveStatusEffectFlow)
            {
                Debug.LogWarning($"[ReflectTrace][Reaction] 반사 미발동: reflect={reflectAmount}, attackerNull={(attacker == null)}, attackerDead={(attacker != null && attacker.IsDead)}");
            }
        }

        return false;
    }

    private int CalculateReflectDamage(int incomingDamageBeforeReduction, int reducedAmount)
    {
        if (incomingDamageBeforeReduction <= 0 || effectData == null)
            return 0;

        if (effectData.reflectReducedAmountDirect)
            return Mathf.Max(0, reducedAmount);

        int reflectBaseDamage = effectData.useReducedAmountAsReflectBase
            ? Mathf.Max(0, reducedAmount)
            : Mathf.Max(0, incomingDamageBeforeReduction);

        switch (effectData.reactionMode)
        {
            case ReactionEffectMode.ReflectFixed:
                {
                    int fixedValue = effectData.useAppliedValueForReaction ? value : effectData.reactionFixedValue;
                    return Mathf.Max(0, fixedValue);
                }
            case ReactionEffectMode.ReflectPercentOfReceived:
                {
                    float percent = effectData.useAppliedValueForReaction ? value : effectData.reactionPercent;
                    if (percent <= 0f) return 0;
                    return Mathf.Max(0, Mathf.RoundToInt(reflectBaseDamage * (percent / 100f)));
                }
            default:
                return 0;
        }
    }

    private int ApplyDamageReduction(ref int damage)
    {
        if (effectData == null || damage <= 0)
            return 0;

        int original = damage;
        int reduced = 0;

        switch (effectData.reductionMode)
        {
            case ReactionDamageReductionMode.Fixed:
                {
                    int fixedReduction = effectData.useAppliedValueForReduction ? value : effectData.reductionFixedValue;
                    reduced = Mathf.Clamp(fixedReduction, 0, original);
                    break;
                }
            case ReactionDamageReductionMode.PercentOfReceived:
                {
                    float percent = effectData.useAppliedValueForReduction ? value : effectData.reductionPercent;
                    if (percent > 0f)
                        reduced = Mathf.Clamp(Mathf.RoundToInt(original * (percent / 100f)), 0, original);
                    break;
                }
        }

        if (reduced > 0)
        {
            damage = Mathf.Max(0, original - reduced);
            Debug.Log($"[반응감쇠] {owner?.Label}: 피해 감소 {original} -> {damage} (감소량 {reduced})");
        }

        return reduced;
    }

    /// <summary>
    /// 지속 턴을 감소시키는 메서드 (특정 타이밍에서 호출)
    /// </summary>
    public void ReduceDuration()
    {
        if (!isActive) return;
        
        remainingTurns--;
        UpdateUI();
        
        Debug.Log($"[StatusEffectInstanceReaction] {effectData.effectName} 지속 턴 감소: {remainingTurns + 1} → {remainingTurns}");
        
        if (remainingTurns <= 0)
        {
            isActive = false;
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 턴이 끝날 때 호출되는 메서드
    /// 특수한 로직이 있을 때만 작동 (기본적으로는 아무것도 안 함)
    /// </summary>
    public void OnTurnEnd()
    {
        // 턴 종료 시 상태이상 관리 일절 안 함
        // 특수한 로직이 필요한 경우에만 여기에 추가
    }
}

