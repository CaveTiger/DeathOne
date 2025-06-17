using UnityEngine;
using UnityEngine.UI;

public class PageOutButton : MonoBehaviour
{
    // 닫을 대상이 되는 페이지(팝업) 오브젝트
    [SerializeField] private Transform page;

    /// <summary>
    /// X버튼 클릭 시 페이지(팝업)를 닫는다.
    /// </summary>
    public void ClosePage()
    {
        if (page != null)
            page.gameObject.SetActive(false);
    }
}
