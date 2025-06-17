using UnityEngine;
using System.Collections;

public class StageCameraUI : MonoBehaviour
{
    [Header("카메라 설정")]
    [SerializeField] private Camera stageCamera;
    [SerializeField] private float moveDuration = 1.5f;
    [SerializeField] private float zoomInSize = 1f;
    [SerializeField] private float defaultSize = 5f;

    private Vector3 originalPosition;
    private bool isZoomed = false;
    public bool IsZoomed => isZoomed;
    private Coroutine currentMoveCoroutine;
    private Coroutine currentZoomCoroutine;

    private void Start()
    {
        if (stageCamera == null)
            stageCamera = Camera.main;

        originalPosition = stageCamera.transform.position;
    }

    /// <summary>
    /// 스테이지 아이콘을 클릭했을 때 호출되는 메서드
    /// </summary>
    /// <param name="targetPosition">줌인할 위치</param>
    public void OnStageIconClick(Vector3 targetPosition)
    {
        if (isZoomed) return;

        // 오른쪽으로 400만큼 이동
        Vector3 camTarget = new Vector3(targetPosition.x + 0.75f, targetPosition.y, stageCamera.transform.position.z);

        if (currentMoveCoroutine != null)
            StopCoroutine(currentMoveCoroutine);
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        currentMoveCoroutine = StartCoroutine(CameraMoveRoutine(camTarget, moveDuration));
        currentZoomCoroutine = StartCoroutine(CameraZoomRoutine(zoomInSize, moveDuration));

        isZoomed = true;
    }

    /// <summary>
    /// 줌을 해제하고 원래 위치로 돌아가는 메서드
    /// </summary>
    public void ResetCamera()
    {
        if (!isZoomed) return;

        // 위치와 줌 모두 원래대로
        if (currentMoveCoroutine != null)
            StopCoroutine(currentMoveCoroutine);
        if (currentZoomCoroutine != null)
            StopCoroutine(currentZoomCoroutine);

        // 오른쪽으로 이동했던 만큼 왼쪽으로 이동
        Vector3 camTarget = new Vector3(originalPosition.x, originalPosition.y, stageCamera.transform.position.z);

        currentMoveCoroutine = StartCoroutine(CameraMoveRoutine(camTarget, moveDuration));
        currentZoomCoroutine = StartCoroutine(CameraZoomRoutine(defaultSize, moveDuration));

        isZoomed = false;
    }

    private IEnumerator CameraMoveRoutine(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = stageCamera.transform.position;
        float time = 0f;

        while (time < duration)
        {
            // 부드러운 이동을 위한 보간
            float t = time / duration;
            t = t * t * (3f - 2f * t); // SmoothStep 보간
            stageCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, t);
            
            time += Time.deltaTime;
            yield return null;
        }

        stageCamera.transform.position = targetPosition;
        currentMoveCoroutine = null;
    }

    private IEnumerator CameraZoomRoutine(float targetSize, float duration)
    {
        float startSize = stageCamera.orthographicSize;
        float time = 0f;

        while (time < duration)
        {
            float t = time / duration;
            t = t * t * (3f - 2f * t); // SmoothStep 보간
            stageCamera.orthographicSize = Mathf.Lerp(startSize, targetSize, t);
            time += Time.deltaTime;
            yield return null;
        }

        stageCamera.orthographicSize = targetSize;

        // 줌 상태 동기화
        isZoomed = Mathf.Approximately(targetSize, zoomInSize);

        currentZoomCoroutine = null;
    }
}
