using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// 스킬 정보를 표시하는 가상마우스 패널
/// VirtualMouseUIPanel을 상속받아 스킬 전용 기능 제공
/// </summary>
public class VirtualMouseSkillPanel : VirtualMouseUIPanel
{
    [Header("스킬 정보 UI 요소들")]
    [SerializeField] private Image skillIcon;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillDamageMinText;
    [SerializeField] private TextMeshProUGUI skillDamageMaxText;
    [SerializeField] private TextMeshProUGUI skillCooldownText;
    
    [Header("스킬 데이터")]
    [SerializeField] private SkillData currentSkillData;
    
    [Header("상태이상 설명 패널")]
    [SerializeField] private StatusEffectDescriptionPanel statusEffectDescriptionPanel;
    [SerializeField] private Transform statusEffectAnchor; // StEfDecAnchor 연동용
    [SerializeField] private GameObject stEfDecAnchor; // StEfDecAnchor GameObject (Inspector에서 할당)
    
    [Header("상태이상 앵커 오프셋")]
    [SerializeField] private float statusAnchorOffsetX = 700f;
    [SerializeField] private float statusAnchorOffsetY = 0f;
    
    [Header("상태이상 값 블록 관리")]
    [SerializeField] private StatusValueBlockManager statusValueBlockManager;
    
    [Header("디버그")]
    [SerializeField] private bool debugSkillPanel = true; // 요약 로그
    
    // 스킬 관련 이벤트
    public System.Action<SkillData> OnSkillDataChanged;
    
    void Start()
    {
        InitializeSkillPanel();
        
        // 상태이상 설명 패널이 할당되지 않았으면 자동으로 찾기
        if (statusEffectDescriptionPanel == null)
        {
            statusEffectDescriptionPanel = FindFirstObjectByType<StatusEffectDescriptionPanel>();
            if (statusEffectDescriptionPanel == null)
            {
                Debug.LogWarning("[VirtualMouseSkillPanel] StatusEffectDescriptionPanel을 찾을 수 없습니다. Inspector에서 직접 연결해주세요.");
            }
        }
        
        // 상태이상 값 블록 매니저가 할당되지 않았으면 자동으로 찾기
        if (statusValueBlockManager == null)
        {
            statusValueBlockManager = FindFirstObjectByType<StatusValueBlockManager>();
            if (statusValueBlockManager == null)
            {
                Debug.LogWarning("[VirtualMouseSkillPanel] StatusValueBlockManager를 찾을 수 없습니다. Inspector에서 직접 연결해주세요.");
            }
        }
    }
    
    /// <summary>
    /// 스킬 패널 초기화
    /// </summary>
    private void InitializeSkillPanel()
    {
        // 기본 설정
        SetTitleInternal("스킬 정보");
        // UI 요소 자동 바인딩 (인스펙터 누락 대비)
        if (skillIcon == null)
        {
            var iconTr = transform.Find("Icon");
            if (iconTr != null) skillIcon = iconTr.GetComponent<UnityEngine.UI.Image>();
        }
        if (skillNameText == null)
        {
            var tr = transform.Find("SkillName");
            if (tr != null) skillNameText = tr.GetComponent<TextMeshProUGUI>();
        }
        if (skillDamageMinText == null)
        {
            var tr = transform.Find("SkillDamageMin");
            if (tr != null) skillDamageMinText = tr.GetComponent<TextMeshProUGUI>();
        }
        if (skillDamageMaxText == null)
        {
            var tr = transform.Find("SkillDamageMax");
            if (tr != null) skillDamageMaxText = tr.GetComponent<TextMeshProUGUI>();
        }
        if (skillCooldownText == null)
        {
            var tr = transform.Find("SkillCooldown");
            if (tr != null) skillCooldownText = tr.GetComponent<TextMeshProUGUI>();
        }
        
        // 상태이상 앵커 자동 연결(인스펙터 누락 대비)
        if (statusEffectAnchor == null)
        {
            var anchorTr = transform.Find("StEfDecAnchor");
            if (anchorTr == null)
            {
                // 패널 형제가 아니라 상위에 있을 수 있으므로 상위에서 검색
                var found = GameObject.Find("StEfDecAnchor");
                if (found != null) anchorTr = found.transform;
            }
            statusEffectAnchor = anchorTr;
        }
        if (statusEffectDescriptionPanel != null && statusEffectAnchor != null)
        {
            statusEffectDescriptionPanel.SetAnchor(statusEffectAnchor);
        }
        
        // 초기화: 임시 생성물 모두 제거 후 비활성화
        if (statusEffectDescriptionPanel != null)
            statusEffectDescriptionPanel.ClearAllDescriptions();
        if (statusValueBlockManager != null)
            statusValueBlockManager.ClearAllBlocks();

        // 초기에는 숨김 (호버될 때만 표시)
        // SetActive로만 제어
        gameObject.SetActive(false);
        
        Debug.Log("[VirtualMouseSkillPanel] 스킬 패널 초기화 완료");
    }
    
