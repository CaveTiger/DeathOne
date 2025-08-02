using UnityEngine;
using System.Collections;

/// <summary>
/// 턴 전환 시의 대기 시간만 스킵할 수 있는 매니저
/// </summary>
public class TurnTransitionSkipManager : MonoBehaviour
{
    public static TurnTransitionSkipManager Instance { get; private set; }
    
    [Header("턴 전환 스킵 설정")]
    [SerializeField] private bool enableTurnSkip = true; // 턴 전환 스킵 기능 활성화 여부
    [SerializeField] private KeyCode skipKey = KeyCode.Space; // 스킵 키 (기본값: 스페이스바)
    [SerializeField] private bool allowMouseClick = true; // 마우스 클릭으로도 스킵 허용
    
    [Header("스킵 가능한 대기 시간")]
    [SerializeField] private bool skipDeathEffects = true; // 죽는 연출 대기 스킵
    [SerializeField] private bool skipEnemyTurnStart = true; // 적 턴 시작 전 대기 스킵
    [SerializeField] private bool skipEnemyTurnEnd = true; // 적 턴 종료 후 대기 스킵
    
    private bool isWaitingForTurnSkip = false;
    private Coroutine currentTurnWaitCoroutine = null;
    private bool skipRequested = false;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void Update()
    {
        if (!enableTurnSkip || !isWaitingForTurnSkip) return;
        
        // 스킵 키 입력 감지
        if (Input.GetKeyDown(skipKey))
        {
            RequestTurnSkip();
        }
        
        // 마우스 클릭 감지
        if (allowMouseClick && Input.GetMouseButtonDown(0))
        {
            RequestTurnSkip();
        }
    }
    
    /// <summary>
    /// 턴 전환 스킵 요청
    /// </summary>
    private void RequestTurnSkip()
    {
        if (!isWaitingForTurnSkip) return;
        
        skipRequested = true;
        Debug.Log("[TurnTransitionSkip] 턴 전환 스킵 요청됨");
    }
    
    /// <summary>
    /// 턴 전환 대기 시간 (스킵 가능)
    /// </summary>
    /// <param name="duration">대기 시간 (초)</param>
    /// <param name="skipType">스킵 타입</param>
    /// <returns></returns>
    public IEnumerator WaitForTurnTransition(float duration, string skipType = "general")
    {
        Debug.Log($"[TurnTransitionSkip] 턴 전환 대기 시작 - duration: {duration}, type: {skipType}");
        
        // 스킵이 비활성화되었거나 대기 시간이 0 이하면 일반 대기
        if (!enableTurnSkip || duration <= 0f)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }
        
        // 특정 타입의 스킵이 비활성화되었으면 일반 대기
        bool canSkip = true;
        switch (skipType)
        {
            case "death":
                canSkip = skipDeathEffects;
                break;
            case "enemyStart":
                canSkip = skipEnemyTurnStart;
                break;
            case "enemyEnd":
                canSkip = skipEnemyTurnEnd;
                break;
        }
        
        if (!canSkip)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }
        
        isWaitingForTurnSkip = true;
        skipRequested = false;
        
        currentTurnWaitCoroutine = StartCoroutine(TurnWaitCoroutine(duration));
        yield return currentTurnWaitCoroutine;
        
        // 대기 완료 후 상태 초기화
        isWaitingForTurnSkip = false;
        skipRequested = false;
        currentTurnWaitCoroutine = null;
        
        Debug.Log("[TurnTransitionSkip] 턴 전환 대기 완료");
    }
    
    private IEnumerator TurnWaitCoroutine(float duration)
    {
        float elapsed = 0f;
        
        while (elapsed < duration && !skipRequested)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
        
        if (skipRequested)
        {
            Debug.Log("[TurnTransitionSkip] 턴 전환 대기 스킵됨");
        }
    }
    
    /// <summary>
    /// 턴 전환 스킵 기능 활성화/비활성화
    /// </summary>
    /// <param name="enabled">활성화 여부</param>
    public void SetTurnSkipEnabled(bool enabled)
    {
        enableTurnSkip = enabled;
        if (!enabled)
        {
            skipRequested = true;
        }
    }
    
    /// <summary>
    /// 현재 턴 전환 대기 중인지 확인
    /// </summary>
    /// <returns>턴 전환 대기 중이면 true</returns>
    public bool IsWaitingForTurnSkip()
    {
        return isWaitingForTurnSkip;
    }
}