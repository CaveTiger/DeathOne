//using UnityEngine;
//using UnityEngine.EventSystems;

//public class SkillStarter : MonoBehaviour
//{
//    public GameObject skillStarter; // 인스펙터에서 SkillStarter 연결

//    public void OnEndDrag(PointerEventData eventData)
//    {
//        canvasGroup.blocksRaycasts = true;

//        GameObject nearestSlot = SlideBarDetector.GetNearestSlot(transform.position);
//        if (nearestSlot != null)
//        {
//            assignedSlot = nearestSlot.transform;
//            transform.position = assignedSlot.position;
//            Debug.Log("슬롯에 장착됨");
//            skillStarter.SetActive(true); //  스킬 사용 가능
//        }
//        else
//        {
//            assignedSlot = null;
//            transform.position = originalPosition;
//            Debug.Log("슬롯 이탈: 원위치");
//            skillStarter.SetActive(false); //  사용 불가
//        }
//    }
//}
