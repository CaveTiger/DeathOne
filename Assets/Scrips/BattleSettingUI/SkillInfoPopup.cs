using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class SkillInfoPopup : MonoBehaviour
{
    [Header("스킬 정보 UI")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDescriptionText;
    [SerializeField] private TextMeshProUGUI skillTypeText;
    [SerializeField] private TextMeshProUGUI damageOrHealText; // 데미지 또는 힐량
    [SerializeField] private TextMeshProUGUI targetText;
    [SerializeField] private TextMeshProUGUI manaCostText;
    [SerializeField] private TextMeshProUGUI cooldownText;

    [Header("상태이상 효과")]
    [SerializeField] private Transform effectContainer; // 상태이상 효과들을 담을 컨테이너
    [SerializeField] private GameObject effectItemPrefab; // 상태이상 효과 아이템 프리팹

    [Header("이펙트 뷰 전환")]
    [SerializeField] private Button previousEffectButton; // 이전 효과 버튼
    [SerializeField] private Button nextEffectButton; // 다음 효과 버튼
    [SerializeField] private TextMeshProUGUI effectPageText; // 효과 페이지 표시 (예: "1/3")
    [SerializeField] private GameObject effectPageBackground; // 효과 페이지 배경

    private SkillData currentSkillData;
    private List<GameObject> effectItems = new List<GameObject>();
    private List<SkillEffectInfo> skillEffects = new List<SkillEffectInfo>(); // 스킬 효과 목록
    private int currentEffectIndex = 0; // 현재 표시 중인 효과 인덱스

    private void OnEnable()
    {
        if (currentSkillData != null)
        {
            DisplaySkillInfo(currentSkillData);
        }
    }

    private void OnDisable()
    {
        ClearEffectItems();
    }

    private void Start()
    {
        // 버튼 이벤트 연결
        if (previousEffectButton != null)
            previousEffectButton.onClick.AddListener(ShowPreviousEffect);
        if (nextEffectButton != null)
            nextEffectButton.onClick.AddListener(ShowNextEffect);
    }

    /// <summary>
    /// 외부에서 스킬 데이터를 받아 정보를 표시합니다.
    /// </summary>
    /// <param name="skillData">표시할 스킬 데이터</param>
    public void SetSkillData(SkillData skillData)
    {
        currentSkillData = skillData;
        if (gameObject.activeSelf)
        {
            DisplaySkillInfo(skillData);
        }
    }

    /// <summary>
    /// 받은 스킬 데이터로 UI를 업데이트합니다.
    /// </summary>
    private void DisplaySkillInfo(SkillData skillData)
    {
        if (skillData == null) return;

        try
        {
            // 기본 정보 업데이트
            skillNameText.text = skillData.Name;
            skillDescriptionText.text = skillData.Description;
            skillTypeText.text = $"타입: {skillData.Type}";

            // 스킬 효과 목록 저장
            skillEffects = skillData.skillEffects ?? new List<SkillEffectInfo>();
            currentEffectIndex = 0;

            // 데미지 또는 힐량 정보 표시
            UpdateDamageHealDisplay();

            // 타겟 정보 (간단한 4가지 타입으로 분류)
            targetText.text = $"대상: {GetSimplifiedTargetText(skillData.SkillTarget)}";

            // 마나 소모
            if (skillData.ManaCost > 0)
            {
                manaCostText.text = $"마나 소모: {skillData.ManaCost}";
                manaCostText.gameObject.SetActive(true);
            }
            else
            {
                manaCostText.gameObject.SetActive(false);
            }

            // 쿨타임
            if (skillData.Cooldown > 0)
            {
                cooldownText.text = $"쿨타임: {skillData.Cooldown}턴";
                cooldownText.gameObject.SetActive(true);
            }
            else
            {
                cooldownText.gameObject.SetActive(false);
            }

            // 스킬 아이콘 업데이트
            if (skillIcon != null && !string.IsNullOrEmpty(skillData.Icon))
            {
                Sprite skillSprite = Resources.Load<Sprite>(skillData.Icon);
                if (skillSprite != null)
                {
                    skillIcon.sprite = skillSprite;
                }
                else
                {
                    Debug.LogWarning($"스킬 아이콘을 찾을 수 없습니다: {skillData.Icon}");
                    skillIcon.sprite = null;
                }
            }

            // 상태이상 효과 정보 업데이트 (현재 인덱스의 효과만)
            UpdateEffectInfo();

            // 이펙트 전환 버튼 상태 업데이트
            UpdateEffectNavigationButtons();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"스킬 정보 표시 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 상태이상 효과 정보를 업데이트합니다. (현재 인덱스의 효과만)
    /// </summary>
    private void UpdateEffectInfo()
    {
        ClearEffectItems();

        if (skillEffects == null || skillEffects.Count == 0)
        {
            return;
        }

        // 현재 인덱스의 효과만 표시
        if (currentEffectIndex >= 0 && currentEffectIndex < skillEffects.Count)
        {
            SkillEffectInfo effectInfo = skillEffects[currentEffectIndex];
            try
            {
                // 상태이상 데이터 로드
                if (StatusEffectManager.Instance != null && 
                    StatusEffectManager.Instance.GetById(effectInfo.EffectID) is StatusEffectData effectData)
                {
                    // 상태이상 효과 아이템 생성
                    if (effectItemPrefab != null && effectContainer != null)
                    {
                        GameObject effectItem = Instantiate(effectItemPrefab, effectContainer);
                        
                        // RectTransform 좌표를 0으로 설정
                        RectTransform rectTransform = effectItem.GetComponent<RectTransform>();
                        if (rectTransform != null)
                        {
                            rectTransform.anchoredPosition = Vector2.zero;
                        }
                        
                                                 // EffectIcon 설정
                         Image effectIcon = effectItem.transform.Find("EffectIcon")?.GetComponent<Image>();
                         if (effectIcon != null)
                         {
                             Debug.Log($"EffectIcon 컴포넌트 찾음: {effectIcon.name}");
                             Sprite iconSprite = GetDynamicEffectIcon(effectData, effectInfo.Value);
                             Debug.Log($"로드된 아이콘: {(iconSprite != null ? iconSprite.name : "null")}");
                             
                             if (iconSprite != null)
                             {
                                 effectIcon.sprite = iconSprite;
                                 effectIcon.gameObject.SetActive(true);
                                 Debug.Log($"아이콘 설정 완료: {iconSprite.name}");
                             }
                             else
                             {
                                 // 아이콘이 없어도 오브젝트는 활성화 (기본 흰색 아이콘 표시)
                                 effectIcon.gameObject.SetActive(true);
                                 Debug.LogWarning($"아이콘을 찾을 수 없어 기본 아이콘을 사용합니다: {effectData.EffectID}");
                             }
                         }
                         else
                         {
                             Debug.LogError("EffectIcon 컴포넌트를 찾을 수 없습니다!");
                         }

                        // EffectsTitle 설정 (수치에 따라 동적 이름 생성)
                        TextMeshProUGUI effectsTitle = effectItem.transform.Find("EffectsTitle")?.GetComponent<TextMeshProUGUI>();
                        if (effectsTitle != null)
                        {
                            string dynamicEffectName = GetDynamicEffectName(effectData, effectInfo.Value);
                            effectsTitle.text = dynamicEffectName;
                        }

                                                 // Value 설정
                         TextMeshProUGUI valueText = effectItem.transform.Find("Value")?.GetComponent<TextMeshProUGUI>();
                         if (valueText != null)
                         {
                             if (effectInfo.Value >= 0)
                             {
                                 string valueDisplay = $"상승 +{effectInfo.Value}";
                                 valueText.text = valueDisplay;
                                 valueText.gameObject.SetActive(true);
                             }
                             else
                             {
                                 string valueDisplay = $"하락 {effectInfo.Value}";
                                 valueText.text = valueDisplay;
                                 valueText.gameObject.SetActive(true);
                             }
                         }

                        // Duration 설정
                        TextMeshProUGUI durationText = effectItem.transform.Find("Duration")?.GetComponent<TextMeshProUGUI>();
                        if (durationText != null)
                        {
                            if (effectInfo.Duration > 0)
                            {
                                durationText.text = $"지속 {effectInfo.Duration}턴";
                                durationText.gameObject.SetActive(true);
                            }
                            else
                            {
                                durationText.gameObject.SetActive(false);
                            }
                        }

                        effectItems.Add(effectItem);
                    }
                }
                else
                {
                    Debug.LogWarning($"상태이상 데이터를 찾을 수 없습니다: {effectInfo.EffectID}");
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"상태이상 효과 정보 처리 중 오류 발생: {e.Message}");
            }
        }
    }

    /// <summary>
    /// 생성된 효과 아이템들을 정리합니다.
    /// </summary>
    private void ClearEffectItems()
    {
        foreach (var item in effectItems)
        {
            if (item != null)
            {
                Destroy(item);
            }
        }
        effectItems.Clear();
    }

    /// <summary>
    /// 수치에 따라 동적 효과 아이콘을 로드합니다.
    /// </summary>
    /// <param name="effectData">상태이상 데이터</param>
    /// <param name="value">효과 수치</param>
    /// <returns>동적 효과 아이콘</returns>
    private Sprite GetDynamicEffectIcon(StatusEffectData effectData, int value)
    {
        // 각 효과 타입별로 아이콘 경로 결정
        string iconPath = "";
        
                 // 공격도 관련 효과
         if (effectData.EffectID == "021002") // AttackPowerBuff
         {
             if (value >= 0)
                 iconPath = "UI/StatusEffect/ATKUp";
             else
                 iconPath = "UI/StatusEffect/ATKDown";
         }
         // 방어도 관련 효과
         else if (effectData.EffectID == "021001") // GuardPowerBuff
         {
             if (value >= 0)
                 iconPath = "UI/StatusEffect/DEFUp";
             else
                 iconPath = "UI/StatusEffect/DEFDown";
         }
         // 속도 관련 효과 (ATK 아이콘 사용)
         else if (effectData.EffectID == "021003") // SpeedBuff
         {
             if (value >= 0)
                 iconPath = "UI/StatusEffect/ATKUp";
             else
                 iconPath = "UI/StatusEffect/ATKDown";
         }
         // 체력 변화 효과 (DEF 아이콘 사용)
         else if (effectData.EffectID == "021004") // HpChange
         {
             if (value >= 0)
                 iconPath = "UI/StatusEffect/DEFUp";
             else
                 iconPath = "UI/StatusEffect/DEFDown";
         }
                 // 기타 효과는 기본 아이콘 사용
         else
         {
             iconPath = effectData.iconPath;
         }
         
         // 아이콘 경로가 비어있으면 기본 아이콘 사용
         if (string.IsNullOrEmpty(iconPath))
         {
             Debug.LogWarning($"아이콘 경로가 비어있습니다. 기본 아이콘을 사용합니다: {effectData.EffectID}");
             return null;
         }
        
                 // 아이콘 로드
         Debug.Log($"아이콘 경로 시도: {iconPath}");
         Sprite iconSprite = Resources.Load<Sprite>(iconPath);
         if (iconSprite == null)
         {
             Debug.LogWarning($"효과 아이콘을 찾을 수 없습니다: {iconPath}");
             
                           // 대체 아이콘 시도
              string fallbackPath = "UI/StatusEffect/ATKUp"; // 기본 아이콘으로 대체
             Debug.Log($"대체 아이콘 시도: {fallbackPath}");
             iconSprite = Resources.Load<Sprite>(fallbackPath);
             if (iconSprite != null)
             {
                 Debug.Log($"대체 아이콘 로드 성공: {fallbackPath}");
             }
             else
             {
                 Debug.LogError($"대체 아이콘도 찾을 수 없습니다: {fallbackPath}");
             }
         }
         else
         {
             Debug.Log($"아이콘 로드 성공: {iconPath}");
         }
         
         return iconSprite;
    }

    /// <summary>
    /// 수치에 따라 동적 효과 이름을 생성합니다.
    /// </summary>
    /// <param name="effectData">상태이상 데이터</param>
    /// <param name="value">효과 수치</param>
    /// <returns>동적 효과 이름</returns>
    private string GetDynamicEffectName(StatusEffectData effectData, int value)
    {
        string baseName = effectData.effectName;
        
                 // 공격도 관련 효과
         if (effectData.EffectID == "021002") // AttackPowerBuff
         {
             if (value >= 0)
                 return "공격도 상승";
             else
                 return "공격도 하락";
         }
         // 방어도 관련 효과
         else if (effectData.EffectID == "021001") // GuardPowerBuff
         {
             if (value >= 0)
                 return "방어도 상승";
             else
                 return "방어도 하락";
         }
         // 속도 관련 효과
         else if (effectData.EffectID == "021003") // SpeedBuff
         {
             if (value >= 0)
                 return "속도 상승";
             else
                 return "속도 하락";
         }
         // 체력 변화 효과
         else if (effectData.EffectID == "021004") // HpChange
         {
             if (value >= 0)
                 return "체력 회복";
             else
                 return "체력 감소";
         }
        
        // 기타 효과는 기본 이름 사용
        return baseName;
    }

    /// <summary>
    /// 스킬 타겟을 간단한 4가지 타입으로 분류합니다.
    /// </summary>
    /// <param name="skillTarget">원본 스킬 타겟 문자열</param>
    /// <returns>간단한 타겟 설명</returns>
    private string GetSimplifiedTargetText(string skillTarget)
    {
        if (string.IsNullOrEmpty(skillTarget))
            return "단일";

        string target = skillTarget.ToLower();
        
        // 아군 타겟
        if (target.Contains("ally") || target.Contains("self") || target.Contains("allies"))
        {
            return "아군";
        }
        // 적군 타겟
        else if (target.Contains("enemy") || target.Contains("enemies"))
        {
            return "적군";
        }
        // 전체 타겟
        else if (target.Contains("all"))
        {
            return "전체";
        }
        // 랜덤 타겟
        else if (target.Contains("random"))
        {
            return "랜덤";
        }
        // 기타 (인접, 특정 등)
        else
        {
            return "특정";
        }
    }

    /// <summary>
    /// 데미지/힐 효과 정보를 생성합니다. (현재 인덱스의 효과만)
    /// </summary>
    private string GetDamageHealInfo()
    {
        List<string> effects = new List<string>();

        // 1. 스킬 자체의 데미지
        if (currentSkillData.DamageMin > 0 || currentSkillData.DamageMax > 0)
        {
            effects.Add($"데미지: {currentSkillData.DamageMin}-{currentSkillData.DamageMax}");
        }

        // 2. 스킬 자체의 힐량 (범위 힐량)
        if (currentSkillData.HealMin > 0 || currentSkillData.HealMax > 0)
        {
            if (currentSkillData.HealMin == currentSkillData.HealMax)
            {
                effects.Add($"힐량: {currentSkillData.HealMin}");
            }
            else
            {
                effects.Add($"힐량: {currentSkillData.HealMin}-{currentSkillData.HealMax}");
            }
        }

        // 3. 현재 인덱스의 상태이상 효과를 통한 데미지/힐량
        if (skillEffects != null && currentEffectIndex >= 0 && currentEffectIndex < skillEffects.Count)
        {
            SkillEffectInfo effectInfo = skillEffects[currentEffectIndex];
            {
                if (StatusEffectManager.Instance != null)
                {
                    StatusEffectData effectData = StatusEffectManager.Instance.GetById(effectInfo.EffectID);
                    if (effectData != null)
                    {
                        // 즉발성 체력 변화 효과 (021004)
                        if (effectData.EffectID == "021004")
                        {
                            if (effectInfo.Value > 0)
                            {
                                effects.Add($"즉시 회복: {effectInfo.Value}");
                            }
                            else if (effectInfo.Value < 0)
                            {
                                effects.Add($"즉시 피해: {Mathf.Abs(effectInfo.Value)}");
                            }
                        }
                        // 지속 피해 효과들
                        else if (effectData.EffectID == "020001" || // 출혈
                                 effectData.EffectID == "020002" || // 독
                                 effectData.EffectID == "020003")   // 화상
                        {
                            if (effectInfo.Duration > 0)
                            {
                                effects.Add($"지속 피해: {effectInfo.Value} ({effectInfo.Duration}턴)");
                            }
                        }
                    }
                }
            }
        }

        return string.Join(", ", effects);
    }

    /// <summary>
    /// 현재 선택된 데미지/힐 효과를 표시합니다.
    /// </summary>
    private void UpdateDamageHealDisplay()
    {
        if (damageOrHealText != null)
        {
            string damageHealInfo = GetDamageHealInfo();
            if (!string.IsNullOrEmpty(damageHealInfo))
            {
                damageOrHealText.text = damageHealInfo;
                damageOrHealText.gameObject.SetActive(true);
            }
            else
            {
                damageOrHealText.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 이전 효과를 표시합니다.
    /// </summary>
    public void ShowPreviousEffect()
    {
        if (skillEffects.Count > 1)
        {
            currentEffectIndex--;
            if (currentEffectIndex < 0)
                currentEffectIndex = skillEffects.Count - 1;
            
            UpdateDamageHealDisplay();
            UpdateEffectInfo();
            UpdateEffectNavigationButtons();
        }
    }

    /// <summary>
    /// 다음 효과를 표시합니다.
    /// </summary>
    public void ShowNextEffect()
    {
        if (skillEffects.Count > 1)
        {
            currentEffectIndex++;
            if (currentEffectIndex >= skillEffects.Count)
                currentEffectIndex = 0;
            
            UpdateDamageHealDisplay();
            UpdateEffectInfo();
            UpdateEffectNavigationButtons();
        }
    }

    /// <summary>
    /// 이펙트 전환 버튼들의 상태를 업데이트합니다.
    /// </summary>
    private void UpdateEffectNavigationButtons()
    {
        bool hasMultipleEffects = skillEffects.Count > 1;
        
        // 이전/다음 버튼 활성화/비활성화
        if (previousEffectButton != null)
            previousEffectButton.gameObject.SetActive(hasMultipleEffects);
        if (nextEffectButton != null)
            nextEffectButton.gameObject.SetActive(hasMultipleEffects);

        // 페이지 텍스트 및 배경 업데이트
        if (effectPageText != null)
        {
            if (hasMultipleEffects)
            {
                effectPageText.text = $"{currentEffectIndex + 1}/{skillEffects.Count}";
                effectPageText.gameObject.SetActive(true);
            }
            else
            {
                effectPageText.gameObject.SetActive(false);
            }
        }
        
        // 페이지 배경 업데이트
        if (effectPageBackground != null)
        {
            effectPageBackground.SetActive(hasMultipleEffects);
        }
    }
} 