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

        Debug.Log($"[AI Adelia] 최종 선택된 스킬: {selectedSkill}");
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
        
        if (alivePlayers.Count == 0)
        {
            Debug.LogWarning("살아있는 타겟이 없습니다");
            return null;
        }

        // 아델리아는 주인공을 우선 타겟팅하는 경향이 있음
        var mainCharacter = alivePlayers.FirstOrDefault(p => p.CharacterId == "000001");
        if (mainCharacter != null)
        {
            // 70% 확률로 주인공 타겟팅
            if (Random.Range(0f, 1f) < 0.7f)
            {
                Debug.Log($"[AI Adelia] 주인공 우선 타겟팅: {mainCharacter.Label}");
                return mainCharacter;
            }
        }

        // 나머지 30% 또는 주인공이 없으면 랜덤 선택
        int randomIndex = Random.Range(0, alivePlayers.Count);
        var selectedTarget = alivePlayers[randomIndex];
        
        Debug.Log($"[AI Adelia] 랜덤 타겟 선택: {selectedTarget.Label}");
        return selectedTarget;
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