using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Collections;

public class NextTurnIndicatorUI : MonoBehaviour
{
    [SerializeField] private GameObject turnblockPrefab;
    [SerializeField] private Transform turnblockSpawnPoint;
    [SerializeField] private Image turnBackground;
    
    [Header("턴 전환 효과")]
    [SerializeField] private GameObject turnTransitionEffect; // 턴 전환 알림 팝업
    [SerializeField] private TextMeshProUGUI nextTurnText; // "다음 턴: [캐릭터명]" 텍스트
    [SerializeField] private float highlightDuration = 1.0f; // 강조 지속 시간
    
    [Header("턴 블록 소모/파괴 효과")]
    [SerializeField] private float consumeScale = 0.8f; // 소모시 축소 비율
    [SerializeField] private float consumeDuration = 0.5f; // 소모 애니메이션 시간
    [SerializeField] private float destroyShakeIntensity = 10f; // 파괴시 흔들림 강도
    [SerializeField] private float destroyShakeDuration = 0.3f; // 파괴시 흔들림 시간
    [SerializeField] private Color consumeColor = new Color(0.5f, 0.5f, 0.5f, 0.8f); // 소모시 색상
    [SerializeField] private Color destroyColor = Color.red; // 파괴시 색상

    public static NextTurnIndicatorUI Instance;

    private void Awake()
    {
        Instance = this;
    }

