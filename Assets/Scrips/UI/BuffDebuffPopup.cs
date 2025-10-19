using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuffDebuffPopup : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] public Image iconImage;
    [SerializeField] private TextMeshProUGUI statusEffectNameText;
    
    [Header("아이콘 경로")]
    [SerializeField] private string iconPath = "UI/StatusEffect/"; // 기존 StatusEffect 폴더 사용
    
    [Header("애니메이션 설정")]
    [SerializeField] private float animationDuration = 0.9f;
    [SerializeField] private float fadeInDuration = 0.1f; // 더 빠른 페이드 인
    [SerializeField] private float fadeOutDuration = 0.8f; // 더 부드러운 페이드 아웃
    [SerializeField] private float moveDistance = 50f;
    
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    private Vector2 startPosition;
    
    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }
        
        startPosition = rectTransform.anchoredPosition;
    }
    
    /// <summary>
    /// 팝업을 설정하고 애니메이션을 시작합니다.
    /// </summary>
    /// <param name="effectName">상태이상 이름</param>
    /// <param name="value">변화 수치</param>
    /// <param name="isBuff">버프인지 디버프인지</param>
    public void ShowPopup(string effectName, int value, bool isBuff)
    {
        // 텍스트 설정 (실제 상태이상 이름 사용)
        string effectText = GetEffectText(effectName, value, isBuff);
        statusEffectNameText.text = effectText;
        
        // 색상 설정
        SetColor(isBuff);
        
        // 애니메이션 시작
        StartCoroutine(AnimatePopup());
    }
    
    // 아이콘은 BattleEffectManager에서 설정하므로 여기서는 제거
    
    /// <summary>
    /// 효과 텍스트를 생성합니다.
    /// </summary>
    private string GetEffectText(string effectName, int value, bool isBuff)
    {
        string changeText = isBuff ? "증가" : "감소";
        string sign = isBuff ? "+" : "-";
        
        return $"{effectName} {changeText}";
    }
    
    // 더 이상 필요 없음 - 실제 상태이상 이름을 직접 사용
    
    /// <summary>
    /// 버프/디버프에 따라 색상을 설정합니다.
    /// </summary>
    private void SetColor(bool isBuff)
    {
        Color textColor = isBuff ? new Color(0.3f, 0.3f, 1f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);
        
        if (statusEffectNameText != null)
        {
            statusEffectNameText.color = textColor;
        }
        
        // 아이콘은 원래 색상 유지 (흰색)
        if (iconImage != null)
        {
            iconImage.color = Color.white;
        }
    }
    
    /// <summary>
    /// 팝업 애니메이션을 실행합니다.
    /// </summary>
    private System.Collections.IEnumerator AnimatePopup()
    {
        // 현재 위치를 시작 위치로 설정 (BattleEffectManager에서 설정한 위치 유지)
        startPosition = rectTransform.anchoredPosition;
        
        // 초기 설정
        rectTransform.anchoredPosition = startPosition;
        canvasGroup.alpha = 0f;
        
        // 페이드 인
        float elapsed = 0f;
        while (elapsed < fadeInDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeInDuration;
            canvasGroup.alpha = t;
            yield return null;
        }
        canvasGroup.alpha = 1f;
        
        // 대기
        yield return new WaitForSeconds(animationDuration - fadeInDuration - fadeOutDuration);
        
        // 위로 이동하면서 페이드 아웃
        elapsed = 0f;
        Vector2 endPosition = startPosition + Vector2.up * moveDistance;
        
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeOutDuration;
            
            canvasGroup.alpha = 1f - t;
            rectTransform.anchoredPosition = Vector2.Lerp(startPosition, endPosition, t);
            
            yield return null;
        }
        
        // 팝업 제거
        Destroy(gameObject);
    }
}
