using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 전술 축복의 대상 선택 UI
/// - 파티 멤버(주인공 포함)를 선택할 수 있는 UI 패널
/// - 전투 중이 아닐 때만 사용 가능 (월드맵/스테이지 씬)
/// </summary>
public class TacticalBlessingTargetSelector : MonoBehaviour
{
    public static TacticalBlessingTargetSelector Instance { get; private set; }

    [Header("UI 참조")]
    [SerializeField] private GameObject panelObject; // 전체 패널 오브젝트
    [SerializeField] private TextMeshProUGUI titleText; // 제목 텍스트 (예: "대상 선택")
    [SerializeField] private Transform buttonParent; // 버튼들이 생성될 부모 Transform
    [SerializeField] private GameObject characterButtonPrefab; // 캐릭터 버튼 프리팹

    [Header("설정")]
    [Tooltip("전술 축복 타입에 따른 제목 텍스트")]
    [SerializeField] private string titleTextOneForAll = "하나를 위한 모두 - 대상 선택";
    [SerializeField] private string titleTextAllForOne = "모두를 위한 하나 - 대상 선택";
    [SerializeField] private string titleTextAuthorityDelegation = "권한대행 - 대상 선택";

    // 현재 선택 대기 중인 축복 데이터
    private BlessingData currentBlessingData;
    private System.Action<CharacterStats> onTargetSelectedCallback;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // 초기에는 패널 숨김
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }
    }

    /// <summary>
    /// 전술 축복 대상 선택 UI를 표시합니다.
    /// </summary>
    /// <param name="blessingData">선택된 전술 축복 데이터</param>
    /// <param name="onSelected">대상 선택 완료 시 호출될 콜백</param>
    public void ShowTargetSelector(BlessingData blessingData, System.Action<CharacterStats> onSelected)
    {
        if (blessingData == null)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] 축복 데이터가 null입니다.");
            return;
        }

        // 전투 중이면 사용 불가
        if (BattleManager.Instance != null)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] 전투 중에는 대상 선택을 할 수 없습니다.");
            return;
        }

        currentBlessingData = blessingData;
        onTargetSelectedCallback = onSelected;

        // 제목 텍스트 설정
        SetTitleText(blessingData);

        // 파티 멤버 버튼 생성
        CreatePartyMemberButtons();

        // 패널 표시
        if (panelObject != null)
        {
            panelObject.SetActive(true);
        }
    }

    /// <summary>
    /// 축복 타입에 따라 제목 텍스트를 설정합니다.
    /// </summary>
    private void SetTitleText(BlessingData blessingData)
    {
        if (titleText == null) return;

        // scriptClass로 축복 타입 판단
        string scriptClass = blessingData.scriptClass ?? "";
        
        if (scriptClass.Contains("OneForAll"))
        {
            titleText.text = titleTextOneForAll;
        }
        else if (scriptClass.Contains("AllForOne"))
        {
            titleText.text = titleTextAllForOne;
        }
        else if (scriptClass.Contains("AuthorityDelegation"))
        {
            titleText.text = titleTextAuthorityDelegation;
        }
        else
        {
            titleText.text = "대상 선택";
        }
    }

    /// <summary>
    /// 파티 멤버 선택 버튼들을 생성합니다.
    /// </summary>
    private void CreatePartyMemberButtons()
    {
        if (buttonParent == null)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] buttonParent가 설정되지 않았습니다.");
            return;
        }

        // 기존 버튼들 제거
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        // 파티 멤버 목록 가져오기 (SpawnManager에서)
        List<CharacterData> partyMembers = GetPartyMembers();
        
        if (partyMembers == null || partyMembers.Count == 0)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] 파티 멤버를 찾을 수 없습니다.");
            return;
        }

        // 각 파티 멤버에 대해 버튼 생성
        foreach (var memberData in partyMembers)
        {
            if (memberData == null) continue;

            CreateCharacterButton(memberData);
        }
    }

    /// <summary>
    /// BattleSettingManager의 슬롯에서 현재 파티 멤버 목록을 가져옵니다.
    /// 세팅 단계(TestStage 씬)에서만 사용됩니다.
    /// </summary>
    private List<CharacterData> GetPartyMembers()
    {
        // BattleSettingManager의 슬롯을 직접 사용 (세팅 단계 기준)
        if (BattleSettingManager.Instance == null)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] BattleSettingManager를 찾을 수 없습니다.");
            return new List<CharacterData>();
        }

        List<CharacterData> partyMembers = new List<CharacterData>();

        // slot1, slot2, slot3, slot4에서 캐릭터 데이터 가져오기
        var slot1 = BattleSettingManager.Instance.slot1;
        var slot2 = BattleSettingManager.Instance.slot2;
        var slot3 = BattleSettingManager.Instance.slot3;
        var slot4 = BattleSettingManager.Instance.slot4;

        if (slot1 != null && slot1.GetCharacterData() != null)
        {
            partyMembers.Add(slot1.GetCharacterData());
        }
        if (slot2 != null && slot2.GetCharacterData() != null)
        {
            partyMembers.Add(slot2.GetCharacterData());
        }
        if (slot3 != null && slot3.GetCharacterData() != null)
        {
            partyMembers.Add(slot3.GetCharacterData());
        }
        if (slot4 != null && slot4.GetCharacterData() != null)
        {
            partyMembers.Add(slot4.GetCharacterData());
        }

        if (partyMembers.Count == 0)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] 파티 슬롯에 캐릭터가 배치되지 않았습니다.");
        }

        return partyMembers;
    }

    /// <summary>
    /// 캐릭터 선택 버튼을 생성합니다.
    /// </summary>
    private void CreateCharacterButton(CharacterData characterData)
    {
        if (characterButtonPrefab == null)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] characterButtonPrefab이 설정되지 않았습니다.");
            return;
        }

        GameObject buttonObj = Instantiate(characterButtonPrefab, buttonParent);
        buttonObj.name = $"Button_{characterData.ID}";

        // 버튼 클릭 이벤트 연결
        Button button = buttonObj.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.AddListener(() => OnCharacterButtonClicked(characterData));
        }

        // 캐릭터 정보 표시 (이름, 스프라이트 등)
        UpdateCharacterButtonUI(buttonObj, characterData);
    }

    /// <summary>
    /// 캐릭터 버튼의 UI를 업데이트합니다.
    /// </summary>
    private void UpdateCharacterButtonUI(GameObject buttonObj, CharacterData characterData)
    {
        // 이름 텍스트 찾기 및 설정
        TextMeshProUGUI nameText = buttonObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null)
        {
            nameText.text = characterData.Label;
        }

        // 스프라이트 이미지 설정
        Image image = buttonObj.GetComponentInChildren<Image>();
        if (image != null && !string.IsNullOrEmpty(characterData.Sprite))
        {
            string spritePath = $"{characterData.Sprite}/Stand";
            Sprite sprite = Resources.Load<Sprite>(spritePath);
            if (sprite != null)
            {
                image.sprite = sprite;
            }
        }
    }

    /// <summary>
    /// 캐릭터 버튼 클릭 시 호출됩니다.
    /// </summary>
    private void OnCharacterButtonClicked(CharacterData characterData)
    {
        if (characterData == null)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] 캐릭터 데이터가 null입니다.");
            return;
        }

        if (currentBlessingData == null)
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] 축복 데이터가 null입니다.");
            return;
        }

        // BattleSettingManager의 슬롯에서 해당 캐릭터가 몇 번 슬롯인지 찾기
        int slotNumber = GetSlotNumberByCharacterData(characterData);
        if (slotNumber == 0)
        {
            Debug.LogWarning($"[TacticalBlessingTargetSelector] 캐릭터 '{characterData.Label}'를 슬롯에서 찾을 수 없습니다.");
            return;
        }

        // BlessingManager에 슬롯 번호 저장
        if (BlessingManager.Instance != null)
        {
            BlessingManager.Instance.SetTacticalBlessingTarget(currentBlessingData.blessingID, slotNumber);
            Debug.Log($"[TacticalBlessingTargetSelector] 전술 축복 대상 설정: {currentBlessingData.blessingName} -> 슬롯 {slotNumber} ({characterData.Label})");
        }
        else
        {
            Debug.LogWarning("[TacticalBlessingTargetSelector] BlessingManager를 찾을 수 없습니다.");
        }

        // 콜백 호출 (전투 중이 아니므로 CharacterStats는 null)
        if (onTargetSelectedCallback != null)
        {
            onTargetSelectedCallback(null);
        }

        // 패널 숨김
        HideTargetSelector();
    }

    /// <summary>
    /// CharacterData를 기반으로 BattleSettingManager의 슬롯 번호를 찾습니다.
    /// </summary>
    /// <param name="characterData">찾을 캐릭터 데이터</param>
    /// <returns>슬롯 번호 (1~4), 찾지 못하면 0)</returns>
    private int GetSlotNumberByCharacterData(CharacterData characterData)
    {
        if (characterData == null || BattleSettingManager.Instance == null)
        {
            return 0;
        }

        // slot1, slot2, slot3, slot4를 순회하며 해당 캐릭터 찾기
        var slot1 = BattleSettingManager.Instance.slot1;
        var slot2 = BattleSettingManager.Instance.slot2;
        var slot3 = BattleSettingManager.Instance.slot3;
        var slot4 = BattleSettingManager.Instance.slot4;

        if (slot1 != null && slot1.GetCharacterData() != null && slot1.GetCharacterData().ID == characterData.ID)
        {
            return 1;
        }
        if (slot2 != null && slot2.GetCharacterData() != null && slot2.GetCharacterData().ID == characterData.ID)
        {
            return 2;
        }
        if (slot3 != null && slot3.GetCharacterData() != null && slot3.GetCharacterData().ID == characterData.ID)
        {
            return 3;
        }
        if (slot4 != null && slot4.GetCharacterData() != null && slot4.GetCharacterData().ID == characterData.ID)
        {
            return 4;
        }

        return 0;
    }

    /// <summary>
    /// 대상 선택 UI를 숨깁니다.
    /// </summary>
    public void HideTargetSelector()
    {
        if (panelObject != null)
        {
            panelObject.SetActive(false);
        }

        currentBlessingData = null;
        onTargetSelectedCallback = null;
    }

    /// <summary>
    /// 취소 버튼 클릭 시 호출됩니다.
    /// </summary>
    public void OnCancelButtonClicked()
    {
        HideTargetSelector();
    }
}


