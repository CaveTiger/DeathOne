using UnityEngine;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

/// <summary>
/// 스킬, 상태이상 호버 시 해당하는 상태이상을 설명하는 패널
/// VirtualMouseSkillPanel과 연동하여 스킬의 상태이상 정보를 표시
/// </summary>
public class StatusEffectDescriptionPanel : MonoBehaviour
{
    [Header("프리팹 및 컨테이너")]
    [SerializeField] private GameObject statusEffectDescriptionPrefab; // StEfDerscription 프리팹
    [SerializeField] private Transform containerAnchor; // StEfDecAnchor 컨테이너
    
    private List<GameObject> currentDescriptionPanels = new List<GameObject>();
    
    [Header("디버그")]
    [SerializeField] private bool debugDescription = true; // 요약 로그
    
    void Awake()
    {
        if (statusEffectDescriptionPrefab == null)
            statusEffectDescriptionPrefab = Resources.Load<GameObject>("Prefab/StEfDerscription");
        
        if (containerAnchor == null)
        {
            GameObject containerObj = GameObject.Find("StEfDecAnchor");
            if (containerObj != null) containerAnchor = containerObj.transform;
        }
        // 시작 시 앵커 비활성화 (호버 시에만 활성)
        if (containerAnchor != null)
        {
            containerAnchor.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// 외부에서 StEfDecAnchor를 지정합니다.
    /// </summary>
    public void SetAnchor(Transform anchor)
    {
        containerAnchor = anchor;
        if (containerAnchor != null)
        {
            containerAnchor.gameObject.SetActive(false);
        }
    }
    
    /// <summary>
    /// 스킬, 상태이상 호버 시 해당하는 상태이상 정보를 표시
    /// </summary>
    public void UpdateStatusEffectDescriptions(SkillData skillData)
    {
        ClearAllDescriptions();
        
        if (skillData == null || skillData.skillEffects == null || skillData.skillEffects.Count == 0)
        {
            // 상태이상 없음 → 앵커 숨김
            if (containerAnchor != null) containerAnchor.gameObject.SetActive(false);
            if (debugDescription)
                Debug.Log("[SEDesc] 상태이상 없음 → 앵커 비활성");
            return;
        }
        
        if (StatusEffectManager.Instance == null)
        {
            Debug.LogError("[StatusEffectDescriptionPanel] StatusEffectManager.Instance가 null입니다.");
            return;
        }
        // 상태이상 존재 → 앵커 활성화
        if (containerAnchor != null) containerAnchor.gameObject.SetActive(true);
        if (debugDescription)
            Debug.Log($"[SEDesc] 상태이상 {skillData.skillEffects.Count}개 → 앵커 활성");
        
        foreach (var effect in skillData.skillEffects)
        {
            if (effect == null || string.IsNullOrEmpty(effect.EffectID)) continue;
            
            StatusEffectData effectData = StatusEffectManager.Instance.GetById(effect.EffectID);
            if (effectData == null) continue;
            
            CreateDescriptionPanel(effectData, effect.Value, effect.Duration);
        }
    }
    
    /// <summary>
    /// 상태이상 설명 패널 생성 및 설정
    /// </summary>
    private void CreateDescriptionPanel(StatusEffectData effectData, int effectValue, int duration)
    {
        if (statusEffectDescriptionPrefab == null || containerAnchor == null) return;
        int displayValue = GetDisplayEffectValue(effectData, effectValue);
        
        GameObject panelObj = Instantiate(statusEffectDescriptionPrefab, containerAnchor);
        panelObj.name = $"StatusEffect_{effectData.EffectID}";
        currentDescriptionPanels.Add(panelObj);
        if (debugDescription)
            Debug.Log($"[SEDesc] Create: {effectData.EffectID}, v={displayValue}, d={duration}");
        
        // 아이콘 설정: 버프/디버프는 동적 아이콘 우선, 그 외는 기본 아이콘
        Image iconImage = panelObj.transform.Find("Icon")?.GetComponent<Image>();
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
        
        // 이름 설정
        TextMeshProUGUI nameText = panelObj.transform.Find("EffectName")?.GetComponent<TextMeshProUGUI>();
        if (nameText == null) nameText = panelObj.GetComponentInChildren<TextMeshProUGUI>();
        if (nameText != null) nameText.text = effectData.effectName;
        
        // 설명 설정
        TextMeshProUGUI descriptionText = panelObj.transform.Find("Description")?.GetComponent<TextMeshProUGUI>();
        if (descriptionText == null)
        {
            TextMeshProUGUI[] texts = panelObj.GetComponentsInChildren<TextMeshProUGUI>();
            foreach (var text in texts)
            {
                if (text != nameText) { descriptionText = text; break; }
            }
        }
        
        if (descriptionText != null)
        {
            // StatusEffectData의 description 필드 참고
            string description = !string.IsNullOrEmpty(effectData.description) 
                ? effectData.description 
                : "효과 설명 없음";
            
            // 수치 정보 추가 (값이 0이 아닐 때만)
            if (displayValue != 0) description += $" ({displayValue})";
            
            // 지속 턴 정보 추가
            if (duration > 0) description += $" - {duration}턴 지속";
            
            descriptionText.text = description;
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
    /// 모든 설명 패널 제거
    /// </summary>
    public void ClearAllDescriptions()
    {
        foreach (GameObject panel in currentDescriptionPanels)
        {
            if (panel != null) Destroy(panel);
        }
        currentDescriptionPanels.Clear();
        // 설명이 모두 제거되면 앵커 숨김
        if (containerAnchor != null)
        {
            containerAnchor.gameObject.SetActive(false);
        }
    }
}