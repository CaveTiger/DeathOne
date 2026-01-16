using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Virtual Mouse 전용 UI Canvas 관리
/// </summary>
public class VirtualMouseCanvas : MonoBehaviour
{
    public static VirtualMouseCanvas Instance { get; private set; }
    
    [Header("Virtual Mouse Canvas 설정")]
    [SerializeField] private Canvas virtualMouseCanvas;
    [SerializeField] private CanvasScaler canvasScaler;
    [SerializeField] private GraphicRaycaster graphicRaycaster;
    
    [Header("Virtual Mouse UI 요소들")]
    [SerializeField] private RectTransform virtualMouseTransform;
    [SerializeField] private Image cursorImage;
    
    [Header("Canvas 설정")]
    [SerializeField] private RenderMode renderMode = RenderMode.ScreenSpaceOverlay;
    [SerializeField] private int sortOrder = 100; // 최상단 렌더링
    [SerializeField] private float scaleFactor = 1f;
    
    private static GameObject persistentRoot;
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            var root = transform.root.gameObject;
            if (persistentRoot == null)
            {
                persistentRoot = root;
                DontDestroyOnLoad(root);
                Debug.Log("[VirtualMouseCanvas] 루트 유지 설정 완료: " + root.name);
            }
            else if (persistentRoot != root)
            {
                Debug.Log("[VirtualMouseCanvas] 중복 루트 감지로 자신 제거: " + gameObject.name);
                Destroy(gameObject);
                return;
            }
        }
        else
        {
            Debug.Log("[VirtualMouseCanvas] 중복 인스턴스 제거");
            Destroy(gameObject);
            return;
        }
    }
    
    void Start()
    {
        InitializeVirtualMouseCanvas();
    }
    
    /// <summary>
    /// Virtual Mouse Canvas 초기화
    /// </summary>
    private void InitializeVirtualMouseCanvas()
    {
        // Canvas 설정
        if (virtualMouseCanvas == null)
        {
            virtualMouseCanvas = GetComponent<Canvas>();
        }
        
        // Canvas 기본 설정
        virtualMouseCanvas.renderMode = renderMode;
        virtualMouseCanvas.sortingOrder = sortOrder;
        
        // CanvasGroup 알파값을 1로 설정 (Inspector에서 0으로 설정되어 있을 수 있음)
        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
        
        // CanvasScaler 설정
        if (canvasScaler == null)
        {
            canvasScaler = GetComponent<CanvasScaler>();
        }
        
        if (canvasScaler != null)
        {
            canvasScaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            canvasScaler.referenceResolution = new Vector2(1920, 1080);
            canvasScaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            canvasScaler.matchWidthOrHeight = 0.5f;
        }
        
        // GraphicRaycaster 설정
        if (graphicRaycaster == null)
        {
            graphicRaycaster = GetComponent<GraphicRaycaster>();
        }
        
        // Virtual Mouse Transform 설정
        if (virtualMouseTransform == null)
        {
            virtualMouseTransform = transform.GetChild(0)?.GetComponent<RectTransform>();
        }
        
        // Cursor Image 설정
        if (cursorImage == null)
        {
            cursorImage = virtualMouseTransform?.GetComponent<Image>();
        }
    }
    
    /// <summary>
    /// Virtual Mouse Transform 가져오기
    /// </summary>
    public RectTransform GetVirtualMouseTransform()
    {
        return virtualMouseTransform;
    }
    
    /// <summary>
    /// Cursor Image 가져오기
    /// </summary>
    public Image GetCursorImage()
    {
        return cursorImage;
    }
    
    /// <summary>
    /// Canvas 정렬 순서 설정
    /// </summary>
    public void SetSortOrder(int order)
    {
        if (virtualMouseCanvas != null)
        {
            virtualMouseCanvas.sortingOrder = order;
        }
    }
    
    /// <summary>
    /// Canvas 스케일 팩터 설정
    /// </summary>
    public void SetScaleFactor(float factor)
    {
        scaleFactor = factor;
        if (canvasScaler != null)
        {
            canvasScaler.scaleFactor = factor;
        }
    }
    
    /// <summary>
    /// Canvas 활성화/비활성화
    /// </summary>
    public void SetCanvasActive(bool active)
    {
        if (virtualMouseCanvas != null)
        {
            virtualMouseCanvas.gameObject.SetActive(active);
        }
    }
}
