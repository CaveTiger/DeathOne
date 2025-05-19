using UnityEngine;

public class TurnIndicatorHandler : MonoBehaviour
{
    public GameObject selectorUI; // 화살표 또는 원형 이미지
    public Transform currentTarget; // 현재 지정된 대상
    public Vector3 offset = new Vector3(0, 1f, 0); // 떠있는 위치
    public RectTransform canvasRectTransform;
    public Camera targetCamera;  // 명시적으로 카메라 지정

    public static TurnIndicatorHandler Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
            Debug.LogWarning("[TurnIndicatorHandler] 카메라가 지정되지 않았습니다. Main Camera를 사용합니다.");
        }
    }

    public void SetIndicator(Transform target, bool enable)
    {
        if (target == null)
        {
            Debug.LogError("[TurnIndicatorHandler] 타겟이 null입니다.");
            selectorUI.SetActive(false);
            return;
        }

        //Debug.Log($"[TurnIndicatorHandler] {target.name}의 턴");
        currentTarget = target;
        selectorUI.SetActive(enable);

        if (enable)
        {
            UpdateIndicatorPosition();
        }
    }

    private void UpdateIndicatorPosition()
    {
        if (currentTarget == null || !selectorUI.activeSelf) return;

        // 월드 위치를 스크린 위치로 변환
        Vector3 screenPos = targetCamera.WorldToScreenPoint(currentTarget.position);
        screenPos.y += 200f; // 타겟 UI와 동일하게 y 오프셋 적용

        selectorUI.GetComponent<RectTransform>().position = screenPos;
        //Debug.Log($"[TurnIndicatorHandler] UI 위치 업데이트: {screenPos} (대상: {currentTarget.name})");
    }
}
