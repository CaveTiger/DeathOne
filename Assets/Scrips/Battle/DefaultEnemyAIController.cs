using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 기본 랜덤 AI 컨트롤러
/// 모든 스킬을 랜덤하게 선택하고 타겟도 랜덤하게 선택합니다.
/// </summary>
public class DefaultEnemyAIController : EnemyAIController
{
    public override string ChooseSkillID()
    {
        var stat = GetComponent<CharacterStats>();
        if (stat.Skills.Length == 0)
        {
            Debug.LogWarning("스킬이 비어있거나 정의되지 않음");
            return null;
        }

        // 사용 가능한 스킬만 필터링 (빈 문자열 제외 + EnemyAIUsable=false 제외)
        var availableSkills = stat.Skills
            .Where(skill => !string.IsNullOrWhiteSpace(skill))
            .Where(skillId =>
            {
                if (!SkillData.skillDict.TryGetValue(skillId, out var skill) || skill == null)
                    return true; // 데이터 미로드/누락은 기존 동작 유지
                return skill.EnemyAIUsable && stat.IsSkillUsable(skill);
            })
            .ToList();
        
        if (availableSkills.Count == 0)
        {
            Debug.LogWarning("사용 가능한 스킬이 없습니다");
            return null;
        }
        
        // 랜덤 선택
        int randomIndex = Random.Range(0, availableSkills.Count);
        string selectedSkill = availableSkills[randomIndex];
        
        Debug.Log($"[AI Default] 스킬 랜덤 선택: {selectedSkill} (인덱스: {randomIndex})");
        return selectedSkill;
    }

    public override CharacterStats ChooseTarget(List<CharacterStats> players)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("타겟 후보가 없습니다");
            return null;
        }

        // 살아있는 플레이어만 필터링
        var alivePlayers = players.Where(p => p != null && !p.IsDead).ToList();
        alivePlayers = ApplyTauntPriorityCandidates(alivePlayers);
        
        if (alivePlayers.Count == 0)
        {
            Debug.LogWarning("살아있는 타겟이 없습니다");
            return null;
        }
        
        // 주인공 연속 공격 방지 시스템
        CharacterStats selectedTarget = ApplyMainCharacterProtection(alivePlayers);
        
        Debug.Log($"[AI Default] 타겟 랜덤 선택: {selectedTarget.Label}");
        return selectedTarget;
    }

    /// <summary>
    /// 주인공 연속 공격 방지 시스템
    /// 플레이어가 2명 이상일 때 주인공이 3회 연속 공격받으면 다음 공격은 다른 플레이어로 강제 타겟팅
    /// </summary>
    private CharacterStats ApplyMainCharacterProtection(List<CharacterStats> alivePlayers)
    {
        // 플레이어가 1명뿐이면 보호 시스템 적용 안함
        if (alivePlayers.Count <= 1)
        {
            return alivePlayers[0];
        }

        // 주인공 찾기 (ID가 "000001"인 캐릭터)
        var mainCharacter = alivePlayers.FirstOrDefault(p => p.CharacterId == "000001");
        if (mainCharacter == null)
        {
            // 주인공을 찾을 수 없으면 일반 랜덤 선택
            int fallbackIndex = Random.Range(0, alivePlayers.Count);
            return alivePlayers[fallbackIndex];
        }

        // 주인공 연속 공격 카운트 확인
        int consecutiveAttacks = GetMainCharacterConsecutiveAttacks();
        
        if (consecutiveAttacks >= 3)
        {
            // 3회 이상 연속 공격받았으면 다른 플레이어로 강제 타겟팅
            var otherPlayers = alivePlayers.Where(p => p.CharacterId != "000001").ToList();
            if (otherPlayers.Count > 0)
            {
                int protectedIndex = Random.Range(0, otherPlayers.Count);
                var protectedTarget = otherPlayers[protectedIndex];
                
                Debug.Log($"[AI 보호] 주인공 연속 공격 방지: {consecutiveAttacks}회 연속 → {protectedTarget.Label}으로 타겟 변경");
                ResetMainCharacterConsecutiveAttacks();
                return protectedTarget;
            }
        }

        // 일반 랜덤 선택 (주인공 포함)
        int normalIndex = Random.Range(0, alivePlayers.Count);
        var selectedTarget = alivePlayers[normalIndex];
        
        // 주인공이 선택되면 연속 공격 카운트 증가
        if (selectedTarget.CharacterId == "000001")
        {
            IncrementMainCharacterConsecutiveAttacks();
            Debug.Log($"[AI 보호] 주인공 타겟팅: 연속 공격 카운트 {consecutiveAttacks + 1}회");
        }
        else
        {
            // 다른 플레이어가 선택되면 카운트 리셋
            ResetMainCharacterConsecutiveAttacks();
        }
        
        return selectedTarget;
    }

    /// <summary>
    /// 주인공 연속 공격 카운트를 가져옵니다
    /// </summary>
    private int GetMainCharacterConsecutiveAttacks()
    {
        // 전역 변수로 관리 (실제로는 더 정교한 시스템 필요)
        if (!PlayerPrefs.HasKey("MainCharacterConsecutiveAttacks"))
        {
            PlayerPrefs.SetInt("MainCharacterConsecutiveAttacks", 0);
        }
        return PlayerPrefs.GetInt("MainCharacterConsecutiveAttacks", 0);
    }

    /// <summary>
    /// 주인공 연속 공격 카운트를 증가시킵니다
    /// </summary>
    private void IncrementMainCharacterConsecutiveAttacks()
    {
        int currentCount = GetMainCharacterConsecutiveAttacks();
        PlayerPrefs.SetInt("MainCharacterConsecutiveAttacks", currentCount + 1);
    }

    /// <summary>
    /// 주인공 연속 공격 카운트를 리셋합니다
    /// </summary>
    private void ResetMainCharacterConsecutiveAttacks()
    {
        PlayerPrefs.SetInt("MainCharacterConsecutiveAttacks", 0);
    }
} 