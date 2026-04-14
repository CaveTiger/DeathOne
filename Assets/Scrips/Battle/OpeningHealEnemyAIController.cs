using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 검증용 패턴:
/// - 사용 가능한 힐 스킬만 선택
/// - 힐 타겟은 자기 자신 우선(재현성 확보)
/// </summary>
public class OpeningHealEnemyAIController : EnemyAIController
{
    private string lastChosenSkillId = null;

    public override string ChooseSkillID()
    {
        var stat = GetComponent<CharacterStats>();
        if (stat == null || stat.Skills == null || stat.Skills.Length == 0)
        {
            Debug.LogWarning("[AI OpeningHeal] 스킬이 비어있거나 정의되지 않음");
            lastChosenSkillId = null;
            return null;
        }

        // 힐 타입 + 사용 가능 스킬만 필터링
        var healSkills = stat.Skills
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .Where(id => SkillData.skillDict.TryGetValue(id, out var skill)
                         && skill != null
                         && skill.EnemyAIUsable
                         && skill.Type == SkillType.Heal
                         && stat.IsSkillUsable(skill))
            .ToList();

        if (healSkills.Count == 0)
        {
            Debug.LogWarning("[AI OpeningHeal] 사용 가능한 힐 스킬이 없습니다. 턴을 넘깁니다.");
            lastChosenSkillId = null;
            return null;
        }

        // 검증 재현성 위해 첫 번째 힐 스킬 고정 선택
        string selected = healSkills[0];
        lastChosenSkillId = selected;
        Debug.Log($"[AI OpeningHeal] 힐 전용 선택: {selected}");
        return selected;
    }

    public override CharacterStats ChooseTarget(List<CharacterStats> players)
    {
        var self = GetComponent<CharacterStats>();
        if (self == null || self.IsDead)
            return null;

        if (!string.IsNullOrEmpty(lastChosenSkillId)
            && SkillData.skillDict.TryGetValue(lastChosenSkillId, out var chosenSkill)
            && chosenSkill != null
            && chosenSkill.Type == SkillType.Heal)
        {
            // 힐 검증용: 자기 자신 고정
            return self;
        }

        // 이 패턴은 힐 전용이므로 기본도 자기 자신으로 반환
        return self;
    }
}
