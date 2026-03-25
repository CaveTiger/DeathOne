using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 캐릭터 업그레이드 패널의 메인 컨트롤러.
/// - 좌측 캐릭터 리스트 패널(그리드/스크롤뷰) 관리
/// - 선택된 캐릭터를 UpgradeStat 들과 현재 대상 표시 UI에 연결
/// </summary>
public class CharacterUpgradePannel : MonoBehaviour
{
    [Header("패널 열기/닫기")]
    [Tooltip("슬라이드 열기/닫기를 담당할 패널의 RectTransform (지정하지 않으면 이 컴포넌트가 붙은 오브젝트의 RectTransform을 사용)")]
    [SerializeField] private RectTransform panelRectTransform;

    [Tooltip("좌측 캐릭터 리스트 패널의 부모 RectTransform (CharacterListPanel 같은 오브젝트). 이 값을 지정하면 이 패널만 슬라이드됩니다.")]
    [SerializeField] private RectTransform characterListPanelRectTransform;

    [Tooltip("패널이 닫혔을 때의 anchoredPosition.x")]
    [SerializeField] private float closedPositionX = -260f;

    [Tooltip("패널이 열렸을 때의 anchoredPosition.x")]
    [SerializeField] private float openPositionX = 0f;

    [Tooltip("현재 보유 영혼먼지를 표시할 텍스트 (선택 사항)")]
    [SerializeField] private TextMeshProUGUI soulDustText;

    [Header("캐릭터 리스트 패널")]
    [Tooltip("좌측 캐릭터 리스트 패널의 Content(그리드 레이아웃이 붙은 Transform)")]
    [SerializeField] private Transform characterListContent;

    [Tooltip("캐릭터 선택 버튼 프리팹 (그리드에 배치할 버튼)")]
    [SerializeField] private GameObject characterButtonPrefab;

    [Header("현재 선택된 캐릭터 표시")]
    [SerializeField] private Image currentTargetIcon;
    [SerializeField] private TextMeshProUGUI currentTargetName;

    [Header("스탯 업그레이드 컨트롤러들")]
    [SerializeField] private UpgradeStat[] upgradeStatControllers;

    private readonly List<UpgradeCharacterButton> createdButtons = new();
    private CharacterData currentTargetCharacter;
    private bool isOpen = false;

    private void Awake()
    {
        if (panelRectTransform == null)
        {
            panelRectTransform = GetComponent<RectTransform>();
        }
    }

    private void OnEnable()
    {
        RefreshCharacterList();
        RestoreSelectedTargetIfPossible();

        // 패널이 켜질 때 영혼먼지 표시 업데이트
        UpdateSoulDustDisplay();

        // 씬 시작 시 이미 열린 상태로 존재할 수 있으므로,
        // 현재 패널 위치를 기준으로 월드맵 입력 차단 상태를 초기화한다.
        RectTransform targetRect = characterListPanelRectTransform != null
            ? characterListPanelRectTransform
            : panelRectTransform;

        if (targetRect != null)
        {
            float currentX = targetRect.anchoredPosition.x;
            float threshold = (closedPositionX + openPositionX) * 0.5f;
            isOpen = currentX >= threshold;
            UpdateWorldMapInputLock();
        }

        // 패널이 켜질 때 기존 선택 대상이 있으면 다시 UI에 반영
        if (currentTargetCharacter != null && currentTargetCharacter.IsUnlocked)
        {
            ApplyTargetToUpgradeStats(currentTargetCharacter);
            UpdateCurrentTargetDisplay(currentTargetCharacter);
        }
        else if (currentTargetCharacter != null && !currentTargetCharacter.IsUnlocked)
        {
            ClearCurrentTargetSelection();
        }
    }

    private void OnDisable()
    {
        // 오브젝트 전체가 꺼질 때(허브에서 SetActive false 등) 월드맵 UI 잠금이 남지 않도록
        try
        {
            WorldMapStageSelection.SetUIOpen(false);
        }
        catch
        {
            // 월드맵이 없는 씬에서는 무시
        }
    }

    /// <summary>
    /// 패널 열기/닫기 토글 (버튼에서 호출).
    /// </summary>
    public void TogglePanel()
    {
        isOpen = !isOpen;

        RectTransform targetRect = characterListPanelRectTransform != null
            ? characterListPanelRectTransform
            : panelRectTransform;

        if (targetRect != null)
        {
            Vector2 pos = targetRect.anchoredPosition;
            pos.x = isOpen ? openPositionX : closedPositionX;
            targetRect.anchoredPosition = pos;
        }

        if (isOpen)
        {
            UpdateSoulDustDisplay();
        }

        UpdateWorldMapInputLock();
    }

