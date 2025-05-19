using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using System.Linq;
using static UnityEngine.GraphicsBuffer;

public class EnemyAIController : MonoBehaviour
{

    public IEnumerator EnemyActionRoutine(CharacterStats enemy)
    {
        if (enemy == null || enemy.Equals(null) || enemy.IsDead)
        {
            yield break; // 이미 파괴된 대상이면 행동 중단
        }

        yield return new WaitForSeconds(0.5f); // 턴 시작 전 텀

        var ai = enemy.GetComponent<EnemyAIController>();
        if (ai == null)
        {
            Debug.LogWarning($"[AI ERROR] {enemy.name}에게 EnemyAIController 없음");
            yield break;
        }

        // 1. 스킬 선택
        string skillID = ai.ChooseSkillID();
        if (string.IsNullOrEmpty(skillID))
        {
            Debug.LogWarning($"[AI ERROR] {enemy.name} 스킬 선택 실패");
            yield break;
        }

        // 2. 타겟 리스트 준비 (턴 매니저에서 플레이어들 리스트 넘겨줘야 함)
        var targets = TurnManager.Instance.allSlots
            .Where(s => s.currentCharacter != null && s.currentCharacter.IsPlayer && !s.currentCharacter.IsDead)
            .Select(s => s.currentCharacter)
            .ToList();

        CharacterStats target = ai.ChooseTarget(targets);
        if (target == null)
        {
            Debug.LogWarning($"[AI ERROR] {enemy.name} 타겟 선택 실패");
            yield break;
        }

        // 3. 스킬 사용
        ai.UseSkill(skillID, enemy, target);

        yield return new WaitForSeconds(1.0f); // 행동 텀

        // 4. 턴 종료
        TurnManager.Instance.EndTurn();
    }


    public string ChooseSkillID()
    {
        var stat = GetComponent<CharacterStats>();
        if (stat.Skills.Length == 0 || string.IsNullOrEmpty(stat.Skills[0]))
        {
            Debug.LogWarning("스킬이 비어있거나 정의되지 않음");
            return null;
        }

        return stat.Skills[0]; // 첫 번째 스킬 ID 반환
    }

    public CharacterStats ChooseTarget(List<CharacterStats> players)
    {
        // 살아있는 플레이어 중 가장 앞 대상 (예시)
        return players.FirstOrDefault(p => !p.IsDead);
    }
    public void UseSkill(string skillID, CharacterStats caster, CharacterStats target)
    {
        //Debug.Log($"[AI] {caster.name}가 선택한 스킬: {skillID}, 타겟: {target?.name}");

        if (string.IsNullOrEmpty(skillID))
        {
            Debug.LogWarning("[AI] 스킬 ID가 비어있습니다.");
            return;
        }

        if (!SkillData.skillDict.TryGetValue(skillID, out var skill))
        {
            Debug.LogWarning($"[AI] 존재하지 않는 스킬 ID: {skillID}");
            return;
        }

        SkillManager.Instance.UseSkill(skill, caster, target);
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
