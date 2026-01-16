using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 스킬 드래그 앤 드롭으로 슬롯에 배치하고 스킬 사용 가능 여부를 제어
/// </summary>
public class SkillStarter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("스킬 활성화 오브젝트")]
    public GameObject skillStarter; // 인스펙터에서 SkillStarter 연결

    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;
    [HideInInspector] public Transform assignedSlot;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = transform.position;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        transform.position = Input.mousePosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.blocksRaycasts = true;

        GameObject nearestSlot = SlideBarDetector.GetNearestSlot(transform.position);
        if (nearestSlot != null)
        {
            assignedSlot = nearestSlot.transform;
            transform.position = assignedSlot.position;
            Debug.Log("슬롯에 장착됨");
            if (skillStarter != null)
            {
                skillStarter.SetActive(true); // 스킬 사용 가능
            }
        }
        else
        {
            assignedSlot = null;
            transform.position = originalPosition;
            Debug.Log("슬롯 이탈: 원위치");
            if (skillStarter != null)
            {
                skillStarter.SetActive(false); // 사용 불가
            }
        }
    }
}
