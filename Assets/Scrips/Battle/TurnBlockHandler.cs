using UnityEngine;
using UnityEngine.UI;

public class TurnBlockHandler : MonoBehaviour
{
    public Text characterNameText; // UI에 표시할 이름 텍스트

    public void SetTurn(CharacterStats character)
    {
        //characterNameText.text = character.Label; // 캐릭터 이름 가져와서 표시
    }
}
