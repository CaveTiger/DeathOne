using UnityEngine;
using UnityEngine.UI;

public class StatusPopupHandler : MonoBehaviour
{
    public Image Icon;
    public Text Description;
    public Text DamageAndTurn;

    private void Start()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 상태이상 팝업을 표시합니다.
    /// </summary>
    /// <param name="icon">상태이상 아이콘</param>
    /// <param name="description">상태이상 설명</param>
    /// <param name="damage">데미지 값</param>
    /// <param name="turns">남은 턴 수</param>
    public void ShowStatusPopup(Sprite icon, string description, int damage, int turns)
    {
        Icon.sprite = icon; //내부 값을 매개변수값으로 수정
        Description.text = description;
        DamageAndTurn.text = $"{damage} / {turns}";
        gameObject.SetActive(true); //활성화
    }

    /// <summary>
    /// 상태이상 팝업을 숨깁니다.
    /// </summary>
    public void HideStatusPopup()
    {
        gameObject.SetActive(false);
    }
}
