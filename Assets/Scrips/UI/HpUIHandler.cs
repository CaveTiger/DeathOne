using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class HpUIHandler : MonoBehaviour
{
    public Transform target; // 따라갈 캐릭터
    [Header("레거시(스프라이트 바)")]
    public SpriteRenderer hpBarFront;
    [Header("월드 UI(Image 바)")]
    [SerializeField] private Image hpBarFrontImage;
    [SerializeField] private TMP_Text hpText;
    [SerializeField] private bool autoBindOnAwake = true;
    public Vector3 offset = new Vector3(0, 1.5f, 0);

    private void Awake()
    {
        if (autoBindOnAwake)
            TryAutoBindReferences();
    }

    private void LateUpdate()
    {
        UpdateHpBarPosition();
    }

    private void TryAutoBindReferences()
    {
        if (target == null && transform.parent != null)
            target = transform.parent;

        if (hpBarFrontImage == null)
        {
            var front = transform.Find("HpFront");
            if (front != null)
                hpBarFrontImage = front.GetComponent<Image>();
        }

        if (hpBarFront == null)
        {
            var front = transform.Find("HpFront");
            if (front != null)
                hpBarFront = front.GetComponent<SpriteRenderer>();
        }

        if (hpText == null)
            hpText = GetComponentInChildren<TMP_Text>(true);
    }

    public void UpdateHpBarPosition()
    {
        if (target != null)
            transform.position = target.position + offset;
    }

    public void UpdateHpBar(int hp, int maxHp)
    {
        int safeMaxHp = Mathf.Max(1, maxHp);
        float ratio = Mathf.Clamp01((float)Mathf.Max(0, hp) / safeMaxHp);

        // 월드 UI(Image) 방식: Fill 사용을 우선, 아니면 스케일로 보정
        if (hpBarFrontImage != null)
        {
            if (hpBarFrontImage.type == Image.Type.Filled)
            {
                hpBarFrontImage.fillAmount = ratio;
            }
            else
            {
                var rect = hpBarFrontImage.rectTransform;
                rect.localScale = new Vector3(ratio, 1f, 1f);
                rect.anchoredPosition = new Vector2((ratio - 1f) * rect.rect.width * 0.5f, rect.anchoredPosition.y);
            }
        }

        // 레거시 SpriteRenderer 방식 호환
        if (hpBarFront != null)
        {
            hpBarFront.transform.localScale = new Vector3(ratio, 1, 1);
            float barWidth = 1f;
            hpBarFront.transform.localPosition = new Vector3((ratio - 1) * barWidth * 0.5f, 0, 0);
        }

        if (hpText != null)
        {
            hpText.text = $"{Mathf.Max(0, hp)}/{safeMaxHp}";
        }
    }
}
