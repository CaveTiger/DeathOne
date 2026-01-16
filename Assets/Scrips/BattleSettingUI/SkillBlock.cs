using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using TMPro;

public class SkillBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image skillIcon;
    public TextMeshProUGUI skillNameText;
    public Transform transformAnchor;

    [Header("데이터")]
    public SkillData skillData;
    // private bool isDragging = false; // 사용하지 않는 필드 제거
    private static DragTool dragToolInstance;
    
    [Header("팝업")]
    public SkillInfoPopup skillInfoPopup; // Inspector에서 연결
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    public string[] skillIDs = new string[4];

    public Transform inventoryContentTransform; // ← Inspector에서 연결

    private void Awake()
    {
        // 앵커를 중앙으로 자동 설정
        var rect = GetComponent<RectTransform>();
        if (rect != null)
        {
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
        }
        if (transformAnchor == null)
            transformAnchor = transform.parent;
        
        // DragTool 안전한 초기화
        InitializeDragTool();
    }

    /// <summary>
    /// DragTool을 안전하게 초기화
    /// </summary>
    private void InitializeDragTool()
    {
        try
        {
            if (dragToolInstance == null)
            {
                dragToolInstance = FindFirstObjectByType<DragTool>();
                if (dragToolInstance != null)
                {
                    Debug.Log("[SkillBlock] DragTool 초기화 성공");
                }
                else
                {
                    Debug.LogWarning("[SkillBlock] DragTool을 찾을 수 없습니다. 드래그 기능이 제한될 수 있습니다.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkillBlock] DragTool 초기화 중 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 외부에서 스킬 데이터를 세팅
    /// </summary>
    public void Initialize(SkillData data)
    {
        skillData = data;
        UpdateUI();
        Debug.Log($"[SkillBlock] 스킬 블록 초기화: {data.Name} (ID: {data.ID})");
    }

    private void UpdateUI()
    {
        if (skillData == null) return;

        // 스킬 이름 설정
        if (skillNameText != null)
        {
            skillNameText.text = skillData.Name;
        }

        // 스킬 아이콘 설정
        if (skillIcon != null && !string.IsNullOrEmpty(skillData.Icon))
        {
            Sprite iconSprite = Resources.Load<Sprite>(skillData.Icon);
            if (iconSprite != null)
            {
                skillIcon.sprite = iconSprite;
            }
            else
            {
                Debug.LogWarning($"[SkillBlock] 스킬 아이콘을 찾을 수 없습니다: {skillData.Icon}");
            }
        }
    }

    private GameObject previewBlock;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 프리뷰 복제 생성
        previewBlock = Instantiate(this.gameObject, DragTool.Instance.transform);
        // ★ 복제된 프리뷰 블록에 skillData 복사
        var previewSkillBlock = previewBlock.GetComponent<SkillBlock>();
        if (previewSkillBlock != null)
        {
            previewSkillBlock.skillData = this.skillData;
            previewSkillBlock.UpdateUI();
        }
        // 프리뷰에만 적용할 설정(이벤트 비활성화 등)
        var cg = previewBlock.GetComponent<CanvasGroup>();
        if (cg == null) cg = previewBlock.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // 프리뷰가 마우스를 정확히 따라다니게
        if (previewBlock != null)
        {
            // Canvas의 RenderMode에 따라 적절한 좌표 변환 사용
            Canvas canvas = previewBlock.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    // Screen Space - Overlay: 직접 마우스 위치 사용
                    previewBlock.GetComponent<RectTransform>().position = Input.mousePosition;
                }
                else
                {
                    // Screen Space - Camera 또는 World Space: 카메라 좌표 변환 사용
                    Vector3 mousePosition = Input.mousePosition;
                    mousePosition.z = canvas.planeDistance; // Canvas의 planeDistance 사용
                    Vector3 worldPosition = canvas.worldCamera.ScreenToWorldPoint(mousePosition);
                    previewBlock.transform.position = worldPosition;
                }
            }
            else
            {
                // Canvas를 찾을 수 없는 경우 기본 처리
                previewBlock.GetComponent<RectTransform>().position = Input.mousePosition;
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 슬롯 감지 및 정착
        bool placedInSlot = false;
        if (BattleSettingManager.Instance != null)
        {
            // 스킬 슬롯들을 찾아서 isPointerOver 확인
            var skillSlots = BattleSettingManager.Instance.GetComponentsInChildren<SkillSlot>();
            foreach (var slot in skillSlots)
            {
                if (slot != null && slot.isPointerOver)
                {
                    Debug.Log($"[SkillBlock] 슬롯 감지됨: {slot.name}");
                    slot.PlaceSkillBlock(this);
                    placedInSlot = true;
                    break;
                }
            }
        }
        
        // 슬롯에 배치되지 않았다면 프리뷰 블록만 제거
        if (!placedInSlot)
        {
            Debug.Log("[SkillBlock] 슬롯 감지 실패, 프리뷰 제거");
        }
        
        // 프리뷰 블록 제거
        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }

    public void ReturnToAnchor()
    {
        if (transformAnchor == null)
        {
            Debug.LogError($"[SkillBlock] transformAnchor가 null입니다: {skillData?.Name}");
            return;
        }

        transform.SetParent(transformAnchor);
        
        // RectTransform 속성을 명시적으로 설정
        var rectTransform = GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        // 인벤토리로 반환
        if (SkillInventoryTab.Instance != null)
        {
            SkillInventoryTab.Instance.ReturnSkillBlock(this);
        }
    }

    /// <summary>
    /// 슬롯에 블럭이 배치될 때
    /// </summary>
    public void SetAnchorToSlot(Transform slotTransform)
    {
        transformAnchor = slotTransform;
    }

    public void ReturnToInventory()
    {
        // 부모를 인벤토리 Content로 변경
        transform.SetParent(inventoryContentTransform, false);

        // RectTransform 값 복구 (좌상단 기준)
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0, 1);
        rt.anchorMax = new Vector2(0, 1);
        rt.pivot = new Vector2(0, 1);
        rt.anchoredPosition = Vector2.zero; // 또는 원하는 위치
        rt.localScale = Vector3.one;
    }

    public void PlaceInSlot(Transform slotTransform)
    {
        transform.SetParent(slotTransform, false);

        // RectTransform 값 복구 (중앙 기준)
        RectTransform rt = GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;
    }

    public void SetInteractable(bool interactable)
    {
        // 버튼/드래그/색상 등 비활성화 처리
        // 예시: 아이콘 회색, 드래그 불가 등
        skillIcon.color = interactable ? Color.white : Color.gray;
        // 필요시 드래그/클릭 이벤트도 막기
    }

    /// <summary>
    /// 클릭 이벤트 처리 (스킬 정보 팝업 등)
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        float currentTime = Time.time;
        if (currentTime - lastClickTime < doubleClickThreshold)
        {
            // 더블클릭 감지
            OnDoubleClick();
        }
        lastClickTime = currentTime;
    }

    private void OnDoubleClick()
    {
        if (skillData != null)
        {
            // 프리팹 연결이 안 되어 있으면 씬에서 찾기 (비활성화된 오브젝트 포함)
            if (skillInfoPopup == null)
            {
                skillInfoPopup = FindObjectsByType<SkillInfoPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
                if (skillInfoPopup == null)
                {
                    Debug.LogError("[SkillBlock] SkillInfoPopup을 찾을 수 없습니다!");
                    return;
                }
            }
            
            skillInfoPopup.SetSkillData(skillData);
            skillInfoPopup.gameObject.SetActive(true);
            Debug.Log($"[SkillBlock] 더블클릭: {skillData.Name} 정보 팝업 열기");
        }
    }
} 