using System.Collections;
using UnityEngine;

/// <summary>
/// 월드맵 UI 진입점. 버튼 OnClick 등에서 메서드만 연결해 사용.
/// MenuPanel anchoredPosition.x 슬라이드 + 메뉴 열기 버튼은 반대 방향으로 슬라이드(겹침 방지).
/// </summary>
public class WorldMapUiHub : MonoBehaviour
{
    [Header("메뉴 패널 (WorldCanvas / MenuPanel)")]
    [SerializeField] private RectTransform menuPanel;

    [Header("메뉴 열기 버튼 (패널과 반대로 슬라이드 — 열릴 때 들어갔다 닫히면 나옴)")]
    [SerializeField] private RectTransform menuOpenButton;
    [Tooltip("메뉴가 닫혀 있을 때 버튼 anchoredPosition.x")]
    [SerializeField] private float menuButtonVisibleAnchoredX = 0f;
    [Tooltip("메뉴가 열려 있을 때 버튼이 숨을 anchoredPosition.x (패널이 나오는 방향과 반대로)")]
    [SerializeField] private float menuButtonHiddenAnchoredX = -300f;

    [Header("캐릭터 업그레이드 패널")]
    [SerializeField] private CharacterUpgradePannel characterUpgradePanel;

    [Header("슬라이드")]
    [SerializeField] private float closedAnchoredX = -300f;
    [SerializeField] private float openAnchoredX = 0f;
    [SerializeField] private float slideDuration = 0.28f;
    [Tooltip("Time.deltaTime 사용. 일시정지 시에도 열리게 하려면 끄고 코드에서 unscaled로 바꾸세요.")]
    [SerializeField] private bool useUnscaledTime = true;

    private Coroutine _slideRoutine;
    private bool _menuOpen;

    /// <summary>메뉴가 완전히 열린 상태로 간주할 때 true (애니메이션 끝 기준).</summary>
    public bool IsMenuOpen => _menuOpen;

    /// <summary>
    /// 월드맵 화면일 때만 허브 메뉴 버튼이 동작 (다른 UI와 겹칠 때 오동작 방지).
    /// </summary>
    private static bool IsWorldMapScreenForHub()
    {
        return GameManager.Instance != null
            && GameManager.Instance.IsCurrentScreenState(GameManager.ScreenState.WorldMap);
    }

    private void Awake()
    {
        if (menuPanel != null && !_menuOpen)
            ApplyMenuClosedLayoutImmediate();
    }

    /// <summary>버튼 연결: 메뉴를 부드럽게 엽니다.</summary>
    public void OpenMenu()
    {
        if (!IsWorldMapScreenForHub()) return;
        if (menuPanel == null) return;
        _menuOpen = true;
        StartSlideToward(openAnchoredX);
    }

    /// <summary>버튼 연결: 메뉴를 부드럽게 닫습니다.</summary>
    public void CloseMenu()
    {
        if (!IsWorldMapScreenForHub()) return;
        if (menuPanel == null) return;
        _menuOpen = false;
        StartSlideToward(closedAnchoredX);
    }

    /// <summary>버튼 연결: 열림/닫힘 토글.</summary>
    public void ToggleMenu()
    {
        if (!IsWorldMapScreenForHub()) return;
        if (_menuOpen)
            CloseMenu();
        else
            OpenMenu();
    }

    /// <summary>애니메이션 없이 즉시 닫힌 위치로 맞춥니다. (월드맵일 때만, 버튼용)</summary>
    public void SnapToClosed()
    {
        if (!IsWorldMapScreenForHub()) return;
        ApplyMenuClosedLayoutImmediate();
    }

    /// <summary>애니메이션 없이 즉시 열린 위치로 맞춥니다. (월드맵일 때만, 버튼용)</summary>
    public void SnapToOpen()
    {
        if (!IsWorldMapScreenForHub()) return;
        ApplyMenuOpenLayoutImmediate();
    }

    private void ApplyMenuClosedLayoutImmediate()
    {
        if (menuPanel == null) return;
        StopSlideIfRunning();
        _menuOpen = false;
        SetAnchoredX(menuPanel, closedAnchoredX);
        if (menuOpenButton != null)
            SetAnchoredX(menuOpenButton, menuButtonVisibleAnchoredX);
    }

    private void ApplyMenuOpenLayoutImmediate()
    {
        if (menuPanel == null) return;
        StopSlideIfRunning();
        _menuOpen = true;
        SetAnchoredX(menuPanel, openAnchoredX);
        if (menuOpenButton != null)
            SetAnchoredX(menuOpenButton, menuButtonHiddenAnchoredX);
    }

    /// <summary>
    /// <see cref="GameManager"/>의 <b>현재</b> 화면 상태에 맞춰 메뉴바·메뉴 열기 버튼 레이아웃을 즉시 재정렬합니다.
    /// 화면 전환 직후 등 어디서든 재사용할 수 있습니다.
    /// </summary>
    public void SyncHubLayoutToScreenState()
    {
        if (GameManager.Instance == null) return;
        SyncHubLayoutToScreenState(GameManager.Instance.CurrentScreenState);
    }

