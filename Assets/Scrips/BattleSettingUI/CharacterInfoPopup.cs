using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfoPopup : MonoBehaviour
{
    [Header("캐릭터 정보 UI")]
    [SerializeField] private Image characterImage;
    [SerializeField] private TextMeshProUGUI characterNameText;
    [SerializeField] private TextMeshProUGUI hpText;
    [SerializeField] private TextMeshProUGUI atkText;
    [SerializeField] private TextMeshProUGUI defText;
    [SerializeField] private TextMeshProUGUI evasionText;
    [SerializeField] private TextMeshProUGUI accuracyText;

    [Header("패시브 정보")]
    [SerializeField] private CharacterInfoPassiveBlock passiveBlock;

    private CharacterData currentCharacterData;

    private void OnEnable()
    {
        if (currentCharacterData != null)
        {
            DisplayCharacterInfo(currentCharacterData);
        }
    }

    /// <summary>
    /// 외부에서 캐릭터 데이터를 받아 정보를 표시합니다.
    /// </summary>
    /// <param name="characterData">표시할 캐릭터 데이터</param>
    public void SetCharacterData(CharacterData characterData)
    {
        currentCharacterData = characterData;
        if (gameObject.activeSelf)
        {
            DisplayCharacterInfo(characterData);
        }
    }

    /// <summary>
    /// 받은 캐릭터 데이터로 UI를 업데이트합니다.
    /// </summary>
    private void DisplayCharacterInfo(CharacterData characterData)
    {
        if (characterData == null) return;

        // 기본 정보 업데이트
        characterNameText.text = characterData.Label;
        hpText.text = $"HP: {characterData.Hp}/{characterData.MaxHp}";
        atkText.text = $"ATK: {characterData.Atk}";
        defText.text = $"DEF: {characterData.Def}";
        evasionText.text = $"회피: {characterData.EvasionRate}%";
        accuracyText.text = $"명중: {characterData.Accuracy}%";

        // 캐릭터 이미지 업데이트
        if (characterImage != null)
        {
            string spritePath = $"UnitSprite/{characterData.Label}/Stand";
            Sprite characterSprite = Resources.Load<Sprite>(spritePath);
            if (characterSprite != null)
            {
                characterImage.sprite = characterSprite;
            }
        }

        // 패시브 정보 업데이트
        if (characterData.Passives != null && characterData.Passives.Count > 0)
        {
            // TODO: 패시브 정보 파싱 및 표시 로직 구현
            // 현재는 첫 번째 패시브만 표시
            string passiveName = characterData.Passives[0];
            string passiveDescription = "패시브 설명"; // TODO: 실제 패시브 설명 데이터 연동
            passiveBlock.SetPassiveInfo(passiveName, passiveDescription);
        }
    }
}
