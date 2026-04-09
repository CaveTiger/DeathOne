using UnityEngine;

public enum PassiveType
{
    /// <summary>
    /// 스탯 보정 전용. TargetStat/Value(·FloatValue)는 <see cref="CharacterData.GetFinalStatValue"/>에서만 반영. 별도 효과 클래스 없음.
    /// </summary>
    None,
    ManaBoost,
    /// <summary>
    /// 자기 턴 시작마다 유즈 1 누적, <see cref="PassiveData.useCount"/> 이상이면 상태이상 부여 후 누적 0. useCount==0이면 누적 없이 매 턴 발동(상시).
    /// 첫 누적 전 기본값은 <see cref="PassiveData.startUseCount"/> (XML StartCount).
    /// XML: UseCount, StartCount, Duration, statusEffectID, statusEffectValue.
    /// </summary>
    TurnIntervalGrantStatus,
    /// <summary>
    /// 지정한 상태이상 EffectID를 면역 처리. XML ImmuneStatusEffectIDs(쉼표 구분)를 사용.
    /// </summary>
    StatusEffectImmunity,
    CustomScript
}

public enum TargetStat
{
    None,
    Hp,
    MaxHp,
    Atk,
    Def,
    Speed,
    Evasion,
    Accuracy
    // 필요시 추가
}

/// <summary>
/// 패시브 런타임 데이터. XML 매핑·복제는 <see cref="PassiveLoader"/> — 확장 순서는 <see cref="PassiveSystemExtensionGuide"/>.
/// </summary>
[System.Serializable]
public class PassiveData
{
    public string passiveID;         // 패시브 고유 ID (ex: "080001")
    public string passiveName;       // 패시브 이름
    [TextArea]
    public string description;       // 설명
    public PassiveType passiveType;  // 효과 타입 (enum)
    public TargetStat targetStat;    // 적용 대상 스탯 (enum)
    public int value;                // 수치 (ex: MaxHp=3 등, 용도 자유)
    public float floatValue;         // 부가 수치(필요시)
    // 마나 관련 필드(확장용)
    public bool grantsMana;          // 마나 시스템 활성화 여부
    public int maxMana;              // 최대 마나
    public int manaRegenInterval;    // 몇 턴마다 1 마나 회복 (0이면 회복 없음)
    // 레어리티 및 코스트
    public RarityList rarity;        // 패시브의 레어리티(등급)
    public int cost;                 // 패시브 장착 비용(코스트)
    public string scriptClass;       // 스크립트 클래스명(확장용)
    /// <summary> TurnIntervalGrantStatus: 턴 시작 시 누적이 이 값 이상이면 발동 후 0으로 리셋. 0=상시(매 턴 발동). 1=매 턴, 2=격턴… (XML UseCount).</summary>
    public int useCount;
    /// <summary> TurnIntervalGrantStatus: 전투 중 해당 패시브 유즈 누적의 초기값(첫 TryTick 직전). UseCount=2일 때 1이면 첫 자기 턴에 1+1로 발동 (XML StartCount / startcount).</summary>
    public int startUseCount;
    /// <summary> TurnIntervalGrantStatus: 부여할 상태이상 지속 턴 (XML Duration).</summary>
    public int grantStatusDuration;
    /// <summary> TurnIntervalGrantStatus: 부여할 상태이상 EffectID (XML statusEffectID 등).</summary>
    public string grantStatusEffectId;
    /// <summary> TurnIntervalGrantStatus: 부여할 상태이상 수치 (XML statusEffectValue).</summary>
    public int grantStatusValue;
    /// <summary> StatusEffectImmunity: 면역할 상태이상 EffectID 목록(CSV, XML ImmuneStatusEffectIDs/immuneStatusEffectIDs).</summary>
    public string immuneStatusEffectIds;
} 