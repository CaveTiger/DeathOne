using UnityEngine;
using UnityEngine.EventSystems;

public class TimeLineBlock : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    //public TimelineActionLog actionLog;
    //public BattleSnapshot snapshot;

    // === 시간 되돌리기 및 소유 정보 ===
    public bool MyBlock;         // 이 블록이 내 것인지(플레이어가 만든 것인지)
    public bool MyTurn;          // 눌릴때 내 턴이 맞는지
    public bool ReturnOnline;    // 시간 되돌리기(롤백) 가능 여부
    public int ReturnCount;      // 남은 시간 되돌리기 횟수

    public void OnPointerEnter(PointerEventData eventData)
    {
        //TimeLinePopupUI.Instance.ShowPopup(actionLog, transform.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        TimeLinePopupUI.Instance.HidePopup();
    }
}
