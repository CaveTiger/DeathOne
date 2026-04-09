using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 가상마우스 UI 패널 관리 스크립트
/// VirtualMouseCanvas 내부의 UI 패널들을 관리하는 역할
/// </summary>
public class VirtualMouseUIPanel : MonoBehaviour
{
    [Header("패널 기본 설정")]
    [SerializeField] private RectTransform panelTransform;
    [SerializeField] private Image backgroundImage;
    
    [Header("패널 상태")]
    [SerializeField] private bool isVisible = false;
    [SerializeField] private bool isInteractable = true;
    
    [Header("화면 경계 처리")]
    [SerializeField] private bool enableScreenBoundaryCheck = true; // 화면 경계 체크 활성화
    [SerializeField] private float screenMargin = 20f; // 화면 가장자리 여백
    
    [Header("고정 오프셋(마우스 기준)")]
    [SerializeField] private bool useFixedOffset = true; // 고정 수치 사용 여부
    [SerializeField] private float fixedOffsetX = 300f;   // 좌우 고정 오프셋
    [SerializeField] private float fixedOffsetY = 0f;    // 상하 고정 오프셋
    
    [Header("패널 콘텐츠 (추후 확장용)")]
    [SerializeField] private Transform contentContainer;
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private Text titleText;
    
    [Header("디버그 정보")]
    [SerializeField] private string panelName = "VirtualMouseUIPanel";
    [SerializeField] protected bool enableDebugLog = true;
    
    // 이벤트
    public System.Action OnPanelShow;
    public System.Action OnPanelHide;
    public System.Action OnPanelInteractableChanged;
    
    void Awake()
    {
        InitializeComponents();
    }
    
    void Start()
    {
        SetupInitialState();
    }
    
    /// <summary>
    /// 컴포넌트들을 자동으로 찾아서 초기화
    /// </summary>
    private void InitializeComponents()
    {
        // RectTransform 찾기
        if (panelTransform == null)
            panelTransform = GetComponent<RectTransform>();
        
        // Background Image 찾기
        if (backgroundImage == null)
            backgroundImage = GetComponent<Image>();
        
        // CanvasGroup은 사용하지 않음 - SetActive만 사용
        
        // Content Container 찾기
        if (contentContainer == null)
            contentContainer = transform.Find("Content");
        
        // ScrollRect 찾기
        if (scrollRect == null)
            scrollRect = GetComponentInChildren<ScrollRect>();
        
        // Title Text 찾기
        if (titleText == null)
            titleText = GetComponentInChildren<Text>();
        
        if (enableDebugLog)
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 컴포넌트 초기화 완료");
    }
    
    /// <summary>
    /// 초기 상태 설정 (자식 클래스에서 오버라이드 가능)
    /// </summary>
    protected virtual void SetupInitialState()
    {
        // 초기에는 숨김 상태
        SetVisible(false, false);
        
        if (enableDebugLog)
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 초기 상태 설정 완료");
    }
    
    /// <summary>
    /// 패널 표시/숨김 설정 (SetActive만 사용)
    /// </summary>
    /// <param name="visible">표시 여부</param>
    /// <param name="animate">애니메이션 사용 여부 (무시됨, SetActive만 사용)</param>
    public void SetVisible(bool visible, bool animate = true)
    {
        // 자식이 SetActive만 호출한 뒤 isVisible이 어긋나면 true→true로 조기 return 하며 영원히 안 켜지는 버그 방지
        if (isVisible == visible && gameObject.activeSelf == visible)
            return;

        isVisible = visible;
        gameObject.SetActive(visible);
        
        if (visible)
            OnPanelShow?.Invoke();
        else
            OnPanelHide?.Invoke();
        
        if (enableDebugLog)
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 표시 상태: {visible}");
    }
    
    /// <summary>
    /// 패널 표시 (SetActive만 사용)
    /// </summary>
    public void ShowPanel()
    {
        if (isVisible && gameObject.activeSelf)
            return;

        isVisible = true;
        gameObject.SetActive(true);
        
        OnPanelShow?.Invoke();
        
        if (enableDebugLog)
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 표시됨");
    }
    
    /// <summary>
    /// 패널 숨김 (SetActive만 사용)
    /// </summary>
    public void HidePanel()
    {
        if (!isVisible && !gameObject.activeSelf)
            return;

        isVisible = false;
        gameObject.SetActive(false);
        
        OnPanelHide?.Invoke();
        
        if (enableDebugLog)
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 숨김됨");
    }

    private void EnsureActiveHierarchy()
    {
        Transform tr = transform;
        while (tr != null)
        {
            if (!tr.gameObject.activeSelf)
            {
                tr.gameObject.SetActive(true);
            }
            tr = tr.parent;
        }
    }

    /// <summary>
    /// 패널 제목 설정
    /// </summary>
    /// <param name="title">제목 텍스트</param>
    public void SetTitle(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
        
        if (enableDebugLog)
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 제목 설정: {title}");
    }
    
