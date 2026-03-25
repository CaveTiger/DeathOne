using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 업그레이드 대상 선택용 버튼.
/// CharacterUpgradePannel이 이 컴포넌트를 가진 프리팹을 인스턴스화하고,
/// Initialize로 CharacterData와 패널을 연결합니다.
/// </summary>
public class UpgradeCharacterButton : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image icon;
    [SerializeField] private TextMeshProUGUI nameText;

    private CharacterData characterData;
    private CharacterUpgradePannel ownerPanel;

    /// <summary>
    /// 버튼을 초기화하고 UI를 설정합니다.
    /// </summary>
    public void Initialize(CharacterData data, CharacterUpgradePannel panel)
    {
        characterData = data;
        ownerPanel = panel;

        if (nameText != null)
        {
            nameText.text = data != null ? data.Label : "";
        }

        if (icon != null)
        {
            if (data != null && !string.IsNullOrEmpty(data.Sprite))
            {
                // CharacterData.Sprite는 폴더 경로를 기준으로 저장됨
                string standPath = $"{data.Sprite}/Stand";
                Sprite sprite = Resources.Load<Sprite>(standPath);
                if (sprite == null)
                {
                    // 과거 데이터 호환: 단일 스프라이트 경로일 수 있음
                    sprite = Resources.Load<Sprite>(data.Sprite);
                }
                icon.sprite = sprite;
                icon.gameObject.SetActive(sprite != null);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 버튼 클릭 시 호출 (Button.onClick에 연결).
    /// 선택된 캐릭터를 CharacterUpgradePannel에 전달합니다.
    /// </summary>
    public void OnClick()
    {
        if (characterData == null || ownerPanel == null)
        {
            Debug.LogWarning("[UpgradeCharacterButton] characterData 또는 ownerPanel이 설정되지 않았습니다.");
            return;
        }

        ownerPanel.SetTargetCharacter(characterData);
        ownerPanel.ClosePanelIfOpen();
    }
}

