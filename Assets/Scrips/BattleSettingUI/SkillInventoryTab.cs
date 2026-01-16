using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class SkillInventoryTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static SkillInventoryTab Instance { get; private set; }
    
    [Header("UI 연결")]
    [SerializeField] public Transform skillListContainer;
    [SerializeField] private SkillBlock skillBlockPrefab;

    private List<SkillBlock> skillBlocks = new List<SkillBlock>();
    public bool isPointerOver = false;
    private bool isInitialized = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            RefreshSkillInventory();
            isInitialized = true;
        }
    }

    public void RefreshSkillInventory()
    {
        Debug.Log("[SkillInventory] 스킬 인벤토리 새로고침 시작");
        
        try
        {
            if (GameProgressManager.Instance == null)
            {
                Debug.LogError("[SkillInventory] GameProgressManager를 찾을 수 없습니다!");
                return;
            }

            if (skillBlockPrefab == null)
            {
                Debug.LogError("[SkillInventory] skillBlockPrefab이 null입니다!");
                return;
            }

            if (skillListContainer == null)
            {
                Debug.LogError("[SkillInventory] skillListContainer가 null입니다!");
                return;
            }

            ClearSkillList();

            // 해금된 스킬들만 블록 생성
            int createdCount = 0;
            var unlockedSkills = GameProgressManager.Instance.CurrentSaveData.unlockedSkills;
            if (unlockedSkills == null)
            {
                Debug.LogWarning("[SkillInventory] unlockedSkills가 null입니다.");
                return;
            }
            
            foreach (var skillID in unlockedSkills)
            {
                if (string.IsNullOrEmpty(skillID))
                {
                    Debug.LogWarning("[SkillInventory] 빈 스킬 ID 발견, 건너뜀");
                    continue;
                }
                
                if (SkillData.skillDict.TryGetValue(skillID, out var skillData))
                {
                    if (skillData == null)
                    {
                        Debug.LogWarning($"[SkillInventory] 스킬 데이터가 null입니다: {skillID}");
                        continue;
                    }
                    
                    try
                    {
                        SkillBlock newBlock = Instantiate(skillBlockPrefab, skillListContainer);
                        if (newBlock != null)
                        {
                            newBlock.Initialize(skillData);
                            newBlock.name = $"SkillBlock_{skillData.Name}";
                            skillBlocks.Add(newBlock);
                            createdCount++;
                        }
                        else
                        {
                            Debug.LogError("[SkillInventory] SkillBlock 생성 실패");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[SkillInventory] 스킬 블록 생성 중 오류 - {skillID}: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SkillInventory] 스킬 데이터를 찾을 수 없습니다: {skillID}");
                }
            }
            Debug.Log($"[SkillInventory] 총 {createdCount}개의 해금된 스킬 블록을 생성했습니다.");

            // 슬롯에 세팅된 스킬ID를 받아와서 사용불가 처리 (안전한 방식으로 개선)
            RefreshSkillBlockInteractable(GetUsedSkillIDsSafely());

            // ScrollRect 작동을 위한 Content Size Fitter 설정 확인
            CheckAndFixContentSizeFitter();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkillInventory] 스킬 인벤토리 새로고침 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 안전하게 사용 중인 스킬ID를 가져오는 메서드
    /// </summary>
    private List<string> GetUsedSkillIDsSafely()
    {
        try
        {
            // 1차: SkillPresetHandler에서 가져오기 시도
            var presetHandler = FindFirstObjectByType<SkillPresetHandler>();
            if (presetHandler != null)
            {
                var usedSkillIDs = presetHandler.GetUsedSkillIDs();
                Debug.Log($"[SkillInventory] SkillPresetHandler에서 사용 중인 스킬 {usedSkillIDs.Count}개 발견");
                return usedSkillIDs;
            }

            // 2차: BattleSettingManager에서 직접 가져오기
            if (BattleSettingManager.Instance != null)
            {
                var partySkillIDs = BattleSettingManager.Instance.GetPartySkillIDsFromSlots();
                var usedSkillIDs = partySkillIDs.Where(id => !string.IsNullOrEmpty(id)).ToList();
                Debug.Log($"[SkillInventory] BattleSettingManager에서 사용 중인 스킬 {usedSkillIDs.Count}개 발견");
                return usedSkillIDs;
            }

            // 3차: 모두 사용 가능 처리
            Debug.Log("[SkillInventory] 사용 중인 스킬 정보를 찾을 수 없어 모든 스킬을 사용 가능으로 설정");
            return new List<string>();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SkillInventory] 사용 중인 스킬 정보 가져오기 중 오류 발생: {e.Message}");
            return new List<string>();
        }
    }

    /// <summary>
    /// 슬롯에 세팅된 스킬ID를 받아 인벤토리 SkillBlock의 사용 가능/불가 상태를 갱신
    /// </summary>
    public void RefreshSkillBlockInteractable(List<string> usedSkillIDs)
    {
        foreach (var block in skillBlocks)
        {
            bool isUsed = usedSkillIDs.Contains(block.skillData.ID);
            block.SetInteractable(!isUsed);
        }
    }

    private void ClearSkillList()
    {
        foreach (var block in skillBlocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
        skillBlocks.Clear();
    }

    /// <summary>
    /// 스킬 블록을 인벤토리로 반환합니다.
    /// </summary>
    public void ReturnSkillBlock(SkillBlock block)
    {
        if (block == null) return;

        block.gameObject.SetActive(true);
        block.transform.SetParent(skillListContainer, true);
        
        // RectTransform 속성을 명시적으로 설정
        var rectTransform = block.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }
        
        if (!skillBlocks.Contains(block))
        {
            skillBlocks.Add(block);
        }
        
        Debug.Log($"[SkillInventory] 스킬 블록 반환: {block.skillData.Name}");
    }

    /// <summary>
    /// 특정 스킬이 해금되어 있는지 확인합니다.
    /// </summary>
    public bool IsSkillUnlocked(string skillId)
    {
        if (GameProgressManager.Instance == null) return false;
        return GameProgressManager.Instance.IsSkillUnlocked(skillId);
    }

    /// <summary>
    /// 특정 스킬의 데이터를 반환합니다.
    /// </summary>
    public SkillData GetSkillData(string skillId)
    {
        if (SkillData.skillDict.TryGetValue(skillId, out var skillData))
        {
            return skillData;
        }
        return null;
    }

    /// <summary>
    /// 해금된 모든 스킬 데이터를 반환합니다.
    /// </summary>
    public List<SkillData> GetUnlockedSkills()
    {
        List<SkillData> unlockedSkills = new List<SkillData>();
        
        if (GameProgressManager.Instance == null) return unlockedSkills;

        foreach (var skillID in GameProgressManager.Instance.CurrentSaveData.unlockedSkills)
        {
            if (SkillData.skillDict.TryGetValue(skillID, out var skillData))
            {
                unlockedSkills.Add(skillData);
            }
        }
        
        return unlockedSkills;
    }

    /// <summary>
    /// Content Size Fitter 설정을 확인하고 ScrollRect 작동을 위해 수정
    /// </summary>
    private void CheckAndFixContentSizeFitter()
    {
        if (skillListContainer == null) return;

        var contentSizeFitter = skillListContainer.GetComponent<UnityEngine.UI.ContentSizeFitter>();
        if (contentSizeFitter != null)
        {
            // Vertical Fit이 Unconstrained면 Preferred Size로 변경
            if (contentSizeFitter.verticalFit == UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained)
            {
                contentSizeFitter.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
                Debug.Log("[SkillInventory] Content Size Fitter Vertical Fit을 Preferred Size로 변경했습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[SkillInventory] Content에 Content Size Fitter 컴포넌트가 없습니다. Unity Inspector에서 추가해주세요.");
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }
} 