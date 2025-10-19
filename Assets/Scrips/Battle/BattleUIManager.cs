using UnityEngine;

public class BattleUIManager : MonoBehaviour
{
    public GameObject uiAll; //UI
    public GameObject battleUI;
    public static BattleUIManager Instance { get; private set; }
    private CanvasGroup uiAllCanvasGroup;
    private CanvasGroup battleUICanvasGroup;
    
    // 전투 모드 상태 추적
    public bool IsInBattleMode { get; private set; } = false;

    [Header("스킬 UI 관리")]
    public Transform skillSetRoot; // 스킬 세트 루트 오브젝트
    public GameObject skillButtonSet; // SkillButtonSet 오브젝트(전체 스킬 UI)

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
        if (skillButtonSet != null)
            skillButtonSet.SetActive(true);
    }

    public void ChangeUIBattle()
    {
        SetCanvasGroupAlpha(uiAllCanvasGroup, 0f);
        SetCanvasGroupAlpha(battleUICanvasGroup, 1f);
        IsInBattleMode = true; // 전투 모드 활성화
    }
    public void ChangeUINormal()
    {

        
        SetCanvasGroupAlpha(uiAllCanvasGroup, 1f);
        SetCanvasGroupAlpha(battleUICanvasGroup, 0f);
        IsInBattleMode = false; // 전투 모드 비활성화
    }

    private void SetCanvasGroupAlpha(CanvasGroup cg, float alpha)
    {
        if (cg == null) return;
        cg.alpha = alpha;
        cg.interactable = alpha > 0.5f;
        cg.blocksRaycasts = alpha > 0.5f;
    }

    /// <summary>
    /// 턴이 온 캐릭터의 스킬 UI만 활성화하고 나머지는 비활성화합니다.
    /// </summary>
    /// <param name="currentCharacter">현재 턴인 캐릭터</param>
    public void UpdateSkillUIForTurn(CharacterStats currentCharacter)
    {
        Debug.Log($"[AI개선] UpdateSkillUIForTurn 시작 - 캐릭터: {currentCharacter?.Label}, 플레이어여부: {currentCharacter?.IsPlayer}");
        float startTime = Time.realtimeSinceStartup;
        
        if (skillSetRoot == null)
        {
            Debug.LogWarning("[BattleUIManager] skillSetRoot가 할당되지 않았습니다.");
            return;
        }
        // 플레이어 차례가 아니면 전체 스킬버튼셋 비활성화
        if (skillButtonSet != null)
        {
            skillButtonSet.SetActive(currentCharacter != null && currentCharacter.IsPlayer);
            Debug.Log($"[AI개선] UpdateSkillUIForTurn - SkillButtonSet 활성화: {skillButtonSet.activeSelf}");
        }

        // 모든 스킬 슬롯을 순회
        Debug.Log("[AI개선] UpdateSkillUIForTurn - 스킬 슬롯 업데이트 시작");
        for (int slotIdx = 0; slotIdx < 4; slotIdx++)
        {
            Transform slot = skillSetRoot.Find($"SkillSlot{slotIdx + 1}");
            if (slot == null) continue;

            // 각 슬롯의 모든 스킬 버튼을 순회
            foreach (Transform child in slot)
            {
                var skillInstance = child.GetComponent<SkillInstance>();
                if (skillInstance != null)
                {
                    // 현재 턴인 캐릭터의 스킬만 활성화
                    bool shouldBeActive = currentCharacter != null && 
                                        currentCharacter.IsPlayer && 
                                        skillInstance.GetCaster() == currentCharacter;
                    
                    child.gameObject.SetActive(shouldBeActive);
                    Debug.Log($"[AI개선] UpdateSkillUIForTurn - 슬롯{slotIdx + 1} 스킬 활성화: {shouldBeActive}");
                }
            }
        }
        Debug.Log("[AI개선] UpdateSkillUIForTurn - 스킬 슬롯 업데이트 완료");
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] UpdateSkillUIForTurn 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    /// <summary>
    /// 모든 스킬 UI를 비활성화합니다.
    /// </summary>
    public void DisableAllSkillUI()
    {
        Debug.Log("[AI개선] DisableAllSkillUI 시작");
        float startTime = Time.realtimeSinceStartup;
        
        if (skillButtonSet != null)
        {
            skillButtonSet.SetActive(false);
            Debug.Log("[AI개선] DisableAllSkillUI - SkillButtonSet 비활성화");
        }
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] DisableAllSkillUI 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }


}