    /// <summary>
    /// 캐릭터 리스트 패널이 열려 있으면 닫습니다. (선택 완료 후 호출용)
    /// </summary>
    public void ClosePanelIfOpen()
    {
        if (!isOpen) return;
        TogglePanel();
    }

    /// <summary>
    /// 월드맵 스테이지 선택 입력 잠금/해제를 갱신합니다.
    /// </summary>
    private void UpdateWorldMapInputLock()
    {
        // WorldMapStageSelection은 다른 스크립트에서 정의된 정적 헬퍼라고 가정
        // 존재하지 않는 씬에서도 컴파일은 되므로 런타임 에러만 없도록 호출.
        try
        {
            WorldMapStageSelection.SetUIOpen(isOpen);
        }
        catch
        {
            // 월드맵이 없는 씬에서는 조용히 무시
        }
    }

    /// <summary>
    /// GameProgressManager에서 현재 영혼먼지 보유량을 읽어와 UI에 표시합니다.
    /// </summary>
    public void UpdateSoulDustDisplay()
    {
        if (soulDustText == null) return;

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[CharacterUpgradePannel] GameProgressManager.Instance가 null입니다.");
            return;
        }

        int currentSoulDust = GameProgressManager.Instance.GetSoulDust();
        soulDustText.text = currentSoulDust.ToString();
    }

    /// <summary>
    /// GameProgressManager의 인벤토리를 기준으로 캐릭터 리스트 버튼을 다시 생성합니다.
    /// </summary>
    public void RefreshCharacterList()
    {
        if (characterListContent == null || characterButtonPrefab == null)
        {
            Debug.LogWarning("[CharacterUpgradePannel] characterListContent 또는 characterButtonPrefab이 설정되지 않았습니다.");
            return;
        }

        // 기존 버튼 정리
        foreach (var btn in createdButtons)
        {
            if (btn != null)
            {
                Destroy(btn.gameObject);
            }
        }
        createdButtons.Clear();

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[CharacterUpgradePannel] GameProgressManager.Instance가 null입니다.");
            return;
        }

        List<CharacterData> inventory = GameProgressManager.Instance.CharacterInventory;
        if (inventory == null || inventory.Count == 0)
        {
            Debug.Log("[CharacterUpgradePannel] 인벤토리에 캐릭터가 없습니다.");
            return;
        }

        foreach (var character in inventory)
        {
            // 잠금 상태이거나 null이면 스킵
            if (character == null || !character.IsUnlocked) continue;

            GameObject go = Instantiate(characterButtonPrefab, characterListContent);
            var button = go.GetComponent<UpgradeCharacterButton>();
            if (button == null)
            {
                Debug.LogError("[CharacterUpgradePannel] characterButtonPrefab에 UpgradeCharacterButton 컴포넌트가 없습니다.");
                Destroy(go);
                continue;
            }

            button.Initialize(character, this);
            createdButtons.Add(button);
        }
    }

    /// <summary>
    /// 외부(캐릭터 선택 버튼)에서 호출: 업그레이드 대상 캐릭터를 변경합니다.
    /// </summary>
    public void SetTargetCharacter(CharacterData character)
    {
        if (character == null || !character.IsUnlocked)
        {
            Debug.LogWarning("[CharacterUpgradePannel] SetTargetCharacter 호출 시 character가 null이거나 사용불가입니다.");
            return;
        }

        currentTargetCharacter = character;
        ApplyTargetToUpgradeStats(character);
        UpdateCurrentTargetDisplay(character);

        if (GameProgressManager.Instance != null)
            GameProgressManager.Instance.SetLastSelectedUpgradeCharacterId(character.ID);
    }

    /// <summary>
    /// 사용불가 캐릭터가 현재 선택 대상이었다면 업그레이드 대상 표시를 null로 비웁니다.
    /// </summary>
    public void ClearSelectionIfUnavailable(string characterId)
    {
        if (currentTargetCharacter == null) return;
        if (string.IsNullOrEmpty(characterId)) return;
        if (currentTargetCharacter.ID != characterId) return;
        ClearCurrentTargetSelection();
    }

    /// <summary>
    /// 저장된 마지막 선택 캐릭터가 있으면 복원하고, 없으면 현재 값/첫 번째 유효 캐릭터로 대상을 맞춥니다.
    /// </summary>
    private void RestoreSelectedTargetIfPossible()
    {
        if (GameProgressManager.Instance == null) return;

        List<CharacterData> inventory = GameProgressManager.Instance.CharacterInventory;
        if (inventory == null || inventory.Count == 0) return;

        // 1) 저장된 선택 ID 우선
        string savedId = GameProgressManager.Instance.GetLastSelectedUpgradeCharacterId();
        if (!string.IsNullOrEmpty(savedId))
        {
            CharacterData savedTarget = inventory.Find(c => c != null && c.IsUnlocked && c.ID == savedId);
            if (savedTarget != null)
            {
                SetTargetCharacter(savedTarget);
                return;
            }
            // 저장된 대상이 사용불가/삭제된 경우: 표시를 비워 null 상태 유지
            GameProgressManager.Instance.SetLastSelectedUpgradeCharacterId("", true);
            ClearCurrentTargetSelection();
            return;
        }

        // 2) 메모리에 남아있는 기존 선택이 아직 유효하면 유지
        if (currentTargetCharacter != null && currentTargetCharacter.IsUnlocked)
        {
            SetTargetCharacter(currentTargetCharacter);
            return;
        }

        // 3) 폴백: 첫 번째 해금 캐릭터 자동 선택
        CharacterData firstUnlocked = inventory.Find(c => c != null && c.IsUnlocked);
        if (firstUnlocked != null)
            SetTargetCharacter(firstUnlocked);
    }

    private void ClearCurrentTargetSelection()
    {
        currentTargetCharacter = null;
        if (upgradeStatControllers != null)
        {
            foreach (var controller in upgradeStatControllers)
            {
                if (controller == null) continue;
                controller.SetTargetCharacter(null);
            }
        }

        if (currentTargetName != null)
            currentTargetName.text = "";
        if (currentTargetIcon != null)
            currentTargetIcon.gameObject.SetActive(false);
    }

    private void ApplyTargetToUpgradeStats(CharacterData character)
    {
        if (upgradeStatControllers == null) return;

        foreach (var controller in upgradeStatControllers)
        {
            if (controller == null) continue;
            controller.SetTargetCharacter(character);
        }
    }

    private void UpdateCurrentTargetDisplay(CharacterData character)
    {
        Debug.Log($"[CharacterUpgradePannel] 대상 선택: ID={character.ID}, Label={character.Label}, SpritePath={character.Sprite}");

        if (currentTargetName != null)
        {
            currentTargetName.text = character.Label;
        }

        if (currentTargetIcon != null)
        {
            if (!string.IsNullOrEmpty(character.Sprite))
            {
                // CharacterData.Sprite는 폴더 경로(예: UnitSprite/Player)이므로 Stand를 우선 로드
                string standPath = $"{character.Sprite}/Stand";
                Sprite sprite = Resources.Load<Sprite>(standPath);
                if (sprite == null)
                {
                    // 과거 데이터 호환: 단일 경로가 들어있을 수도 있어 1회 폴백
                    sprite = Resources.Load<Sprite>(character.Sprite);
                }
                currentTargetIcon.sprite = sprite;
                currentTargetIcon.gameObject.SetActive(sprite != null);
                Debug.Log($"[CharacterUpgradePannel] Sprite Load 결과: basePath='{character.Sprite}', standPath='{standPath}', success={sprite != null}, spriteName={(sprite != null ? sprite.name : "null")}");
            }
            else
            {
                currentTargetIcon.gameObject.SetActive(false);
                Debug.LogWarning("[CharacterUpgradePannel] SpritePath가 비어 있어 currentTargetIcon을 숨깁니다.");
            }
        }
        else
        {
            Debug.LogWarning("[CharacterUpgradePannel] currentTargetIcon 참조가 비어 있습니다.");
        }
    }

    /// <summary>
    /// 버튼에서 호출: 현재 선택된 캐릭터의 업그레이드 보너스를 모두 초기화하고
    /// 투자한 영혼먼지를 전액 환불합니다.
    /// </summary>
    public void ResetCurrentCharacterUpgrade()
    {
        if (currentTargetCharacter == null)
        {
            Debug.LogWarning("[CharacterUpgradePannel] 초기화할 대상 캐릭터가 없습니다.");
            return;
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[CharacterUpgradePannel] GameProgressManager.Instance가 null입니다.");
            return;
        }

        int refundAmount = currentTargetCharacter.totalSoulDustSpent;

        // 업그레이드 보너스만 순수 초기화 (환불 로직은 아래에서 처리)
        GameProgressManager.ResetCharacterUpgradeBonuses(currentTargetCharacter);

        // 전액 환불
        if (refundAmount > 0)
        {
            GameProgressManager.Instance.AddSoulDust(refundAmount);
        }

        // UI 즉시 갱신
        ApplyTargetToUpgradeStats(currentTargetCharacter);
        UpdateCurrentTargetDisplay(currentTargetCharacter);
        UpdateSoulDustDisplay();
    }
}
