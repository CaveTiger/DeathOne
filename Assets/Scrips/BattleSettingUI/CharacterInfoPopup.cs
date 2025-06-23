using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

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
    [SerializeField] private Transform passiveContainer; // 패시브 블록들을 담을 컨테이너

    private CharacterData currentCharacterData;
    private List<CharacterInfoPassiveBlock> passiveBlocks = new List<CharacterInfoPassiveBlock>();

    private void OnEnable()
    {
        if (currentCharacterData != null)
        {
            DisplayCharacterInfo(currentCharacterData);
        }
    }

    private void OnDisable()
    {
        ClearPassiveBlocks();
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

        try
        {
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
                else
                {
                    Debug.LogWarning($"캐릭터 스프라이트를 찾을 수 없습니다: {spritePath}");
                    characterImage.sprite = null;
                }
            }

            // 패시브 정보 업데이트
            UpdatePassiveInfo(characterData);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"캐릭터 정보 표시 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 패시브 정보를 업데이트합니다.
    /// </summary>
    private void UpdatePassiveInfo(CharacterData characterData)
    {
        ClearPassiveBlocks();

        if (characterData.Passives == null || characterData.Passives.Count == 0)
        {
            if (passiveBlock != null)
            {
                passiveBlock.SetPassiveInfo("패시브 없음", "이 캐릭터는 패시브가 없습니다.");
            }
            return;
        }

        foreach (string passiveId in characterData.Passives)
        {
            try
            {
                // 패시브 데이터 로드
                string passivePath = $"Data/Passive/{passiveId}";
                TextAsset passiveXml = Resources.Load<TextAsset>(passivePath);
                
                if (passiveXml != null)
                {
                    // XML 파싱 및 패시브 정보 추출
                    // TODO: 실제 XML 파싱 로직 구현
                    string passiveName = $"패시브 {passiveId}";
                    string passiveDescription = "패시브 설명"; // 실제 파싱된 설명으로 대체

                    // 패시브 블록 생성 및 정보 설정
                    CharacterInfoPassiveBlock newPassiveBlock = Instantiate(passiveBlock, passiveContainer);
                    newPassiveBlock.SetPassiveInfo(passiveName, passiveDescription);
                    passiveBlocks.Add(newPassiveBlock);
                }
                else
                {
                    Debug.LogWarning($"패시브 데이터를 찾을 수 없습니다: {passivePath}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"패시브 정보 처리 중 오류 발생: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 생성된 패시브 블록들을 정리합니다.
    /// </summary>
    private void ClearPassiveBlocks()
    {
        foreach (var block in passiveBlocks)
        {
            if (block != null)
            {
                Destroy(block.gameObject);
            }
        }
        passiveBlocks.Clear();
    }
}
