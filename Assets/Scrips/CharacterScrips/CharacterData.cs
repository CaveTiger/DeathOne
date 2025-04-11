using System.Collections.Generic;

public class CharacterData 
{
    public string ID;
    public string ParentID;
    public bool Specimen;

    public string Label;
    public string Description;

    public int Hp, Atk, Def, Speed;
    public float EvasionRate, Accuracy;

    public List<string> Skills = new();
    public List<string> Passives = new();

    public RarityList Rarity;
    public string Sprite;
    public string Pattern;

    public static Dictionary<string, CharacterData> characterDict = new();
}
