using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

public abstract class EnemyAIController : MonoBehaviour
{
    [Header("AI 실패 복구")]
    [SerializeField] private bool enableRetryOnSkillStartFailure = true;
    [SerializeField, Range(1, 5)] private int maxSkillStartRetryCount = 2;
    [SerializeField] private float retryDelaySeconds = 0.12f;
    private string currentChosenSkillIdForTargeting;

    public IEnumerator EnemyActionRoutine(CharacterStats enemy)
    {
        Debug.Log($"[AI개선] EnemyActionRoutine 시작 - 적: {enemy?.Label}");
        float startTime = Time.realtimeSinceStartup;
        
        if (enemy == null || enemy.Equals(null) || !enemy.IsCombatCapable())
        {
            Debug.Log("[AI개선] EnemyActionRoutine - 적이 null이거나 생존 행동 불가");
            if (enemy != null && TurnManager.Instance != null)
                TurnManager.Instance.EndTurn();
            yield break;
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

        if (enemy == null || enemy.Equals(null) || !enemy.IsCombatCapable())
        {
            Debug.Log("[AI개선] EnemyActionRoutine - 대기 후 생존 행동 불가, 턴 종료");
            if (TurnManager.Instance != null)
                TurnManager.Instance.EndTurn();
            yield break;
        }

        var ai = enemy.GetComponent<EnemyAIController>();
        if (ai == null)
        {
            Debug.LogWarning($"[AI ERROR] {enemy.name}에게 EnemyAIController 없음");
            yield break;
        }

        // 1~3. 스킬 선택/타겟 선택/사용 시도 (실패 시 같은 턴에서 재시도)
        bool actionStarted = false;
        int retryLimit = enableRetryOnSkillStartFailure ? Mathf.Max(1, maxSkillStartRetryCount) : 1;
        for (int attempt = 1; attempt <= retryLimit; attempt++)
        {
            // 1) 스킬 선택
            Debug.Log($"[AI개선] EnemyActionRoutine - 스킬 선택 시작 (시도 {attempt}/{retryLimit})");
            string skillID = ai.ChooseSkillID();
            if (string.IsNullOrEmpty(skillID))
            {
                Debug.LogWarning($"[AI ERROR] {enemy.name} 스킬 선택 실패 (시도 {attempt}/{retryLimit})");
                if (attempt < retryLimit)
                {
                    if (retryDelaySeconds > 0f) yield return new WaitForSeconds(retryDelaySeconds);
                    continue;
                }
                break;
            }
            currentChosenSkillIdForTargeting = skillID;

            // 2) 타겟 선택
            var targets = TurnManager.Instance.allSlots
                .Where(s => s.currentCharacter != null && s.currentCharacter.IsPlayer && !s.currentCharacter.IsDead)
                .Select(s => s.currentCharacter)
                .ToList();

            CharacterStats target = ai.ChooseTarget(targets);
            if (target == null)
            {
                Debug.LogWarning($"[AI ERROR] {enemy.name} 타겟 선택 실패 (시도 {attempt}/{retryLimit}, skill={skillID})");
                if (attempt < retryLimit)
                {
                    if (retryDelaySeconds > 0f) yield return new WaitForSeconds(retryDelaySeconds);
                    continue;
                }
                break;
            }

            if (enemy == null || enemy.Equals(null) || !enemy.IsCombatCapable())
            {
                Debug.Log("[AI개선] EnemyActionRoutine - 시전 직전 생존 행동 불가, 턴 종료");
                if (TurnManager.Instance != null)
                    TurnManager.Instance.EndTurn();
                yield break;
            }

            // 3) 스킬 사용
            Debug.Log($"[AI개선] EnemyActionRoutine - 스킬 사용 시작 (시도 {attempt}/{retryLimit}, skill={skillID}, target={target.Label})");
            actionStarted = ai.UseSkill(skillID, enemy, target);
            if (actionStarted)
                break;

            Debug.LogWarning(
                $"[AI개선] EnemyActionRoutine - 스킬 시작 실패 (시도 {attempt}/{retryLimit}, skill={skillID}, " +
                $"reason={SkillManager.LastUseSkillFailureReason}, detail={SkillManager.LastUseSkillFailureDetail})");
            if (attempt < retryLimit && retryDelaySeconds > 0f)
                yield return new WaitForSeconds(retryDelaySeconds);
        }

        if (!actionStarted)
        {
            Debug.LogError($"[AI개선] EnemyActionRoutine - 재시도 후에도 스킬 시작 실패, 턴 종료: {enemy?.Label}");
            if (TurnManager.Instance != null)
                TurnManager.Instance.EndTurn();
            yield break;
        }

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
    /// 타겟 우선 규칙:
    /// - 1순위: 지목형 디버프 대상(도발보다 우선)
    /// - 2순위: 도발 표식 버프 대상
    /// - 없으면 기존 후보 유지
    /// </summary>
    protected List<CharacterStats> ApplyTauntPriorityCandidates(List<CharacterStats> candidates)
    {
        if (candidates == null || candidates.Count == 0)
            return candidates;

        bool isRandomTargetSkill = false;
        bool isAreaLikeSkill = false;
        if (!string.IsNullOrEmpty(currentChosenSkillIdForTargeting)
            && SkillData.skillDict.TryGetValue(currentChosenSkillIdForTargeting, out var chosenSkill)
            && chosenSkill != null)
        {
            switch (chosenSkill.TargetType)
            {
                case SkillTargetType.RandomEnemy:
                case SkillTargetType.RandomAlly:
                case SkillTargetType.RandomTarget:
                    isRandomTargetSkill = true;
                    break;
            }

            switch (chosenSkill.TargetType)
            {
                case SkillTargetType.AllAllies:
                case SkillTargetType.AllEnemies:
                case SkillTargetType.AllUnits:
                case SkillTargetType.Adjacent:
                case SkillTargetType.SelfAndAdjacent:
                case SkillTargetType.AdjacentArea:
                case SkillTargetType.SelfAndAdjacentArea:
                    isAreaLikeSkill = true;
                    break;
            }
        }

        var markTargets = candidates
            .Where(c => c != null && !c.IsDead && HasMarkPriorityDebuff(c))
            .ToList();

        if (markTargets.Count > 0)
        {
            Debug.Log($"[AI Mark] 지목 대상 우선 적용: {markTargets.Count}명");
            return markTargets;
        }

        var tauntTargets = candidates
            .Where(c => c != null && !c.IsDead && HasTauntLikeMarker(c))
            .ToList();

        if (isRandomTargetSkill && tauntTargets.Count > 0)
        {
            bool allowTauntOnRandom = tauntTargets.Any(HasTauntRandomOverride);
            if (!allowTauntOnRandom)
                return candidates; // 일반 도발은 랜덤 타겟에 영향 없음
        }

        if (isAreaLikeSkill && tauntTargets.Count > 0)
        {
            bool allowTauntOnArea = tauntTargets.Any(HasTauntAoEOverride);
            if (!allowTauntOnArea)
                return candidates; // 일반 도발은 광역/범위 타겟에 영향 없음
        }

        if (tauntTargets.Count > 0)
        {
            Debug.Log($"[AI Taunt] 도발 대상 우선 적용: {tauntTargets.Count}명");
            return tauntTargets;
        }

        return candidates;
    }

    private static bool HasTauntLikeMarker(CharacterStats target)
    {
        if (target == null) return false;
        var controller = target.GetComponent<StatusEffectController>();
        if (controller == null) return false;
        return controller.HasAllyTargetBlockByBuff();
    }

    private static bool HasMarkPriorityDebuff(CharacterStats target)
    {
        if (target == null) return false;
        var controller = target.GetComponent<StatusEffectController>();
        if (controller == null) return false;
        return controller.HasMarkPriorityDebuff();
    }

    private static bool HasTauntRandomOverride(CharacterStats target)
    {
        if (target == null) return false;
        var controller = target.GetComponent<StatusEffectController>();
        if (controller == null) return false;
        return controller.HasTauntAffectsRandomTargeting();
    }

    private static bool HasTauntAoEOverride(CharacterStats target)
    {
        if (target == null) return false;
        var controller = target.GetComponent<StatusEffectController>();
        if (controller == null) return false;
        return controller.HasTauntProtectsAgainstAoE();
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
    
    public bool UseSkill(string skillID, CharacterStats caster, CharacterStats target)
    {
        Debug.Log($"[AI개선] UseSkill 시작 - 스킬: {skillID}, 시전자: {caster?.Label}, 타겟: {target?.Label}");
        float startTime = Time.realtimeSinceStartup;
        
        if (caster == null || !caster.IsMyTurn)
        {
            Debug.LogWarning("[UseSkill] 지금은 내 턴이 아닙니다. 스킬 발동 중지.");
            return false;
        }
        if (!SkillData.skillDict.TryGetValue(skillID, out var skill))
        {
            Debug.LogWarning($"[AI] 존재하지 않는 스킬 ID: {skillID}");
            return false;
        }

        if (target == null)
        {
            Debug.LogWarning("[UseSkill] 타겟이 없습니다. 스킬 발동 중지.");
            return false;
        }

        Debug.Log("[AI개선] UseSkill - SkillManager.UseSkill 호출");
        bool started = SkillManager.Instance.UseSkill(skill, caster, target, skill);
        if (!started)
            Debug.LogWarning($"[AI개선] UseSkill - 스킬 시작 실패: {skillID}, type={skill.Type}, targetType={skill.TargetType}, target={target.Label}, caster={caster.Label}, reason={SkillManager.LastUseSkillFailureReason}, detail={SkillManager.LastUseSkillFailureDetail}");
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] UseSkill 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
        return started;
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
