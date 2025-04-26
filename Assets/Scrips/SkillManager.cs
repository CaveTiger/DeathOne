using UnityEngine;
 

public class SkillManager : MonoBehaviour
{
    public static SkillManager Instance { get; private set; }

    void Awake() { Instance = this; }

    [SerializeField] GameObject CharacterUnit;
    public void UseSkill(SkillData skill, CharacterStats caster, CharacterStats target)
    {

        if (!skill.IsUsable()) return;

        // ��Ÿ�� ���
        skill.currentCooldown = skill.cooldown;

        // ��ų ������ ���� ���� �б�
        switch (skill.Type)
        {
            case SkillType.Damage:
                target.TakeDamage(CalculateDamage(skill, caster, target), caster.Accuracy);
                break;
            case SkillType.Heal:
                target.Heal(skill.healAmount);
                break;
            case SkillType.linkage:
                target.TakeDamage(CalculateDamage(skill, caster, target), caster.Accuracy);//������ ���� �� ���� ������ �����صξ��ٸ� ���ذ� ���� ������� ��
                break;
            case SkillType.Piercing:
                target.TakeDamage(CalculateDamage(skill, caster, target), caster.Accuracy);//������ �����ϴ� ����
                break;
            default:
                Debug.Log("���Ÿ�Ե� �ƴ� �����Դϴ�.");
                break;
        }
    }
    private int CalculateDamage(SkillData skill, CharacterStats caster, CharacterStats target)
    {
        // 1. DamageMin ~ DamageMax ���̿��� ���� ���ذ� ����
        int baseDamage = UnityEngine.Random.Range(skill.DamageMin, skill.DamageMax + 1);

        // 2. ������ ATK vs ��� DEF ��
        int atk = caster.Atk;
        int def = target.Def;

        // 3. ATK > DEF �� ���, ���̸�ŭ 10%�� ���� (��: atk 20, def 10 => +10% x 10 = +100%)
        //    DEF > ATK �� ���, ���̸�ŭ 10%�� ���� (�ּ� ���ط� ����: 10)
        int difference = atk - def;
        float multiplier = 1f + (difference * 0.1f);
        multiplier = Mathf.Max(multiplier, 0.1f); // �ּ� ���ط� ������ ���� multiplier ���Ѽ� ����

        // 4. ���� ���ط� ����
        int finalDamage = Mathf.RoundToInt(baseDamage * multiplier);

        // 5. ġ��Ÿ, ���߷�, ȸ���� ���� ���Ŀ� ���� �������� ����
        // 6. ���� takeDamage���� ���� ���� �� Ư��ȿ�� �ݿ�

        return finalDamage;
    }
}