    /// <summary>
    /// 지정한 화면 상태 기준으로 허브 메뉴 UI를 즉시 재정렬합니다. (상태가 아직 GM에 반영되기 전에도 호출 가능)
    /// </summary>
    public void SyncHubLayoutToScreenState(GameManager.ScreenState state)
    {
        StopSlideIfRunning();

        switch (state)
        {
            case GameManager.ScreenState.WorldMap:
                ApplyMenuClosedLayoutImmediate();
                break;

            case GameManager.ScreenState.CharacterUpgrade:
                ApplyMenuChromeOverlayHiddenImmediate();
                break;

            default:
                // 월드맵이 아닌 나머지(스테이지 선택, 파티, 전투 등): 메뉴 크롬은 겹침 방지로 모두 왼쪽 밖
                ApplyMenuChromeOverlayHiddenImmediate();
                break;
        }
    }

    /// <summary>
    /// 메뉴바·메뉴 열기 버튼을 모두 왼쪽(화면 밖)으로 즉시 맞춤. 오버레이/전환 화면 공통.
    /// </summary>
    private void ApplyMenuChromeOverlayHiddenImmediate()
    {
        _menuOpen = false;
        if (menuPanel != null)
            SetAnchoredX(menuPanel, closedAnchoredX);
        if (menuOpenButton != null)
            SetAnchoredX(menuOpenButton, menuButtonHiddenAnchoredX);
    }

    /// <summary>
    /// 버튼 연결: 업그레이드 패널 루트를 켭니다. (비활성 오브젝트는 여기서만 활성화)
    /// 패널 내부 슬라이드/토글은 패널 안 버튼의 TogglePanel 등으로 처리하세요.
    /// </summary>
    public void OpenCharacterUpgradePanel()
    {
        if (!IsWorldMapScreenForHub()) return;
        if (characterUpgradePanel == null)
        {
            Debug.LogWarning("[WorldMapUiHub] characterUpgradePanel이 지정되지 않았습니다.");
            return;
        }

        characterUpgradePanel.gameObject.SetActive(true);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentScreenState(GameManager.ScreenState.CharacterUpgrade);
            SyncHubLayoutToScreenState();
        }
    }

    /// <summary>버튼 연결: 업그레이드 패널 루트를 끕니다. (업그레이드 화면일 때만)</summary>
    public void CloseCharacterUpgradePanel()
    {
        if (GameManager.Instance == null
            || !GameManager.Instance.IsCurrentScreenState(GameManager.ScreenState.CharacterUpgrade))
            return;

        if (characterUpgradePanel == null)
        {
            Debug.LogWarning("[WorldMapUiHub] characterUpgradePanel이 지정되지 않았습니다.");
            return;
        }

        characterUpgradePanel.gameObject.SetActive(false);

        GameManager.Instance.SetCurrentScreenState(GameManager.ScreenState.WorldMap);
        SyncHubLayoutToScreenState();
    }

    private void StartSlideToward(float menuTargetX)
    {
        StopSlideIfRunning();
        bool opening = Mathf.Approximately(menuTargetX, openAnchoredX);
        float buttonTargetX = opening ? menuButtonHiddenAnchoredX : menuButtonVisibleAnchoredX;
        _slideRoutine = StartCoroutine(SlideRoutine(menuTargetX, buttonTargetX));
    }

    private void StopSlideIfRunning()
    {
        if (_slideRoutine != null)
        {
            StopCoroutine(_slideRoutine);
            _slideRoutine = null;
        }
    }

    private IEnumerator SlideRoutine(float menuTargetX, float buttonTargetX)
    {
        Vector2 menuFrom = menuPanel.anchoredPosition;
        Vector2 menuTo = new Vector2(menuTargetX, menuFrom.y);

        bool hasButton = menuOpenButton != null;
        Vector2 buttonFrom = hasButton ? menuOpenButton.anchoredPosition : Vector2.zero;
        Vector2 buttonTo = hasButton ? new Vector2(buttonTargetX, buttonFrom.y) : Vector2.zero;

        bool menuDone = Mathf.Approximately(menuFrom.x, menuTo.x);
        bool buttonDone = !hasButton || Mathf.Approximately(buttonFrom.x, buttonTo.x);
        if (menuDone && buttonDone)
        {
            _slideRoutine = null;
            yield break;
        }

        float dur = Mathf.Max(0.0001f, slideDuration);
        float t = 0f;

        while (t < 1f)
        {
            t += DeltaTime() / dur;
            float smooth = Mathf.SmoothStep(0f, 1f, Mathf.Clamp01(t));
            if (!menuDone)
                menuPanel.anchoredPosition = Vector2.LerpUnclamped(menuFrom, menuTo, smooth);
            if (hasButton && !buttonDone)
                menuOpenButton.anchoredPosition = Vector2.LerpUnclamped(buttonFrom, buttonTo, smooth);
            yield return null;
        }

        if (!menuDone)
            menuPanel.anchoredPosition = menuTo;
        if (hasButton && !buttonDone)
            menuOpenButton.anchoredPosition = buttonTo;
        _slideRoutine = null;
    }

    private float DeltaTime()
    {
        return useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
    }

    private static void SetAnchoredX(RectTransform rt, float x)
    {
        Vector2 p = rt.anchoredPosition;
        p.x = x;
        rt.anchoredPosition = p;
    }
}
