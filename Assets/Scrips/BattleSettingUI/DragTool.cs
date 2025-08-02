using UnityEngine;
using UnityEngine.EventSystems;

public class DragTool : MonoBehaviour
{
    public static DragTool Instance { get; private set; }

    private RectTransform rectTransform;
    private Canvas rootCanvas;
    private bool isDragging = false;

    // === 추가: 프리뷰 블럭 관리 ===
    public GameObject CurrentPreviewBlock { get; private set; }

    void Awake()
    {
        Instance = this;
        rectTransform = GetComponent<RectTransform>();
        rootCanvas = GetComponentInParent<Canvas>();
    }

    private void Update()
    {
        if (isDragging)
        {
            UpdatePosition();
        }
    }

    private void UpdatePosition()
    {
        Vector2 mousePos = Input.mousePosition;
        Vector2 localPoint;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            mousePos,
            rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : rootCanvas.worldCamera,
            out localPoint
        );
        rectTransform.anchoredPosition = localPoint;
    }

    // === 추가: 프리뷰 블럭 생성 ===
    /*
    public void SpawnPreviewBlock(SkillBlock original)
    {
        if (CurrentPreviewBlock != null)
            Destroy(CurrentPreviewBlock);

        // SkillBlock 프리팹을 복제해서 DragTool의 자식으로 붙임
        CurrentPreviewBlock = Instantiate(original.gameObject, this.transform);
        // 복제된 블럭의 상호작용/이벤트 비활성화 등 필요시 처리
        var cg = CurrentPreviewBlock.GetComponent<CanvasGroup>();
        if (cg == null) cg = CurrentPreviewBlock.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }
    */

    // === 추가: 프리뷰 블럭 삭제 ===
    public void DestroyPreviewBlock()
    {
        if (CurrentPreviewBlock != null)
            Destroy(CurrentPreviewBlock);
        CurrentPreviewBlock = null;
    }

    public void StartDragging()
    {
        isDragging = true;
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        Debug.Log("[DragTool] 드래그 시작");
    }

    public void StopDragging()
    {
        isDragging = false;
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
            canvasGroup.blocksRaycasts = true;
        Debug.Log("[DragTool] 드래그 종료");
    }

    // 마우스 위치에 SkillSlot이 있는지 감지
    public bool IsOverSlot(out SkillSlot targetSlot)
    {
        targetSlot = null;
        // 마우스 위치에서 UI Raycast
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            targetSlot = result.gameObject.GetComponentInParent<SkillSlot>();
            if (targetSlot != null)
                return true;
        }
        return false;
    }

    // 마우스 위치에 CharacterSlot이 있는지 감지
    public bool IsOverCharacterSlot(out BattleSettingCharacterSlot targetSlot)
    {
        targetSlot = null;
        // 마우스 위치에서 UI Raycast
        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = Input.mousePosition
        };

        var raycastResults = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, raycastResults);

        foreach (var result in raycastResults)
        {
            targetSlot = result.gameObject.GetComponentInParent<BattleSettingCharacterSlot>();
            if (targetSlot != null)
                return true;
        }
        return false;
    }
}
