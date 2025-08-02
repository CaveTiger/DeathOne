using UnityEngine;
using TMPro;

public class RewardUIManager : MonoBehaviour
{
    public static RewardUIManager Instance { get; private set; }

    // 1. 보상 데이터
    private BattleResultData currentResult;

    // 2. UI 오브젝트 연결 (Hierarchy 이름과 동일하게)
    [Header("슬롯 관련")] 
    public Transform SlotSet;
    public Transform Cslot1;
    public Transform Cslot2;
    public Transform Cslot3;
    public Transform Cslot4;
    public GameObject BackGround1;
    public GameObject BackGround2;
    public GameObject BackGround3;
    public GameObject BackGround4;

    [Header("슬롯 배경")] 
    public GameObject SlotBG1;
    public GameObject SlotBG2;
    public GameObject SlotBG3;
    public GameObject SlotBG4;

    [Header("캐릭터 출력")] 
    public Transform CharacterSlot1;
    public Transform CharacterSlot2;
    public Transform CharacterSlot3;
    public Transform CharacterSlot4;

    [Header("영혼 먼지")] 
    public TextMeshProUGUI SoulDustReward;
    public TextMeshProUGUI SoulDustRewardCount;

    [Header("보상 리스트(스크롤뷰)")]
    public Transform RwardBlockScrollView; // 스크롤뷰 전체
    public Transform Content; // Viewport/Content
    public GameObject characterRewardBlockPrefab;
    public GameObject skillRewardBlockPrefab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        // DontDestroyOnLoad(gameObject); // 월드맵 전용 매니저이므로 생략
    }

    /// <summary>
    /// 보상 데이터를 받아 UI를 갱신한다
    /// </summary>
    public void ShowReward(BattleResultData result)
    {
        currentResult = result;
        UpdateUI();
    }

    private void UpdateUI()
    {
        // 캐릭터 출력 슬롯 초기화
        Transform[] charSlots = { CharacterSlot1, CharacterSlot2, CharacterSlot3, CharacterSlot4 };
        for (int i = 0; i < charSlots.Length; i++)
        {
            if (currentResult != null && currentResult.unlockedCharacterIDs != null && i < currentResult.unlockedCharacterIDs.Count)
            {
                charSlots[i].gameObject.SetActive(true);
                string charID = currentResult.unlockedCharacterIDs[i];
                // 캐릭터 데이터에서 스프라이트 로드
                if (CharacterData.characterDict.TryGetValue(charID, out var characterData))
                {
                    string spritePath = $"UnitSprite/{characterData.ID}/Stand";
                    var sprite = Resources.Load<Sprite>(spritePath);
                    var image = charSlots[i].GetComponentInChildren<UnityEngine.UI.Image>();
                    if (image != null && sprite != null)
                        image.sprite = sprite;
                }
            }
            else
            {
                charSlots[i].gameObject.SetActive(false);
            }
        }

        // 1. 영혼먼지 텍스트 갱신
        if (SoulDustRewardCount != null)
            SoulDustRewardCount.text = currentResult != null ? currentResult.soulDustGained.ToString() : "0";

        // 2. 기존 보상 프리팹 삭제
        if (Content != null)
        {
            foreach (Transform child in Content)
                Destroy(child.gameObject);
        }

        // 3. 스킬 보상 프리팹 생성
        if (currentResult != null && currentResult.unlockedSkillIDs != null)
        {
            foreach (var skillID in currentResult.unlockedSkillIDs)
            {
                var obj = Instantiate(skillRewardBlockPrefab, Content);
                var rewardUI = obj.GetComponent<RewardItemUI>();
                if (rewardUI != null)
                    rewardUI.SetupRewardItem(skillID, "스킬");
            }
        }

        // 4. 캐릭터 보상 프리팹 생성
        if (currentResult != null && currentResult.unlockedCharacterIDs != null)
        {
            foreach (var charID in currentResult.unlockedCharacterIDs)
            {
                var obj = Instantiate(characterRewardBlockPrefab, Content);
                var rewardUI = obj.GetComponent<RewardItemUI>();
                if (rewardUI != null)
                    rewardUI.SetupRewardItem(charID, "캐릭터");
            }
        }
    }

    // (확장) 닫기, 애니메이션, 사운드 등 추가 가능
} 