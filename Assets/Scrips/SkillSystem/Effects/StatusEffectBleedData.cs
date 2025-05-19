using UnityEngine;

// ���� ȿ��
[CreateAssetMenu(menuName = "Scriptable Objects/StatusEffect/Bleed")]
public class StatusEffectBleedData : StatusEffectData
{
    private void OnEnable()
    {
        EffectID = "020001";
        effectType = StatusEffectType.ContinuousDamage;
        effectName = "����";
        description = "�� �ϸ��� ���ظ� �Խ��ϴ�.";
        iconPath = "StatusEffect/Bleed";
    }
}

