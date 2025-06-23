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

    public float Scale { get; set; } = 1.0f; // 기본값 1.0

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
            IsCustomized = this.IsCustomized
        };

        // 등급에 따른 스탯 조절
        clone.AdjustStatsByRarity();
        return clone;
    }
}
