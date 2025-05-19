using System.Collections.Generic;

public class CharacterData 
{
    public string ID;
    public string ParentID;
    public bool Specimen;

    public string Label;
    public string Description;

    public int Hp, MaxHp,Atk, Def, Speed;
    public float EvasionRate, Accuracy;

    public List<string> Skills = new();
    public List<string> Passives = new();

    public RarityList Rarity;
    public string Sprite;
    public PatternType Pattern;

    public static Dictionary<string, CharacterData> characterDict = new();
    public CharacterData Clone()
    {
        return new CharacterData
        {
            ID = this.ID,
            ParentID = this.ParentID,
            Specimen = this.Specimen,
            Label = this.Label,
            Description = this.Description,

            Hp = this.Hp,
            MaxHp = this.MaxHp,
            Atk = this.Atk,
            Def = this.Def,
            EvasionRate = this.EvasionRate,
            Accuracy = this.Accuracy,
            Speed = this.Speed,

            Skills = this.Skills != null ? new List<string>(this.Skills) : new List<string>(),

            Passives = this.Passives != null ? new List<string>(this.Passives) : new List<string>(),

            Rarity = this.Rarity,
            Sprite = this.Sprite,
            Pattern = this.Pattern
        };
    }
}
