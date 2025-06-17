using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 파티 세팅에서 플레이어블 캐릭터 한 명의 슬롯 역할을 담당.
/// 캐릭터 이미지와 데이터 참조를 보유.
/// </summary>
public class BattleSttingCharacterSlot : MonoBehaviour
{
    [SerializeField] private Image characterImage; // 캐릭터 이미지를 표시할 UI
    private CharacterData characterData;           // 슬롯에 할당된 캐릭터 데이터
    [SerializeField] private int isNumber; // 슬롯 번호 직접 기입 필요

    /// <summary>
    /// 슬롯에 캐릭터를 할당하고 이미지를 갱신한다.
    /// </summary>
    public void SetCharacter(CharacterData data, Sprite sprite)
    {
        characterData = data;
        characterImage.sprite = sprite;
    }

    /// <summary>
    /// 현재 슬롯에 할당된 캐릭터 데이터를 반환한다.
    /// </summary>
    public CharacterData GetCharacterData()
    {
        return characterData;
    }
}
