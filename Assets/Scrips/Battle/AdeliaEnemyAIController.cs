using UnityEngine;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 아델리아 전용 AI 컨트롤러
/// 턴에 따라 특정 스킬을 사용하는 패턴을 가집니다.
/// - 1-4턴: 스킬 1 (어설픈 다루기)
/// - 5턴: 스킬 3 (상황 관찰) - 버프
/// - 6턴: 스킬 2 (버서크 임펙트) - 연계 공격
/// - 스킬 4는 마나 부족으로 사용 불가
/// </summary>
public class AdeliaEnemyAIController : EnemyAIController
{
    private int turnCount = 0;
    private const string BerserkImpactSkillId = "011002";

    public override string ChooseSkillID()
    {
        var stat = GetComponent<CharacterStats>();
        if (stat.Skills.Length == 0)
        {
            Debug.LogWarning("스킬이 비어있거나 정의되지 않음");
            return null;
        }

        // 턴 카운트 증가 (턴 매니저에서 관리하는 것이 더 좋지만 일단 여기서)
        turnCount++;
        Debug.Log($"[AI Adelia] 턴 {turnCount} - 스킬 선택 시작");

        // 우선 규칙: 버서크 임펙트(011002)가 쿨타임/사용제한에 막히지 않으면 무조건 사용.
        string berserkId = stat.Skills.FirstOrDefault(id => id == BerserkImpactSkillId);
        if (!string.IsNullOrEmpty(berserkId) &&
            SkillData.skillDict.TryGetValue(berserkId, out var berserkSkill) &&
            berserkSkill != null &&
            berserkSkill.EnemyAIUsable &&
            stat.IsSkillUsable(berserkSkill))
        {
            Debug.Log($"[AI Adelia] 우선 규칙 발동: 버서크 임펙트 고정 사용 ({berserkId})");
            return berserkId;
        }

        string selectedSkill = null;

        // 턴에 따른 스킬 선택
        if (turnCount <= 4)
        {
            // 1-4턴: 스킬 1 (어설픈 다루기)
            selectedSkill = stat.Skills[0];
            Debug.Log($"[AI Adelia] 턴 {turnCount}: 스킬 1 선택 (어설픈 다루기)");
        }
        else if (turnCount == 5)
        {
            // 5턴: 스킬 3 (상황 관찰) - 버프
            selectedSkill = stat.Skills[2];
            Debug.Log($"[AI Adelia] 턴 {turnCount}: 스킬 3 선택 (상황 관찰)");
        }
        else if (turnCount == 6)
        {
            // 6턴: 스킬 2 (버서크 임펙트) - 연계 공격
            selectedSkill = stat.Skills[1];
            Debug.Log($"[AI Adelia] 턴 {turnCount}: 스킬 2 선택 (버서크 임펙트)");
        }
        else
        {
            // 7턴 이후: 다시 1-4턴 패턴으로 순환
            int cycleTurn = ((turnCount - 1) % 6) + 1;
            if (cycleTurn <= 4)
            {
                selectedSkill = stat.Skills[0];
                Debug.Log($"[AI Adelia] 턴 {turnCount} (순환 {cycleTurn}): 스킬 1 선택 (어설픈 다루기)");
            }
            else if (cycleTurn == 5)
            {
                selectedSkill = stat.Skills[2];
                Debug.Log($"[AI Adelia] 턴 {turnCount} (순환 {cycleTurn}): 스킬 3 선택 (상황 관찰)");
            }
            else
            {
                selectedSkill = stat.Skills[1];
                Debug.Log($"[AI Adelia] 턴 {turnCount} (순환 {cycleTurn}): 스킬 2 선택 (버서크 임펙트)");
            }
        }

        // 스킬 유효성 검사
        if (string.IsNullOrEmpty(selectedSkill))
        {
            Debug.LogWarning($"[AI Adelia] 선택된 스킬이 null입니다. 턴: {turnCount}");
            return null;
        }

        // EnemyAIUsable=false 또는 쿨다운 중인 스킬은 패턴에서 제외하고 대체한다.
        if (SkillData.skillDict.TryGetValue(selectedSkill, out var selectedData) && selectedData != null
            && (!selectedData.EnemyAIUsable || !stat.IsSkillUsable(selectedData)))
        {
            var fallbackSkills = stat.Skills
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Where(id =>
                {
                    if (!SkillData.skillDict.TryGetValue(id, out var skill) || skill == null)
                        return true;
                    return skill.EnemyAIUsable && stat.IsSkillUsable(skill);
                })
                .ToList();

            if (fallbackSkills.Count == 0)
            {
                Debug.LogWarning($"[AI Adelia] 사용 가능한 스킬이 없습니다. 턴: {turnCount}");
                return null;
            }

            selectedSkill = fallbackSkills[Random.Range(0, fallbackSkills.Count)];
            Debug.Log($"[AI Adelia] EnemyAIUsable/쿨다운 제한으로 대체 스킬 선택: {selectedSkill}");
        }

        Debug.Log($"[AI Adelia] 최종 선택된 스킬: {selectedSkill}");
        return selectedSkill;
    }

