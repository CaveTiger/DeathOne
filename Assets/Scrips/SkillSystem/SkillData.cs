using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Damage,
    Heal,
    linkage,
    Piercing,
    Buff,
    Debuff
}
public class SkillData
{
    public static Dictionary<string, SkillData> skillDict = new Dictionary<string, SkillData>();

    public string ID { get; set; }
    public string ParentID { get; set; }
    public bool Specimen { get; set; }
    public string Name { get; set; }
    public string Icon { get; set; }
    public string Group { get; set; }
    public string Description { get; set; }
    public int DamageMin { get; set; }
    public int DamageMax { get; set; }
    public float Cooldown { get; set; }
    public float Range { get; set; }
    public string SkillTarget { get; set; }
    public string Motion { get; set; }
    public string AttackPoint { get; set; }
    public string AttackEffect { get; set; }
    public int ManaCost { get; set; }
    public int StaminaCost { get; set; }
    public int HealthCost { get; set; }
    public int cooldown{  get; set; }
    public int currentCooldown {  get; set; }
    public int healAmount { get; set; }
    public SkillType Type { get; set; }
    public List<SkillEffectInfo> skillEffects = new List<SkillEffectInfo>();
    public SkillData Clone()
    {
        return new SkillData
        {
            ID = this.ID,
            ParentID = this.ParentID,
            Specimen = this.Specimen,
            Name = this.Name,
            Icon = this.Icon,
            Group = this.Group,
            Description = this.Description,
            DamageMin = this.DamageMin,
            DamageMax = this.DamageMax,
            Cooldown = this.Cooldown,
            Range = this.Range,
            SkillTarget = this.SkillTarget,
            Motion = this.Motion,
            AttackPoint = this.AttackPoint,
            AttackEffect = this.AttackEffect,
            skillEffects = new List<SkillEffectInfo>(this.skillEffects),
            ManaCost = this.ManaCost,
            StaminaCost = this.StaminaCost,
            HealthCost = this.HealthCost,
        };
    }
    public bool IsUsable()
    {
        return currentCooldown <= 0;
    }
}

public class SkillEffectInfo
{
    public string EffectID;
    public int Value;
    public int Duration;
}

