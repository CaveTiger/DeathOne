using UnityEngine;
using UnityEngine.UI;
using System;

/// <summary>
/// 파티 세팅에서 플레이어블 캐릭터 한 명의 슬롯 역할을 담당.
/// 캐릭터 이미지와 데이터 참조를 보유.
/// </summary>
public class BattleSttingCharacterSlot : MonoBehaviour
{
    [Header("참조")]
    [SerializeField] private CharacterInventoryTab inventoryTab; // Inspector에서 연결

    [Header("UI")]
    [SerializeField] private Image characterImage; // 캐릭터 이미지를 표시할 UI
    [SerializeField] private int slotNumber;       // 슬롯 번호
    [SerializeField] private Image slotBackground; // 슬롯 배경 이미지 (드래그 앤 드롭 시각적 피드백용)

    private CharacterData characterData;           // 슬롯에 할당된 캐릭터 데이터
    private CharacterBlock currentCharacterBlock;  // 현재 슬롯에 있는 캐릭터 블록

    // 슬롯에 캐릭터가 배치되거나 제거될 때 호출될 이벤트
    public static event Action<BattleSttingCharacterSlot, CharacterData> OnSlotChanged;

    private void Awake()
    {
        if (slotBackground != null)
        {
            slotBackground.color = new Color(1f, 1f, 1f, 0.5f); // 기본 상태: 반투명
        }
    }

    /// <summary>
    /// 슬롯에 캐릭터를 할당하고 이미지를 갱신한다.
    /// </summary>
    public void SetCharacter(CharacterData data, Sprite sprite)
    {
        characterData = data;
        if (characterImage != null)
        {
            characterImage.sprite = sprite;
            characterImage.enabled = sprite != null;
        }
        OnSlotChanged?.Invoke(this, data);
    }

    /// <summary>
    /// 현재 슬롯에 할당된 캐릭터 데이터를 반환한다.
    /// </summary>
    public CharacterData GetCharacterData()
    {
        return characterData;
    }

    /// <summary>
    /// 슬롯에 캐릭터를 배치할 수 있는지 확인한다.
    /// </summary>
    public bool CanAcceptCharacter(CharacterData newCharacter)
    {
        // 이미 같은 캐릭터가 있는 경우
        if (characterData != null && characterData.Label == newCharacter.Label)
        {
            return false;
        }

        // TODO: 추가적인 제한 조건 구현 (예: 직업 제한, 레벨 제한 등)
        return true;
    }

    /// <summary>
    /// 슬롯에 캐릭터 블록을 배치한다.
    /// </summary>
    public void PlaceCharacterBlock(CharacterBlock block)
    {
        // 기존 캐릭터 블록이 있다면 인벤토리로 복귀
        if (currentCharacterBlock != null)
        {
            inventoryTab.ReturnCharacterBlock(currentCharacterBlock);
        }

        // 새 블록의 데이터를 기반으로 슬롯의 캐릭터와 이미지를 설정
        SetCharacter(block.characterData, block.characterImage.sprite);

        // 새 블록을 현재 슬롯의 블록으로 참조하고, 부모로 설정한 뒤 비활성화하여 숨김
        currentCharacterBlock = block;
        currentCharacterBlock.transform.SetParent(this.transform);
        currentCharacterBlock.gameObject.SetActive(false);

        // 시각적 상태 최종 복구
        block.ResetVisuals();

        if (slotBackground != null)
        {
            slotBackground.color = new Color(1f, 1f, 1f, 0.8f); // 캐릭터 배치 시: 더 불투명하게
        }
    }

    /// <summary>
    /// 현재 슬롯에서 캐릭터를 제거한다.
    /// </summary>
    public void RemoveCurrentCharacter()
    {
        if (currentCharacterBlock != null)
        {
            Destroy(currentCharacterBlock.gameObject);
            currentCharacterBlock = null;
        }

        characterData = null;
        if (characterImage != null)
        {
            characterImage.sprite = null;
            characterImage.enabled = false;
        }

        if (slotBackground != null)
        {
            slotBackground.color = new Color(1f, 1f, 1f, 0.5f); // 캐릭터 제거 시: 다시 반투명하게
        }

        OnSlotChanged?.Invoke(this, null);
    }

    /// <summary>
    /// 슬롯 번호를 반환한다.
    /// </summary>
    public int GetSlotNumber()
    {
        return slotNumber;
    }
}
