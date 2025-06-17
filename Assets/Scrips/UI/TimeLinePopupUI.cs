using UnityEngine;
using UnityEngine.UI;

public class TimeLinePopupUI : MonoBehaviour
{
    public static TimeLinePopupUI Instance;
    [SerializeField] private GameObject popupPanel;
    [SerializeField] private Text popupText;

    private void Awake()
    {
        Instance = this;
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }

    //public void ShowPopup(TimelineActionLog log, Vector3 position)
    //{
    //    if (popupPanel == null || popupText == null) return;
    //    popupPanel.SetActive(true);
    //    popupPanel.transform.position = position;
    //    popupText.text = log.ToDisplayString(); // 상세 정보 표시
    //}

    public void HidePopup()
    {
        if (popupPanel != null)
            popupPanel.SetActive(false);
    }
} 