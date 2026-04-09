using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 테스트/확장용 패턴:
/// - 첫 행동 1회는 Buff 타입 스킬을 우선 사용
/// - 이후는 기본 랜덤 AI와 동일하게 동작
/// </summary>
public class OpeningBuffEnemyAIController : EnemyAIController
{
    private bool openingBuffConsumed = false;
    private string lastChosenSkillId = null;

    public override string ChooseSkillID()
    {
        var stat = GetComponent<CharacterStats>();
        if (stat == null || stat.Skills == null || stat.Skills.Length == 0)
        {
            Debug.LogWarning("[AI OpeningBuff] 스킬이 비어있거나 정의되지 않음");
            lastChosenSkillId = null;
            return null;
        }

        var availableSkills = stat.Skills.Where(skill => !string.IsNullOrWhiteSpace(skill)).ToList();
        if (availableSkills.Count == 0)
        {
            Debug.LogWarning("[AI OpeningBuff] 사용 가능한 스킬이 없습니다");
            lastChosenSkillId = null;
            return null;
        }

        if (!openingBuffConsumed)
        {
            foreach (var id in availableSkills)
            {
                if (!SkillData.skillDict.TryGetValue(id, out var skill) || skill == null)
                    continue;
                if (skill.Type != SkillType.Buff || !skill.IsUsable())
                    continue;

                openingBuffConsumed = true;
                lastChosenSkillId = id;
                Debug.Log($"[AI OpeningBuff] 첫 행동 버프 우선 선택: {id} ({skill.Name})");
                return id;
            }

            openingBuffConsumed = true;
            Debug.Log("[AI OpeningBuff] 첫 행동 버프 스킬 없음, 랜덤으로 전환");
        }

        int randomIndex = Random.Range(0, availableSkills.Count);
        string selected = availableSkills[randomIndex];
        lastChosenSkillId = selected;
        Debug.Log($"[AI OpeningBuff] 랜덤 스킬 선택: {selected} (인덱스: {randomIndex})");
        return selected;
    }

    public override CharacterStats ChooseTarget(List<CharacterStats> players)
    {
        var self = GetComponent<CharacterStats>();
        if (self != null && !string.IsNullOrEmpty(lastChosenSkillId)
            && SkillData.skillDict.TryGetValue(lastChosenSkillId, out var chosenSkill)
            && chosenSkill != null
            && (chosenSkill.Type == SkillType.Buff || chosenSkill.Type == SkillType.Heal))
        {
            // 버프/힐은 자기 진영 대상으로 안전하게 고정해 테스트 재현성을 높인다.
            if (chosenSkill.TargetType == SkillTargetType.Me
                || chosenSkill.TargetType == SkillTargetType.AllAllies
                || chosenSkill.TargetType == SkillTargetType.Ally)
            {
                return self;
            }
        }

        if (players == null || players.Count == 0)
            return null;

        var alivePlayers = players.Where(p => p != null && !p.IsDead).ToList();
        alivePlayers = ApplyTauntPriorityCandidates(alivePlayers);
        if (alivePlayers.Count == 0)
            return null;

        int randomIndex = Random.Range(0, alivePlayers.Count);
        return alivePlayers[randomIndex];
    }
}