    /// <summary>
    /// 초기 상태 설정 오버라이드: 스킬 패널은 SetActive로만 제어
    /// </summary>
    protected override void SetupInitialState()
    {
        // 부모 클래스의 SetupInitialState()를 호출하지 않음 (SetVisible 사용 안 함)
        // SetActive로만 제어
        gameObject.SetActive(false);
        // StEfDecAnchor도 함께 숨김
        HideStEfDecAnchor();
        
        if (enableDebugLog)
            Debug.Log("[VirtualMouseSkillPanel] 스킬 패널 초기 상태 설정 완료 (SetActive만 사용)");
    }

    /// <summary>
    /// 상태이상 설명 앵커 위치를 고정 수치로 갱신합니다(마우스 기준 좌/우 대칭).
    /// VirtualMouse에서 패널 위치를 정한 직후 호출합니다.
    /// </summary>
    public void UpdateStatusAnchorPosition(Vector2 mouseScreenPos, bool showOnLeft)
    {
        if (statusEffectAnchor == null) return;
        RectTransform anchorRt = statusEffectAnchor as RectTransform;
        if (anchorRt == null) return;

        // 요구사항: 부모 중심 기준으로 X=±statusAnchorOffsetX, Y=statusAnchorOffsetY 고정 배치
        float x = showOnLeft ? -statusAnchorOffsetX : statusAnchorOffsetX;
        float y = statusAnchorOffsetY;
        anchorRt.anchoredPosition = new Vector2(x, y);
    }

    /// <summary>
    /// 외부에서 초기화가 필요할 때 호출: 임시 생성물 제거 + 패널 비활성화
    /// </summary>
    public void ResetSkillHoverUI()
    {
        currentSkillData = null;
        if (statusEffectDescriptionPanel != null)
            statusEffectDescriptionPanel.ClearAllDescriptions();
        if (statusValueBlockManager != null)
            statusValueBlockManager.ClearAllBlocks();
        // SetActive로만 제어
        gameObject.SetActive(false);
        // StEfDecAnchor도 함께 숨김
        HideStEfDecAnchor();
    }
    
    /// <summary>
    /// 스킬 데이터 설정
    /// </summary>
    /// <param name="skillData">표시할 스킬 데이터</param>
    public void SetSkillData(SkillData skillData)
    {
        if (skillData == null)
        {
            Debug.LogWarning("[VirtualMouseSkillPanel] 스킬 데이터가 null입니다.");
            return;
        }
        
        // 같은 데이터여도 항상 갱신 (호버 시 정보 갱신 보장)
        bool isSameData = (currentSkillData != null && currentSkillData.ID == skillData.ID);
        currentSkillData = skillData;
        
        if (debugSkillPanel)
        {
            int effectCount = currentSkillData?.skillEffects != null ? currentSkillData.skillEffects.Count : 0;
            Debug.Log($"[VMSkillPanel] SetSkillData: {currentSkillData.ID}:{currentSkillData.Name}, effects={effectCount}, isSameData={isSameData}");
        }
        
        // 항상 UI 갱신 (같은 데이터여도 호버 시 정보 갱신 보장)
        UpdateSkillUI();
        
        // 상태이상 설명 패널 활성/비활성 제어(결정권: 스킬 패널)
        bool hasEffects = (skillData.skillEffects != null && skillData.skillEffects.Count > 0);
        if (statusEffectAnchor != null)
        {
            statusEffectAnchor.gameObject.SetActive(hasEffects);
        }
        if (statusEffectDescriptionPanel != null)
        {
            if (hasEffects)
                statusEffectDescriptionPanel.UpdateStatusEffectDescriptions(skillData);
            else
                statusEffectDescriptionPanel.ClearAllDescriptions();
        }
        
        // 상태이상 값 블록 업데이트
        if (statusValueBlockManager != null)
        {
            statusValueBlockManager.UpdateStatusValueBlocks(skillData);
        }
        
        OnSkillDataChanged?.Invoke(skillData);
        
        Debug.Log($"[VirtualMouseSkillPanel] 스킬 데이터 설정: {skillData.Name}");
    }
    
