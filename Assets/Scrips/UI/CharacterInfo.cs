using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CharacterInfo : MonoBehaviour
{
    protected CharacterStats characterStats;

    [Header("Info Role")]
    [Tooltip("이 Info UI가 턴 대상 전용이면 true")]
    [SerializeField] private bool isTurnTargetInfo = true;
    [Tooltip("이 Info UI가 선택 대상 전용이면 true")]
    [SerializeField] private bool isSelectedTargetInfo = false;
    
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
    [SerializeField] protected Text collapseChanceText; // 붕괴확률 표시용 텍스트

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

    private void OnValidate()
    {
        // 둘 다 true면 역할이 모호해지므로 인스펙터에서 바로 확인 가능하게 경고
        if (isTurnTargetInfo && isSelectedTargetInfo)
        {
            Debug.LogWarning("[CharacterInfo] isTurnTargetInfo와 isSelectedTargetInfo가 둘 다 true입니다. 역할이 모호할 수 있습니다.", this);
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
        
        // 붕괴 확률 표시 (아군만)
        if (collapseChanceText != null && characterStats != null)
        {
            bool isAllyNotMainCharacter = characterStats.IsPlayer && 
                                         !string.IsNullOrEmpty(characterStats.CharacterId) && 
                                         characterStats.CharacterId != "000001";
            
            if (isAllyNotMainCharacter)
            {
                collapseChanceText.text = $"{CollapseChance:P0}"; // 퍼센트로 표시
                collapseChanceText.gameObject.SetActive(true); // 텍스트 활성화
            }
            else
            {
                collapseChanceText.gameObject.SetActive(false); // 적이나 주인공은 숨김
            }
        }
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

    /// <summary>
    /// 인스펙터에서 지정한 역할이 '턴 대상 UI'인지 반환합니다.
    /// </summary>
    public bool IsTurnTargetInfo()
    {
        return isTurnTargetInfo;
    }

    /// <summary>
    /// 인스펙터에서 지정한 역할이 '선택 대상 UI'인지 반환합니다.
    /// </summary>
    public bool IsSelectedTargetInfo()
    {
        return isSelectedTargetInfo;
    }

    public string Name { get; protected set; }
    public int CHp { get; protected set; }
    public int Atk { get; protected set; }
    public int Def { get; protected set; }
    public int Speed { get; protected set; }
    public float Evasion { get; protected set; }
    public float Accuracy { get; protected set; }
    public float CollapseChance { get; protected set; }
}
