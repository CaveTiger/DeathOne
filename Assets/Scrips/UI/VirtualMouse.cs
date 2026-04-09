using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// UI 전용 Virtual Mouse 시스템 - UI 요소들이 마우스를 따라가도록 관리
/// </summary>
public class VirtualMouse : MonoBehaviour
{
    public static VirtualMouse Instance { get; private set; }
    
    [Header("Virtual Mouse UI 설정")]
    [SerializeField] private RectTransform virtualMouseTransform;
    [SerializeField] private Image cursorImage;
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private GraphicRaycaster graphicRaycaster; // UI 레이캐스터
    [Header("디버그")]
    [SerializeField] private bool debugHoverDetect = false; // 요약 로그(진입/데이터)
    [SerializeField] private bool debugHoverVerbose = false; // 상세 히트 로그([VM-Hit])
    
    [Header("커서 텍스처")]
    [SerializeField] private Texture2D defaultCursor;
    [SerializeField] private Texture2D hoverCursor;
    [SerializeField] private Texture2D attackCursor;
    [SerializeField] private Texture2D interactCursor;
    
    [Header("UI 요소 관리")]
    [SerializeField] private List<RectTransform> followElements = new List<RectTransform>();
    [SerializeField] private bool enableVirtualMouse = true;
    
    [Header("호버 감지 설정")]
    [SerializeField] private string[] hoverableTags = {"Skill", "Item", "Character"}; // 호버 가능한 태그들
    [SerializeField] private LayerMask hoverLayerMask = 1 << 5; // UI 레이어 (인덱스 5)
    [SerializeField] private bool enableHoverDetection = true; // 호버 감지 활성화
    
    private Vector2 currentMousePosition;
    private CursorType currentCursorType = CursorType.Default;
    
    // 호버 감지 관련
    private GameObject currentHoveredObject;
    private VirtualMouseUIPanel currentActivePanel;
    
    [Header("패널 참조")]
    [SerializeField] private VirtualMouseSkillPanel skillPanel;
    [SerializeField] private GameObject stEfDecAnchor; // StEfDecAnchor GameObject (Inspector에서 할당)
    [Header("상태이상 팝업 핸들러")]
    [SerializeField] private VirtualMouseStEfPanel statusStEfPanel; // 신 네이밍 패널(StEfPanel)
    private bool statusHoverActive = false;
    
    // 호버 유효성 체크용
    private Coroutine hoverValidationCoroutine;
    
    /// <summary>
    /// 상태이상 타입별 데이터 추출 델리게이트
    /// </summary>
    private delegate void StatusEffectDataExtractor(GameObject obj, out Sprite icon, out string description, out int value, out int turns);
    
    /// <summary>
    /// 등록된 상태이상 타입별 데이터 추출 메서드 매핑
    /// </summary>
    private Dictionary<System.Type, StatusEffectDataExtractor> statusEffectExtractors = new Dictionary<System.Type, StatusEffectDataExtractor>();
    
