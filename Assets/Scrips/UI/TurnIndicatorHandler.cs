using UnityEngine;

public class TurnIndicatorHandler : MonoBehaviour
{
    public GameObject selectorUI; // 화살표 또는 원형 이미지
    public Transform currentTarget; // 현재 지정된 대상
    public Vector3 offset = new Vector3(0, 1f, 0); // 떠있는 위치
    public RectTransform canvasRectTransform;
    [Header("카메라 설정 (현재 미사용, 향후 월드→스크린 좌표 변환용)")]
    public Camera targetCamera;  // 명시적으로 카메라 지정 (현재는 사용 안 함)
    [SerializeField] private CharacterInfoPlayer playerInfoUI;

    public static TurnIndicatorHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        // 메인 카메라를 기본값으로 명시적 설정 (경고 제거)
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }
    }

    public void SetIndicator(Transform target, bool enable)
    {
        Debug.Log($"[AI개선] SetIndicator 시작 - 타겟: {target?.name}, 활성화: {enable}");
        float startTime = Time.realtimeSinceStartup;
        
        if (target == null)
        {
            // null을 받는 것은 정상적인 상황 (인디케이터 숨기기)
            if (selectorUI != null)
                selectorUI.SetActive(false);
            Debug.Log("[AI개선] SetIndicator - 타겟이 null, 인디케이터 숨김");
            return;
        }

        // GameObject가 파괴되었는지 확인
        if (target.gameObject == null)
        {
            Debug.LogWarning("[TurnIndicatorHandler] 타겟 GameObject가 파괴되었습니다.");
            if (selectorUI != null)
                selectorUI.SetActive(false);
            return;
        }

        //Debug.Log($"[TurnIndicatorHandler] {target.name}의 턴");
        currentTarget = target;
        if (selectorUI != null)
            selectorUI.SetActive(enable);

        if (enable)
        {
            UpdateIndicatorPosition();
        }

        // CharacterStats 컴포넌트 접근 시 추가 null 체크
        if (target.gameObject != null)
        {
            CharacterStats characterStats = target.gameObject.GetComponent<CharacterStats>();
            if (characterStats != null && characterStats.IsPlayer && playerInfoUI != null)
                playerInfoUI.SetCharacterStats(characterStats);
        }
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] SetIndicator 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    private void UpdateIndicatorPosition()
    {
        if (currentTarget == null || currentTarget.gameObject == null || selectorUI == null || !selectorUI.activeSelf) return;

        // 월드 공간에서 직접 위치 설정
        Vector3 worldPos = currentTarget.position + new Vector3(0, 2f, 0);

        RectTransform rectTransform = selectorUI.GetComponent<RectTransform>();
        if (rectTransform != null)
            rectTransform.position = worldPos;
        //Debug.Log($"[TurnIndicatorHandler] UI 위치 업데이트: {worldPos} (대상: {currentTarget.name})");
    }

    void LateUpdate()
    {
        if (selectorUI != null && selectorUI.activeSelf)
            UpdateIndicatorPosition();
    }
}
