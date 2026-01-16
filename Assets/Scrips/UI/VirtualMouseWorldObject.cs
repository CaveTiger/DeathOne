using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// VirtualMouse의 월드 오브젝트 버전 - 월드 스페이스에서 마우스를 따라다니며 상태이상 호버 감지
/// </summary>
public class VirtualMouseWorldObject : MonoBehaviour
{
    public static VirtualMouseWorldObject Instance { get; private set; }
    
    [Header("참조 설정")]
    [SerializeField] private VirtualMouse virtualMouse;
    [SerializeField] private Camera targetCamera;
    
    [Header("충돌 감지 설정")]
    [SerializeField] private LayerMask statusEffectLayerMask = -1; // 상태이상 레이어 마스크 (기본: 모든 레이어)
    
    [Header("디버그")]
    [SerializeField] private bool debugHover = false;
    
    private Vector3 currentWorldPosition;
    private GameObject currentHoveredStatusEffect;
    
    // 호버 유효성 체크용
    private Coroutine hoverValidationCoroutine;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        // 카메라 자동 탐색
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null)
            {
                targetCamera = FindFirstObjectByType<Camera>();
            }
        }
        
        // VirtualMouse 자동 탐색
        if (virtualMouse == null)
        {
            virtualMouse = VirtualMouse.Instance;
            if (virtualMouse == null)
            {
                virtualMouse = FindFirstObjectByType<VirtualMouse>();
            }
        }
    }
    
    void Update()
    {
        // 전투 행동 수행 중이면 호버 감지 비활성화
        if (IsInCombatAction())
        {
            // 전투 중에는 현재 호버 상태 정리
            if (currentHoveredStatusEffect != null && virtualMouse != null)
            {
                virtualMouse.OnWorldObjectHoverExit(currentHoveredStatusEffect);
                currentHoveredStatusEffect = null;
            }
            return;
        }
        
        // 카메라가 없으면 다시 찾기
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            if (targetCamera == null) return;
        }
        
        // 마우스 위치를 월드 좌표로 변환
        Vector3 screenPos = Input.mousePosition;
        screenPos.z = targetCamera.nearClipPlane;
        currentWorldPosition = targetCamera.ScreenToWorldPoint(screenPos);
        
        // 마우스 위치에서 충돌 체크
        CheckCollisionAtPosition();
    }
    
    /// <summary>
    /// 현재 마우스 위치에서 상태이상 오브젝트 충돌 체크
    /// </summary>
    private void CheckCollisionAtPosition()
    {
        // 마우스 위치에서 레이어 마스크에 해당하는 콜라이더만 감지
        Collider2D[] hits = Physics2D.OverlapPointAll(currentWorldPosition, statusEffectLayerMask);
        
        if (debugHover)
        {
            Debug.Log($"[VM-World] 충돌 체크: 월드 위치={currentWorldPosition}, 레이어 마스크={statusEffectLayerMask.value}, 감지된 콜라이더 수={hits.Length}");
        }
        
        GameObject newHovered = null;
        float closestDistance = float.MaxValue;
        
        foreach (var hit in hits)
        {
            if (hit == null || hit.gameObject == null)
            {
                if (debugHover)
                    Debug.LogWarning("[VM-World] hit 또는 gameObject가 null");
                continue;
            }
            
            if (debugHover)
            {
                Debug.Log($"[VM-World] 충돌 감지: {hit.gameObject.name}, 레이어={LayerMask.LayerToName(hit.gameObject.layer)}");
            }
            
            // 상태이상 컴포넌트 체크
            if (IsStatusEffectObject(hit.gameObject))
            {
                float distance = Vector3.Distance(currentWorldPosition, hit.transform.position);
                if (debugHover)
                {
                    Debug.Log($"[VM-World] 상태이상 오브젝트 확인: {hit.gameObject.name}, 거리={distance}");
                }
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    newHovered = hit.gameObject;
                }
            }
            else if (debugHover)
            {
                Debug.Log($"[VM-World] 상태이상 오브젝트 아님: {hit.gameObject.name}");
            }
        }
        
        // 호버 상태 변경 처리
        if (newHovered != currentHoveredStatusEffect)
        {
            if (debugHover)
            {
                Debug.Log($"[VM-World] 호버 상태 변경: {currentHoveredStatusEffect?.name ?? "null"} → {newHovered?.name ?? "null"}");
            }
            
            // 이전 오브젝트에서 벗어남
            if (currentHoveredStatusEffect != null && virtualMouse != null)
            {
                if (debugHover)
                {
                    Debug.Log($"[VM-World] 이전 오브젝트 호버 벗어남: {currentHoveredStatusEffect.name}");
                }
                virtualMouse.OnWorldObjectHoverExit(currentHoveredStatusEffect);
            }
            
            currentHoveredStatusEffect = newHovered;
            
            // 새로운 오브젝트에 호버
            if (currentHoveredStatusEffect != null && virtualMouse != null)
            {
                if (debugHover)
                {
                    Debug.Log($"[VM-World] 새 오브젝트 호버 진입: {currentHoveredStatusEffect.name}, virtualMouse={(virtualMouse != null ? "있음" : "null")}");
                }
                virtualMouse.OnWorldObjectHoverEnter(currentHoveredStatusEffect);
                // 호버 유효성 체크 코루틴 시작
                StartHoverValidation();
            }
            else if (currentHoveredStatusEffect == null)
            {
                // 호버가 해제되었으므로 코루틴 중지
                StopHoverValidation();
            }
        }
    }
    
    /// <summary>
    /// 오브젝트가 상태이상 아이콘인지 확인
    /// </summary>
    private bool IsStatusEffectObject(GameObject obj)
    {
        if (obj == null) return false;
        
        // 상태이상 컴포넌트 체크 (직접 컴포넌트만)
        var instance = obj.GetComponent<StatusEffectInstance>();
        var buff = obj.GetComponent<StatusEffectInstanceBuff>();
        var reaction = obj.GetComponent<StatusEffectInstanceReaction>();
        
        return instance != null || buff != null || reaction != null;
    }
    
    /// <summary>
    /// VirtualMouse 참조 설정
    /// </summary>
    public void SetVirtualMouse(VirtualMouse vm)
    {
        virtualMouse = vm;
    }
    
    /// <summary>
    /// 카메라 참조 설정
    /// </summary>
    public void SetTargetCamera(Camera cam)
    {
        targetCamera = cam;
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
    /// 현재 호버된 상태이상 오브젝트가 유효한지 확인
    /// </summary>
    public bool IsHoveredStatusEffectValid()
    {
        if (currentHoveredStatusEffect == null)
        {
            return false;
        }
        return currentHoveredStatusEffect.activeInHierarchy;
    }
    
    /// <summary>
    /// 현재 호버된 상태이상 오브젝트 가져오기
    /// </summary>
    public GameObject GetCurrentHoveredStatusEffect()
    {
        return currentHoveredStatusEffect;
    }
    
    /// <summary>
    /// 호버 유효성 체크 코루틴 시작
    /// </summary>
    private void StartHoverValidation()
    {
        // 이미 실행 중이면 중지 후 재시작
        StopHoverValidation();
        hoverValidationCoroutine = StartCoroutine(HoverValidationCoroutine());
        if (debugHover)
        {
            Debug.Log($"[VM-World] 호버 유효성 체크 코루틴 시작 - currentHoveredStatusEffect: {(currentHoveredStatusEffect != null ? currentHoveredStatusEffect.name : "null")}");
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
            if (debugHover)
            {
                Debug.Log("[VM-World] 호버 유효성 체크 코루틴 중지");
            }
        }
    }
    
    /// <summary>
    /// 호버 유효성 체크 코루틴 - 5초마다 호버된 상태이상 오브젝트가 유효한지 확인
    /// </summary>
    private IEnumerator HoverValidationCoroutine()
    {
        if (debugHover)
        {
            Debug.Log("[VM-World] 호버 유효성 체크 코루틴 시작됨");
        }
        
        while (true)
        {
            yield return new WaitForSeconds(5f);
            
            if (debugHover)
            {
                Debug.Log($"[VM-World] 호버 유효성 체크 실행 - currentHoveredStatusEffect: {(currentHoveredStatusEffect != null ? currentHoveredStatusEffect.name : "null")}, activeInHierarchy: {(currentHoveredStatusEffect != null ? currentHoveredStatusEffect.activeInHierarchy.ToString() : "N/A")}");
            }
            
            // 호버 상태가 없으면 코루틴 종료
            if (currentHoveredStatusEffect == null)
            {
                if (debugHover)
                {
                    Debug.Log("[VM-World] 호버 상태가 없어 유효성 체크 코루틴 종료");
                }
                hoverValidationCoroutine = null;
                yield break;
            }
            
            // 현재 호버된 상태이상 오브젝트가 유효한지 확인
            if (!currentHoveredStatusEffect.activeInHierarchy)
            {
                if (debugHover)
                {
                    Debug.Log($"[VM-World] 호버된 상태이상 오브젝트가 비활성화되어 호버 해제: {currentHoveredStatusEffect.name}");
                }
                if (virtualMouse != null)
                {
                    virtualMouse.OnWorldObjectHoverExit(currentHoveredStatusEffect);
                }
                currentHoveredStatusEffect = null;
                hoverValidationCoroutine = null;
                yield break;
            }
        }
    }
}
