using UnityEngine;
using System.Collections;

public class BattleCamera : MonoBehaviour
{
    public static BattleCamera Instance { get; private set; }
    private Coroutine currentZoomCoroutine;

    void Awake()
    {
        Instance = this;
    }

    public IEnumerator CameraZoom(float targetSize, float duration)
    {
        // 이전에 실행 중인 줌 코루틴이 있다면 중지
        if (currentZoomCoroutine != null)
        {
            StopCoroutine(currentZoomCoroutine);
        }

        // 새로운 줌 코루틴 시작
        currentZoomCoroutine = StartCoroutine(CameraZoomRoutine(targetSize, duration));
        yield return currentZoomCoroutine;
    }

    private IEnumerator CameraZoomRoutine(float targetSize, float duration)
    {
        Camera cam = Camera.main;
        float startSize = cam.orthographicSize;
        float time = 0f;
        while (time < duration)
        {
            cam.orthographicSize = Mathf.Lerp(startSize, targetSize, time / duration);
            time += Time.deltaTime;
            yield return null;
        }
        cam.orthographicSize = targetSize;
        currentZoomCoroutine = null;
    }
}
