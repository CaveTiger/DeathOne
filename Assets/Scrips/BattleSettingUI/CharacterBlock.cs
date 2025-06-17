using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CharacterBlock : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [Header("UI")]
    public Image characterImage;

    [Header("데이터")]
    public CharacterData characterData;

    private Transform originalParent;
    private Canvas parentCanvas;
    private Vector3 originalLocalPosition;

    public CharacterInfoPopup characterInfoPopup; // Inspector에서 연결

    private float lastClickTime = 0f;
    private const float doubleClickThreshold = 0.3f;

    /// <summary>
    /// 외부에서 캐릭터 데이터와 이미지를 세팅
    /// </summary>
    public void Initialize(CharacterData data, Sprite sprite)
    {
        characterData = data;
        characterImage.sprite = sprite;
    }

    // 드래그 시작
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        originalLocalPosition = transform.localPosition;
        parentCanvas = GetComponentInParent<Canvas>();
        transform.SetParent(parentCanvas.transform, true);
    }

    // 드래그 중
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // 드래그 종료
    public void OnEndDrag(PointerEventData eventData)
    {
        // 파티 슬롯 위에 놓였는지 판정 후 처리 필요
        // 아니면 원래 자리로 복귀
        transform.SetParent(originalParent, true);
        transform.localPosition = originalLocalPosition;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (Time.time - lastClickTime < doubleClickThreshold)
        {
            // 더블클릭 시 팝업에 정보 전달 및 활성화
            if (characterInfoPopup != null)
            {
                characterInfoPopup.SetCharacterData(characterData);
                characterInfoPopup.gameObject.SetActive(true);
            }
        }
        lastClickTime = Time.time;
    }
}