    public void CreateTurnBlocks(List<CharacterStats> turnOrder)
    {
        Debug.Log($"[턴UI] CreateTurnBlocks 시작 - 턴 순서 개수: {turnOrder?.Count ?? 0}");
        float startTime = Time.realtimeSinceStartup;
        
        // 기존 블록 모두 삭제
        int existingBlocks = turnblockSpawnPoint.childCount;
        Debug.Log($"[턴UI] CreateTurnBlocks - 기존 블록 삭제 시작 (개수: {existingBlocks})");
        foreach (Transform child in turnblockSpawnPoint)
            Destroy(child.gameObject);
        Debug.Log("[턴UI] CreateTurnBlocks - 기존 블록 삭제 완료");

        // 새 블록 생성
        Debug.Log("[턴UI] CreateTurnBlocks - 새 블록 생성 시작");
        foreach (var character in turnOrder)
        {
            var block = Instantiate(turnblockPrefab, turnblockSpawnPoint);
            var text = block.GetComponentInChildren<Text>();
            if (text != null)
                text.text = character.Label;
            Debug.Log($"[턴UI] CreateTurnBlocks - 블록 생성: {character.Label}");
        }
        Debug.Log("[턴UI] CreateTurnBlocks - 새 블록 생성 완료");
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴UI] CreateTurnBlocks 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    public void RemoveCurrentTurnBlock()
    {
        Debug.Log("[턴UI] RemoveCurrentTurnBlock 시작");
        float startTime = Time.realtimeSinceStartup;
        
        if (turnblockSpawnPoint.childCount > 0)
        {
            // 현재 턴 블록을 소모 효과와 함께 제거
            var currentBlock = turnblockSpawnPoint.GetChild(0);
            StartCoroutine(ConsumeAndRemoveBlock(currentBlock.gameObject));
            Debug.Log($"[턴UI] RemoveCurrentTurnBlock - 첫 번째 블록 소모 제거 (총 블록 수: {turnblockSpawnPoint.childCount})");
        }
        else
        {
            Debug.Log("[턴UI] RemoveCurrentTurnBlock - 삭제할 블록이 없음");
        }
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[턴UI] RemoveCurrentTurnBlock 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    /// <summary>
    /// 특정 캐릭터의 턴 블록을 파괴 효과와 함께 제거 (사망시 사용)
    /// </summary>
    public void DestroyCharacterTurnBlock(CharacterStats deadCharacter)
    {
        Debug.Log($"[턴UI] DestroyCharacterTurnBlock 시작 - 사망 캐릭터: {deadCharacter?.Label}");
        
        for (int i = 0; i < turnblockSpawnPoint.childCount; i++)
        {
            var block = turnblockSpawnPoint.GetChild(i);
            var text = block.GetComponentInChildren<Text>();
            if (text != null && text.text == deadCharacter.Label)
            {
                StartCoroutine(DestroyAndRemoveBlock(block.gameObject));
                Debug.Log($"[턴UI] DestroyCharacterTurnBlock - {deadCharacter.Label} 블록 파괴");
                break;
            }
        }
    }

    /// <summary>
    /// 턴 블록을 소모 효과와 함께 제거
    /// </summary>
    private IEnumerator ConsumeAndRemoveBlock(GameObject block)
    {
        if (block == null) yield break;

        var image = block.GetComponent<Image>();
        var rectTransform = block.GetComponent<RectTransform>();
        
        if (image != null && rectTransform != null)
        {
            // 1. 소모 효과 시작 (색상 변경 + 축소)
            Color originalColor = image.color;
            Vector3 originalScale = rectTransform.localScale;
            
            float elapsed = 0f;
            while (elapsed < consumeDuration)
            {
                // 매 프레임마다 오브젝트가 파괴되었는지 확인
                if (block == null || image == null || rectTransform == null)
                {
                    Debug.LogWarning("[턴UI] ConsumeAndRemoveBlock - 오브젝트가 파괴되어 중단");
                    yield break;
                }
                
                float progress = elapsed / consumeDuration;
                
                // 색상 변경 (회색으로)
                image.color = Color.Lerp(originalColor, consumeColor, progress);
                
                // 크기 축소
                float scale = Mathf.Lerp(1f, consumeScale, progress);
                rectTransform.localScale = originalScale * scale;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 2. 다음 턴 알림 표시 (오브젝트가 여전히 유효한지 확인)
            if (block != null && turnblockSpawnPoint.childCount > 1)
            {
                var nextBlock = turnblockSpawnPoint.GetChild(1);
                if (nextBlock != null)
                {
                    var nextText = nextBlock.GetComponentInChildren<Text>();
                    if (nextText != null && nextTurnText != null)
                    {
                        nextTurnText.text = $"이번 턴: {nextText.text}";
                        if (turnTransitionEffect != null)
                        {
                            turnTransitionEffect.SetActive(true);
                        }
                    }
                }
            }
            
            // 3. 잠시 대기 후 제거
            yield return new WaitForSeconds(0.2f);
            
            // 4. 블록 제거 및 알림 숨김 (최종 확인)
            if (block != null)
            {
                Destroy(block);
            }
            if (turnTransitionEffect != null)
            {
                turnTransitionEffect.SetActive(false);
            }
        }
        else
        {
            // 컴포넌트가 없으면 바로 제거
            if (block != null)
            {
                Destroy(block);
            }
        }
    }

    /// <summary>
    /// 턴 블록을 파괴 효과와 함께 제거 (사망시)
    /// </summary>
    private IEnumerator DestroyAndRemoveBlock(GameObject block)
    {
        if (block == null) yield break;

        var image = block.GetComponent<Image>();
        var rectTransform = block.GetComponent<RectTransform>();
        
        if (image != null && rectTransform != null)
        {
            // 1. 파괴 효과 시작 (빨간색 + 흔들림)
            Color originalColor = image.color;
            Vector3 originalPosition = rectTransform.anchoredPosition;
            
            // 빨간색으로 변경
            image.color = destroyColor;
            
            // 흔들림 효과
            float elapsed = 0f;
            while (elapsed < destroyShakeDuration)
            {
                // 매 프레임마다 오브젝트가 파괴되었는지 확인
                if (block == null || image == null || rectTransform == null)
                {
                    Debug.LogWarning("[턴UI] DestroyAndRemoveBlock - 오브젝트가 파괴되어 중단");
                    yield break;
                }
                
                float shakeX = Random.Range(-destroyShakeIntensity, destroyShakeIntensity);
                float shakeY = Random.Range(-destroyShakeIntensity, destroyShakeIntensity);
                rectTransform.anchoredPosition = originalPosition + new Vector3(shakeX, shakeY, 0);
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 2. 크래킹 효과 (회전 + 확대)
            elapsed = 0f;
            float crackDuration = 0.2f;
            while (elapsed < crackDuration)
            {
                // 매 프레임마다 오브젝트가 파괴되었는지 확인
                if (block == null || image == null || rectTransform == null)
                {
                    Debug.LogWarning("[턴UI] DestroyAndRemoveBlock - 오브젝트가 파괴되어 중단");
                    yield break;
                }
                
                float progress = elapsed / crackDuration;
                
                // 회전
                rectTransform.rotation = Quaternion.Euler(0, 0, progress * 15f);
                
                // 확대
                float scale = 1f + (progress * 0.3f);
                rectTransform.localScale = Vector3.one * scale;
                
                elapsed += Time.deltaTime;
                yield return null;
            }
            
            // 3. 블록 제거 (최종 확인)
            if (block != null)
            {
                Destroy(block);
            }
        }
        else
        {
            // 컴포넌트가 없으면 바로 제거
            if (block != null)
            {
                Destroy(block);
            }
        }
    }

    /// <summary>
    /// 턴 전환 알림을 표시
    /// </summary>
    public void ShowTurnTransition(CharacterStats nextCharacter)
    {
        if (nextTurnText != null && turnTransitionEffect != null)
        {
            nextTurnText.text = $"이번 턴: {nextCharacter.Label}";
            turnTransitionEffect.SetActive(true);
            StartCoroutine(HideTurnTransitionAfterDelay());
        }
    }

    /// <summary>
    /// 일정 시간 후 턴 전환 알림을 숨김
    /// </summary>
    private IEnumerator HideTurnTransitionAfterDelay()
    {
        yield return new WaitForSeconds(2.0f);
        if (turnTransitionEffect != null)
        {
            turnTransitionEffect.SetActive(false);
        }
    }
}