    public enum CursorType
    {
        Default,
        Hover,
        Attack,
        Interact
    }
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            // 지속 유지 책임은 Canvas 쪽에서 루트 기준으로 처리
            Debug.Log("[VirtualMouse] Instance set");
        }
        else
        {
            Debug.Log("[VirtualMouse] 중복된 Virtual Mouse 제거");
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
        StopHoverValidation();
        // 파괴 직전에도 패널이 남지 않도록 (DontDestroyOnLoad 재구성 시)
        ForceHidePanelsInternal();
    }

    /// <summary>
    /// 스킬/상태이상 호버 UI를 모두 끕니다. 전투 종료·UI 정상 모드 전환·호버 유효성 상실 시.</summary>
    public void DismissAllHoverUi()
    {
        StopHoverValidation();
        currentHoveredObject = null;
        ForceHidePanelsInternal();
        SetCursor(CursorType.Default);
    }

    private void ForceHidePanelsInternal()
    {
        HideStatusEffectPopup();
        if (currentActivePanel != null)
        {
            if (currentActivePanel is VirtualMouseSkillPanel)
            {
                currentActivePanel.SetVisible(false, false);
                if (stEfDecAnchor != null)
                    stEfDecAnchor.SetActive(false);
            }
            else
            {
                currentActivePanel.SetVisible(false, false);
            }
            currentActivePanel = null;
        }
    }
    
    void Start()
    {
        InitializeVirtualMouse();
    }
    
    void Update()
    {
        if (enableVirtualMouse)
        {
            UpdateMousePosition();
            UpdateFollowElements();
            
            // 전투 행동 수행 중인지 확인: BattleUIManager의 IsInBattleMode 플래그 사용
            bool isInCombatAction = IsInCombatAction();
            
            if (enableHoverDetection && !isInCombatAction)
            {
                DetectHoveredObject();
            }
            else if (isInCombatAction)
            {
                // 전투 중에는 호버 감지 비활성화 및 현재 호버 상태 정리
                if (currentHoveredObject != null)
                {
                    OnHoverExit(currentHoveredObject);
                    currentHoveredObject = null;
                }
            }
        }
    }
    
    /// <summary>
    /// Virtual Mouse 초기화
    /// </summary>
    private void InitializeVirtualMouse()
    {
        if (virtualMouseTransform == null)
        {
            virtualMouseTransform = GetComponent<RectTransform>();
        }
        
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();
        }
        // 씬에 상위 Canvas가 전혀 없다면 자체 Canvas를 생성해 최상단에 렌더되도록 함
        if (targetCanvas == null)
        {
            targetCanvas = gameObject.AddComponent<Canvas>();
            targetCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            targetCanvas.sortingOrder = 1000; // 항상 최상단에 보이도록 충분히 큰 값
            var scaler = gameObject.GetComponent<UnityEngine.UI.CanvasScaler>();
            if (scaler == null)
            {
                scaler = gameObject.AddComponent<UnityEngine.UI.CanvasScaler>();
                scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
                scaler.referenceResolution = new Vector2(1920, 1080);
                scaler.matchWidthOrHeight = 0.5f;
            }
        }
        if (graphicRaycaster == null && targetCanvas != null)
        {
            graphicRaycaster = targetCanvas.GetComponent<GraphicRaycaster>();
            if (graphicRaycaster == null)
            {
                graphicRaycaster = targetCanvas.gameObject.AddComponent<GraphicRaycaster>();
            }
        }
        
        // 패널/앵커 기본 비활성화 (호버 시에만 표시)
        // 스킬 패널은 SetActive로만 제어
        if (skillPanel != null)
        {
            skillPanel.SetVisible(false, false);
        }
        var panelSkillInfoObj = transform.Find("PanelSkillInfo");
        if (panelSkillInfoObj != null)
        {
            panelSkillInfoObj.gameObject.SetActive(false);
        }
        // StEfDecAnchor는 Inspector에서 할당받음
        if (stEfDecAnchor != null)
        {
            stEfDecAnchor.SetActive(false);
        }

        // 기본 커서 설정
        SetCursor(CursorType.Default);

        // 상태이상 팝업 핸들러 자동 탐색(인스펙터 누락 대비)
        if (statusStEfPanel == null)
        {
            // 비활성화된 오브젝트도 찾기
            statusStEfPanel = FindFirstObjectByType<VirtualMouseStEfPanel>(FindObjectsInactive.Include);
            if (statusStEfPanel != null && !statusStEfPanel.gameObject.activeSelf)
            {
                // 비활성화되어 있으면 활성화
                statusStEfPanel.gameObject.SetActive(true);
            }
        }
        
        // 상태이상 타입별 데이터 추출 메서드 등록
        RegisterStatusEffectExtractors();
    }
    
    /// <summary>
    /// 상태이상 타입별 데이터 추출 메서드 등록
    /// </summary>
    private void RegisterStatusEffectExtractors()
    {
        statusEffectExtractors[typeof(StatusEffectInstanceStun)] = ExtractStatusEffectStunData;

        // StatusEffectInstance: GetStatusPopupData 메서드 사용
        statusEffectExtractors[typeof(StatusEffectInstance)] = ExtractStatusEffectInstanceData;
        
        // StatusEffectInstanceReaction: EffectData/value/remainingTurns 직접 사용
        statusEffectExtractors[typeof(StatusEffectInstanceReaction)] = ExtractStatusEffectReactionData;
        
        // StatusEffectInstanceBuff: EffectData/value/remainingTurns 직접 사용
        statusEffectExtractors[typeof(StatusEffectInstanceBuff)] = ExtractStatusEffectBuffData;
        
        Debug.Log($"[VirtualMouse] 상태이상 타입 {statusEffectExtractors.Count}개 등록 완료");
    }
    
    /// <summary>
    /// 마우스 위치 업데이트
    /// </summary>
    private void UpdateMousePosition()
    {
        currentMousePosition = Input.mousePosition;
        
        if (virtualMouseTransform != null)
        {
            // Canvas 좌표로 변환
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetCanvas.transform as RectTransform,
                currentMousePosition,
                targetCanvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : targetCanvas.worldCamera,
                out localPoint
            );
            
            virtualMouseTransform.anchoredPosition = localPoint;
        }
    }
    
    /// <summary>
    /// 따라갈 UI 요소들 업데이트
    /// </summary>
    private void UpdateFollowElements()
    {
        if (followElements == null || followElements.Count == 0) return;
        
        foreach (var element in followElements)
        {
            if (element != null && element.gameObject.activeInHierarchy)
            {
                // Virtual Mouse 위치를 따라가도록 설정
                element.anchoredPosition = virtualMouseTransform.anchoredPosition;
            }
        }
    }
    
    /// <summary>
    /// 커서 타입 설정
    /// </summary>
    public void SetCursor(CursorType cursorType)
    {
        currentCursorType = cursorType;
        
        Texture2D cursorTexture = GetCursorTexture(cursorType);
        
        // 에디터에서 참조 누락 시에도 안전하게 동작하도록 가드
        if (cursorTexture == null)
        {
            // 시스템 기본 커서 사용, 가상 커서 이미지 비활성
            Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
            if (cursorImage != null)
            {
                cursorImage.enabled = false;
            }
            Debug.LogWarning("[VirtualMouse] 커서 텍스처가 설정되지 않았습니다. 시스템 기본 커서를 사용합니다.");
            return;
        }
        
        Cursor.SetCursor(cursorTexture, Vector2.zero, CursorMode.Auto);
        
        // Virtual Mouse 이미지도 업데이트
        if (cursorImage != null)
        {
            cursorImage.enabled = true;
            cursorImage.sprite = Sprite.Create(cursorTexture, new Rect(0, 0, cursorTexture.width, cursorTexture.height), Vector2.zero);
        }
    }
    
    /// <summary>
    /// 커서 텍스처 가져오기
    /// </summary>
    private Texture2D GetCursorTexture(CursorType cursorType)
    {
        switch (cursorType)
        {
            case CursorType.Default:
                return defaultCursor;
            case CursorType.Hover:
                return hoverCursor;
            case CursorType.Attack:
                return attackCursor;
            case CursorType.Interact:
                return interactCursor;
            default:
                return defaultCursor;
        }
    }
    
    /// <summary>
    /// 따라갈 UI 요소 추가
    /// </summary>
    public void AddFollowElement(RectTransform element)
    {
        if (element != null && !followElements.Contains(element))
        {
            followElements.Add(element);
        }
    }
    
    /// <summary>
    /// 따라갈 UI 요소 제거
    /// </summary>
    public void RemoveFollowElement(RectTransform element)
    {
        if (followElements.Contains(element))
        {
            followElements.Remove(element);
        }
    }
    
    /// <summary>
    /// 모든 따라갈 UI 요소 제거
    /// </summary>
    public void ClearFollowElements()
    {
        followElements.Clear();
    }
    
    /// <summary>
    /// Virtual Mouse 활성화/비활성화
    /// </summary>
    public void SetVirtualMouseEnabled(bool enabled)
    {
        enableVirtualMouse = enabled;
    }
    
    /// <summary>
    /// 현재 마우스 위치 가져오기
    /// </summary>
    public Vector2 GetMousePosition()
    {
        return currentMousePosition;
    }
    
    /// <summary>
    /// 현재 커서 타입 가져오기
    /// </summary>
    public CursorType GetCurrentCursorType()
    {
        return currentCursorType;
    }
    
    /// <summary>
    /// 따라갈 UI 요소 개수 가져오기
    /// </summary>
    public int GetFollowElementCount()
    {
        return followElements.Count;
    }
    
    // 호버 감지 관련 메서드들
    
    /// <summary>
    /// 호버된 오브젝트 감지 (2D) - 태그 + UI 레이어 기반
    /// </summary>
    private void DetectHoveredObject()
    {
        GameObject newHoveredObject = null;
        bool uiDetectedSkill = false; // UI에서 스킬 관련 컴포넌트를 감지했는지 여부

        // 1) UI 그래픽 레이캐스트 (캔버스 UI용) - 우선순위 최우선
        if (EventSystem.current != null)
        {
            PointerEventData ped = new PointerEventData(EventSystem.current);
            ped.position = currentMousePosition;
            var results = new System.Collections.Generic.List<RaycastResult>();
            // 모든 GraphicRaycaster를 대상으로 레이캐스트
            EventSystem.current.RaycastAll(ped, results);

            foreach (var rr in results)
            {
                var go = rr.gameObject;
                if (go == null) continue;
                // 맞은 오브젝트 또는 부모 체인에서 스킬 관련 컴포넌트 탐색
                var skillInstance = go.GetComponentInParent<SkillInstance>();
                var skillBlock = go.GetComponentInParent<SkillBlock>();
                var statusEffectComponent = FindStatusEffectComponent(go);
                
                // 상태이상 우선 체크 (상태이상과 스킬 분리)
                if (statusEffectComponent != null)
                {
                    newHoveredObject = statusEffectComponent.gameObject;
                    if (debugHoverDetect && debugHoverVerbose) Debug.Log($"[VM-Hit][UI] {go.name} → StatusEffect({statusEffectComponent.GetType().Name})");
                    break;
                }
                
                // 스킬 관련 컴포넌트 체크 (상태이상이 아닐 때만)
                if (skillInstance != null)
                {
                    newHoveredObject = skillInstance.gameObject;
                    uiDetectedSkill = true;
                    if (debugHoverDetect && debugHoverVerbose) Debug.Log($"[VM-Hit][UI] {go.name} → SkillInstance({newHoveredObject.name})");
                    break;
                }
                if (skillBlock != null)
                {
                    newHoveredObject = skillBlock.gameObject;
                    uiDetectedSkill = true;
                    if (debugHoverDetect && debugHoverVerbose) Debug.Log($"[VM-Hit][UI] {go.name} → SkillBlock({newHoveredObject.name})");
                    break;
                }
                if (IsHoverableTag(go.tag))
                {
                    newHoveredObject = go;
                    if (debugHoverDetect && debugHoverVerbose) Debug.Log($"[VM-Hit][UI] {go.name} → Tag({go.tag})");
                    break;
                }
            }
        }

        // 2) 물리 2D 레이캐스트 (월드 오브젝트용) - UI에서 감지 실패 시에만 사용
        // 상태이상 호버는 VirtualMouseWorldObject의 콜라이더 충돌에 맡김 (월드 오브젝트끼리 충돌)
        // 여기서는 스킬 등 다른 월드 오브젝트만 체크
        if (!uiDetectedSkill && Camera.main != null)
        {
            // 스크린 좌표를 월드 좌표로 변환 (2D 카메라용)
            Vector3 screenPos = new Vector3(currentMousePosition.x, currentMousePosition.y, Camera.main.nearClipPlane);
            Vector3 worldPos = Camera.main.ScreenToWorldPoint(screenPos);
            Vector2 worldPoint2D = new Vector2(worldPos.x, worldPos.y);
            
            // 월드 좌표에서 레이캐스트 (방향은 없고 거리 0으로 오버랩 체크)
            RaycastHit2D hit = Physics2D.Raycast(worldPoint2D, Vector2.zero, 0f, hoverLayerMask);
            
            if (debugHoverDetect && debugHoverVerbose)
            {
                Debug.Log($"[VM-Hit][PHY] 레이캐스트 시도: screenPos={currentMousePosition}, worldPos={worldPoint2D}, layerMask={hoverLayerMask.value}, hit={(hit.collider != null ? hit.collider.name : "null")}");
            }
            
            if (hit.collider != null)
            {
                var go = hit.collider.gameObject;
                if (debugHoverDetect && debugHoverVerbose)
                {
                    Debug.Log($"[VM-Hit][PHY] 충돌 감지: {go.name}, Layer={LayerMask.LayerToName(go.layer)}");
                }
                
                // UI에서도 감지 안 된 경우에만 다른 타입 체크 (상태이상은 제외 - 콜라이더 충돌에 맡김)
                if (newHoveredObject == null)
                {
                    var skillInstance = go.GetComponentInParent<SkillInstance>();
                    var skillBlock = go.GetComponentInParent<SkillBlock>();
                    
                    if (skillInstance != null)
                    {
                        newHoveredObject = skillInstance.gameObject;
                        if (debugHoverDetect && debugHoverVerbose) Debug.Log($"[VM-Hit][PHY] {go.name} → SkillInstance({newHoveredObject.name})");
                    }
                    else if (skillBlock != null)
                    {
                        newHoveredObject = skillBlock.gameObject;
                        if (debugHoverDetect && debugHoverVerbose) Debug.Log($"[VM-Hit][PHY] {go.name} → SkillBlock({newHoveredObject.name})");
                    }
                }
            }
        }
        
        // 호버된 오브젝트가 변경되었는지 확인
        if (newHoveredObject != currentHoveredObject)
        {
            OnHoveredObjectChanged(currentHoveredObject, newHoveredObject);
            currentHoveredObject = newHoveredObject;
        }
    }
    
    /// <summary>
    /// 태그가 호버 가능한지 확인
    /// </summary>
    private bool IsHoverableTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return false;
        
        foreach (string hoverableTag in hoverableTags)
        {
            if (tag == hoverableTag)
                return true;
        }
        return false;
    }
    
    /// <summary>
    /// 호버된 오브젝트가 변경될 때 호출
    /// </summary>
    private void OnHoveredObjectChanged(GameObject previousObject, GameObject newObject)
    {
        // 이전 오브젝트에서 벗어남
        if (previousObject != null)
        {
            OnHoverExit(previousObject);
        }
        
        // 새로운 오브젝트에 호버
        if (newObject != null)
        {
            OnHoverEnter(newObject);
        }
    }
    
    /// <summary>
    /// 월드 오브젝트에서 호버 진입 (VirtualMouseWorldObject에서 호출)
    /// </summary>
    public void OnWorldObjectHoverEnter(GameObject hoveredObject)
    {
        // 전투 행동 수행 중이면 호버 감지 비활성화
        if (IsInCombatAction())
        {
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] 전투 중이므로 호버 감지 비활성화: {hoveredObject?.name}");
            }
            return;
        }
        
        if (hoveredObject == null)
        {
            if (debugHoverDetect)
            {
                Debug.LogWarning("[VirtualMouse] 월드 오브젝트 호버 진입: hoveredObject가 null입니다.");
            }
            return;
        }
        
        if (debugHoverDetect)
        {
            Debug.Log($"[VirtualMouse] 월드 오브젝트 호버 진입 시도: {hoveredObject.name}, 활성화: {hoveredObject.activeSelf}, 레이어: {LayerMask.LayerToName(hoveredObject.layer)}");
        }
        
        // 상태이상 인스턴스라면 전용 팝업으로 처리
        Component statusEffectComponent = FindStatusEffectComponent(hoveredObject);
        if (debugHoverDetect)
        {
            Debug.Log($"[VirtualMouse] FindStatusEffectComponent 결과: {(statusEffectComponent != null ? statusEffectComponent.GetType().Name : "null")}, 오브젝트: {hoveredObject.name}");
        }
        
        if (statusEffectComponent != null)
        {
            // 다른 호버 패널(스킬 설명 등) 숨기기
            if (currentActivePanel != null)
            {
                if (debugHoverDetect)
                {
                    Debug.Log($"[VirtualMouse] 상태이상 호버 시 다른 패널 숨김: {currentActivePanel.name}");
                }
                currentActivePanel.SetVisible(false, false);
                currentActivePanel = null;
            }
            
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] 상태이상 컴포넌트 발견: {statusEffectComponent.GetType().Name}, gameObject: {statusEffectComponent.gameObject.name}, 팝업 표시 시도");
            }
            ShowStatusEffectPopup(statusEffectComponent.gameObject);
            SetCursor(CursorType.Hover);
            // 호버 유효성 체크 코루틴 시작
            StartHoverValidation();
        }
        else
        {
            if (debugHoverDetect)
            {
                Debug.LogWarning($"[VirtualMouse] 월드 오브젝트 호버 진입 실패: {hoveredObject.name}에서 상태이상 컴포넌트를 찾을 수 없습니다. 등록된 타입 수: {statusEffectExtractors.Count}");
            }
        }
    }
    
    /// <summary>
    /// 월드 오브젝트에서 호버 벗어남 (VirtualMouseWorldObject에서 호출)
    /// </summary>
    public void OnWorldObjectHoverExit(GameObject hoveredObject)
    {
        if (hoveredObject == null)
        {
            if (debugHoverDetect)
            {
                Debug.LogWarning("[VirtualMouse] 월드 오브젝트 호버 벗어남: hoveredObject가 null입니다.");
            }
            return;
        }
        
        if (debugHoverDetect)
        {
            Debug.Log($"[VirtualMouse] 월드 오브젝트 호버 벗어남: {hoveredObject.name}, statusHoverActive={statusHoverActive}");
        }
        
        // 상태이상 호버가 활성화되어 있으면 팝업 숨김
        if (statusHoverActive)
        {
            Component statusEffectComponent = FindStatusEffectComponent(hoveredObject);
            if (statusEffectComponent != null)
            {
                if (debugHoverDetect)
                {
                    Debug.Log($"[VirtualMouse] 상태이상 팝업 숨김: {hoveredObject.name}");
                }
            }
            else if (debugHoverDetect)
            {
                Debug.LogWarning($"[VirtualMouse] 월드 오브젝트 호버 벗어남: {hoveredObject.name}에서 상태이상 컴포넌트를 찾을 수 없습니다. (오브젝트 삭제 가능성)");
            }
            // 컴포넌트를 찾을 수 없어도 (오브젝트가 삭제되었을 수 있으므로) 팝업은 숨김
            HideStatusEffectPopup();
        }
        // 호버 유효성 체크 코루틴 중지
        StopHoverValidation();
    }
    
    /// <summary>
    /// 오브젝트에 호버 진입 (UI 레이캐스트 기반 - 스킬 등)
    /// 상태이상 호버는 VirtualMouseWorldObject의 콜라이더 충돌에 맡김
    /// </summary>
    private void OnHoverEnter(GameObject hoveredObject)
    {
        if (debugHoverDetect)
            Debug.Log($"[VirtualMouse] 호버 진입: {hoveredObject.name}");
        
        // UI 레이캐스트로 감지된 상태이상도 처리 (UI 상태이상) - 우선 처리
        // 월드 스페이스 상태이상은 VirtualMouseWorldObject의 콜라이더 충돌에서 처리
        Component statusEffectComponent = FindStatusEffectComponent(hoveredObject);
        if (statusEffectComponent != null)
        {
            ShowStatusEffectPopup(statusEffectComponent.gameObject);
            SetCursor(CursorType.Hover);
            // 호버 유효성 체크 코루틴 시작
            StartHoverValidation();
            return;
        }

        // SkillInfoPopup이 활성화되어 있으면 스킬 호버 패널 표시 안 함 (상태이상은 위에서 이미 처리됨)
        var skillInfoPopups = FindObjectsByType<SkillInfoPopup>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        if (skillInfoPopups != null && skillInfoPopups.Length > 0)
        {
            bool hasActivePopup = false;
            foreach (var popup in skillInfoPopups)
            {
                if (popup != null && popup.gameObject.activeSelf)
                {
                    hasActivePopup = true;
                    break;
                }
            }
            if (hasActivePopup)
            {
                return; // SkillInfoPopup이 활성화되어 있으면 스킬 호버 패널 표시 안 함
            }
        }

        // 오브젝트 타입에 따라 적절한 패널 활성화(스킬 등)
        VirtualMouseUIPanel targetPanel = DetermineTargetPanel(hoveredObject);
        
        if (targetPanel != null)
        {
            // 다른 패널이 활성화되어 있으면 숨김(즉시)
            if (currentActivePanel != null && currentActivePanel != targetPanel)
            {
                if (currentActivePanel is VirtualMouseSkillPanel)
                {
                    currentActivePanel.SetVisible(false, false);
                    if (stEfDecAnchor != null)
                        stEfDecAnchor.SetActive(false);
                }
                else
                {
                    currentActivePanel.SetVisible(false, false);
                }
            }
            
            // 상태이상 패널이 활성화되어 있으면 숨김
            if (statusHoverActive && statusStEfPanel != null)
            {
                statusStEfPanel.HideStatusPopup();
                statusHoverActive = false;
            }
            
            // 같은 패널이어도 데이터는 항상 갱신 (중요!)
            // 데이터를 먼저 설정한 후 패널 표시
            SetupPanelData(targetPanel, hoveredObject);
            
            // 패널 활성화 (같은 패널이어도 다시 표시하여 갱신 보장)
            currentActivePanel = targetPanel;
            
            if (targetPanel is VirtualMouseSkillPanel)
            {
                targetPanel.SetVisible(true, false);
                if (stEfDecAnchor != null)
                    stEfDecAnchor.SetActive(true);
            }
            else
            {
                targetPanel.SetVisible(true, false);
            }
            
            // 화면 좌우에 맞게 위치 조정
            targetPanel.PositionPanelLeftOrRight(currentMousePosition);
            bool showOnLeft = targetPanel.ShouldShowPanelOnLeft(currentMousePosition);
            // 상태이상 앵커도 동일 규칙으로 배치
            if (targetPanel is VirtualMouseSkillPanel vmSkill)
            {
                vmSkill.UpdateStatusAnchorPosition(currentMousePosition, showOnLeft);
            }
            
            // 커서 타입 변경
            SetCursor(CursorType.Hover);
            // 호버 유효성 체크 코루틴 시작
            StartHoverValidation();
        }
    }
    
    /// <summary>
    /// 오브젝트에서 호버 벗어남
    /// </summary>
    private void OnHoverExit(GameObject hoveredObject)
    {
        if (debugHoverDetect)
            Debug.Log($"[VirtualMouse] 호버 벗어남: {hoveredObject.name}, statusHoverActive={statusHoverActive}");
        
        // 상태이상 팝업 활성 중이면 우선 숨김
        if (statusHoverActive)
        {
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] OnHoverExit에서 상태이상 팝업 숨김 시도");
            }
            HideStatusEffectPopup();
        }
        
        // 다른 패널이 활성화되어 있으면 숨김
        if (currentActivePanel != null)
        {
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] OnHoverExit에서 다른 패널 숨김: {currentActivePanel.name}");
            }
            
            if (currentActivePanel is VirtualMouseSkillPanel)
            {
                currentActivePanel.SetVisible(false, false);
                if (stEfDecAnchor != null)
                    stEfDecAnchor.SetActive(false);
            }
            else
            {
                currentActivePanel.SetVisible(false, false);
            }
            
            currentActivePanel = null;
        }
        
        // 커서 타입 복원
        SetCursor(CursorType.Default);
        // 호버 유효성 체크 코루틴 중지
        StopHoverValidation();
    }
    
    /// <summary>
    /// 전투 행동 수행 중인지 확인 (BattleUIManager의 IsInBattleMode 플래그 사용)
    /// </summary>
    /// <returns>true면 전투 행동 수행 중 (호버 감지 비활성화), false면 통상 상태 (호버 감지 활성화)</returns>
    private bool IsInCombatAction()
    {
        // BattleUIManager 싱글톤을 통해 전투 모드 상태 확인
        if (BattleUIManager.Instance == null)
        {
            // BattleUIManager가 없으면 통상 상태로 간주
            return false;
        }
        
        // IsInBattleMode 플래그로 전투 상태 확인
        // ChangeUIBattle()에서 true로 설정, ChangeUINormal()에서 false로 설정
        return BattleUIManager.Instance.IsInBattleMode;
    }
    
    /// <summary>
    /// 호버된 오브젝트에 따라 대상 패널 결정
    /// </summary>
    private VirtualMouseUIPanel DetermineTargetPanel(GameObject hoveredObject)
    {
        // 컴포넌트 기준으로 판단 (태그 의존 제거)
        if (hoveredObject.GetComponentInParent<SkillInstance>() != null ||
            hoveredObject.GetComponentInParent<SkillBlock>() != null)
            return skillPanel;
        
        // 기본 패널 반환 (추후 구현)
        return null;
    }
    
    /// <summary>
    /// 패널에 데이터 설정
    /// </summary>
    private void SetupPanelData(VirtualMouseUIPanel panel, GameObject hoveredObject)
    {
        // 스킬 패널인 경우
        if (panel is VirtualMouseSkillPanel skillPanel)
        {
            // 스킬 데이터 가져오기
            SkillData skillData = GetSkillDataFromObject(hoveredObject);
            if (debugHoverDetect)
            {
                Debug.Log($"[VM-Data] from={hoveredObject?.name}, skillData={(skillData!=null ? skillData.ID+":"+skillData.Name : "NULL")}");
            }
            if (skillData != null)
            {
                skillPanel.SetSkillData(skillData);
            }
        }
        // 상태이상 패널은 DetermineTargetPanel에서 별도 처리
        // 추후 다른 패널 타입들도 추가
    }

    // === 상태이상 팝업 표시/숨김 ===
    
    /// <summary>
    /// 호버된 오브젝트에서 등록된 상태이상 컴포넌트 찾기
    /// </summary>
    private Component FindStatusEffectComponent(GameObject obj)
    {
        if (obj == null)
        {
            // if (debugHoverDetect)
            //     Debug.LogWarning("[VM-Status] FindStatusEffectComponent: obj가 null입니다.");
            return null;
        }

        // 넉다운 전용 슬롯은 호버하지 않음(투명 프리팹 대비). 부모에 다른 상태이상이 있어도 여기서 차단.
        if (obj.GetComponentInParent<StatusEffectInstanceKnockdown>() != null)
            return null;
        
        // if (debugHoverDetect)
        // {
        //     Debug.Log($"[VM-Status] 상태이상 컴포넌트 검색 시작: {obj.name}, 활성화: {obj.activeSelf}, 등록된 타입 수={statusEffectExtractors.Count}");
        // }
        
        // 등록된 타입 순서대로 체크
        foreach (var type in statusEffectExtractors.Keys)
        {
            // if (debugHoverDetect && debugHoverVerbose)
            // {
            //     Debug.Log($"[VM-Status] 타입 체크 중: {type.Name} on {obj.name}");
            // }
            
            var component = obj.GetComponentInParent(type);
            if (component != null)
            {
                // if (debugHoverDetect)
                // {
                //     Debug.Log($"[VM-Status] 상태이상 컴포넌트 발견: {type.Name} on {obj.name}, component.gameObject: {component.gameObject.name}");
                // }
                return component;
            }
            // else if (debugHoverDetect && debugHoverVerbose)
            // {
            //     Debug.Log($"[VM-Status] 타입 {type.Name} 컴포넌트 없음");
            // }
        }
        
        // if (debugHoverDetect)
        // {
        //     Debug.LogWarning($"[VM-Status] 상태이상 컴포넌트 없음: {obj.name}, 모든 타입 체크 완료");
        // }
        
        return null;
    }
    
    private void ShowStatusEffectPopup(GameObject statusObj)
    {
        if (statusObj == null)
        {
            if (debugHoverDetect)
                Debug.LogWarning("[VirtualMouse] ShowStatusEffectPopup: statusObj가 null입니다.");
            return;
        }
        
        if (debugHoverDetect)
        {
            Debug.Log($"[VirtualMouse] ShowStatusEffectPopup 시작: {statusObj.name}, 활성화: {statusObj.activeSelf}");
        }
        
        // 다른 호버 패널(스킬 설명 등) 숨기기
        if (currentActivePanel != null)
        {
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] 상태이상 호버 시 다른 패널 숨김: {currentActivePanel.name}");
            }
            if (currentActivePanel is VirtualMouseSkillPanel)
            {
                currentActivePanel.SetVisible(false, false);
            }
            else
            {
                currentActivePanel.SetVisible(false, false);
            }
            currentActivePanel = null;
        }
        
        // 상태이상 인스턴스에서 정보만 읽기
        StatusEffectInstanceStun statusStun = statusObj.GetComponentInParent<StatusEffectInstanceStun>();
        StatusEffectInstance statusInstance = statusObj.GetComponentInParent<StatusEffectInstance>();
        StatusEffectInstanceBuff statusBuff = statusObj.GetComponentInParent<StatusEffectInstanceBuff>();
        StatusEffectInstanceReaction statusReaction = statusObj.GetComponentInParent<StatusEffectInstanceReaction>();
        
        StatusEffectData effectData = null;
        int value = 0;
        int turns = 0;
        
        if (statusStun != null)
        {
            effectData = statusStun.EffectData;
            value = 0;
            turns = 0;
            if (debugHoverDetect)
                Debug.Log($"[VirtualMouse] StatusEffectInstanceStun 발견: effectData={(effectData != null ? effectData.effectName : "null")}");
        }
        else if (statusInstance != null)
        {
            effectData = statusInstance.EffectData;
            value = statusInstance.value;
            turns = statusInstance.remainingTurns;
            
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] StatusEffectInstance 발견: effectData={(effectData != null ? effectData.effectName : "null")}, value={value}, turns={turns}");
            }
        }
        else if (statusBuff != null)
        {
            effectData = statusBuff.EffectData;
            value = statusBuff.value;
            turns = statusBuff.remainingTurns;
            
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] StatusEffectInstanceBuff 발견: effectData={(effectData != null ? effectData.effectName : "null")}, value={value}, turns={turns}");
            }
        }
        else if (statusReaction != null)
        {
            effectData = statusReaction.EffectData;
            value = statusReaction.value;
            turns = statusReaction.remainingTurns;
            
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] StatusEffectInstanceReaction 발견: effectData={(effectData != null ? effectData.effectName : "null")}, value={value}, turns={turns}");
            }
        }
        else
        {
            if (debugHoverDetect)
            {
                Debug.LogWarning($"[VirtualMouse] 상태이상 컴포넌트를 찾을 수 없습니다: {statusObj.name}");
            }
            return;
        }
        
        // VirtualMouseStEfPanel 직접 사용
        if (effectData != null)
        {
            // statusStEfPanel이 null이면 찾기
            if (statusStEfPanel == null)
            {
                if (debugHoverDetect)
                    Debug.Log("[VirtualMouse] statusStEfPanel이 null, 찾는 중...");
                // 비활성화된 오브젝트도 찾기
                statusStEfPanel = FindFirstObjectByType<VirtualMouseStEfPanel>(FindObjectsInactive.Include);
                if (statusStEfPanel == null)
                {
                    Debug.LogWarning("[VirtualMouse] VirtualMouseStEfPanel을 찾을 수 없어 상태이상 팝업을 표시하지 않습니다.");
                    return;
                }
                if (debugHoverDetect)
                {
                    Debug.Log($"[VirtualMouse] statusStEfPanel 찾음: {statusStEfPanel.name}, 활성화: {statusStEfPanel.gameObject.activeSelf}");
                }
                // 비활성화되어 있으면 활성화 (부모까지 포함)
                if (!statusStEfPanel.gameObject.activeInHierarchy)
                {
                    if (debugHoverDetect)
                    {
                        Debug.Log($"[VirtualMouse] statusStEfPanel 활성화: {statusStEfPanel.name}, activeSelf={statusStEfPanel.gameObject.activeSelf}, activeInHierarchy={statusStEfPanel.gameObject.activeInHierarchy}");
                    }
                    // 부모 오브젝트도 활성화
                    Transform parent = statusStEfPanel.transform.parent;
                    while (parent != null && !parent.gameObject.activeSelf)
                    {
                        parent.gameObject.SetActive(true);
                        parent = parent.parent;
                    }
                    statusStEfPanel.gameObject.SetActive(true);
                }
            }
            
            // 아이콘 결정 (버프/디버프는 동적 아이콘 우선)
            Sprite icon = null;
            if (effectData.effectType == StatusEffectType.Buff || effectData.effectType == StatusEffectType.Debuff)
            {
                icon = effectData.GetDynamicIcon(value);
                if (icon == null)
                {
                    icon = effectData.GetIcon();
                }
            }
            else
            {
                icon = effectData.icon;
                if (icon == null)
                {
                    icon = effectData.GetIcon();
                }
            }
            
            string description;
            bool stunDescriptionPanel = effectData.effectType == StatusEffectType.Stun;
            if (stunDescriptionPanel)
            {
                description = string.IsNullOrWhiteSpace(effectData.description)
                    ? "한 턴간 쉽니다."
                    : effectData.description.Trim();
            }
            else
            {
                description = string.IsNullOrEmpty(effectData.description) ? effectData.effectName : effectData.description;
            }
            
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] VirtualMouseStEfPanel로 팝업 표시: icon={(icon != null ? icon.name : "null")}, desc={description}, value={value}, turns={turns}, stunDescMode={stunDescriptionPanel}");
            }
            
            // 오브젝트가 활성화되어 있는지 다시 확인
            if (!statusStEfPanel.gameObject.activeInHierarchy)
            {
                if (debugHoverDetect)
                {
                    Debug.LogWarning($"[VirtualMouse] statusStEfPanel이 여전히 비활성화 상태입니다. 강제 활성화 시도");
                }
                Transform parent = statusStEfPanel.transform.parent;
                while (parent != null && !parent.gameObject.activeSelf)
                {
                    parent.gameObject.SetActive(true);
                    parent = parent.parent;
                }
                statusStEfPanel.gameObject.SetActive(true);
            }
            
            statusStEfPanel.ShowStatusPopup(icon, description, value, turns, stunDescriptionPanel);
            statusHoverActive = true;
        }
        else
        {
            Debug.LogWarning($"[VirtualMouse] 상태이상 팝업 데이터 추출 실패: {statusObj.name}");
        }
    }
    
    // === 상태이상 타입별 데이터 추출 메서드 ===

    private void ExtractStatusEffectStunData(GameObject obj, out Sprite icon, out string description, out int value, out int turns)
    {
        icon = null;
        description = string.Empty;
        value = 0;
        turns = 0;
        var st = obj.GetComponentInParent<StatusEffectInstanceStun>();
        if (st == null || st.EffectData == null) return;
        icon = st.EffectData.GetIcon();
        description = string.IsNullOrWhiteSpace(st.EffectData.description)
            ? "한 턴간 쉽니다."
            : st.EffectData.description.Trim();
    }
    
    /// <summary>
    /// StatusEffectInstance 타입 데이터 추출
    /// </summary>
    private void ExtractStatusEffectInstanceData(GameObject obj, out Sprite icon, out string description, out int value, out int turns)
    {
        icon = null;
        description = string.Empty;
        value = 0;
        turns = 0;
        
        var inst = obj.GetComponentInParent<StatusEffectInstance>();
        if (inst != null)
        {
            inst.GetStatusPopupData(out icon, out description, out value, out turns);
        }
    }
    
    /// <summary>
    /// StatusEffectInstanceReaction 타입 데이터 추출
    /// </summary>
    private void ExtractStatusEffectReactionData(GameObject obj, out Sprite icon, out string description, out int value, out int turns)
    {
        icon = null;
        description = string.Empty;
        value = 0;
        turns = 0;
        
        var rx = obj.GetComponentInParent<StatusEffectInstanceReaction>();
        if (rx != null && rx.EffectData != null)
        {
            // 버프/디버프 타입은 동적 아이콘 우선 사용 (음수값 대응)
            if (rx.EffectData.effectType == StatusEffectType.Buff || rx.EffectData.effectType == StatusEffectType.Debuff)
            {
                icon = rx.EffectData.GetDynamicIcon(rx.value);
                if (icon == null)
                {
                    icon = rx.EffectData.GetIcon();
                }
            }
            else
            {
                icon = rx.EffectData.icon;
                if (icon == null)
                {
                    icon = rx.EffectData.GetIcon();
                }
            }
            
            description = string.IsNullOrEmpty(rx.EffectData.description) ? rx.EffectData.effectName : rx.EffectData.description;
            value = rx.value;
            turns = Mathf.Max(rx.remainingTurns, 0);
        }
    }
    
    /// <summary>
    /// StatusEffectInstanceBuff 타입 데이터 추출
    /// </summary>
    private void ExtractStatusEffectBuffData(GameObject obj, out Sprite icon, out string description, out int value, out int turns)
    {
        icon = null;
        description = string.Empty;
        value = 0;
        turns = 0;
        
        var bf = obj.GetComponentInParent<StatusEffectInstanceBuff>();
        if (bf != null && bf.EffectData != null)
        {
            // 버프/디버프 타입은 동적 아이콘 우선 사용 (음수값 대응)
            if (bf.EffectData.effectType == StatusEffectType.Buff || bf.EffectData.effectType == StatusEffectType.Debuff)
            {
                icon = bf.EffectData.GetDynamicIcon(bf.value);
                if (icon == null)
                {
                    icon = bf.EffectData.GetIcon();
                }
            }
            else
            {
                icon = bf.EffectData.icon;
                if (icon == null)
                {
                    icon = bf.EffectData.GetIcon();
                }
            }
            
            description = string.IsNullOrEmpty(bf.EffectData.description) ? bf.EffectData.effectName : bf.EffectData.description;
            value = bf.value;
            turns = Mathf.Max(bf.remainingTurns, 0);
        }
    }

    private void HideStatusEffectPopup()
    {
        if (statusStEfPanel != null) statusStEfPanel.HideStatusPopup();
        statusHoverActive = false;
    }
    
    /// <summary>
    /// 오브젝트에서 스킬 데이터 가져오기
    /// </summary>
    private SkillData GetSkillDataFromObject(GameObject obj)
    {
        // SkillInstance에서 스킬 데이터 가져오기
        var skillInstance = obj.GetComponent<SkillInstance>();
        if (skillInstance != null)
        {
            return skillInstance.GetSkillData();
        }
        
        // SkillBlock에서 스킬 데이터 가져오기
        var skillBlock = obj.GetComponent<SkillBlock>();
        if (skillBlock != null)
        {
            return skillBlock.skillData;
        }
        
        Debug.LogWarning($"[VirtualMouse] 오브젝트에서 스킬 데이터를 찾을 수 없습니다: {obj.name}");
        return null;
    }
    
    /// <summary>
    /// 호버 감지 활성화/비활성화
    /// </summary>
    public void SetHoverDetectionEnabled(bool enabled)
    {
        enableHoverDetection = enabled;
        if (!enabled)
            DismissAllHoverUi();
    }
    
    /// <summary>
    /// 현재 호버된 오브젝트 가져오기
    /// </summary>
    public GameObject GetCurrentHoveredObject()
    {
        return currentHoveredObject;
    }
    
    /// <summary>
    /// 현재 활성 패널 가져오기
    /// </summary>
    public VirtualMouseUIPanel GetCurrentActivePanel()
    {
        return currentActivePanel;
    }
    
    /// <summary>
    /// 호버 유효성 체크 코루틴 시작
    /// </summary>
    private void StartHoverValidation()
    {
        // 이미 실행 중이면 중지 후 재시작
        StopHoverValidation();
        hoverValidationCoroutine = StartCoroutine(HoverValidationCoroutine());
        if (debugHoverDetect)
        {
            Debug.Log($"[VirtualMouse] 호버 유효성 체크 코루틴 시작 - currentHoveredObject: {(currentHoveredObject != null ? currentHoveredObject.name : "null")}, statusHoverActive: {statusHoverActive}");
        }
    }
    
    /// <summary>
    /// 호버 유효성 체크 코루틴 중지
    /// </summary>
    private void StopHoverValidation()
    {
        if (hoverValidationCoroutine != null)
        {
            StopCoroutine(hoverValidationCoroutine);
            hoverValidationCoroutine = null;
            if (debugHoverDetect)
            {
                Debug.Log("[VirtualMouse] 호버 유효성 체크 코루틴 중지");
            }
        }
    }
    
    /// <summary>
    /// 호버 유효성 체크 코루틴 - 5초마다 호버된 오브젝트가 유효한지 확인
    /// </summary>
    private IEnumerator HoverValidationCoroutine()
    {
        if (debugHoverDetect)
        {
            Debug.Log("[VirtualMouse] 호버 유효성 체크 코루틴 시작됨");
        }
        
        while (true)
        {
            yield return new WaitForSeconds(5f);
            
            if (debugHoverDetect)
            {
                Debug.Log($"[VirtualMouse] 호버 유효성 체크 실행 - currentHoveredObject: {(currentHoveredObject != null ? currentHoveredObject.name : "null")}, statusHoverActive: {statusHoverActive}, activeInHierarchy: {(currentHoveredObject != null ? currentHoveredObject.activeInHierarchy.ToString() : "N/A")}");
            }
            
            // 호버 상태가 없으면 코루틴 종료 (스킬 패널만 SetActive로 켠 경우 isVisible 불일치로 패널이 남을 수 있어 강제 정리)
            if (currentHoveredObject == null && !statusHoverActive)
            {
                if (debugHoverDetect)
                {
                    Debug.Log("[VirtualMouse] 호버 상태가 없어 유효성 체크 코루틴 종료");
                }
                hoverValidationCoroutine = null;
                ForceHidePanelsInternal();
                SetCursor(CursorType.Default);
                yield break;
            }
            
            // 현재 호버된 오브젝트가 유효한지 확인
            if (currentHoveredObject != null)
            {
                if (!currentHoveredObject.activeInHierarchy)
                {
                    if (debugHoverDetect)
                    {
                        Debug.Log($"[VirtualMouse] 호버된 오브젝트가 비활성화되어 호버 해제: {currentHoveredObject.name}");
                    }
                    OnHoverExit(currentHoveredObject);
                    currentHoveredObject = null;
                    continue;
                }
            }
            
            // 상태이상 호버가 활성화되어 있는 경우 체크
            if (statusHoverActive)
            {
                if (debugHoverDetect)
                {
                    Debug.Log($"[VirtualMouse] 상태이상 호버 활성화 상태 확인 - statusHoverActive: {statusHoverActive}, statusStEfPanel: {(statusStEfPanel != null ? statusStEfPanel.name : "null")}, currentHoveredObject: {(currentHoveredObject != null ? currentHoveredObject.name : "null")}");
                }
                
                // 상태이상 호버는 VirtualMouseWorldObject에서 관리되므로
                // VirtualMouseWorldObject의 currentHoveredStatusEffect를 확인
                bool worldObjectHoverValid = false;
                if (VirtualMouseWorldObject.Instance != null)
                {
                    worldObjectHoverValid = VirtualMouseWorldObject.Instance.IsHoveredStatusEffectValid();
                    if (debugHoverDetect)
                    {
                        var worldHoveredObj = VirtualMouseWorldObject.Instance.GetCurrentHoveredStatusEffect();
                        Debug.Log($"[VirtualMouse] 월드 오브젝트 호버 상태 확인 - 유효: {worldObjectHoverValid}, 오브젝트: {(worldHoveredObj != null ? worldHoveredObj.name : "null")}");
                    }
                }
                
                // 상태이상 호버가 활성화되어 있지만 월드 오브젝트 호버가 유효하지 않으면 해제
                if (!worldObjectHoverValid)
                {
                    if (debugHoverDetect)
                    {
                        Debug.Log("[VirtualMouse] 상태이상 호버가 활성화되어 있지만 월드 오브젝트 호버가 유효하지 않아 호버 해제");
                    }
                    HideStatusEffectPopup();
                }
            }
        }
    }
}
