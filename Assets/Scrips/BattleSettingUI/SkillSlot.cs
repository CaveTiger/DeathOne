using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

/// <summary>
/// 주인공 스킬 세팅에서 스킬 한 개의 슬롯 역할을 담당.
/// 슬롯 기반 단방향 전달 구조: currentSkillID만 관리하고 GetSkillID()로 반환
/// </summary>
public class SkillSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI")]
    [SerializeField] private Image skillIcon; // 스킬 아이콘을 표시할 UI
    [SerializeField] private int slotIndex; // 슬롯 인덱스 (0-3)
    [SerializeField] private Collider2D slotCollider; // 슬롯 콜라이더
    
    public bool isPointerOver = false; // 마우스가 슬롯 위에 있는지 확인

    // public 프로퍼티로 slotIndex에 직접 접근 가능하도록 추가
    public int SlotIndex => slotIndex;

    public SkillBlock currentSkillBlock;
    private string currentSkillID = ""; // 현재 슬롯에 배치된 스킬 ID

    private void Awake()
    {
        Debug.Log($"[SlotBased][SkillSlot{slotIndex}] 슬롯 초기화 완료");
    }

    public void Initialize(int index, string skillID)
    {
        slotIndex = index;
        currentSkillID = skillID;
        UpdateSlotUI(skillID);
        Debug.Log($"[SlotBased][SkillSlot{slotIndex}] Initialize: {skillID}");
    }

    public void PlaceSkillBlock(SkillBlock skillBlock)
    {
        if (skillBlock == null)
        {
            Debug.LogError("[SlotBased][SkillSlot] skillBlock이 null입니다!");
            return;
        }

        // 다른 슬롯에서 이동해 온 경우, 이전 슬롯의 참조/ID를 먼저 정리한다.
        SkillSlot previousSlot = skillBlock.GetComponentInParent<SkillSlot>();
        if (previousSlot != null && previousSlot != this && previousSlot.currentSkillBlock == skillBlock)
        {
            previousSlot.currentSkillBlock = null;
            string previousFallbackSkill = BattleSettingManager.Instance != null
                ? BattleSettingManager.Instance.GetDefaultSkillForSlot(previousSlot.slotIndex)
                : "";
            previousSlot.currentSkillID = previousFallbackSkill;
            previousSlot.UpdateSlotUI(previousFallbackSkill);

            if (BattleSettingManager.Instance != null)
            {
                BattleSettingManager.Instance.SetPlayerSkill(previousSlot.slotIndex, previousFallbackSkill);
            }

            Debug.Log($"[SlotBased][SkillSlot{previousSlot.slotIndex}] 이동으로 기존 슬롯 기본기 복구: {previousFallbackSkill}");
        }
        
        // 기존 스킬 블록이 있으면 인벤토리로 반환
        if (currentSkillBlock != null && currentSkillBlock != skillBlock)
        {
            Debug.Log($"[SlotBased][SkillSlot{slotIndex}] 기존 스킬 블록을 인벤토리로 반환: {currentSkillBlock.skillData?.Name}");
            if (SkillInventoryTab.Instance != null)
            {
                SkillInventoryTab.Instance.ReturnSkillBlock(currentSkillBlock);
            }
            currentSkillBlock = null;
        }

        // 새 블럭을 슬롯의 자식으로 이동
        skillBlock.transform.SetParent(this.transform, false);

        // RectTransform 값 초기화 (중앙 정렬)
        var rectTransform = skillBlock.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
        }

        // SkillBlock 컴포넌트 참조 갱신
        currentSkillBlock = skillBlock;

        // currentSkillID 갱신 (단방향 전달용)
        if (currentSkillBlock != null && currentSkillBlock.skillData != null)
        {
            currentSkillID = currentSkillBlock.skillData.ID;
            UpdateSlotUI(currentSkillID);
            Debug.Log($"[SlotBased][SkillSlot{slotIndex}] PlaceSkillBlock: {currentSkillID}");
        }
        else
        {
            Debug.LogError("[SlotBased][SkillSlot] currentSkillBlock 또는 skillData가 null입니다!");
            return;
        }

        // BattleSettingManager에 반영 (단방향 전달)
        if (BattleSettingManager.Instance != null)
        {
            BattleSettingManager.Instance.SetPlayerSkill(slotIndex, currentSkillID);
        }
    }

    public void RemoveSkillBlock()
    {
        if (currentSkillBlock != null)
        {
            Debug.Log($"[SlotBased][SkillSlot{slotIndex}] 스킬 블록을 인벤토리로 반환: {currentSkillBlock.skillData?.Name}");
            
            // 인벤토리로 반환
            if (SkillInventoryTab.Instance != null)
            {
                SkillInventoryTab.Instance.ReturnSkillBlock(currentSkillBlock);
            }
            
            var removedSkillID = currentSkillID;
            currentSkillBlock = null;
            string fallbackSkill = BattleSettingManager.Instance != null
                ? BattleSettingManager.Instance.GetDefaultSkillForSlot(slotIndex)
                : "";
            currentSkillID = fallbackSkill;
            UpdateSlotUI(fallbackSkill);
            
            Debug.Log($"[SlotBased][SkillSlot{slotIndex}] 스킬 제거: {removedSkillID}, 기본기 복구: {fallbackSkill}");
            
            // BattleSettingManager에 반영 (단방향 전달)
            if (BattleSettingManager.Instance != null)
            {
                BattleSettingManager.Instance.SetPlayerSkill(slotIndex, fallbackSkill);
            }
        }
    }

    private void UpdateSlotUI(string skillID)
    {
        if (string.IsNullOrEmpty(skillID))
        {
            // 빈 슬롯: 아이콘만 비움
            if (skillIcon != null) skillIcon.sprite = null;
            return;
        }
        
        if (SkillData.skillDict.TryGetValue(skillID, out var skillData))
        {
            if (skillIcon != null && !string.IsNullOrEmpty(skillData.Icon))
            {
                Sprite iconSprite = Resources.Load<Sprite>(skillData.Icon);
                if (iconSprite != null)
                {
                    skillIcon.sprite = iconSprite;
                }
                else
                {
                    Debug.LogWarning($"[SlotBased][SkillSlot{slotIndex}] 스킬 아이콘을 찾을 수 없습니다: {skillData.Icon}");
                    skillIcon.sprite = null;
                }
            }
        }
        else
        {
            Debug.LogWarning($"[SlotBased][SkillSlot{slotIndex}] 스킬 데이터를 찾을 수 없습니다: {skillID}");
            if (skillIcon != null) skillIcon.sprite = null;
        }
    }

    /// <summary>
    /// 현재 슬롯의 스킬ID를 반환 (단방향 전달용)
    /// </summary>
    public string GetSkillID()
    {
        return currentSkillID;
    }

    public SkillData GetSkillData()
    {
        if (string.IsNullOrEmpty(currentSkillID)) return null;
        if (SkillData.skillDict.TryGetValue(currentSkillID, out var skillData))
        {
            return skillData;
        }
        return null;
    }

    public void SetSkill(string skillID)
    {
        // 기존 스킬 블록이 있으면 인벤토리로 반환
        if (currentSkillBlock != null)
        {
            RemoveSkillBlock();
        }

        // 새 스킬 설정
        if (!string.IsNullOrEmpty(skillID))
        {
            currentSkillID = skillID;
            UpdateSlotUI(skillID);
            Debug.Log($"[SlotBased][SkillSlot{slotIndex}] SetSkill: {skillID}");
            
            // BattleSettingManager에 반영 (단방향 전달)
            if (BattleSettingManager.Instance != null)
            {
                BattleSettingManager.Instance.SetPlayerSkill(slotIndex, skillID);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        Debug.Log($"[SkillSlot{slotIndex}] 마우스 진입");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        Debug.Log($"[SkillSlot{slotIndex}] 마우스 이탈");
    }
} 