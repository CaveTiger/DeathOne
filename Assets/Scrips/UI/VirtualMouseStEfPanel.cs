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

    [Tooltip("스턴 등 수치 없는 상태이상용 본문. 비어 있으면 durationValueText에 설명만 넣고 위쪽 수치 줄은 숨깁니다.")]
    [SerializeField] private TextMeshProUGUI descriptionBodyText;

    [Header("Visibility Control")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private bool isVisible = false;

    /// <summary>
    /// CanvasGroup은 반드시 이 패널 GameObject에 붙어 있어야 함.
    /// 인스펙터에서 실수로 VirtualMouse 루트의 CanvasGroup을 넣으면,
    /// SetVisible(false)가 전체 캔버스 알파를 0으로 만들어 스킬 패널까지 안 보임.
    /// </summary>
    private void EnsureCanvasGroupOnThisPanel()
    {
        if (canvasGroup != null && canvasGroup.gameObject != gameObject)
        {
            Debug.LogWarning(
                $"[VirtualMouseStEfPanel] CanvasGroup이 다른 오브젝트('{canvasGroup.gameObject.name}')를 가리킵니다. " +
                $"'{name}' 전용으로 교체합니다.");
            canvasGroup = null;
        }

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    private void Awake()
    {
        EnsureCanvasGroupOnThisPanel();
        EnsureDescriptionReference();
        SetDescriptionActive(false);
    }

    private void Start()
    {
        EnsureCanvasGroupOnThisPanel();
        EnsureDescriptionReference();
        SetDescriptionActive(false);
        SetVisible(false, true);
    }

    /// <summary>
    /// 상태이상 팝업을 표시합니다.
    /// </summary>
    /// <param name="useDescriptionInsteadOfNumbers">true면 설명 문구만 표시하고 피해/지속 수치 줄은 숨깁니다(기절 등).</param>
    public void ShowStatusPopup(Sprite icon, string description, int damage, int turns, bool useDescriptionInsteadOfNumbers = false)
    {
        // 오브젝트가 활성화되어 있는지 확인 (코루틴 시작 전 필수)
        if (!gameObject.activeInHierarchy)
        {
            EnsureActiveHierarchy();
            if (!gameObject.activeInHierarchy)
            {
                gameObject.SetActive(true);
            }
        }

        EnsureDescriptionReference();
        bool descMode = useDescriptionInsteadOfNumbers && !string.IsNullOrWhiteSpace(description);
        if (descMode)
        {
            if (descriptionBodyText != null)
            {
                SetDescriptionActive(true);
                descriptionBodyText.text = description;
                SetNumericRowsActive(false);
            }
            else
            {
                if (damageLabelText != null) damageLabelText.gameObject.SetActive(false);
                if (damageValueText != null) damageValueText.gameObject.SetActive(false);
                if (durationLabelText != null) durationLabelText.gameObject.SetActive(false);
                if (durationValueText != null)
                {
                    durationValueText.gameObject.SetActive(true);
                    durationValueText.text = description;
                }
            }
        }
        else
        {
            if (descriptionBodyText != null)
            {
                SetDescriptionActive(false);
            }
            SetNumericRowsActive(true);
            if (damageValueText != null) damageValueText.text = damage.ToString();
            if (durationValueText != null) durationValueText.text = turns.ToString();
        }

        SetVisible(true, false);
    }

    private void SetNumericRowsActive(bool active)
    {
        if (damageLabelText != null) damageLabelText.gameObject.SetActive(active);
        if (damageValueText != null) damageValueText.gameObject.SetActive(active);
        if (durationLabelText != null) durationLabelText.gameObject.SetActive(active);
        if (durationValueText != null) durationValueText.gameObject.SetActive(active);
    }

    /// <summary>
    /// Description 텍스트 참조가 비어 있으면 자식에서 자동 연결 시도.
    /// </summary>
    private void EnsureDescriptionReference()
    {
        if (descriptionBodyText != null)
            return;

        Transform descriptionTransform = transform.Find("Description");
        if (descriptionTransform == null)
            return;

        descriptionBodyText = descriptionTransform.GetComponent<TextMeshProUGUI>();
    }

    private void SetDescriptionActive(bool active)
    {
        if (descriptionBodyText != null)
            descriptionBodyText.gameObject.SetActive(active);
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
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        // 비활성 오브젝트에서는 StartCoroutine 불가(OnDestroy·Dismiss 경로 등)
        if (!visible && !gameObject.activeInHierarchy)
        {
            StopAllCoroutines();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
            return;
        }

        if (immediate)
        {
            if (visible)
            {
                EnsureActiveHierarchy();
                if (!gameObject.activeInHierarchy)
                    gameObject.SetActive(true);
            }
            if (canvasGroup != null)
            {
                canvasGroup.alpha = visible ? 1f : 0f;
                canvasGroup.interactable = visible;
                canvasGroup.blocksRaycasts = visible;
            }
            if (!visible) gameObject.SetActive(false);
            return;
        }

        // 간단한 페이드 처리 (반드시 활성 계층에서만)
        if (!gameObject.activeInHierarchy)
        {
            SetVisible(visible, true);
            return;
        }

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