    public override CharacterStats ChooseTarget(List<CharacterStats> players)
    {
        if (players == null || players.Count == 0)
        {
            Debug.LogWarning("[AI Adelia] 타겟 후보가 없습니다");
            return null;
        }

        // 살아있는 플레이어만 필터링
        var alivePlayers = players.Where(p => p != null && p.gameObject != null && !p.IsDead).ToList();
        alivePlayers = ApplyTauntPriorityCandidates(alivePlayers);
        
        if (alivePlayers.Count == 0)
        {
            Debug.LogWarning("[AI Adelia] 살아있는 타겟이 없습니다");
            return null;
        }

        // 주인공 찾기 (CharacterId == "000001")
        var mainCharacter = alivePlayers.FirstOrDefault(p => p != null && !string.IsNullOrEmpty(p.CharacterId) && p.CharacterId == "000001");
        
        // 아군(주인공 제외) 찾기
        var allies = alivePlayers.Where(p => p != null && (string.IsNullOrEmpty(p.CharacterId) || p.CharacterId != "000001")).ToList();
        
        // 아군이 살아있는 경우: 아군 중 하나를 우선 타겟팅 (주인공은 제외)
        if (allies.Count > 0)
        {
            int randomIndex = Random.Range(0, allies.Count);
            var selectedTarget = allies[randomIndex];
            
            if (selectedTarget != null && selectedTarget.gameObject != null)
            {
                Debug.Log($"[AI Adelia] 아군 우선 타겟팅: {selectedTarget.Label} (아군 {allies.Count}명 중 선택, 주인공은 제외)");
                return selectedTarget;
            }
        }
        
        // 아군이 모두 죽은 경우에만 주인공 타겟팅
        if (mainCharacter != null && mainCharacter.gameObject != null)
        {
            Debug.Log($"[AI Adelia] 아군 전멸 - 주인공 타겟팅: {mainCharacter.Label}");
            return mainCharacter;
        }
        
        // 주인공도 없으면 랜덤 선택 (폴백)
        if (alivePlayers.Count > 0)
        {
            int randomIndex = Random.Range(0, alivePlayers.Count);
            var selectedTarget = alivePlayers[randomIndex];
            
            if (selectedTarget != null && selectedTarget.gameObject != null)
            {
                Debug.Log($"[AI Adelia] 폴백 - 랜덤 타겟 선택: {selectedTarget.Label}");
                return selectedTarget;
            }
        }
        
        Debug.LogError("[AI Adelia] 타겟 선택 실패 - 모든 후보가 유효하지 않음");
        return null;
    }

    /// <summary>
    /// 턴 카운트를 리셋합니다 (전투 시작 시 호출)
    /// </summary>
    public void ResetTurnCount()
    {
        turnCount = 0;
        Debug.Log("[AI Adelia] 턴 카운트 리셋");
    }
} 