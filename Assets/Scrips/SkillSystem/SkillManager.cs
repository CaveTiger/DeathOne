using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    void Awake() { Instance = this; }

    [SerializeField] GameObject CharacterUnit;

    /// <summary>
    /// 스킬이 범위 스킬인지 확인합니다
    /// </summary>
    private bool IsRangeSkill(SkillData skill)
    {
        if (skill == null || string.IsNullOrEmpty(skill.SkillTarget))
            return false;

        string target = skill.SkillTarget.ToLower();
        return target.Contains("all") || target.Contains("adjacent") || 
               target.Contains("random") || target.Contains("lowest") || 
               target.Contains("highest") || target.Contains("weakest") || 
               target.Contains("strongest");
    }
    public CharacterInfoPlayer playerInfoUI; // 인스펙터에서 PlayerInfo 오브젝트 할당

    public void UseSkill(SkillData skill, CharacterStats caster, CharacterStats target, SkillData motionData)
    {
        if (!skill.IsUsable()) return;
        skill.currentCooldown = skill.cooldown;

        // 범위 스킬인지 확인
        if (IsRangeSkill(skill))
        {
            UseRangeSkill(skill, caster, target, motionData);
            return;
        }

        // 단일 타겟 스킬 검증
        // 공격 스킬: 아군 타겟 불가
        if ((skill.Type == SkillType.Damage || skill.Type == SkillType.Piercing || skill.Type == SkillType.linkage)
            && target != null && target.IsPlayer == caster.IsPlayer)
        {
            Debug.LogWarning("[SkillManager] 공격 스킬은 아군을 타겟팅할 수 없습니다.");
            return;
        }

        // 버프/힐 스킬: 적 타겟 불가 (단, 'Me'는 본인만)
        if ((skill.Type == SkillType.Buff || skill.Type == SkillType.Heal)
            && skill.SkillTarget != "Me"
            && target != null && target.IsPlayer != caster.IsPlayer)
        {
            Debug.LogWarning("[SkillManager] 버프/힐 스킬은 적을 타겟팅할 수 없습니다.");
            return;
        }

        // 연출 시작
        StartCoroutine(PlaySkillEffect(skill, caster, target, motionData));
    }

    /// <summary>
    /// 범위 스킬을 사용합니다
    /// </summary>
    private void UseRangeSkill(SkillData skill, CharacterStats caster, CharacterStats target, SkillData motionData)
    {
        List<CharacterStats> targets = GetRangeTargets(skill, caster, target);
        
        if (targets.Count == 0)
        {
            Debug.LogWarning("[SkillManager] 범위 스킬의 타겟이 없습니다.");
            return;
        }

        Debug.Log($"[SkillManager] 범위 스킬 사용: {skill.Name}, 타겟 수: {targets.Count}");
        
        // 범위 스킬 연출 시작
        StartCoroutine(PlayRangeSkillEffect(skill, caster, targets, motionData));
    }

    /// <summary>
    /// 범위 스킬의 타겟들을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetRangeTargets(SkillData skill, CharacterStats caster, CharacterStats centerTarget)
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        
        if (skill == null || string.IsNullOrEmpty(skill.SkillTarget))
            return targets;

        string targetType = skill.SkillTarget.ToLower();
        
        switch (targetType)
        {
            case "allenemies":
                targets = GetAliveCharacters(!caster.IsPlayer);
                break;
                
            case "allallies":
                targets = GetAliveCharacters(caster.IsPlayer);
                break;
                
            case "allunits":
                targets = GetAliveCharacters();
                break;
                
            case "adjacent":
                targets = GetAdjacentTargets(caster, centerTarget);
                break;
                       
            case "selfandadjacent":
                targets = GetSelfAndAdjacentTargets(caster, centerTarget);
                break;
                
            case "randomenemy":
                targets = GetRandomTargets(!caster.IsPlayer, 1);
                break;
                
            case "randomally":
                targets = GetRandomTargets(caster.IsPlayer, 1);
                break;
                
            case "randomtarget":
                targets = GetRandomTargets(null, 1);
                break;
                
            case "lowesthpally":
                targets = GetLowestHpTargets(caster.IsPlayer);
                break;
                
            case "highesthpenemy":
                targets = GetHighestHpTargets(!caster.IsPlayer);
                break;
                
            case "weakestenemy":
                targets = GetWeakestEnemyTargets();
                break;
                
            case "strongestally":
                targets = GetStrongestAllyTargets(caster.IsPlayer);
                break;
        }
        
        return targets;
    }

    /// <summary>
    /// 생존한 캐릭터들을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetAliveCharacters(bool? isPlayer = null)
    {
        List<CharacterStats> characters = new List<CharacterStats>();
        
        // 임시로 간단한 방법 사용
        // TODO: BattleManager에서 GetAllCharacters 메서드 구현 필요
        var allCharacters = FindObjectsOfType<CharacterStats>();
        foreach (var character in allCharacters)
        {
            if (character != null && !character.IsDead)
            {
                if (isPlayer == null || character.IsPlayer == isPlayer)
                {
                    characters.Add(character);
                }
            }
        }
        
        return characters;
    }



    /// <summary>
    /// 인접 슬롯의 타겟들을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetAdjacentTargets(CharacterStats caster, CharacterStats centerTarget)
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        
        // TODO: 실제 슬롯 기반 인접 계산 구현
        // 현재는 간단히 거리 기반으로 처리하되, 아군/적군 구분
        var allCharacters = GetAliveCharacters();
        foreach (var character in allCharacters)
        {
            if (character != centerTarget && character != caster)
            {
                // 아군 스킬인 경우 아군만, 적군 스킬인 경우 적군만 타겟팅
                if (character.IsPlayer == caster.IsPlayer)
                {
                    float distance = Vector3.Distance(centerTarget.transform.position, character.transform.position);
                    if (distance <= 2f) // 인접 거리
                    {
                        targets.Add(character);
                    }
                }
            }
        }
        
        return targets;
    }

    /// <summary>
    /// 자신과 타겟 기준 좌우 인접 슬롯의 타겟들을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetSelfAndAdjacentTargets(CharacterStats caster, CharacterStats centerTarget)
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        
        // 자신 추가
        if (centerTarget != null && !centerTarget.IsDead)
        {
            targets.Add(centerTarget);
        }
        
        // 인접 타겟들 추가
        targets.AddRange(GetAdjacentTargets(caster, centerTarget));
        
        return targets;
    }

    /// <summary>
    /// 랜덤 타겟들을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetRandomTargets(bool? isPlayer, int count)
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        var candidates = GetAliveCharacters(isPlayer);
        
        if (candidates.Count == 0) return targets;
        
        // 랜덤 선택
        for (int i = 0; i < count && candidates.Count > 0; i++)
        {
            int randomIndex = UnityEngine.Random.Range(0, candidates.Count);
            targets.Add(candidates[randomIndex]);
            candidates.RemoveAt(randomIndex);
        }
        
        return targets;
    }

    /// <summary>
    /// 체력이 가장 낮은 타겟들을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetLowestHpTargets(bool isPlayer)
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        var candidates = GetAliveCharacters(isPlayer);
        
        if (candidates.Count == 0) return targets;
        
        // 체력이 가장 낮은 캐릭터 찾기
        CharacterStats lowestHp = candidates[0];
        foreach (var character in candidates)
        {
            if (character.Hp < lowestHp.Hp)
            {
                lowestHp = character;
            }
        }
        
        targets.Add(lowestHp);
        return targets;
    }

    /// <summary>
    /// 체력이 가장 높은 타겟들을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetHighestHpTargets(bool isPlayer)
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        var candidates = GetAliveCharacters(isPlayer);
        
        if (candidates.Count == 0) return targets;
        
        // 체력이 가장 높은 캐릭터 찾기
        CharacterStats highestHp = candidates[0];
        foreach (var character in candidates)
        {
            if (character.Hp > highestHp.Hp)
            {
                highestHp = character;
            }
        }
        
        targets.Add(highestHp);
        return targets;
    }

    /// <summary>
    /// 방어력이 가장 낮은 적을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetWeakestEnemyTargets()
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        var enemies = GetAliveCharacters(false);
        
        if (enemies.Count == 0) return targets;
        
        // 방어력이 가장 낮은 적 찾기
        CharacterStats weakest = enemies[0];
        foreach (var enemy in enemies)
        {
            if (enemy.Def < weakest.Def)
            {
                weakest = enemy;
            }
        }
        
        targets.Add(weakest);
        return targets;
    }

    /// <summary>
    /// 공격력이 가장 높은 아군을 가져옵니다
    /// </summary>
    private List<CharacterStats> GetStrongestAllyTargets(bool isPlayer)
    {
        List<CharacterStats> targets = new List<CharacterStats>();
        var allies = GetAliveCharacters(isPlayer);
        
        if (allies.Count == 0) return targets;
        
        // 공격력이 가장 높은 아군 찾기
        CharacterStats strongest = allies[0];
        foreach (var ally in allies)
        {
            if (ally.Atk > strongest.Atk)
            {
                strongest = ally;
            }
        }
        
        targets.Add(strongest);
        return targets;
    }

    private IEnumerator PlaySkillEffect(SkillData skill, CharacterStats caster, CharacterStats target, SkillData motionData)
    {
        Debug.Log($"[스킬연출] PlaySkillEffect 시작 - 스킬: {skill?.Name}, 시전자: {caster?.Label}, 타겟: {target?.Label}");
        float startTime = Time.realtimeSinceStartup;
        
        // 스킬 사용 후 즉시 스킬 UI 비활성화 (UX 개선)
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.DisableAllSkillUI();
            Debug.Log("[스킬연출] PlaySkillEffect - 스킬 사용 후 스킬 UI 즉시 비활성화");
        }
        
        // 1. 초기 대기
        Debug.Log("[스킬연출] PlaySkillEffect - 초기 0.4초 대기 시작");
        yield return new WaitForSeconds(0.4f); // 원래 방식으로 복구
        Debug.Log("[스킬연출] PlaySkillEffect - 초기 0.4초 대기 완료");
        
        BattleUIManager.Instance.ChangeUIBattle();
        
        // 2. 카메라 줌인
        Debug.Log("[스킬연출] PlaySkillEffect - 카메라 줌인 시작");
        yield return StartCoroutine(BattleCamera.Instance.CameraZoom(2.4f, 0.4f));
        Debug.Log("[스킬연출] PlaySkillEffect - 카메라 줌인 완료");

        var casterMotion = GetMotionController(caster);
        var targetMotion = GetMotionController(target);

        // 3. 캐릭터 이동 (죽지 않은 캐릭터만)
        Debug.Log("[스킬연출] PlaySkillEffect - 캐릭터 이동 시작");
        
        // 힐 스킬인 경우 캐릭터 위치 조정
        if (skill.Type == SkillType.Heal)
        {
            // 캐스터를 뒤쪽으로, 타겟을 앞쪽으로 배치
            if (casterMotion != null && !caster.IsDead)
            {
                Vector3 casterBackPosition = GetHealCasterPosition(caster);
                casterMotion.MoveToPosition(casterBackPosition);
                Debug.Log($"[스킬연출] 힐 캐스터 위치 조정: {caster.Label} -> {casterBackPosition}");
            }
            if (targetMotion != null && !target.IsDead)
            {
                Vector3 targetFrontPosition = GetHealTargetPosition(target);
                targetMotion.MoveToPosition(targetFrontPosition);
                Debug.Log($"[스킬연출] 힐 타겟 위치 조정: {target.Label} -> {targetFrontPosition}");
            }
        }
        else
        {
            // 일반 스킬은 기존 방식
            if (casterMotion != null && !caster.IsDead)
                casterMotion.MoveToBattlePosition();
            if (targetMotion != null && !target.IsDead)
                targetMotion.MoveToBattlePosition();
        }
        
        Debug.Log("[스킬연출] PlaySkillEffect - 캐릭터 이동 완료");

        Debug.Log("[스킬연출] PlaySkillEffect - 이동 후 0.3초 대기 시작");
        yield return new WaitForSeconds(0.3f); // 원래 방식으로 복구
        Debug.Log("[스킬연출] PlaySkillEffect - 이동 후 0.3초 대기 완료");

        // 4. 스킬 모션 재생
        Debug.Log("[스킬연출] PlaySkillEffect - 스킬 모션 재생 시작");
        if (casterMotion != null && !caster.IsDead)
            casterMotion.PlaySkillMotion(skill.Motion);
        Debug.Log("[스킬연출] PlaySkillEffect - 스킬 모션 재생 완료");

        // 5. 타격 타이밍 계산
        float hitTiming = 0.3f; // 타격 타이밍 (초)
        Debug.Log($"[스킬연출] PlaySkillEffect - 타격 타이밍 대기 시작 ({hitTiming}초)");
        yield return new WaitForSeconds(hitTiming);
        Debug.Log("[스킬연출] PlaySkillEffect - 타격 타이밍 대기 완료");

        // 6. 타겟 모션 재생 (타격 순간) - 힐 스킬은 제외
        Debug.Log("[스킬연출] PlaySkillEffect - 타겟 모션 재생 시작");
        if (targetMotion != null && !target.IsDead && skill.Type != SkillType.Heal)
            targetMotion.PlayHitMotion();
        Debug.Log("[스킬연출] PlaySkillEffect - 타겟 모션 재생 완료");

        // 7. 피해/효과 처리 (타격 순간과 동시에)
        Debug.Log("[스킬연출] PlaySkillEffect - 피해/효과 처리 시작");
        switch (skill.Type)
        {
            case SkillType.Damage:
            case SkillType.Piercing:
                int damage = CalculateDamage(skill, caster, target);
                target.TakeDamage(damage, caster.Accuracy, skill, caster.transform.position);
                ApplyStatusEffects(skill, target);
                break;
            case SkillType.Buff:
                ApplyBuffEffects(skill, caster, target);
                ApplyStatusEffects(skill, target);
                break;
            case SkillType.Heal:
                int healAmount = UnityEngine.Random.Range(skill.HealMin, skill.HealMax + 1);
                Debug.Log($"[스킬연출] 힐 스킬 사용: {skill.Name}, 힐량 범위: {skill.HealMin}-{skill.HealMax}, 실제 힐량: {healAmount}, 타겟: {target.Label}");
                target.Heal(healAmount);
                ApplyStatusEffects(skill, target);
                break;
            case SkillType.Debuff:
                ApplyStatusEffects(skill, target);
                break;
            case SkillType.linkage:
                // 연계 스킬 특수 처리
                break;
            default:
                Debug.LogWarning("알 수 없는 스킬 타입입니다.");
                break;
        }
        Debug.Log("[스킬연출] PlaySkillEffect - 피해/효과 처리 완료");

        // 8. 공격 모션이 끝날 때까지 추가 대기 (카메라 줌아웃과 맞춤)
        float remain = 0.7f - hitTiming; // 전체 모션 시간 - 타격 타이밍
        if (remain > 0)
        {
            Debug.Log($"[스킬연출] PlaySkillEffect - 모션 완료 대기 시작 ({remain}초)");
            yield return new WaitForSeconds(remain);
            Debug.Log("[스킬연출] PlaySkillEffect - 모션 완료 대기 완료");
        }

        // 9. 카메라 줌아웃 시작 (캐릭터 복귀는 줌아웃 완료 후)
        Debug.Log("[스킬연출] PlaySkillEffect - 카메라 줌아웃 시작");
        yield return StartCoroutine(BattleCamera.Instance.CameraZoom(5f, 0.5f));
        Debug.Log("[스킬연출] PlaySkillEffect - 카메라 줌아웃 완료");

        // 10. 카메라 줌아웃 완료 순간 캐릭터 모션 리셋과 위치 복귀
        Debug.Log("[스킬연출] PlaySkillEffect - 캐릭터 모션 리셋 및 위치 복귀 시작");
        if (casterMotion != null && !caster.IsDead)
        {
            casterMotion.ResetMotion();
            casterMotion.ResetPosition();
        }
        if (targetMotion != null && !target.IsDead)
        {
            targetMotion.ResetMotion();
            targetMotion.ResetPosition();
        }
        Debug.Log("[스킬연출] PlaySkillEffect - 캐릭터 모션 리셋 및 위치 복귀 완료");

        // 11. 최종 대기 (카메라 줌아웃 완료 후)
        Debug.Log("[스킬연출] PlaySkillEffect - 최종 0.5초 대기 시작");
        yield return new WaitForSeconds(0.5f); // 원래 방식으로 복구
        Debug.Log("[스킬연출] PlaySkillEffect - 최종 0.5초 대기 완료");

        // 12. UI 정상 모드로 복귀
        Debug.Log("[스킬연출] PlaySkillEffect - UI 정상 모드 복귀");
        BattleUIManager.Instance.ChangeUINormal();

        // 적 턴일 때 스킬 완료 로그
        if (!caster.IsPlayer)
        {
            Debug.Log("[스킬연출] PlaySkillEffect - 적 턴 스킬 완료, 카메라/UI 리셋");
        }

        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[스킬연출] PlaySkillEffect 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
        
        // 스킬 연출 완료 후 턴 종료
        Debug.Log("[스킬연출] PlaySkillEffect - 턴 종료 호출 시작");
        
        // TurnManager.Instance가 null인 경우 재시도
        int retryCount = 0;
        while (TurnManager.Instance == null && retryCount < 10)
        {
            Debug.LogWarning($"[스킬연출] PlaySkillEffect - TurnManager.Instance null, 재시도 {retryCount + 1}/10");
            yield return new WaitForSeconds(0.1f);
            retryCount++;
        }
        
        if (TurnManager.Instance != null)
        {
            Debug.Log("[스킬연출] PlaySkillEffect - TurnManager.Instance 존재, EndTurn 호출");
            TurnManager.Instance.EndTurn();
        }
        else
        {
            Debug.LogError("[스킬연출] PlaySkillEffect - TurnManager.Instance가 null입니다! 재시도 실패");
        }
        Debug.Log("[스킬연출] PlaySkillEffect - 턴 종료 호출 완료");
        {
            Debug.LogError("[스킬연출] PlaySkillEffect - TurnManager.Instance가 null입니다!");
        }
    }

    /// <summary>
    /// 범위 스킬 연출을 재생합니다
    /// </summary>
    private IEnumerator PlayRangeSkillEffect(SkillData skill, CharacterStats caster, List<CharacterStats> targets, SkillData motionData)
    {
        Debug.Log($"[스킬연출] PlayRangeSkillEffect 시작 - 스킬: {skill?.Name}, 시전자: {caster?.Label}, 타겟 수: {targets.Count}");
        
        // 스킬 사용 후 즉시 스킬 UI 비활성화
        if (BattleUIManager.Instance != null)
        {
            BattleUIManager.Instance.DisableAllSkillUI();
        }
        
        // 1. 초기 대기
        yield return new WaitForSeconds(0.4f);
        
        BattleUIManager.Instance.ChangeUIBattle();
        
        // 2. 카메라 줌인 (힐 스킬은 제외, 전체 공격은 제자리에서만)
        if (skill.Type != SkillType.Heal)
        {
            yield return StartCoroutine(BattleCamera.Instance.CameraZoom(2.4f, 0.4f));
        }

        var casterMotion = GetMotionController(caster);

        // 3. 캐릭터 이동 (힐 스킬은 제외, 전체 공격은 제자리에서만)
        if (skill.Type != SkillType.Heal)
        {
            // 일반 스킬은 기존 방식
            if (casterMotion != null && !caster.IsDead)
                casterMotion.MoveToBattlePosition();
            
            foreach (var target in targets)
            {
                if (target != null && !target.IsDead)
                {
                    var targetMotion = GetMotionController(target);
                    if (targetMotion != null)
                        targetMotion.MoveToBattlePosition();
                }
            }
        }
        // 힐 스킬은 모든 캐릭터가 제자리에서 작동 (움직임 없음)

        yield return new WaitForSeconds(0.3f);

        // 4. 스킬 모션 재생
        if (casterMotion != null && !caster.IsDead)
            casterMotion.PlaySkillMotion(skill.Motion);

        // 5. 타격 타이밍 계산
        float hitTiming = 0.3f;
        yield return new WaitForSeconds(hitTiming);

        // 6. 모든 타겟에 효과 적용
        foreach (var target in targets)
        {
            if (target != null && !target.IsDead)
            {
                var targetMotion = GetMotionController(target);
                if (targetMotion != null && skill.Type != SkillType.Heal)
                    targetMotion.PlayHitMotion();

                // 피해/효과 처리
                switch (skill.Type)
                {
                    case SkillType.Damage:
                    case SkillType.Piercing:
                        int damage = CalculateDamage(skill, caster, target);
                        target.TakeDamage(damage, caster.Accuracy, skill, caster.transform.position);
                        ApplyStatusEffects(skill, target);
                        break;
                    case SkillType.Buff:
                        ApplyBuffEffects(skill, caster, target);
                        ApplyStatusEffects(skill, target);
                        break;
                    case SkillType.Heal:
                        int rangeHealAmount = UnityEngine.Random.Range(skill.HealMin, skill.HealMax + 1);
                        target.Heal(rangeHealAmount);
                        ApplyStatusEffects(skill, target);
                        break;
                    case SkillType.Debuff:
                        ApplyStatusEffects(skill, target);
                        break;
                }
            }
        }

        // 7. 공격 모션이 끝날 때까지 추가 대기
        float remain = 0.7f - hitTiming;
        if (remain > 0)
        {
            yield return new WaitForSeconds(remain);
        }

        // 8. 카메라 줌아웃 (힐 스킬은 제외, 전체 공격은 제자리에서만)
        if (skill.Type != SkillType.Heal)
        {
            yield return StartCoroutine(BattleCamera.Instance.CameraZoom(5f, 0.4f));
        }

        // 9. 캐릭터 원위치 (힐 스킬은 제외, 전체 공격은 제자리에서만)
        if (skill.Type != SkillType.Heal)
        {
            Debug.Log("[스킬연출] PlayRangeSkillEffect - 일반 스킬 캐릭터 원위치 시작");
            if (casterMotion != null && !caster.IsDead)
            {
                casterMotion.ResetMotion();
                casterMotion.ResetPosition();
                Debug.Log($"[스킬연출] 범위 스킬 캐스터 원위치: {caster.Label}");
            }
            
            foreach (var target in targets)
            {
                if (target != null && !target.IsDead)
                {
                    var targetMotion = GetMotionController(target);
                    if (targetMotion != null)
                    {
                        targetMotion.ResetMotion();
                        targetMotion.ResetPosition();
                        Debug.Log($"[스킬연출] 범위 스킬 타겟 원위치: {target.Label}");
                    }
                }
            }
            Debug.Log("[스킬연출] PlayRangeSkillEffect - 일반 스킬 캐릭터 원위치 완료");
        }

        yield return new WaitForSeconds(0.3f);

        // 10. 턴 종료
        Debug.Log("[스킬연출] PlayRangeSkillEffect - 턴 종료 호출 시작");
        
        // TurnManager.Instance가 null인 경우 재시도
        int retryCount = 0;
        while (TurnManager.Instance == null && retryCount < 10)
        {
            Debug.LogWarning($"[스킬연출] PlayRangeSkillEffect - TurnManager.Instance null, 재시도 {retryCount + 1}/10");
            yield return new WaitForSeconds(0.1f);
            retryCount++;
        }
        
        if (TurnManager.Instance != null)
        {
            Debug.Log("[스킬연출] PlayRangeSkillEffect - TurnManager.Instance 존재, EndTurn 호출");
            TurnManager.Instance.EndTurn();
        }
        else
        {
            Debug.LogError("[스킬연출] PlayRangeSkillEffect - TurnManager.Instance가 null입니다! 재시도 실패");
        }
        Debug.Log("[스킬연출] PlayRangeSkillEffect - 턴 종료 호출 완료");
    }

    /// <summary>
    /// 힐 스킬 캐스터의 뒤쪽 위치를 계산합니다.
    /// </summary>
    /// <param name="caster">캐스터 캐릭터</param>
    /// <returns>캐스터의 뒤쪽 위치</returns>
    private Vector3 GetHealCasterPosition(CharacterStats caster)
    {
        Vector3 centerPosition = Vector3.zero; // 중앙 기준점
        float backOffset = 2f; // 뒤쪽 오프셋
        
        if (caster.IsPlayer)
        {
            // 플레이어 캐릭터는 왼쪽 뒤쪽
            return centerPosition + new Vector3(-backOffset, 0, 0);
        }
        else
        {
            // 적 캐릭터는 오른쪽 뒤쪽
            return centerPosition + new Vector3(backOffset, 0, 0);
        }
    }

    /// <summary>
    /// 힐 스킬 타겟의 앞쪽 위치를 계산합니다.
    /// </summary>
    /// <param name="target">타겟 캐릭터</param>
    /// <returns>타겟의 앞쪽 위치</returns>
    private Vector3 GetHealTargetPosition(CharacterStats target)
    {
        Vector3 centerPosition = Vector3.zero; // 중앙 기준점
        float frontOffset = 1f; // 앞쪽 오프셋
        
        if (target.IsPlayer)
        {
            // 플레이어 캐릭터는 왼쪽 앞쪽
            return centerPosition + new Vector3(-frontOffset, 0, 0);
        }
        else
        {
            // 적 캐릭터는 오른쪽 앞쪽
            return centerPosition + new Vector3(frontOffset, 0, 0);
        }
    }

    /// <summary>
    /// 캐릭터에서 모션 컨트롤러를 안전하게 가져옵니다.
    /// </summary>
    private CharacterMotionController GetMotionController(CharacterStats character)
    {
        if (character == null)
        {
            Debug.LogWarning("[SkillManager] 캐릭터가 null입니다.");
            return null;
        }

        var motion = character.GetComponent<CharacterMotionController>();
        if (motion == null)
        {
            motion = character.GetComponentInChildren<CharacterMotionController>();
        }
        
        if (motion == null)
        {
            Debug.LogWarning($"[SkillManager] {character.Label}에서 CharacterMotionController를 찾을 수 없습니다.");
        }
        
        return motion;
    }

    private void ApplyStatusEffects(SkillData skill, CharacterStats target)
    {
        Debug.Log($"[SkillManager] ApplyStatusEffects: target={target?.Label}, skill={skill?.Name}");
        if (skill?.skillEffects == null || target == null)
        {
            Debug.LogWarning("[SkillManager] 스킬 효과 또는 타겟이 null입니다.");
            return;
        }

        foreach (var effect in skill.skillEffects)
        {
            Debug.Log($"[SkillManager] 적용 시도 EffectID: {effect.EffectID}, Value: {effect.Value}, Duration: {effect.Duration}");
            if (effect == null || string.IsNullOrEmpty(effect.EffectID))
            {
                Debug.LogWarning("[SkillManager] 유효하지 않은 효과 데이터");
                continue;
            }

            var effectData = StatusEffectManager.Instance.GetById(effect.EffectID);
            if (effectData != null)
            {
                Debug.Log($"[SkillManager] 상태이상 데이터 로드 성공: {effectData.effectName}");
                target.AddStatusEffectPrefab(effectData, effect.Duration, effect.Value);
            }
            else
            {
                Debug.LogWarning($"[SkillManager] 상태이상 데이터를 찾을 수 없음: {effect.EffectID}");
            }
        }
    }

    private void ApplyBuffEffects(SkillData skill, CharacterStats caster, CharacterStats target)
    {
        if (skill?.skillEffects == null || caster == null)
            return;

        List<CharacterStats> targets = new List<CharacterStats>();
        switch (skill.SkillTarget)
        {
            case "Me":
                targets.Add(caster);
                break;
            case "Ally":
                // 아군만, 본인 제외
                if (target != null && target.IsPlayer == caster.IsPlayer && !target.IsDead && target != caster)
                    targets.Add(target);
                break;
            case "AllAllies":
                foreach (var slot in TurnManager.Instance.allSlots)
                {
                    var character = slot.currentCharacter;
                    if (character != null && !character.IsDead && character.IsPlayer == caster.IsPlayer)
                        targets.Add(character);
                }
                break;
        }

        // 적을 타겟팅한 경우 아무에게도 버프를 적용하지 않음
        if (targets.Count == 0)
        {
            Debug.LogWarning("[SkillManager] 버프 스킬은 적을 타겟팅할 수 없습니다.");
            return;
        }

        foreach (var buffTarget in targets)
        {
            foreach (var effect in skill.skillEffects)
            {
                if (effect == null || string.IsNullOrEmpty(effect.EffectID))
                    continue;

                var effectData = StatusEffectManager.Instance.GetById(effect.EffectID);
                if (effectData != null)
                    buffTarget.AddStatusEffectPrefab(effectData, effect.Duration, effect.Value);
            }
        }
    }

    private int CalculateDamage(SkillData skill, CharacterStats caster, CharacterStats target)
    {
        int baseDamage = UnityEngine.Random.Range(skill.DamageMin, skill.DamageMax + 1);

        int atk = caster.Atk;
        int def = target.Def;

        int difference = atk - def;
        float multiplier = 1f + (difference * 0.1f);
        multiplier = Mathf.Max(multiplier, 0.1f); 

        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

        return finalDamage;
    }

    private void ApplyPiercingDamage(SkillData skill, CharacterStats caster, CharacterStats target)
    {
        // 예시: 방어 무시 데미지
        int baseDamage = UnityEngine.Random.Range(skill.DamageMin, skill.DamageMax + 1);
        int finalDamage = baseDamage; // 방어력 무시
        target.TakeDamage(finalDamage, caster.Accuracy, skill);
        ApplyStatusEffects(skill, target);
    }

    // 링케이드의 효과는 스킬을 작동 후 스킬장전상태가 되고 연계스킬이 장전된만큼 턴이 끝날때 한번에 공격 그런 과정서 연계 자체에 달린 계수만큼 피해증가
    //private void ApplyLinkageDamage(SkillData skill, CharacterStats caster, CharacterStats target)
    //{
    //    // 예시: 타겟에게 데미지, 추가로 인접 적에게도 데미지
    //    int damage = CalculateDamage(skill, caster, target);
    //    target.TakeDamage(damage, caster.Accuracy, skill);
    //    ApplyStatusEffects(skill, target);

    //    // 추가 타겟 처리 (예: 인접 적)
    //    var linkedTargets = FindLinkedTargets(target);
    //    foreach (var linked in linkedTargets)
    //    {
    //        linked.TakeDamage(damage / 2, caster.Accuracy, skill); // 예: 절반 데미지
    //        ApplyStatusEffects(skill, linked);
    //    }
    //}
}
