using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// StatusValueBlock 프리팹을 관리하여 상태이상의 값/지속시간을 표시
/// 스킬 정보 패널에서 상태이상의 간단한 정보(아이콘, 값/지속시간)만 표시
/// </summary>
public class StatusValueBlockManager : MonoBehaviour
{
    [Header("프리팹 및 컨테이너")]
    [SerializeField] private GameObject statusValueBlockPrefab; // StatusValueBlock 프리팹
    [SerializeField] private Transform containerTransform; // StatusEffectList 등 상위 컨테이너
    
    private List<GameObject> currentValueBlocks = new List<GameObject>();
    
    [Header("디버그")]
    [SerializeField] private bool debugValueBlocks = true; // 요약 로그
    
    void Awake()
    {
        if (statusValueBlockPrefab == null)
            statusValueBlockPrefab = Resources.Load<GameObject>("Prefab/StatusValueBlock");
        
        if (containerTransform == null)
            containerTransform = transform; // 자기 자신을 컨테이너로 사용
    }
    
    /// <summary>
    /// 스킬 데이터의 상태이상 정보를 값/지속시간 형식으로 표시
    /// </summary>
    public void UpdateStatusValueBlocks(SkillData skillData)
    {
        ClearAllBlocks();
        
        if (skillData == null || skillData.skillEffects == null || skillData.skillEffects.Count == 0)
            return;
        
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("[StatusValueBlockManager] StatusEffectManager.Instance가 null입니다.");
            return;
        }
        
        foreach (var effect in skillData.skillEffects)
        {
            if (effect == null || string.IsNullOrEmpty(effect.EffectID)) continue;
            
            StatusEffectData effectData = StatusEffectManager.Instance.GetById(effect.EffectID);
            if (effectData == null) continue;
            
            CreateValueBlock(effectData, effect.Value, effect.Duration);
        }
        if (debugValueBlocks)
            Debug.Log($"[SEValue] 생성 블록 수: {currentValueBlocks.Count}");
    }
    
    /// <summary>
    /// StatusValueBlock 생성 및 설정
    /// </summary>
    private void CreateValueBlock(StatusEffectData effectData, int effectValue, int duration)
    {
        if (statusValueBlockPrefab == null || containerTransform == null) return;
        int displayValue = GetDisplayEffectValue(effectData, effectValue);
        
        GameObject blockObj = Instantiate(statusValueBlockPrefab, containerTransform);
        blockObj.name = $"StatusValue_{effectData.EffectID}";
        currentValueBlocks.Add(blockObj);
        
        // 아이콘 설정
        Image iconImage = blockObj.transform.Find("Icon")?.GetComponent<Image>();
        if (iconImage == null)
        {
            // Icon 오브젝트를 못 찾으면 자식에서 Image 컴포넌트 찾기
            iconImage = blockObj.GetComponentInChildren<Image>();
        }
        
        if (iconImage != null)
        {
            Sprite sprite = null;
            
            // 버프/디버프 타입은 동적 아이콘 우선 사용 (음수값 대응)
            if (effectData.effectType == StatusEffectType.Buff || effectData.effectType == StatusEffectType.Debuff)
            {
                // 동적 아이콘 시도 (음수값일 때 negativeIcon 사용)
                sprite = effectData.GetDynamicIcon(displayValue);
                
                // 동적 아이콘이 없으면 기본 아이콘 시도
                if (sprite == null)
                {
                    sprite = effectData.GetIcon();
                }
            }
            else
            {
                // 그 외 타입은 기본 아이콘 우선
                sprite = effectData.icon;
                if (sprite == null)
                {
                    sprite = effectData.GetIcon();
                }
            }
            
            iconImage.sprite = sprite;
        }
        
        // 값/지속시간 텍스트 설정 (형식: "값/지속시간")
        TextMeshProUGUI valueText = blockObj.transform.Find("ValueText")?.GetComponent<TextMeshProUGUI>();
        if (valueText == null)
        {
            // ValueText 오브젝트를 못 찾으면 자식에서 TextMeshProUGUI 찾기
            TextMeshProUGUI[] texts = blockObj.GetComponentsInChildren<TextMeshProUGUI>();
            valueText = texts.Length > 0 ? texts[0] : null;
        }
        
        if (valueText != null)
        {
            // 형식: "값/지속시간" (예: "3/2", "5/5")
            valueText.text = $"{displayValue}/{duration}";
        }
    }

    private static int GetDisplayEffectValue(StatusEffectData effectData, int effectValue)
    {
        if (effectData != null
            && effectData.effectType == StatusEffectType.ContinuousDamage
            && effectData.treatContinuousValueAsHeal)
        {
            return Mathf.Abs(effectValue);
        }

        return effectValue;
    }
    
    /// <summary>
    /// 모든 값 블록 제거
    /// </summary>
    public void ClearAllBlocks()
    {
        foreach (GameObject block in currentValueBlocks)
        {
            if (block != null) Destroy(block);
        }
        currentValueBlocks.Clear();
        if (debugValueBlocks)
            Debug.Log("[SEValue] ClearAllBlocks");
    }
}
