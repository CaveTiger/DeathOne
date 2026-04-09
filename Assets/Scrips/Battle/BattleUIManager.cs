using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

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

    [Header("상태이상 UI 관리")]
    [SerializeField] private Transform stEfUI; // 상태이상 UI 루트 오브젝트 (UI 하위의 StEfUI)
    [SerializeField] private GameObject statusEffectDamagePrefab; // 상태이상 대미지 프리팹 (아이콘 + 대미지 통합)
    [SerializeField] private float statusEffectPopupDelay = 0.06f; // 상태이상 피해 팝업 간격 (초)
    [SerializeField] private float postStatusPopupSettleDelay = 0.03f; // 모든 팝업 처리 후 추가 안정화 대기
    [SerializeField] private float maxStatusPopupWaitSeconds = 0.20f; // 턴 진행을 막는 최대 대기 상한

    // 상태이상 피해 팝업 큐
    private Queue<StatusEffectPopupData> statusEffectPopupQueue = new Queue<StatusEffectPopupData>();
    private bool isProcessingStatusEffectPopups = false;
    
    /// <summary>
    /// 상태이상 피해 팝업 처리가 완료될 때까지 대기하는 코루틴
    /// </summary>
    public IEnumerator WaitForStatusEffectPopupsToComplete()
    {
        float start = Time.realtimeSinceStartup;
        while (isProcessingStatusEffectPopups || statusEffectPopupQueue.Count > 0)
        {
            if (Time.realtimeSinceStartup - start >= maxStatusPopupWaitSeconds)
                yield break;
            yield return null;
        }
        
        // 턴 템포 저하를 막기 위해 추가 대기는 짧게 유지
        if (postStatusPopupSettleDelay > 0f)
            yield return new WaitForSeconds(postStatusPopupSettleDelay);
    }

    // 상태이상 피해 팝업 데이터 구조
    private struct StatusEffectPopupData
    {
        public Vector3 position;
        public int damage;
        public StatusEffectData effectData;
        public int effectValue;
        public CharacterStats owner; // 체력 감소를 위한 캐릭터 참조
    }

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

        if (VirtualMouse.Instance != null)
            VirtualMouse.Instance.DismissAllHoverUi();
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

    /// <summary>
    /// 상태이상 피해 팝업을 큐에 추가합니다 (순차 표시를 위해)
    /// </summary>
    /// <param name="position">월드 좌표 위치</param>
    /// <param name="damage">피해량</param>
    /// <param name="effectData">상태이상 데이터</param>
    /// <param name="effectValue">상태이상 수치</param>
    /// <param name="owner">피해를 받는 캐릭터</param>
    public void CreateStatusEffectDamagePopup(Vector3 position, int damage, StatusEffectData effectData, int effectValue, CharacterStats owner)
    {
        // 큐에 데이터 추가
        StatusEffectPopupData popupData = new StatusEffectPopupData
        {
            position = position,
            damage = damage,
            effectData = effectData,
            effectValue = effectValue,
            owner = owner
        };
        
        statusEffectPopupQueue.Enqueue(popupData);
        
        // 코루틴이 실행 중이 아니면 시작
        if (!isProcessingStatusEffectPopups)
        {
            StartCoroutine(ProcessStatusEffectPopupsCoroutine());
        }
    }

    /// <summary>
    /// 상태이상 피해 팝업을 순차적으로 처리하는 코루틴 (0.5초 간격)
    /// </summary>
    private IEnumerator ProcessStatusEffectPopupsCoroutine()
    {
        isProcessingStatusEffectPopups = true;
        
        CharacterStats lastOwner = null; // 마지막으로 피해를 받은 캐릭터 (사망 체크용)

        while (statusEffectPopupQueue.Count > 0)
        {
            StatusEffectPopupData popupData = statusEffectPopupQueue.Dequeue();
            
            // 캐릭터가 파괴되었거나 이미 사망한 경우 스킵
            if (popupData.owner == null || popupData.owner.gameObject == null || popupData.owner.IsDead)
            {
                continue;
            }
            
            // 양수는 지속 피해, 음수는 지속 회복으로 해석한다.
            if (popupData.damage > 0)
            {
                int hpBeforeDot = popupData.owner.Hp;
                popupData.owner.Hp -= popupData.damage;
                popupData.owner.Hp = Mathf.Max(0, popupData.owner.Hp);
                popupData.owner.ApplyNearDeathDamagePressureForCollapse(hpBeforeDot, popupData.damage);
                Debug.Log($"[StatusEffect] {popupData.owner.Label}: {popupData.effectData?.effectName} 지속 피해 {popupData.damage}, 남은 HP: {popupData.owner.Hp}");
            }
            else if (popupData.damage < 0)
            {
                int healAmount = Mathf.Abs(popupData.damage);
                popupData.owner.Heal(healAmount);
                Debug.Log($"[StatusEffect] {popupData.owner.Label}: {popupData.effectData?.effectName} 지속 회복 {healAmount}, 현재 HP: {popupData.owner.Hp}");
            }
            else
            {
                Debug.Log($"[StatusEffect] {popupData.owner.Label}: {popupData.effectData?.effectName} 효과값 0 (체력 변동 없음)");
            }
            
            // 체력바 업데이트(Heal 내부에서도 갱신하지만, 피해 경로 통일성을 위해 한 번 더 보장)
            if (popupData.owner.HpUI != null)
                popupData.owner.HpUI.UpdateHpBar(popupData.owner.Hp, popupData.owner.MaxHp);
            
            // 실제 팝업 생성
            CreateStatusEffectDamagePopupInternal(popupData.position, popupData.damage, popupData.effectData, popupData.effectValue);
            
            // 사망 체크는 하지 않음 (팝업 애니메이션 완료 후 처리)
            // 중간에 사망해도 오버 대미지를 보여주기 위해 계속 진행
            if (popupData.owner.IsDead)
            {
                Debug.Log($"[StatusEffect] {popupData.owner.Label} 이미 사망 상태, 오버 대미지 표시 계속");
            }
            
            lastOwner = popupData.owner;
            
            // 다음 팝업까지 대기 (짧은 간격)
            yield return new WaitForSeconds(statusEffectPopupDelay);
        }
        
        // 모든 상태이상 정산 처리 완료 후 사망 체크 (팝업 애니메이션 완료 전에 체크)
        // 실제 사망 처리는 팝업 애니메이션 완료 후에 이루어짐
        if (lastOwner != null && lastOwner.gameObject != null)
        {
            // 체력이 0 이하인 경우 사망 체크 (하지만 DeathAction은 아직 호출하지 않음)
            if (lastOwner.Hp <= 0 && !lastOwner.IsDead)
            {
                lastOwner.Deathcheck(allowAllyCollapseDiceRoll: true);
                // DeathAction은 팝업 애니메이션 완료 후에 호출됨
            }
        }

        isProcessingStatusEffectPopups = false;
    }

    /// <summary>
    /// 상태이상 수치 팝업(지속 피해/지속 회복)을 실제로 생성합니다 (내부 메서드)
    /// </summary>
    private void CreateStatusEffectDamagePopupInternal(Vector3 position, int damage, StatusEffectData effectData, int effectValue)
    {
        if (statusEffectDamagePrefab == null)
        {
            Debug.LogWarning("[BattleUIManager] statusEffectDamagePrefab이 할당되지 않았습니다.");
            return;
        }

        if (stEfUI == null)
        {
            Debug.LogWarning("[BattleUIManager] stEfUI가 할당되지 않았습니다.");
            return;
        }

        // Canvas 찾기
        Canvas canvas = stEfUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleUIManager] Canvas를 찾을 수 없습니다.");
            return;
        }

        // 월드 좌표를 스크린 좌표로 변환
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[BattleUIManager] Main Camera를 찾을 수 없습니다.");
            return;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(position);
        
        // 스크린 좌표를 Canvas 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        
        // 데미지 팝업을 우하단에 배치
        Vector2 damageOffset = new Vector2(30, -40);
        localPoint += damageOffset;
        
        // 프리팹 인스턴스 생성
        GameObject popup = Instantiate(statusEffectDamagePrefab, stEfUI);
        RectTransform rectTransform = popup.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = localPoint;
        }
        
        // 아이콘 설정
        if (effectData != null)
        {
            Transform iconTransform = popup.transform.Find("Icon");
            if (iconTransform != null)
            {
                UnityEngine.UI.Image iconImage = iconTransform.GetComponent<UnityEngine.UI.Image>();
                if (iconImage != null)
                {
                    // 동적 아이콘 가져오기
                    Sprite icon = effectData.GetDynamicIcon(effectValue);
                    if (icon == null)
                    {
                        icon = effectData.GetIcon();
                    }
                    
                    if (icon != null)
                    {
                        iconImage.sprite = icon;
                    }
                }
            }
        }
        
        // 수치 텍스트 설정(양수: 피해, 음수: 회복)
        Transform damageTransform = popup.transform.Find("Damage");
        if (damageTransform != null)
        {
            TextMeshProUGUI damageText = damageTransform.GetComponent<TextMeshProUGUI>();
            if (damageText != null)
            {
                if (damage < 0)
                {
                    damageText.text = $"+{Mathf.Abs(damage)}";
                    damageText.color = Color.green;
                }
                else
                {
                    damageText.text = damage.ToString();
                    damageText.color = Color.red;
                }
            }
        }

        // 애니메이션 코루틴 시작
        StartCoroutine(AnimateStatusEffectDamagePopup(popup, rectTransform));
    }

    /// <summary>
    /// 상태이상 피해 팝업 애니메이션 (위로 이동하며 페이드 아웃)
    /// </summary>
    private IEnumerator AnimateStatusEffectDamagePopup(GameObject popup, RectTransform rectTransform)
    {
        if (popup == null || rectTransform == null) yield break;

        float duration = 0.75f; // 애니메이션 지속시간 (절반으로 단축)
        float moveDistance = 10f; // 위로 이동할 거리 (픽셀)
        float fadeStartTime = 0.25f; // 페이드 아웃 시작 시간 (절반으로 단축)
        
        Vector2 startPosition = rectTransform.anchoredPosition;
        Vector2 targetPosition = startPosition + new Vector2(0, moveDistance);
        
        TextMeshProUGUI damageText = popup.transform.Find("Damage")?.GetComponent<TextMeshProUGUI>();
        UnityEngine.UI.Image iconImage = popup.transform.Find("Icon")?.GetComponent<UnityEngine.UI.Image>();
        Color originalColor = damageText != null ? damageText.color : Color.red;
        
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            if (popup == null) yield break;
            
            float t = elapsed / duration;
            
            // 위치 이동 (위로)
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, targetPosition, t);
            
            // 페이드 아웃 (fadeStartTime 이후부터)
            if (elapsed > fadeStartTime)
            {
                float fadeT = (elapsed - fadeStartTime) / (duration - fadeStartTime);
                float alpha = Mathf.Lerp(1f, 0f, fadeT);
                
                Color currentColor = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                
                // 대미지 텍스트 페이드
                if (damageText != null)
                {
                    damageText.color = currentColor;
                }
                
                // 아이콘 페이드
                if (iconImage != null)
                {
                    iconImage.color = currentColor;
                }
            }
            
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        // 완전히 투명하게 만들고 오브젝트 제거
        if (damageText != null)
            damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        if (iconImage != null)
            iconImage.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        
        if (popup != null)
            Destroy(popup);
    }

}
