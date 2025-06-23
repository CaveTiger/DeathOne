using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using TMPro;

public class CharacterBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image characterImage;
    public TextMeshProUGUI countText; // 수량 표시 텍스트

    [Header("데이터")]
    public CharacterData characterData;

    private Transform originalParent;
    private Canvas parentCanvas;
    private Vector3 originalLocalPosition;
    private bool isDragging = false;

    public CharacterInfoPopup characterInfoPopup; // Inspector에서 연결

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    // 이 캐릭터 블록이 클릭되었을 때 호출될 이벤트
    public static event Action<CharacterBlock> OnCharacterSelected;

    // 파티 슬롯에 캐릭터가 배치될 때 호출될 이벤트
    public static event Action<CharacterBlock, BattleSttingCharacterSlot> OnCharacterPlacedInSlot;

    /// <summary>
    /// 외부에서 캐릭터 데이터와 수량을 세팅
    /// </summary>
    public void Initialize(CharacterData data, int count)
    {
        characterData = data;

        // 스프라이트 로드
        if (!string.IsNullOrEmpty(data.Sprite))
        {
            characterImage.sprite = Resources.Load<Sprite>(data.Sprite);
        }

        // 수량 텍스트 업데이트
        if (count > 1)
        {
            countText.text = $"x{count}";
            countText.gameObject.SetActive(true);
        }
        else
        {
            countText.gameObject.SetActive(false);
        }
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        parentCanvas = GetComponentInParent<Canvas>();
        transform.SetParent(parentCanvas.transform, true);
        
        // 드래그 시작 시 시각적 피드백
        var canvasGroup = gameObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false; // 드래그 중에는 자기 자신은 레이캐스트에서 제외
    }

    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
        
        // 파티 슬롯 위에 놓였는지 확인
        var results = new System.Collections.Generic.List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        BattleSttingCharacterSlot targetSlot = null;
        foreach (var result in results)
        {
            targetSlot = result.gameObject.GetComponent<BattleSttingCharacterSlot>();
            if (targetSlot != null) break;
        }

        if (targetSlot != null && targetSlot.CanAcceptCharacter(characterData))
        {
            // 파티 슬롯에 배치
            targetSlot.PlaceCharacterBlock(this);
        }
        else
        {
            // 원래 자리로 복귀
            transform.SetParent(originalParent, true);
            ResetVisuals(); // 시각적 상태 복구
        }
    }

    /// <summary>
    /// 드래그 후 시각적 상태를 원래대로 복구합니다.
    /// </summary>
    public void ResetVisuals()
    {
        var canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true; // 레이캐스트 다시 활성화
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;

        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            // 더블클릭
            if (characterInfoPopup != null)
            {
                characterInfoPopup.SetCharacterData(characterData);
                characterInfoPopup.gameObject.SetActive(true);
            }
        }
        else
        {
            // 싱글클릭
            OnCharacterSelected?.Invoke(this);
        }

        lastClickTime = Time.time;
    }
}