    /// <summary>
    /// 스킬 UI 업데이트
    /// </summary>
    private void UpdateSkillUI()
    {
        if (currentSkillData == null) return;
        
        // 스킬 아이콘 설정
        if (skillIcon != null)
        {
            Sprite iconSprite = Resources.Load<Sprite>(currentSkillData.Icon);
            if (iconSprite != null)
            {
                skillIcon.sprite = iconSprite;
            }
            else
            {
                Debug.LogWarning($"[VirtualMouseSkillPanel] 스킬 아이콘을 찾을 수 없습니다: {currentSkillData.Icon}");
            }
        }
        
        // 스킬 이름 설정
        if (skillNameText != null)
        {
            skillNameText.text = currentSkillData.Name;
        }
        
        // 스킬 데미지(민/맥스) 설정
        if (skillDamageMinText != null)
        {
            skillDamageMinText.text = currentSkillData.DamageMin > 0 ? currentSkillData.DamageMin.ToString() : "-";
        }
        if (skillDamageMaxText != null)
        {
            skillDamageMaxText.text = currentSkillData.DamageMax > 0 ? currentSkillData.DamageMax.ToString() : "-";
        }
        
        // 스킬 쿨타임 설정
        if (skillCooldownText != null)
        {
            skillCooldownText.text = $"쿨타임: {currentSkillData.Cooldown}턴";
        }
        
        // 비용 표시는 사용하지 않음 (요청에 따라 제거)
        
        // 스킬 타입에 따른 색상 설정
        SetSkillTypeColor();
    }
    
    /// <summary>
    /// 스킬 타입에 따른 색상 설정
    /// </summary>
    private void SetSkillTypeColor()
    {
        if (currentSkillData == null) return;
        
        Color skillColor = Color.white;
        
        // 스킬 타입에 따른 색상 결정 (추후 SkillData에 타입 필드 추가 시 확장)
        // 현재는 기본 색상 사용
        
        // 배경 색상 설정
        SetBackgroundColorInternal(skillColor * 0.1f);
        
        // 텍스트 색상 설정
        if (skillNameText != null)
        {
            skillNameText.color = skillColor;
        }
    }
    
    /// <summary>
    /// 현재 스킬 데이터 가져오기
    /// </summary>
    public SkillData GetCurrentSkillData()
    {
        return currentSkillData;
    }
    
    /// <summary>
    /// 스킬 패널 표시 (스킬 데이터와 함께)
    /// </summary>
    /// <param name="skillData">표시할 스킬 데이터</param>
    public void ShowSkillPanel(SkillData skillData)
    {
        SetSkillData(skillData);
        // SetActive로만 제어
        gameObject.SetActive(true);
        if (debugSkillPanel)
            Debug.Log("[VMSkillPanel] ShowSkillPanel 호출됨");
    }

    /// <summary>
    /// 호버 진입 시 호출: 데이터 설정 후 패널 표시
    /// </summary>
    public void OnHoverEnter(SkillData skillData)
    {
        SetSkillData(skillData);
        // SetActive로만 제어
        gameObject.SetActive(true);
    }
    
    /// <summary>
    /// 스킬 패널 숨김
    /// </summary>
    public void HideSkillPanel()
    {
        // SetActive로만 제어
        gameObject.SetActive(false);
        currentSkillData = null;
        if (debugSkillPanel)
            Debug.Log("[VMSkillPanel] HideSkillPanel 호출됨");
        
        // StEfDecAnchor도 함께 숨김
        HideStEfDecAnchor();
        
        // 상태이상 설명 패널도 정리
        if (statusEffectDescriptionPanel != null)
        {
            statusEffectDescriptionPanel.ClearAllDescriptions();
            // 앵커도 함께 숨김은 ClearAllDescriptions에서 처리됨
        }
        
        // 상태이상 값 블록도 정리
        if (statusValueBlockManager != null)
        {
            statusValueBlockManager.ClearAllBlocks();
        }
    }
    
    /// <summary>
    /// StEfDecAnchor 숨김
    /// </summary>
    private void HideStEfDecAnchor()
    {
        if (stEfDecAnchor != null)
        {
            stEfDecAnchor.SetActive(false);
        }
    }

    /// <summary>
    /// 호버가 끝났을 때(패널 비활성화 처리와 별개로) 상태이상 UI를 즉시 정리합니다.
    /// 상위 패널에서 비활성화를 처리하더라도, 생성된 프리팹은 여기서 파괴합니다.
    /// </summary>
    public void OnHoverExit()
    {
        if (statusEffectDescriptionPanel != null)
        {
            statusEffectDescriptionPanel.ClearAllDescriptions();
        }
        if (statusValueBlockManager != null)
        {
            statusValueBlockManager.ClearAllBlocks();
        }
        // 패널 자체도 숨김 (SetActive로만 제어)
        gameObject.SetActive(false);
        // StEfDecAnchor도 함께 숨김
        HideStEfDecAnchor();
    }
    
    /// <summary>
    /// 스킬 패널 정보 가져오기
    /// </summary>
    public string GetSkillPanelInfo()
    {
        if (currentSkillData == null)
            return "스킬 데이터 없음";
        
        return $"스킬: {currentSkillData.Name}, ID: {currentSkillData.ID}";
    }
}
