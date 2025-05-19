using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/Poison")]
public class StatusEffectPoisonData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "020002";
        effectType = StatusEffectType.ContinuousDamage;
        effectName = "�ߵ�";
        description = "2�ϸ��� ���ظ� �Խ��ϴ�.";
        iconPath = "StatusEffect/Poison";
    }
    // �ߵ� ȿ��: 2�ϸ��� �� ���� ���ظ� ����
    public override void OnSpecialEffect(CharacterStats target, StatusEffectInstance instance)
    {
        // ���Ӽ�ġ�� Ȧ���� ���� ���� ����
        if (instance.remainingTurns % 2 == 1)
        {
            target.Hp -= instance.value;
            Debug.Log($"�ߵ� ����: {instance.value} (������: {instance.remainingTurns})");
        }
    }
}
