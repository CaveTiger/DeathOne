using UnityEngine;
 

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    void Awake() { Instance = this; }

    [SerializeField] GameObject CharacterUnit;
    public void UseSkill(SkillData skill, CharacterStats caster, CharacterStats target)
    {

        if (!skill.IsUsable()) return;

        // 쿨타임 등록
        skill.currentCooldown = skill.cooldown;

        // 스킬 종류에 따라 실행 분기
        switch (skill.Type)
        {
            case SkillType.Damage:
                target.TakeDamage(CalculateDamage(skill, caster, target), caster.Accuracy);
                break;
            case SkillType.Heal:
                target.Heal(skill.healAmount);
                break;
            case SkillType.linkage:
                target.TakeDamage(CalculateDamage(skill, caster, target), caster.Accuracy);//공격이 들어가기 전 연계 공격을 예약해두었다면 피해가 예약 순서대로 들어감
                break;
            case SkillType.Piercing:
                target.TakeDamage(CalculateDamage(skill, caster, target), caster.Accuracy);//방어력을 무시하는 공격
                break;
            default:
                Debug.Log("어느타입도 아닌 공격입니다.");
                break;
        }
    }
    private int CalculateDamage(SkillData skill, CharacterStats caster, CharacterStats target)
    {
        // 1. DamageMin ~ DamageMax 사이에서 랜덤 피해값 추출
        int baseDamage = UnityEngine.Random.Range(skill.DamageMin, skill.DamageMax + 1);

        // 2. 공격자 ATK vs 대상 DEF 비교
        int atk = caster.Atk;
        int def = target.Def;

        // 3. ATK > DEF 일 경우, 차이만큼 10%씩 증가 (예: atk 20, def 10 => +10% x 10 = +100%)
        //    DEF > ATK 일 경우, 차이만큼 10%씩 감소 (최소 피해량 보장: 10)
        int difference = atk - def;
        float multiplier = 1f + (difference * 0.1f);
        multiplier = Mathf.Max(multiplier, 0.1f); // 최소 피해량 보장을 위해 multiplier 하한선 설정

        // 4. 계산된 피해량 적용
        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

        // 5. 치명타, 명중률, 회피율 등은 이후에 별도 로직에서 판정
        // 6. 이후 takeDamage에서 최종 적용 및 특수효과 반영

        return finalDamage;
    }
}
