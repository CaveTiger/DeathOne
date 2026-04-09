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

        HealVirtualMouseVisibility();
        
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
    /// 월드맵(SampleScene) 복귀 등 씬 로드 후 호출. DontDestroyOnLoad라 Start가 다시 안 돌 수 있어
    /// <see cref="InitializeVirtualMouseCanvas"/> 전체를 한 번 더 돌려 스케일·CanvasGroup·Scaler 등을 맞춤. (스냅샷과 무관)
    /// </summary>
    public void EnsureVisibleAfterWorldMapLoad()
    {
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);
        InitializeVirtualMouseCanvas();
    }

    /// <summary>
    /// 루트 또는 자손 RectTransform 이 (0,0,0) 스케일이면 UI 전체가 안 보임. CanvasGroup 알파도 1로.
    /// </summary>
    private void HealVirtualMouseVisibility()
    {
        var canvasRt = GetComponent<RectTransform>();
        if (canvasRt != null && canvasRt.localScale.sqrMagnitude < 1e-6f)
        {
            canvasRt.localScale = Vector3.one;
            Debug.LogWarning("[VirtualMouseCanvas] 루트 localScale이 0에 가까워 (1,1,1)로 복구했습니다.");
        }

        foreach (var rt in GetComponentsInChildren<RectTransform>(true))
        {
            Vector3 ls = rt.localScale;
            if (ls.x == 0f && ls.y == 0f && ls.z == 0f)
            {
                rt.localScale = Vector3.one;
                Debug.LogWarning($"[VirtualMouseCanvas] 자손 RectTransform scale (0,0,0) 복구: {rt.name}");
            }
        }

        CanvasGroup canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            if (canvasGroup.alpha < 0.99f)
                Debug.LogWarning("[VirtualMouseCanvas] CanvasGroup alpha가 1이 아니어서 1로 복구했습니다.");
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
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
