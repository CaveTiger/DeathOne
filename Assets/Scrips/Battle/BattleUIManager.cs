using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public GameObject uiAll; //UI
    public GameObject battleUI;
    public static BattleUIManager Instance { get; private set; }
    private CanvasGroup uiAllCanvasGroup;
    private CanvasGroup battleUICanvasGroup;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("UIManager 중복! 삭제됨");
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // CanvasGroup 컴포넌트 캐싱
        if (uiAll != null)
        {
            uiAllCanvasGroup = uiAll.GetComponent<CanvasGroup>();
            if (uiAllCanvasGroup == null)
                uiAllCanvasGroup = uiAll.AddComponent<CanvasGroup>();
        }
        if (battleUI != null)
        {
            battleUICanvasGroup = battleUI.GetComponent<CanvasGroup>();
            if (battleUICanvasGroup == null)
                battleUICanvasGroup = battleUI.AddComponent<CanvasGroup>();
        }
    }

    private void Start()
    {
        // battleUI를 처음부터 투명하게 설정
        SetCanvasGroupAlpha(battleUICanvasGroup, 0f);
    }

    public void ChangeUIBattle()
    {
        SetCanvasGroupAlpha(uiAllCanvasGroup, 0f);
        SetCanvasGroupAlpha(battleUICanvasGroup, 1f);
    }
    public void ChangeUINormal()
    {
        SetCanvasGroupAlpha(uiAllCanvasGroup, 1f);
        SetCanvasGroupAlpha(battleUICanvasGroup, 0f);
    }

    private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
    {
        if (cg == null) return;
        cg.alpha = alpha;
        cg.interactable = alpha > 0.5f;
        cg.blocksRaycasts = alpha > 0.5f;
    }
}
