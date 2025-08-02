using UnityEngine;
using TMPro;

/// <summary>
/// 월드맵에서 영혼먼지와 강자의 정수를 표시하는 UI 스크립트
/// </summary>
public class WorldMapCurrencyUI : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private TextMeshProUGUI soulDustText;
    [SerializeField] private TextMeshProUGUI essenceText;
    
    [Header("업데이트 설정")]
    [SerializeField] private float updateInterval = 0.5f; // UI 업데이트 간격
    
    private float lastUpdateTime;
    
    private void Start()
    {
        // UI 요소가 할당되지 않았다면 자동으로 찾기
        if (soulDustText == null)
            soulDustText = transform.Find("SoulDustText")?.GetComponent<TextMeshProUGUI>();
            
        if (essenceText == null)
            essenceText = transform.Find("EssenceText")?.GetComponent<TextMeshProUGUI>();
            
        // 초기 업데이트
        UpdateCurrencyDisplay();
    }
    
    private void Update()
    {
        // 일정 간격으로 UI 업데이트
        if (Time.time - lastUpdateTime >= updateInterval)
        {
            UpdateCurrencyDisplay();
            lastUpdateTime = Time.time;
        }
    }
    
    /// <summary>
    /// 영혼먼지와 강자의 정수 표시를 업데이트합니다.
    /// </summary>
    public void UpdateCurrencyDisplay()
    {
        if (GameProgressManager.Instance == null) 
        {
            Debug.LogWarning("[영혼먼지][UI] GameProgressManager.Instance가 null입니다.");
            return;
        }
        
        // 영혼먼지 표시
        if (soulDustText != null)
        {
            int soulDust = GameProgressManager.Instance.GetSoulDust();
            soulDustText.text = soulDust.ToString();
            Debug.Log($"[영혼먼지][UI] UI 업데이트: {soulDust}");
        }
        else
        {
            Debug.LogWarning("[영혼먼지][UI] soulDustText가 null입니다.");
        }
        
        // 강자의 정수 표시
        if (essenceText != null)
        {
            int essence = GameProgressManager.Instance.GetEssence();
            essenceText.text = essence.ToString();
        }
        else
        {
            Debug.LogWarning("[영혼먼지][UI] essenceText가 null입니다.");
        }
    }
    
    /// <summary>
    /// 외부에서 강제로 UI를 업데이트합니다.
    /// </summary>
    public void ForceUpdate()
    {
        UpdateCurrencyDisplay();
    }

    /// <summary>
    /// 영혼먼지 상태를 콘솔에 출력하는 테스트 메서드
    /// </summary>
    public void TestSoulDustStatus()
    {
        Debug.Log("=== [영혼먼지][테스트] 상태 확인 시작 ===");
        
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("[영혼먼지][테스트] GameProgressManager.Instance가 null입니다.");
            return;
        }
        
        // GameProgressManager의 디버그 메서드 호출
        GameProgressManager.Instance.DebugSoulDustStatus();
        
        // UI 상태 확인
        Debug.Log($"[영혼먼지][테스트] soulDustText 존재: {soulDustText != null}");
        if (soulDustText != null)
        {
            Debug.Log($"[영혼먼지][테스트] soulDustText 내용: {soulDustText.text}");
        }
        
        Debug.Log("=== [영혼먼지][테스트] 상태 확인 완료 ===");
    }

    /// <summary>
    /// 영혼먼지 테스트 추가 (임시)
    /// </summary>
    public void AddTestSoulDust()
    {
        if (GameProgressManager.Instance != null)
        {
            Debug.Log("[영혼먼지][테스트] 테스트용 영혼먼지 10개 추가");
            GameProgressManager.Instance.AddSoulDust(10);
            UpdateCurrencyDisplay();
        }
    }
    
    /// <summary>
    /// 재화가 변경될 때 호출되는 메서드 (RewardManager에서 호출 가능)
    /// </summary>
    public void OnCurrencyChanged()
    {
        UpdateCurrencyDisplay();
    }
} 