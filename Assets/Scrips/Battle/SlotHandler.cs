using UnityEngine;

public class SlotHandler : MonoBehaviour
{
    public CharacterStats currentCharacter;

    public void FindUnit()
    {
        // 자식 중에서 CharacterStats 컴포넌트를 가진 객체 탐색
        currentCharacter = GetComponentInChildren<CharacterStats>();

        if (currentCharacter != null)
        {
            //Debug.Log($"[슬롯 {name}] 캐릭터 연결 성공: {currentCharacter.name}");
        }
        else
        {
            //Debug.LogWarning($"[슬롯 {name}] 캐릭터가 자식에 존재하지 않음!");
        }
    }
    public CharacterStats SlotCharacterLoad()
    {
        FindUnit();
        return currentCharacter;
    }
}
