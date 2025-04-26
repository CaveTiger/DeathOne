using UnityEngine;
using UnityEngine.EventSystems;

public class SkillButtonDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Vector3 originalPosition;
    private CanvasGroup canvasGroup;

    [HideInInspector] public Transform assignedSlot;

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
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
            Debug.Log("½½·Ô¿¡ ÀåÂøµÊ");
        }
        else
        {
            assignedSlot = null;
            transform.position = originalPosition;
            Debug.Log("½½·Ô ÀÌÅ»: ¿øÀ§Ä¡");
        }
    }
}
