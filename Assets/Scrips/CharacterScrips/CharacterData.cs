using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class CharacterData 
{
    public string ID;
    public string ParentID;
    public bool Specimen;

    public string Label;
    public string Description;

    public int Hp, MaxHp, KDP, MaxKDP, Atk, Def, Speed;
    public float EvasionRate, Accuracy;

    public List<string> Skills = new();
    public List<string> Passives = new();

    public RarityList Rarity;
    public string Sprite;
    public PatternType Pattern;

    public bool IsCustomized = false;
    public bool IsUnlocked = false;

    public float Scale { get; set; } = 1.0f; // 기본값 1.0

    public int maxPassiveCost = 10; // 기본값 10, 필요시 XML/에디터에서 지정

    [Header("업그레이드 시스템")]
    // 등급별 영혼먼지 투자 한도
    private static readonly Dictionary<RarityList, int> MaxSoulDustLimitByRarity = new()
    {
        { RarityList.Normal, 100 },
        { RarityList.Rare, 75 },
        { RarityList.One, 150 },
        { RarityList.Uniqu, 50 },
        { RarityList.Legend, 50 }
    };

    public int upgradeHpBonus = 0;
    public int upgradeMaxHpBonus = 0;
    public int upgradeAtkBonus = 0;
    public int upgradeDefBonus = 0;
    public int upgradeSpeedBonus = 0;
    public float upgradeEvasionBonus = 0f;
    public float upgradeAccuracyBonus = 0f;
    public int totalSoulDustSpent = 0; // 투자한 총 영혼먼지

    public static Dictionary<string, CharacterData> characterDict = new();

    /// <summary>
    /// 캐릭터 등급에 따른 스탯 조절
    /// </summary>
    public void AdjustStatsByRarity()
    {
        // 등급별 최대 스탯 제한
        switch (Rarity)
        {
            case RarityList.Normal:
                if (Hp > 50) Hp = 50;
                if (MaxHp > 50) MaxHp = 50;
                if (Atk > 50) Atk = 50;
                if (Def > 50) Def = 50;
                if (Speed > 50) Speed = 50;
                break;
            case RarityList.Rare:
                if (Hp > 60) Hp = 60;
                if (MaxHp > 60) MaxHp = 60;
                if (Atk > 60) Atk = 60;
                if (Def > 60) Def = 60;
                if (Speed > 60) Speed = 60;
                break;
            case RarityList.One:
            case RarityList.Uniqu:
                if (Hp > 75) Hp = 75;
                if (MaxHp > 75) MaxHp = 75;
                if (Atk > 75) Atk = 75;
                if (Def > 75) Def = 75;
                if (Speed > 75) Speed = 75;
                break;
            case RarityList.Legend:
                if (Hp > 100) Hp = 100;
                if (MaxHp > 100) MaxHp = 100;
                if (Atk > 100) Atk = 100;
                if (Def > 100) Def = 100;
                if (Speed > 100) Speed = 100;
                break;
        }
    }

    /// <summary>
    /// 캐릭터 데이터 복제 시 등급에 따른 스탯 조절
    /// </summary>
    public CharacterData Clone()
    {
        CharacterData clone = new CharacterData
        {
            ID = this.ID,
            ParentID = this.ParentID,
            Specimen = this.Specimen,
            Label = this.Label,
            Description = this.Description,
            Hp = this.Hp,
            MaxHp = this.MaxHp,
            KDP = this.KDP,
            MaxKDP = this.MaxKDP,
            Atk = this.Atk,
            Def = this.Def,
            Speed = this.Speed,
            EvasionRate = this.EvasionRate,
            Accuracy = this.Accuracy,
            Skills = new List<string>(this.Skills),
            Passives = new List<string>(this.Passives),
            Rarity = this.Rarity,
            Sprite = this.Sprite,
            Pattern = this.Pattern,
            Scale = this.Scale,
            IsCustomized = this.IsCustomized,
            IsUnlocked = this.IsUnlocked,
            maxPassiveCost = this.maxPassiveCost,
            // 업그레이드 보너스 필드 초기화 (0으로 시작)
            upgradeHpBonus = 0,
            upgradeMaxHpBonus = 0,
            upgradeAtkBonus = 0,
            upgradeDefBonus = 0,
            upgradeSpeedBonus = 0,
            upgradeEvasionBonus = 0f,
            upgradeAccuracyBonus = 0f,
            totalSoulDustSpent = 0
        };

        // 등급에 따른 스탯 조절
        clone.AdjustStatsByRarity();
        return clone;
    }

    /// <summary>
    /// 업그레이드 보너스를 포함한 최종 스탯 값을 반환합니다.
    /// </summary>
    /// <param name="statType">조회할 스탯 타입</param>
    /// <returns>기본 스탯 + 업그레이드 보너스</returns>
    public float GetFinalStatValue(TargetStat statType)
    {
        float passiveBonus = GetPassiveStatBonus(statType);
        bool traceMora = DebugTraceFlags.PassiveStatTraceMora && ID == "000007";

        if (traceMora)
        {
            Debug.Log($"[PassiveTrace][FinalStat] ID={ID} Stat={statType} Base(Hp/MaxHp/Atk/Def)={Hp}/{MaxHp}/{Atk}/{Def} Upgrade(Hp/MaxHp/Atk/Def)={upgradeHpBonus}/{upgradeMaxHpBonus}/{upgradeAtkBonus}/{upgradeDefBonus} PassiveBonus={passiveBonus}");
        }

        switch (statType)
        {
            case TargetStat.Hp:
                return Hp + upgradeHpBonus + passiveBonus;
            case TargetStat.MaxHp:
                return MaxHp + upgradeMaxHpBonus + passiveBonus;
            case TargetStat.Atk:
                return Atk + upgradeAtkBonus + passiveBonus;
            case TargetStat.Def:
                return Def + upgradeDefBonus + passiveBonus;
            case TargetStat.Speed:
                return Speed + upgradeSpeedBonus + passiveBonus;
            case TargetStat.Evasion:
                return EvasionRate + upgradeEvasionBonus + passiveBonus;
            case TargetStat.Accuracy:
                return Accuracy + upgradeAccuracyBonus + passiveBonus;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 상시 반영되는 스탯 부스트형(Type=None) 패시브 보너스를 계산합니다.
    /// </summary>
    private float GetPassiveStatBonus(TargetStat statType)
    {
        if (Passives == null || Passives.Count == 0) return 0f;

        bool traceMora = DebugTraceFlags.PassiveStatTraceMora && ID == "000007";
        float total = 0f;

        for (int i = 0; i < Passives.Count; i++)
        {
            string passiveId = Passives[i];
            if (string.IsNullOrEmpty(passiveId)) continue;

            PassiveData passiveData = PassiveLoader.GetByIdStatic(passiveId);
            if (passiveData == null)
            {
                if (traceMora)
                    Debug.LogWarning($"[PassiveTrace][Data] ID={ID} passiveId={passiveId} -> PassiveData null");
                continue;
            }
            if (passiveData.passiveType != PassiveType.None)
            {
                if (traceMora)
                    Debug.Log($"[PassiveTrace][Data] ID={ID} passiveId={passiveId} type={passiveData.passiveType} (stat-bonus 계산 제외)");
                continue;
            }

            bool matches = passiveData.targetStat == statType;

            // MaxHp 부스트는 전투 시작 시 현재 HP에도 함께 반영되도록 취급한다.
            if (statType == TargetStat.Hp && passiveData.targetStat == TargetStat.MaxHp)
            {
                matches = true;
            }

            if (!matches) continue;

            if (statType == TargetStat.Evasion || statType == TargetStat.Accuracy)
                total += passiveData.floatValue != 0f ? passiveData.floatValue : passiveData.value;
            else
                total += passiveData.value;

            if (traceMora)
            {
                Debug.Log($"[PassiveTrace][Data] ID={ID} passiveId={passiveId} target={passiveData.targetStat} statType={statType} add={(statType == TargetStat.Evasion || statType == TargetStat.Accuracy ? (passiveData.floatValue != 0f ? passiveData.floatValue : passiveData.value) : passiveData.value)} total={total}");
            }
        }

        return total;
    }

    /// <summary>
    /// 업그레이드 보너스만 반환합니다.
    /// </summary>
    /// <param name="statType">조회할 스탯 타입</param>
    /// <returns>업그레이드 보너스 값</returns>
    public float GetUpgradeBonus(TargetStat statType)
    {
        switch (statType)
        {
            case TargetStat.Hp:
                return upgradeHpBonus;
            case TargetStat.MaxHp:
                return upgradeMaxHpBonus;
            case TargetStat.Atk:
                return upgradeAtkBonus;
            case TargetStat.Def:
                return upgradeDefBonus;
            case TargetStat.Speed:
                return upgradeSpeedBonus;
            case TargetStat.Evasion:
                return upgradeEvasionBonus;
            case TargetStat.Accuracy:
                return upgradeAccuracyBonus;
            default:
                return 0;
        }
    }

    /// <summary>
    /// 이 캐릭터 등급에서 사용할 수 있는 최대 영혼먼지 투자량을 반환합니다.
    /// </summary>
    public int GetMaxSoulDustLimit()
    {
        if (MaxSoulDustLimitByRarity.TryGetValue(Rarity, out int limit))
            return limit;

        return 0;
    }

    /// <summary>
    /// 현재 등급 한도에서 추가로 투자 가능한 영혼먼지량을 반환합니다.
    /// </summary>
    public int GetRemainingSoulDustCapacity()
    {
        int max = GetMaxSoulDustLimit();
        int remaining = max - totalSoulDustSpent;
        return remaining < 0 ? 0 : remaining;
    }
}
