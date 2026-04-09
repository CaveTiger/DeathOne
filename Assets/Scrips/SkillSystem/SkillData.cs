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

public enum SkillTargetType
{
    None,
    Me,
    Ally,
    Enemy,
    AllAllies,
    AllEnemies,
    AllUnits,
    Adjacent,
    SelfAndAdjacent,
    AdjacentArea,
    SelfAndAdjacentArea,
    RandomEnemy,
    RandomAlly,
    RandomTarget,
    LowestHpAlly,
    HighestHpEnemy,
    WeakestEnemy,
    StrongestAlly
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
    public SkillTargetType TargetType => ParseSkillTarget(SkillTarget);
    public string Motion { get; set; }
    public string GhostSpritePath { get; set; } // 주인공 영체 연출용 스프라이트 경로(비어있지 않으면 사용)
    /// <summary>영체 스프라이트 로컬 스케일. XML 미지정 또는 0 이하면 런타임 기본 1.</summary>
    public float GhostProxyScale { get; set; } = -1f;
    public string AttackPoint { get; set; }
    /// <summary>공격 유형(베기/타격/관통 등). 로드 후 비어 있으면 none. 자식 XML에서 태그 생략 시 null로 두고 부모 상속(SkillLoader).</summary>
    public string AttackType { get; set; }
    public string AttackEffect { get; set; }
    public int ManaCost { get; set; }
    public int StaminaCost { get; set; }
    public int HealthCost { get; set; }
    public int CurrentCooldown { get; set; }
    public int HealAmount { get; set; }
    public int HealMin { get; set; }
    public int HealMax { get; set; }
    public SkillType Type { get; set; }
    public bool HasExplicitType { get; set; }
    public string UseSkillId { get; set; }
    
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
            GhostSpritePath = this.GhostSpritePath,
            GhostProxyScale = this.GhostProxyScale,
            AttackPoint = this.AttackPoint,
            AttackType = this.AttackType,
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
            HasExplicitType = this.HasExplicitType,
            UseSkillId = this.UseSkillId,
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

    public static SkillTargetType ParseSkillTarget(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return SkillTargetType.None;

        string t = raw.Trim().ToLowerInvariant();
        switch (t)
        {
            case "me":
            case "self":
                return SkillTargetType.Me;
            case "ally":
                return SkillTargetType.Ally;
            case "enemy":
                return SkillTargetType.Enemy;
            case "allallies":
                return SkillTargetType.AllAllies;
            case "allenemies":
                return SkillTargetType.AllEnemies;
            case "allunits":
            case "all":
                return SkillTargetType.AllUnits;
            case "adjacent":
                return SkillTargetType.Adjacent;
            case "selfandadjacent":
                return SkillTargetType.SelfAndAdjacent;
            case "adjacentarea":
                return SkillTargetType.AdjacentArea;
            case "selfandadjacentarea":
                return SkillTargetType.SelfAndAdjacentArea;
            case "randomenemy":
                return SkillTargetType.RandomEnemy;
            case "randomally":
                return SkillTargetType.RandomAlly;
            case "randomtarget":
                return SkillTargetType.RandomTarget;
            case "lowesthpally":
                return SkillTargetType.LowestHpAlly;
            case "highesthpenemy":
                return SkillTargetType.HighestHpEnemy;
            case "weakestenemy":
                return SkillTargetType.WeakestEnemy;
            case "strongestally":
                return SkillTargetType.StrongestAlly;
            default:
                return SkillTargetType.None;
        }
    }
}

public class SkillEffectInfo
{
    public string EffectID;
    public int Value;
    public int Duration;
    public float Chance = 1.0f;
}

