using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfo : MonoBehaviour
{
    protected CharacterStats characterStats;
    
    [Header("UI References")]
    [SerializeField] protected GameObject infoPanel;
    [SerializeField] protected GameObject scrollView; // Scroll View 오브젝트
    [SerializeField] protected ScrollRect scrollRect; // ScrollRect 컴포넌트
    [SerializeField] protected RectTransform contentRect;
    [SerializeField] protected Scrollbar scrollbarVertical; // Vertical Scrollbar
    [SerializeField] protected Text nameText;
    [SerializeField] protected Text hpText;
    [SerializeField] protected Text atkText;
    [SerializeField] protected Text defText;
    [SerializeField] protected Text speedText;
    [SerializeField] protected Text evasionText;
    [SerializeField] protected Text accuracyText;

    [Header("Scroll Settings")]
    [SerializeField] protected float scrollSpeed = 10f;
    [SerializeField] protected float scrollThreshold = 0.1f;

    protected virtual void Awake()
    {
        if (scrollRect != null)
        {
            scrollRect.vertical = true;
            scrollRect.horizontal = false;
            scrollRect.scrollSensitivity = scrollSpeed;
            if (scrollbarVertical != null)
                scrollRect.verticalScrollbar = scrollbarVertical;
        }
    }

    public virtual void SetCharacterStats(CharacterStats stats)
    {
        characterStats = stats;
        UpdateInfo();
        ShowInfo();
        ResetScroll();
    }

    public virtual void UpdateInfo()
    {
        if (characterStats == null) return;
        
        Name = characterStats.Label;
        CHp = characterStats.Hp;
        Atk = characterStats.Atk;
        Def = characterStats.Def;
        Speed = characterStats.Speed;
        Evasion = characterStats.Evasion;
        Accuracy = characterStats.Accuracy;
    }

    public virtual void ShowInfo()
    {
        if (characterStats == null) return;

        if (infoPanel != null)
            infoPanel.SetActive(true);

        if (nameText != null)
            nameText.text = $"{Name}";
        if (hpText != null)
            hpText.text = $"{CHp}";
        if (atkText != null)
            atkText.text = $"{Atk}";
        if (defText != null)
            defText.text = $"{Def}";
        if (speedText != null)
            speedText.text = $"{Speed}";
        if (evasionText != null)
            evasionText.text = $"{Evasion:P0}";
        if (accuracyText != null)
            accuracyText.text = $"{Accuracy:P0}";
    }

    protected virtual void ResetScroll()
    {
        if (scrollRect != null)
        {
            scrollRect.verticalNormalizedPosition = 1f; // 맨 위로 스크롤
        }
    }

    public virtual void HideInfo()
    {
        if (infoPanel != null)
            infoPanel.SetActive(false);
    }

    public string Name { get; protected set; }
    public int CHp { get; protected set; }
    public int Atk { get; protected set; }
    public int Def { get; protected set; }
    public int Speed { get; protected set; }
    public float Evasion { get; protected set; }
    public float Accuracy { get; protected set; }
}
