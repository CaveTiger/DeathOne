using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    void Awake() { Instance = this; }

    [SerializeField] GameObject CharacterUnit;
    public CharacterInfoPlayer playerInfoUI; // 인스펙터에서 PlayerInfo 오브젝트 할당

    public void UseSkill(SkillData skill, CharacterStats caster, CharacterStats target, SkillData motionData)
    {
        if (!skill.IsUsable()) return;
        skill.currentCooldown = skill.cooldown;

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

    private IEnumerator PlaySkillEffect(SkillData skill, CharacterStats caster, CharacterStats target, SkillData motionData)
    {
        // 1. 초기 대기
        yield return new WaitForSeconds(0.7f);
        BattleUIManager.Instance.ChangeUIBattle();
        // 2. 카메라 줌인
        yield return StartCoroutine(BattleCamera.Instance.CameraZoom(2.4f, 0.4f));

        var casterMotion = caster.GetComponent<CharacterMotionController>();
        if (casterMotion == null)
            casterMotion = caster.GetComponentInChildren<CharacterMotionController>();

        var targetMotion = target.GetComponent<CharacterMotionController>();
        if (targetMotion == null)
            targetMotion = target.GetComponentInChildren<CharacterMotionController>();

        // 3. 피해/효과 처리
        switch (skill.Type)
        {
            case SkillType.Damage:
            case SkillType.Piercing:
                int damage = CalculateDamage(skill, caster, target);
                target.TakeDamage(damage, caster.Accuracy, skill);
                ApplyStatusEffects(skill, target);
                break;
            case SkillType.Buff:
                ApplyBuffEffects(skill, caster, target);
                ApplyStatusEffects(skill, target);
                break;
            case SkillType.Heal:
                target.Heal(skill.healAmount);
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

        

        // 4. 캐릭터 이동
        if (casterMotion != null)
            casterMotion.MoveToBattlePosition();
        if (targetMotion != null)
            targetMotion.MoveToBattlePosition();

            yield return new WaitForSeconds(0.5f);

        if (casterMotion != null)
            casterMotion.PlaySkillMotion(motionData.Motion);
        if (targetMotion != null)
            targetMotion.PlayHitMotion();

        yield return new WaitForSeconds(0.7f);

        // 5. 모션 리셋
        if (casterMotion != null)
            casterMotion.ResetMotion();
        if (targetMotion != null)
            targetMotion.ResetMotion();
        if (casterMotion != null)
            casterMotion.ResetPosition();
        if (targetMotion != null)
            targetMotion.ResetPosition();

        // 6. 카메라 줌아웃
        yield return StartCoroutine(BattleCamera.Instance.CameraZoom(5f, 0.3f));

        BattleUIManager.Instance.ChangeUINormal();

        if (caster.IsPlayer)
            TurnManager.Instance.EndTurn();
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
