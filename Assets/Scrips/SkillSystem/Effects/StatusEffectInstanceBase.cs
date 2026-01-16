using UnityEngine;

/// <summary>
/// 상태이상 인스턴스 공통 베이스 (최소 구성)
/// - 공통 필드 보유
/// - 기초 초기화(필드 세팅)만 제공
/// - 아이콘/UI/효과 적용 등 부가 로직은 포함하지 않음
/// </summary>
public abstract class StatusEffectInstanceBase : MonoBehaviour
{
    // 공통 데이터 (기존 코드 호환을 위해 public 유지)
    public StatusEffectData effectData;
    public int remainingTurns;
    public int value;
    public CharacterStats owner;
    public int triggerCount;

    // 읽기 전용 접근자 (외부 조회용)
    public StatusEffectData EffectData => effectData;
    public int RemainingTurns => remainingTurns;
    public int Value => value;
    public CharacterStats Owner => owner;
    public int TriggerCount => triggerCount;

    /// <summary>
    /// 기초 초기화: 공통 필드만 세팅합니다.
    /// (부가 로직 금지)
    /// </summary>
    /// <param name="data">상태이상 데이터</param>
    /// <param name="duration">지속 턴</param>
    /// <param name="effectValue">효과 수치</param>
    /// <param name="target">대상</param>
    public virtual void InitializeBasic(StatusEffectData data, int duration, int effectValue, CharacterStats target)
    {
        effectData = data;
        remainingTurns = duration;
        value = effectValue;
        owner = target;
        triggerCount = data != null ? data.maxTriggerCount : 0;
    }
}


