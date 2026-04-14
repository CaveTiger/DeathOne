using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Linq;
using TMPro;

public class CharacterBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image characterImage;
    public TextMeshProUGUI countText; // 수량 표시 텍스트
    public Transform transformAnchor;

    [Header("데이터")]
    public CharacterData characterData;
    // private bool isDragging = false; // 사용하지 않는 필드 제거
    public CharacterInfoPopup characterInfoPopup; // Inspector에서 연결
    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;
    private static DragTool dragToolInstance;

    [Header("캐릭터 스킬ID 4개 (파티세팅/전투 생성 기준)")]
    [SerializeField]
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
                    Debug.Log("[CharacterBlock] DragTool 초기화 성공");
                }
                else
                {
                    Debug.LogWarning("[CharacterBlock] DragTool을 찾을 수 없습니다. 드래그 기능이 제한될 수 있습니다.");
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[CharacterBlock] DragTool 초기화 중 오류: {e.Message}");
        }
    }

    /// <summary>
    /// 외부에서 캐릭터/스킬 데이터를 세팅
    /// </summary>
    public void Initialize(CharacterData data)
    {
        characterData = data;

        // 스프라이트 로드
        if (!string.IsNullOrEmpty(data.Sprite))
        {
            // 캐릭터 폴더 경로 기준으로 Stand를 우선 사용한다.
            string standPath = $"{data.Sprite}/Stand";
            Sprite loaded = Resources.Load<Sprite>(standPath);
            if (loaded == null)
                loaded = Resources.Load<Sprite>(data.Sprite); // 구형 단일 경로 데이터 호환
            characterImage.sprite = loaded;
        }

        // 해금 여부에 따라 UI/상호작용 처리
        if (data.IsUnlocked)
        {
            // 해금된 캐릭터: 정상 색상, 상호작용 가능
            characterImage.color = Color.white;
            countText.gameObject.SetActive(false);
        }
        else
        {
            // 미해금 캐릭터: 회색 처리, 상호작용 불가, 잠금 아이콘 등 표시
            characterImage.color = new Color(0.5f, 0.5f, 0.5f, 1f);
            countText.text = "잠김";
            countText.gameObject.SetActive(true);
        }

        // 스킬ID도 복사
        if (data != null && data.Skills != null && data.Skills.Count == 4)
        {
            for (int i = 0; i < 4; i++)
                skillIDs[i] = data.Skills[i];
        }
    }

    private GameObject previewBlock;

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 현재 슬롯에 있는 블록인지 확인하고 슬롯에서 제거
        var currentSlot = GetCurrentSlot();
        if (currentSlot != null)
        {
            Debug.Log($"[CharacterBlock] 슬롯에서 드래그 시작: {characterData?.Label}");
            currentSlot.RemoveCharacterBlock();
        }
        
        // 프리뷰 복제 생성
        previewBlock = Instantiate(this.gameObject, DragTool.Instance.transform);
        // ★ 복제된 프리뷰 블록에 characterData 복사
        var previewCharacterBlock = previewBlock.GetComponent<CharacterBlock>();
        if (previewCharacterBlock != null)
        {
            previewCharacterBlock.characterData = this.characterData;
            previewCharacterBlock.Initialize(this.characterData);
        }
        // 프리뷰에만 적용할 설정(이벤트 비활성화 등)
        var cg = previewBlock.GetComponent<CanvasGroup>();
        if (cg == null) cg = previewBlock.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        
        // 드래그 시작 시 시각적 피드백
        if (characterImage != null)
        {
            characterImage.color = new Color(1f, 1f, 1f, 0.8f);
        }
        Debug.Log($"[CharacterBlock] 드래그 시작: {characterData?.Label}");
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
        // 시각적 피드백 복구
        if (characterImage != null)
        {
            characterImage.color = Color.white;
        }
        
        // 슬롯 감지 및 정착
        bool placedInSlot = false;
        if (BattleSettingManager.Instance != null)
        {
            var slots = new[] { 
                BattleSettingManager.Instance.slot1,
                BattleSettingManager.Instance.slot2,
                BattleSettingManager.Instance.slot3,
                BattleSettingManager.Instance.slot4
            };
            
            foreach (var slot in slots)
            {
                if (slot != null && slot.isPointerOver)
                {
                    // 슬롯에 성공적으로 배치됨
                    slot.PlaceCharacterBlock(this);
                    placedInSlot = true;
                    break;
                }
            }
        }
        
        // 슬롯에 배치되지 않았다면 프리뷰 블록만 제거
        if (!placedInSlot)
        {
            Debug.Log($"[CharacterBlock] 슬롯 감지 실패, 프리뷰 제거: {characterData?.Label}");
        }
        
        // 프리뷰 블록 제거
        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
    }

    /// <summary>
    /// 블록을 원래 위치로 복귀시킵니다. (클로닝 방식에서는 원본이 움직이지 않음)
    /// </summary>
    public void ReturnToAnchor()
    {
        Debug.Log($"[CharacterBlock] ReturnToAnchor 호출: {characterData?.Label}");
        
        // 클로닝 방식에서는 원본이 움직이지 않으므로 별도 처리 불필요
        // 프리뷰 블록만 제거하면 됨
        if (previewBlock != null)
        {
            Destroy(previewBlock);
            previewBlock = null;
        }
        
        Debug.Log($"[CharacterBlock] 원본 위치 유지 완료: {characterData?.Label}");
    }

    /// <summary>
    /// 매니저 쪽으로 캐릭터 정보를 전달하는 메서드(호출만 준비, 실제 호출은 외부에서)
    /// </summary>
    public void NotifyManagerCharacterInfo(BattleSettingCharacterSlot slot)
    {
        if (BattleSettingManager.Instance != null)
        {
            // BattleSettingManager.Instance.HandleCharacterPlacedInSlot(this, slot); // 임시 주석처리
        }
    }

    /// <summary>
    /// 클릭 이벤트 처리 (캐릭터 정보 팝업 등)
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
        if (characterData != null)
        {
            // 프리팹 연결이 안 되어 있으면 씬에서 찾기 (비활성화된 오브젝트 포함)
            if (characterInfoPopup == null)
            {
                characterInfoPopup = FindObjectsByType<CharacterInfoPopup>(FindObjectsInactive.Include, FindObjectsSortMode.None).FirstOrDefault();
                if (characterInfoPopup == null)
                {
                    Debug.LogError("[CharacterBlock] CharacterInfoPopup을 찾을 수 없습니다!");
                    return;
                }
            }
            
            characterInfoPopup.SetCharacterData(characterData);
            characterInfoPopup.gameObject.SetActive(true);
            Debug.Log($"[CharacterBlock] 더블클릭: {characterData.Label} 정보 팝업 열기");
        }
    }

    // 슬롯에 블럭이 배치될 때
    public void SetAnchorToSlot(Transform slotTransform)
    {
        transformAnchor = slotTransform;
    }

    /// <summary>
    /// 현재 블록이 어느 슬롯에 있는지 확인합니다.
    /// </summary>
    private BattleSettingCharacterSlot GetCurrentSlot()
    {
        if (BattleSettingManager.Instance == null) return null;
        
        // 현재 부모가 슬롯인지 확인
        var parentSlot = transform.parent?.GetComponent<BattleSettingCharacterSlot>();
        if (parentSlot != null && parentSlot.currentCharacterBlock == this)
        {
            return parentSlot;
        }
        
        // 기존 방식으로도 확인 (1~4번 슬롯)
        var slots = new[] { 
            BattleSettingManager.Instance.slot1,
            BattleSettingManager.Instance.slot2,
            BattleSettingManager.Instance.slot3,
            BattleSettingManager.Instance.slot4
        };
        
        foreach (var slot in slots)
        {
            if (slot != null && slot.currentCharacterBlock == this)
            {
                return slot;
            }
        }
        
        return null;
    }
}
