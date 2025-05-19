using UnityEngine;

public class SlotHandler : MonoBehaviour
{
    public CharacterStats currentCharacter;

    public void FindUnit()
    {
        // �ڽ� �߿��� CharacterStats ������Ʈ�� ���� ��ü Ž��
        currentCharacter = GetComponentInChildren<CharacterStats>();

        if (currentCharacter != null)
        {
            //Debug.Log($"[���� {name}] ĳ���� ���� ����: {currentCharacter.name}");
        }
        else
        {
            //Debug.LogWarning($"[���� {name}] ĳ���Ͱ� �ڽĿ� �������� ����!");
        }
    }
    public CharacterStats SlotCharacterLoad()
    {
        FindUnit();
        return currentCharacter;
    }
}
