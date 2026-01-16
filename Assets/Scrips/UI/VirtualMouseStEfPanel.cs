using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 상태이상 상세 팝업 패널 (VirtualMouse 계열, StEf=StatusEffect 약어)
/// </summary>
public class VirtualMouseStEfPanel : MonoBehaviour
{
    public TextMeshProUGUI damageLabelText;
    public TextMeshProUGUI damageValueText;
    public TextMeshProUGUI durationLabelText;
    public TextMeshProUGUI durationValueText;

    [Header("Visibility Control")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool isVisible = false;

    private void Start()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();

        SetVisible(false, true);
    }

    /// <summary>
    /// 상태이상 팝업을 표시합니다.
    /// </summary>
    public void ShowStatusPopup(Sprite icon, string description, int damage, int turns)
    {
        // Icon/Description 의존성 제거: 전달값 무시

        // 오브젝트가 활성화되어 있는지 확인 (코루틴 시작 전 필수)
        if (!gameObject.activeInHierarchy)
        {
            EnsureActiveHierarchy();
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
        }

        // 값 세팅
        if (damageValueText != null) damageValueText.text = damage.ToString();
        if (durationValueText != null) durationValueText.text = turns.ToString();

        SetVisible(true, false);
    }

    /// <summary>
    /// 상태이상 팝업을 숨깁니다.
    /// </summary>
    public void HideStatusPopup()
    {
        SetVisible(false, false);
    }

    /// <summary>
    /// 패널 가시성 제어 (스킬 패널과 유사한 통제)
    /// </summary>
    public void SetVisible(bool visible, bool immediate)
    {
        isVisible = visible;
        if (immediate)
        {
            if (visible)
            {
                EnsureActiveHierarchy();
                if (!gameObject.activeInHierarchy)
                    gameObject.SetActive(true);
            }
            canvasGroup.alpha = visible ? 1f : 0f;
            canvasGroup.interactable = visible;
            canvasGroup.blocksRaycasts = visible;
            if (!visible) gameObject.SetActive(false);
            return;
        }

        // 간단한 페이드 처리
        StopAllCoroutines();
        StartCoroutine(visible ? FadeTo(1f) : FadeTo(0f, deactivateOnEnd: true));
    }

    private System.Collections.IEnumerator FadeTo(float target, bool deactivateOnEnd = false)
    {
        float duration = 0.15f;
        float start = canvasGroup.alpha;
        float t = 0f;
        if (target > start)
        {
            EnsureActiveHierarchy();
            if (!gameObject.activeInHierarchy) gameObject.SetActive(true);
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
        while (t < duration)
        {
            t += Time.deltaTime;
            float p = Mathf.Clamp01(t / duration);
            canvasGroup.alpha = Mathf.Lerp(start, target, p);
            yield return null;
        }
        canvasGroup.alpha = target;
        canvasGroup.interactable = target > 0.99f;
        canvasGroup.blocksRaycasts = target > 0.99f;
        if (deactivateOnEnd && target <= 0.01f)
            gameObject.SetActive(false);
    }

    private void EnsureActiveHierarchy()
    {
        Transform tr = transform;
        while (tr != null)
        {
            if (!tr.gameObject.activeSelf) tr.gameObject.SetActive(true);
            tr = tr.parent;
        }
    }
}