    /// <summary>
    /// 패널 배경 색상 설정
    /// </summary>
    /// <param name="color">배경 색상</param>
    public void SetBackgroundColor(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
    
    /// <summary>
    /// 패널 크기 설정
    /// </summary>
    /// <param name="size">크기</param>
    public void SetPanelSize(Vector2 size)
    {
        if (panelTransform != null)
        {
            panelTransform.sizeDelta = size;
        }
    }
    
    /// <summary>
    /// 패널 위치 설정
    /// </param>
    /// <param name="position">위치</param>
    public void SetPanelPosition(Vector2 position)
    {
        if (panelTransform != null)
        {
            panelTransform.anchoredPosition = position;
        }
    }
    
    // 페이드 애니메이션 코루틴 제거됨 - SetActive만 사용
    
    /// <summary>
    /// 현재 패널 상태 정보 반환
    /// </summary>
    public string GetPanelInfo()
    {
        return $"패널명: {panelName}, 표시: {isVisible}, 상호작용: {isInteractable}";
    }
    
    /// <summary>
    /// 디버그 로그 활성화/비활성화
    /// </summary>
    /// <param name="enable">활성화 여부</param>
    public void SetDebugLog(bool enable)
    {
        enableDebugLog = enable;
    }
    
    // 상속받을 클래스들이 사용할 수 있는 보호된 메서드들
    
    /// <summary>
    /// 패널 제목 설정 (상속 클래스에서 사용)
    /// </summary>
    /// <param name="title">제목 텍스트</param>
    protected void SetTitleInternal(string title)
    {
        if (titleText != null)
        {
            titleText.text = title;
        }
        
        if (enableDebugLog)
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 제목 설정: {title}");
    }
    
    /// <summary>
    /// 패널 배경 색상 설정 (상속 클래스에서 사용)
    /// </summary>
    /// <param name="color">배경 색상</param>
    protected void SetBackgroundColorInternal(Color color)
    {
        if (backgroundImage != null)
        {
            backgroundImage.color = color;
        }
    }
    
    /// <summary>
    /// 패널 크기 설정 (상속 클래스에서 사용)
    /// </summary>
    /// <param name="size">크기</param>
    protected void SetPanelSizeInternal(Vector2 size)
    {
        if (panelTransform != null)
        {
            panelTransform.sizeDelta = size;
        }
    }
    
    /// <summary>
    /// 패널 위치 설정 (상속 클래스에서 사용)
    /// </summary>
    /// <param name="position">위치</param>
    protected void SetPanelPositionInternal(Vector2 position)
    {
        if (panelTransform != null)
        {
            panelTransform.anchoredPosition = position;
        }
    }
    
    /// <summary>
    /// 콘텐츠 컨테이너 가져오기 (상속 클래스에서 사용)
    /// </summary>
    /// <returns>콘텐츠 컨테이너 Transform</returns>
    protected Transform GetContentContainer()
    {
        return contentContainer;
    }
    
    /// <summary>
    /// 스크롤 Rect 가져오기 (상속 클래스에서 사용)
    /// </summary>
    /// <returns>ScrollRect 컴포넌트</returns>
    protected ScrollRect GetScrollRect()
    {
        return scrollRect;
    }
    
    /// <summary>
    /// 제목 텍스트 가져오기 (상속 클래스에서 사용)
    /// </summary>
    /// <returns>Text 컴포넌트</returns>
    protected Text GetTitleText()
    {
        return titleText;
    }
    
    /// <summary>
    /// 화면의 좌우를 구분하여 패널 위치 결정
    /// </summary>
    /// <param name="mousePosition">마우스 위치 (스크린 좌표)</param>
    /// <returns>패널이 왼쪽에 표시되어야 하면 true, 오른쪽에 표시되어야 하면 false</returns>
    public bool ShouldShowPanelOnLeft(Vector2 mousePosition)
    {
        Vector2 screenSize = new Vector2(Screen.width, Screen.height);
        float screenCenterX = screenSize.x / 2f;
        
        // 마우스가 화면 오른쪽 절반에 있으면 패널을 왼쪽에 표시
        return mousePosition.x > screenCenterX;
    }
    
    /// <summary>
    /// 마우스 위치에 따라 패널을 좌우 중 적절한 위치에 배치
    /// </summary>
    /// <param name="mousePosition">마우스 위치 (스크린 좌표)</param>
    /// <param name="offset">마우스에서 패널까지의 거리</param>
    public void PositionPanelLeftOrRight(Vector2 mousePosition, float offset = 20f)
    {
        if (panelTransform == null) return;
        // 요구사항: 부모 중심 기준으로 X=±fixedOffsetX, Y=fixedOffsetY 고정 배치
        float offX = useFixedOffset ? fixedOffsetX : offset;
        float offY = useFixedOffset ? fixedOffsetY : 0f;
        float x = ShouldShowPanelOnLeft(mousePosition) ? -offX : offX;
        Vector2 pos = new Vector2(x, offY);
        panelTransform.anchoredPosition = pos;
        
        if (enableDebugLog)
        {
            string side = ShouldShowPanelOnLeft(mousePosition) ? "왼쪽" : "오른쪽";
            Debug.Log($"[VirtualMouseUIPanel] {panelName} 패널을 마우스 {side}에 배치: {pos}");
        }
    }
}
