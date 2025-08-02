using System.Collections;
using UnityEngine;
using TMPro;

/// <summary>
/// 데미지 팝업을 관리하는 클래스
/// 데미지 수치를 표시하고 애니메이션 효과를 제공
/// </summary>
public class DamagePopup : MonoBehaviour
{
    [Header("UI 컴포넌트")]
    [SerializeField] private TextMeshProUGUI damageText; // TMP 텍스트 컴포넌트
    [SerializeField] private TextMesh textMesh; // 일반 TextMesh 컴포넌트 (월드 스페이스용)

    [Header("애니메이션 설정")]
    [SerializeField] private float moveSpeed = 1f; // 위로 이동하는 속도
    [SerializeField] private float fadeStartTime = 0.5f; // 페이드 아웃 시작 시간
    [SerializeField] private float scaleDuration = 0.2f; // 스케일 애니메이션 지속시간
    [SerializeField] private float maxScale = 1.5f; // 최대 스케일

    private Vector3 startPosition;
    private Color originalColor;
    private float duration;
    private bool isInitialized = false;

    private void Awake()
    {
        // 컴포넌트 자동 찾기
        if (damageText == null)
            damageText = GetComponent<TextMeshProUGUI>();
        if (textMesh == null)
            textMesh = GetComponent<TextMesh>();

        // 초기 위치 저장
        startPosition = transform.position;
    }

    /// <summary>
    /// 데미지 팝업을 초기화합니다
    /// </summary>
    /// <param name="damageString">표시할 데미지 문자열</param>
    /// <param name="textColor">텍스트 색상</param>
    /// <param name="popupDuration">팝업 지속시간</param>
    public void Initialize(string damageString, Color textColor, float popupDuration)
    {
        duration = popupDuration;
        originalColor = textColor;

        // 텍스트 설정
        if (damageText != null)
        {
            damageText.text = damageString;
            damageText.color = textColor;
        }
        else if (textMesh != null)
        {
            textMesh.text = damageString;
            textMesh.color = textColor;
        }

        // 초기 스케일 설정
        transform.localScale = Vector3.zero;

        isInitialized = true;

        // 애니메이션 시작
        StartCoroutine(PopupAnimation());
    }

    /// <summary>
    /// 팝업 애니메이션 코루틴
    /// </summary>
    private IEnumerator PopupAnimation()
    {
        if (!isInitialized) yield break;

        float elapsed = 0f;

        // 1. 스케일 인 애니메이션
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            float scale = Mathf.Lerp(0f, maxScale, t);
            transform.localScale = Vector3.one * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 최대 스케일로 설정
        transform.localScale = Vector3.one * maxScale;

        // 2. 정상 스케일로 복귀
        elapsed = 0f;
        while (elapsed < scaleDuration)
        {
            float t = elapsed / scaleDuration;
            float scale = Mathf.Lerp(maxScale, 1f, t);
            transform.localScale = Vector3.one * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;

        // 3. 위로 이동 및 페이드 아웃
        elapsed = 0f;
        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = currentPosition + Vector3.up * 2f; // 위로 2유닛 이동

        while (elapsed < duration)
        {
            float t = elapsed / duration;
            
            // 위치 이동
            transform.position = Vector3.Lerp(currentPosition, targetPosition, t);

            // 페이드 아웃 (fadeStartTime 이후부터)
            if (elapsed > fadeStartTime)
            {
                float fadeT = (elapsed - fadeStartTime) / (duration - fadeStartTime);
                Color currentColor = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0f), fadeT);
                
                if (damageText != null)
                    damageText.color = currentColor;
                else if (textMesh != null)
                    textMesh.color = currentColor;
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 완전히 투명하게 만들고 오브젝트 제거
        if (damageText != null)
            damageText.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
        else if (textMesh != null)
            textMesh.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);

        Destroy(gameObject);
    }

    /// <summary>
    /// 크리티컬 데미지용 특별 애니메이션
    /// </summary>
    public void PlayCriticalAnimation()
    {
        StartCoroutine(CriticalAnimation());
    }

    /// <summary>
    /// 크리티컬 애니메이션 코루틴
    /// </summary>
    private IEnumerator CriticalAnimation()
    {
        // 크리티컬 시 더 큰 스케일과 빠른 애니메이션
        float elapsed = 0f;
        float criticalScaleDuration = scaleDuration * 0.5f;
        float criticalMaxScale = maxScale * 1.5f;

        while (elapsed < criticalScaleDuration)
        {
            float t = elapsed / criticalScaleDuration;
            float scale = Mathf.Lerp(1f, criticalMaxScale, t);
            transform.localScale = Vector3.one * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 크기로 복귀
        elapsed = 0f;
        while (elapsed < criticalScaleDuration)
        {
            float t = elapsed / criticalScaleDuration;
            float scale = Mathf.Lerp(criticalMaxScale, 1f, t);
            transform.localScale = Vector3.one * scale;

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = Vector3.one;
    }

    /// <summary>
    /// 회복량 표시용 특별 애니메이션
    /// </summary>
    public void PlayHealAnimation()
    {
        StartCoroutine(HealAnimation());
    }

    /// <summary>
    /// 회복 애니메이션 코루틴
    /// </summary>
    private IEnumerator HealAnimation()
    {
        // 회복 시 위로 더 많이 이동하고 천천히 페이드 아웃
        float healMoveDistance = 3f; // 일반 데미지보다 더 많이 이동
        float healDuration = duration * 1.5f; // 더 오래 지속

        Vector3 currentPosition = transform.position;
        Vector3 targetPosition = currentPosition + Vector3.up * healMoveDistance;

        float elapsed = 0f;
        while (elapsed < healDuration)
        {
            float t = elapsed / healDuration;
            
            // 부드러운 이동
            transform.position = Vector3.Lerp(currentPosition, targetPosition, t);

            // 천천히 페이드 아웃
            float fadeT = t;
            Color currentColor = Color.Lerp(originalColor, new Color(originalColor.r, originalColor.g, originalColor.b, 0f), fadeT);
            
            if (damageText != null)
                damageText.color = currentColor;
            else if (textMesh != null)
                textMesh.color = currentColor;

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
} 