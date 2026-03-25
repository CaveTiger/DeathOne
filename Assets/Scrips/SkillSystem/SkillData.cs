using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillType
{
    Damage,
    Heal,
    Linkage,
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
    public int Cooldown { get; set; }
    public float Range { get; set; }
    public string SkillTarget { get; set; }
    public string Motion { get; set; }
    public string AttackPoint { get; set; }
    public string AttackEffect { get; set; }
    public int ManaCost { get; set; }
    public int StaminaCost { get; set; }
    public int HealthCost { get; set; }
    public int CurrentCooldown { get; set; }
    public int HealAmount { get; set; }
    public int HealMin { get; set; }
    public int HealMax { get; set; }
    public SkillType Type { get; set; }
    
    [Header("스킬 사용 횟수 제한")]
    public bool UseCountYes { get; set; } // 사용 횟수 제한 여부
    public int UseCount { get; set; } // 최대 사용 횟수
    public int CurrentUseCount { get; set; } // 현재 남은 사용 횟수
    
    public List<SkillEffectInfo> skillEffects = new List<SkillEffectInfo>();
    public float KnockdownMultiplier { get; set; } = 1.0f;
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
            CurrentCooldown = 0, // 게임 시작 시 항상 0으로 초기화
            HealAmount = this.HealAmount,
            HealMin = this.HealMin,
            HealMax = this.HealMax,
            Type = this.Type,
            KnockdownMultiplier = this.KnockdownMultiplier
        };
    }
    public bool IsUsable()
    {
        // 쿨다운 체크
        if (CurrentCooldown > 0) return false;
        
        // 사용 횟수 제한 체크
        if (UseCountYes && CurrentUseCount <= 0) return false;
        
        return true;
    }
}

public class SkillEffectInfo
{
    public string EffectID;
    public int Value;
    public int Duration;
}

