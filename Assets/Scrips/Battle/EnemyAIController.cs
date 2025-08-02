using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

public abstract class EnemyAIController : MonoBehaviour
{

    public IEnumerator EnemyActionRoutine(CharacterStats enemy)
    {
        Debug.Log($"[AI개선] EnemyActionRoutine 시작 - 적: {enemy?.Label}");
        float startTime = Time.realtimeSinceStartup;
        
        if (enemy == null || enemy.Equals(null) || enemy.IsDead)
        {
            Debug.Log("[AI개선] EnemyActionRoutine - 적이 null이거나 사망 상태");
            yield break; // 이미 파괴된 대상이면 행동 중단
        }

        Debug.Log("[AI개선] EnemyActionRoutine - 턴 시작 전 0.8초 대기 시작");
        // 턴 전환 스킵 시스템 사용
        if (TurnTransitionSkipManager.Instance != null)
        {
            yield return TurnTransitionSkipManager.Instance.WaitForTurnTransition(0.8f, "enemyStart");
        }
        else
        {
            yield return new WaitForSeconds(0.8f); // 기존 방식 (스킵 불가)
        }
        Debug.Log("[AI개선] EnemyActionRoutine - 턴 시작 전 0.8초 대기 완료");

        var ai = enemy.GetComponent<EnemyAIController>();
        if (ai == null)
        {
            Debug.LogWarning($"[AI ERROR] {enemy.name}에게 EnemyAIController 없음");
            yield break;
        }

        // 1. 스킬 선택
        Debug.Log("[AI개선] EnemyActionRoutine - 스킬 선택 시작");
        string skillID = ai.ChooseSkillID();
        if (string.IsNullOrEmpty(skillID))
        {
            Debug.LogWarning($"[AI ERROR] {enemy.name} 스킬 선택 실패");
            yield break;
        }
        Debug.Log($"[AI개선] EnemyActionRoutine - 선택된 스킬: {skillID}");

        // 2. 타겟 리스트 준비 (턴 매니저에서 플레이어들 리스트 넘겨줘야 함)
        Debug.Log("[AI개선] EnemyActionRoutine - 타겟 선택 시작");
        var targets = TurnManager.Instance.allSlots
            .Where(s => s.currentCharacter != null && s.currentCharacter.IsPlayer && !s.currentCharacter.IsDead)
            .Select(s => s.currentCharacter)
            .ToList();

        CharacterStats target = ai.ChooseTarget(targets);
        if (target == null)
        {
            Debug.LogWarning($"[AI ERROR] {enemy.name} 타겟 선택 실패");
            TurnManager.Instance.EndTurn();
            yield break;
        }
        Debug.Log($"[AI개선] EnemyActionRoutine - 선택된 타겟: {target.Label}");

        // 3. 스킬 사용
        Debug.Log("[AI개선] EnemyActionRoutine - 스킬 사용 시작");
        ai.UseSkill(skillID, enemy, target);

        Debug.Log("[AI개선] EnemyActionRoutine - 행동 후 0.5초 대기 시작");
        // 턴 전환 스킵 시스템 사용
        if (TurnTransitionSkipManager.Instance != null)
        {
            yield return TurnTransitionSkipManager.Instance.WaitForTurnTransition(0.5f, "enemyEnd");
        }
        else
        {
            yield return new WaitForSeconds(0.5f); // 기존 방식 (스킵 불가)
        }
        Debug.Log("[AI개선] EnemyActionRoutine - 행동 후 0.5초 대기 완료");

        // 턴 종료는 SkillManager에서 처리됨
        Debug.Log("[AI개선] EnemyActionRoutine - 스킬 사용 완료, 턴 종료는 SkillManager에서 처리");
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] EnemyActionRoutine 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }


    /// <summary>
    /// 스킬을 선택하는 추상 메서드
    /// </summary>
    public abstract string ChooseSkillID();

    /// <summary>
    /// 타겟을 선택하는 추상 메서드
    /// </summary>
    public abstract CharacterStats ChooseTarget(List<CharacterStats> players);



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
    
    public void UseSkill(string skillID, CharacterStats caster, CharacterStats target)
    {
        Debug.Log($"[AI개선] UseSkill 시작 - 스킬: {skillID}, 시전자: {caster?.Label}, 타겟: {target?.Label}");
        float startTime = Time.realtimeSinceStartup;
        
        if (caster == null || !caster.IsMyTurn)
        {
            Debug.LogWarning("[UseSkill] 지금은 내 턴이 아닙니다. 스킬 발동 중지.");
            return;
        }
        if (!SkillData.skillDict.TryGetValue(skillID, out var skill))
        {
            Debug.LogWarning($"[AI] 존재하지 않는 스킬 ID: {skillID}");
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("[UseSkill] 타겟이 없습니다. 스킬 발동 중지.");
            return;
        }

        Debug.Log("[AI개선] UseSkill - SkillManager.UseSkill 호출");
        SkillManager.Instance.UseSkill(skill, caster, target, skill);
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] UseSkill 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    public void SkipTurn()
    {
        StartCoroutine(SkipTurnCoroutine());
    }

    private IEnumerator SkipTurnCoroutine()
    {
        yield return null; // 한 프레임 쉼       // 다음 턴 시작
    }
}
