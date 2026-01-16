using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// [레거시] 상태이상 팝업 핸들러
/// VirtualMouse가 VirtualMouseStEfPanel을 직접 사용하도록 변경되어 더 이상 사용되지 않음
/// </summary>
[System.Obsolete("StatusPopupHandler는 레거시 코드입니다. VirtualMouse가 VirtualMouseStEfPanel을 직접 사용합니다.")]
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
