using UnityEngine;

public enum PassiveType
{
    None,
    ManaBoost,  // 최대 마나 증가
    // 추후 확장 가능
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
    // 기타 효과 확장 가능
} 