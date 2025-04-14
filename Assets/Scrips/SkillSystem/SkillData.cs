using System.Collections.Generic;

public class SkillData
{
    public string ID;
    public string ParentID;
    public string Name;
    public int DamageMin;
    public int DamageMax;
    public List<string> SkillEffects = new List<string>();
    public string SkillTarget;
    public string Motion;
    public string AttackPoint;
    public string AttackEffect;
    public bool Specimen;
}

