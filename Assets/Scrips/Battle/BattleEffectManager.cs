using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전투 연출을 관리하는 매니저 클래스
/// 피격 이펙트, 데미지 팝업, 캐릭터 밀림, 데스 애니메이션 등을 담당
/// </summary>
public class BattleEffectManager : MonoBehaviour
{
    public static BattleEffectManager Instance { get; private set; }

    [Header("이펙트 프리팹")]
    [SerializeField] private GameObject hitUIEffectPrefab; // 히트 UI 이펙트 프리팹
    [SerializeField] private GameObject damageCountPrefab; // 데미지 카운트 프리팹
    [SerializeField] private GameObject deathIconPrefab; // 데스 아이콘 프리팹
    [SerializeField] private GameObject buffDebuffPopupPrefab; // 버프/디버프 팝업 프리팹

    [Header("연출 설정")]
    // [SerializeField] private float hitShakeDuration = 0.2f; // 사용하지 않는 필드 제거
    // [SerializeField] private float hitShakeIntensity = 0.1f; // 사용하지 않는 필드 제거
    [SerializeField] private float knockbackDistance = 0.3f; // 밀림 거리
    [SerializeField] private float knockbackDuration = 0.3f; // 밀림 지속시간
    [SerializeField] private float damagePopupDuration = 1.5f; // 데미지 팝업 지속시간
    [SerializeField] private float deathEffectDuration = 2.8f; // 데스 이펙트 지속시간(데스 호흡 길게)

    [Header("색상 설정")]
    [SerializeField] private Color normalDamageColor = Color.red; // 일반 데미지 색상 (빨간색)
    [SerializeField] private Color criticalDamageColor = new Color(1f, 0.5f, 0f, 1f); // 크리티컬 데미지 색상 (주황색)
    [SerializeField] private Color healColor = Color.green; // 회복 색상

    [Header("UI Canvas 설정")]
    [SerializeField] private Transform worldUI; // WorldUI Transform (UI 하위의 WorldUI 또는 StEfUI)

    /// <summary>
    /// WorldUI Canvas를 반환합니다 (할당된 worldUI 사용)
    /// </summary>
    private Canvas GetWorldUICanvas()
    {
        if (worldUI == null)
        {
            Debug.LogWarning("[BattleEffectManager] worldUI가 할당되지 않았습니다. Inspector에서 할당해주세요.");
            return null;
        }

        Canvas canvas = worldUI.GetComponentInParent<Canvas>();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleEffectManager] worldUI의 부모에 Canvas를 찾을 수 없습니다.");
            return null;
        }

        return canvas;
    }

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

    private void Start()
    {
        // 모든 CharacterStats에 이벤트 구독
        SubscribeToCharacterEvents();
    }

    private void OnDestroy()
    {
        // 이벤트 구독 해제
        UnsubscribeFromCharacterEvents();
    }

    /// <summary>
    /// 모든 CharacterStats에 이벤트를 구독합니다
    /// </summary>
    private void SubscribeToCharacterEvents()
    {
        CharacterStats[] characters = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
        foreach (var character in characters)
        {
            character.OnTakeDamageEvent += OnCharacterTakeDamage;
            character.OnHealEvent += OnCharacterHeal;
            character.OnDeathEvent += OnCharacterDeath;
            character.OnBuffEvent += OnCharacterBuff; // 버프 이벤트 구독 추가
        }
    }

    /// <summary>
    /// 모든 CharacterStats에서 이벤트 구독을 해제합니다
    /// </summary>
    private void UnsubscribeFromCharacterEvents()
    {
        CharacterStats[] characters = FindObjectsByType<CharacterStats>(FindObjectsSortMode.None);
        foreach (var character in characters)
        {
            if (character != null)
            {
                character.OnTakeDamageEvent -= OnCharacterTakeDamage;
                character.OnHealEvent -= OnCharacterHeal;
                character.OnDeathEvent -= OnCharacterDeath;
                character.OnBuffEvent -= OnCharacterBuff; // 버프 이벤트 구독 해제 추가
            }
        }
    }

    /// <summary>
    /// 캐릭터가 데미지를 받았을 때 호출되는 이벤트 핸들러
    /// </summary>
    public void OnCharacterTakeDamage(CharacterStats target, int damage, bool isCritical, Vector3 attackerPosition)
    {
        Debug.Log($"OnCharacterTakeDamage 호출됨: {target.name}, 데미지: {damage}, 크리티컬: {isCritical}");
        PlayHitEffect(target, damage, isCritical, attackerPosition);
    }

    /// <summary>
    /// 캐릭터가 힐을 받았을 때 호출되는 이벤트 핸들러
    /// </summary>
    public void OnCharacterHeal(CharacterStats target, int healAmount, Vector3 healerPosition)
    {
        Debug.Log($"OnCharacterHeal 호출됨: {target.name}, 힐량: {healAmount}");
        PlayHealEffect(target, healAmount);
    }

    /// <summary>
    /// 캐릭터가 버프를 받았을 때 호출되는 이벤트 핸들러
    /// </summary>
    public void OnCharacterBuff(CharacterStats target, int buffValue, Vector3 casterPosition)
    {
        Debug.Log($"OnCharacterBuff 호출됨: {target.name}, 버프 수치: {buffValue}");
        PlayBuffEffect(target, buffValue);
    }

    /// <summary>
    /// 캐릭터가 사망했을 때 호출되는 이벤트 핸들러
    /// </summary>
    public void OnCharacterDeath(CharacterStats target)
    {
        PlayDeathEffect(target);
    }

    /// <summary>
    /// 피격 연출을 실행합니다 (이펙트 + 흔들림 + 밀림)
    /// </summary>
    /// <param name="target">피격 대상 캐릭터</param>
    /// <param name="damage">피해량</param>
    /// <param name="isCritical">크리티컬 여부</param>
    /// <param name="attackerPosition">공격자 위치 (밀림 방향 결정용)</param>
    public void PlayHitEffect(CharacterStats target, int damage, bool isCritical, Vector3 attackerPosition)
    {
        if (target == null) return;

        Debug.Log($"PlayHitEffect 호출됨: {target.name}, 데미지: {damage}");
        StartCoroutine(HitEffectCoroutine(target, damage, isCritical, attackerPosition));
    }

    /// <summary>
    /// 피해무시(블록) 연출을 재생합니다
    /// </summary>
    /// <param name="target">블록한 대상</param>
    /// <param name="attackerPosition">공격자 위치</param>
    public void PlayBlockEffect(CharacterStats target, Vector3 attackerPosition)
    {
        if (target == null) return;

        Debug.Log($"PlayBlockEffect 호출됨: {target.name}");
        StartCoroutine(BlockEffectCoroutine(target, attackerPosition));
    }

    /// <summary>
    /// 피격 연출 코루틴
    /// </summary>
    private IEnumerator HitEffectCoroutine(CharacterStats target, int damage, bool isCritical, Vector3 attackerPosition)
    {
        // Canvas 찾기
        Canvas canvas = GetWorldUICanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleEffectManager] WorldUI Canvas를 찾을 수 없습니다.");
            yield break;
        }

        // 1. 히트 UI 이펙트 생성 (좌상단)
        GameObject hitEffect = null;
        if (hitUIEffectPrefab != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            
            hitEffect = Instantiate(hitUIEffectPrefab, canvas.transform);
            RectTransform rectTransform = hitEffect.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 히트 표시를 좌상단에 배치
                rectTransform.anchoredPosition = new Vector2(localPoint.x - 30, localPoint.y + 40);
            }
        }

        // 2. 크리티컬인 경우 추가 이펙트 (히트 UI를 크리티컬 색상으로 변경)
        GameObject criticalEffect = null;
        if (isCritical && hitUIEffectPrefab != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            
            criticalEffect = Instantiate(hitUIEffectPrefab, canvas.transform);
            RectTransform rectTransform = criticalEffect.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // 크리티컬 텍스트를 히트 표시 위쪽에 배치
                rectTransform.anchoredPosition = new Vector2(localPoint.x - 30, localPoint.y + 80);
            }
            
            TMPro.TextMeshProUGUI criticalText = criticalEffect.GetComponent<TMPro.TextMeshProUGUI>();
            if (criticalText != null)
            {
                criticalText.text = "CRITICAL!";
                criticalText.color = criticalDamageColor;
                criticalText.fontSize += 5; // 크리티컬 텍스트를 약간 크게
            }
        }

        // 3. 데미지 팝업 생성 및 함께 애니메이션
        GameObject damagePopup = CreatePopup(target.transform.position, damage, isCritical, false);
        
        // 히트 표시, 크리티컬 이펙트, 데미지 팝업을 함께 애니메이션
        if (hitEffect != null || damagePopup != null || criticalEffect != null)
        {
            StartCoroutine(AnimateHitAndDamage(hitEffect, damagePopup, criticalEffect, target.transform.position));
        }

        // 4. 캐릭터 흔들림 효과 (임시 비활성화)
        // yield return StartCoroutine(ShakeCharacter(target, hitShakeDuration, hitShakeIntensity));

        // 5. 캐릭터 밀림 효과
        yield return StartCoroutine(KnockbackCharacter(target, attackerPosition, knockbackDistance, knockbackDuration));
        
        // 코루틴이 완료되었음을 알림
        yield return null;
    }

    /// <summary>
    /// 피해무시(블록) 연출 코루틴
    /// </summary>
    private IEnumerator BlockEffectCoroutine(CharacterStats target, Vector3 attackerPosition)
    {
        // Canvas 찾기
        Canvas canvas = GetWorldUICanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleEffectManager] WorldUI Canvas를 찾을 수 없습니다.");
            yield break;
        }

        // BLOCK UI 이펙트 생성
        GameObject blockEffect = null;
        if (hitUIEffectPrefab != null)
        {
            Vector3 screenPosition = Camera.main.WorldToScreenPoint(target.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.GetComponent<RectTransform>(),
                screenPosition,
                canvas.worldCamera,
                out Vector2 localPoint
            );
            
            blockEffect = Instantiate(hitUIEffectPrefab, canvas.transform);
            RectTransform rectTransform = blockEffect.GetComponent<RectTransform>();
            if (rectTransform != null)
            {
                // BLOCK 표시를 좌상단에 배치
                rectTransform.anchoredPosition = new Vector2(localPoint.x - 30, localPoint.y + 40);
            }
            
            // 텍스트를 BLOCK으로 변경하고 색상을 파란색으로 설정
            TMPro.TextMeshProUGUI blockText = blockEffect.GetComponent<TMPro.TextMeshProUGUI>();
            if (blockText != null)
            {
                blockText.text = "BLOCK";
                blockText.color = Color.blue;
                blockText.fontSize += 3; // BLOCK 텍스트를 약간 크게
            }
        }

        // BLOCK 이펙트 애니메이션
        if (blockEffect != null)
        {
            StartCoroutine(AnimateBlockEffect(blockEffect, target.transform.position));
        }

        // 캐릭터 밀림 효과는 없음 (블록했으므로)
        yield return null;
    }

    /// <summary>
    /// 데미지 팝업을 생성합니다
    /// </summary>

    /// <param name="position">팝업 위치</param>
    /// <param name="damage">데미지 수치</param>
    /// <param name="isCritical">크리티컬 여부</param>
    private GameObject CreatePopup(Vector3 position, int amount, bool isCritical, bool isHeal = false, bool isBuff = false)
    {
        if (damageCountPrefab == null) return null;

        // Canvas 찾기
        Canvas canvas = GetWorldUICanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleEffectManager] WorldUI Canvas를 찾을 수 없습니다.");
            return null;
        }

        // 월드 좌표를 스크린 좌표로 변환
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[BattleEffectManager] Main Camera를 찾을 수 없습니다.");
            return null;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(position);
        
        // 스크린 좌표를 Canvas 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        
        // 데미지 팝업을 우하단에 배치
        Vector2 damageOffset = new Vector2(30, -40); // 우하단으로 오프셋
        localPoint += damageOffset;
        
        GameObject popup = Instantiate(damageCountPrefab, canvas.transform);
        RectTransform rectTransform = popup.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = localPoint;
        }
        
        TMPro.TextMeshProUGUI popupText = popup.GetComponent<TMPro.TextMeshProUGUI>();
        if (popupText != null)
        {
            if (isHeal)
            {
                popupText.text = "+" + amount.ToString();
                popupText.color = healColor;
            }
            else if (isBuff)
            {
                popupText.text = "+" + amount.ToString();
                popupText.color = new Color(0.3f, 0.3f, 1f, 1f); // 연한 파란색
            }
            else
            {
                popupText.text = amount.ToString();
                popupText.color = isCritical ? criticalDamageColor : normalDamageColor;
                
                // 크리티컬인 경우 폰트 크기를 약간 크게
                if (isCritical)
                {
                    popupText.fontSize += 10;
                }
            }
        }
        
        // 데미지 팝업 애니메이션은 별도로 실행하지 않음 (함께 애니메이션할 예정)
        // Destroy(popup, damagePopupDuration); // 함께 애니메이션에서 처리
        
        return popup;
    }

    /// <summary>
    /// 버프 전용 팝업을 생성합니다
    /// </summary>
    /// <param name="position">생성 위치</param>
    /// <param name="buffValue">버프 수치</param>
    private GameObject CreateBuffPopup(Vector3 position, int buffValue)
    {
        if (damageCountPrefab == null) return null;

        // Canvas 찾기
        Canvas canvas = GetWorldUICanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleEffectManager] WorldUI Canvas를 찾을 수 없습니다.");
            return null;
        }

        // 월드 좌표를 스크린 좌표로 변환
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[BattleEffectManager] Main Camera를 찾을 수 없습니다.");
            return null;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(position);
        
        // 스크린 좌표를 Canvas 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        
        // 버프 팝업을 우상단에 배치
        Vector2 buffOffset = new Vector2(30, 40); // 우상단으로 오프셋
        localPoint += buffOffset;
        
        GameObject popup = Instantiate(damageCountPrefab, canvas.transform);
        RectTransform rectTransform = popup.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = localPoint;
        }
        
        TMPro.TextMeshProUGUI popupText = popup.GetComponent<TMPro.TextMeshProUGUI>();
        if (popupText != null)
        {
            popupText.text = $"+{buffValue}";
            popupText.color = new Color(0.3f, 0.3f, 1f, 1f); // 연한 파란색
        }
        
        return popup;
    }

    /// <summary>
    /// 새로운 버프/디버프 팝업을 생성합니다
    /// </summary>
    /// <param name="position">생성 위치</param>
    /// <param name="effectName">상태이상 이름</param>
    /// <param name="value">변화 수치</param>
    /// <param name="isBuff">버프인지 디버프인지</param>
    /// <param name="effectData">상태이상 데이터</param>
    private Dictionary<Vector3, int> popupCountByPosition = new Dictionary<Vector3, int>(); // 위치별 팝업 카운터
    
    public void CreateNewBuffDebuffPopup(Vector3 position, string effectName, int value, bool isBuff, StatusEffectData effectData = null)
    {
        Debug.Log($"[BattleEffectManager] CreateNewBuffDebuffPopup 호출됨: {effectName}, 수치={value}, isBuff={isBuff}");
        
        if (buffDebuffPopupPrefab == null)
        {
            Debug.LogWarning("[BattleEffectManager] buffDebuffPopupPrefab이 설정되지 않았습니다.");
            return;
        }

        // Canvas 찾기
        Canvas canvas = GetWorldUICanvas();
        if (canvas == null)
        {
            Debug.LogWarning("[BattleEffectManager] WorldUI Canvas를 찾을 수 없습니다.");
            return;
        }

        // 월드 좌표를 스크린 좌표로 변환
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning("[BattleEffectManager] Main Camera를 찾을 수 없습니다.");
            return;
        }

        Vector3 screenPosition = mainCamera.WorldToScreenPoint(position);
        
        // 스크린 좌표를 Canvas 좌표로 변환
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.GetComponent<RectTransform>(),
            screenPosition,
            canvas.worldCamera,
            out Vector2 localPoint
        );
        
        // 위치별 팝업 카운터 관리
        Vector3 roundedPosition = new Vector3(Mathf.Round(position.x * 10) / 10, Mathf.Round(position.y * 10) / 10, Mathf.Round(position.z * 10) / 10);
        if (!popupCountByPosition.ContainsKey(roundedPosition))
        {
            popupCountByPosition[roundedPosition] = 0;
        }
        
        // 팝업을 캐릭터 위에 배치 (순차적으로 아래쪽으로)
        Vector2 popupOffset = new Vector2(0, 80 - (popupCountByPosition[roundedPosition] * 100)); // 첫 번째는 80, 두 번째는 -20, 세 번째는 -120...
        localPoint += popupOffset;
        
        Debug.Log($"[BattleEffectManager] 팝업 위치 조정: {effectName}, 위치={roundedPosition}, 카운터={popupCountByPosition[roundedPosition]}, 오프셋={popupOffset}");
        
        // 팝업 카운터 증가
        popupCountByPosition[roundedPosition]++;
        
        GameObject popup = Instantiate(buffDebuffPopupPrefab, canvas.transform);
        RectTransform rectTransform = popup.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = localPoint;
        }
        
        // 팝업 스크립트에 정보 전달
        BuffDebuffPopup popupScript = popup.GetComponent<BuffDebuffPopup>();
        if (popupScript != null)
        {
            popupScript.ShowPopup(effectName, value, isBuff);
            
            // 상태이상 데이터가 있으면 아이콘 설정
            if (effectData != null && popupScript.iconImage != null)
            {
                Sprite iconSprite = effectData.GetDynamicIcon(value);
                popupScript.iconImage.sprite = iconSprite;
                Debug.Log($"[BattleEffectManager] 아이콘 설정: {effectName}, 아이콘={(iconSprite != null ? "성공" : "실패")}");
            }
            else
            {
                Debug.LogWarning($"[BattleEffectManager] 아이콘 설정 실패: effectData={effectData != null}, iconImage={popupScript.iconImage != null}");
            }
        }
        else
        {
            Debug.LogError("[BattleEffectManager] BuffDebuffPopup 스크립트를 찾을 수 없습니다.");
        }
    }

    /// <summary>
    /// 히트 표시와 데미지 팝업을 함께 애니메이션합니다
    /// </summary>
    private IEnumerator AnimateHitAndDamage(GameObject hitEffect, GameObject damagePopup, GameObject criticalEffect, Vector3 targetPosition)
    {
        if (hitEffect == null && damagePopup == null && criticalEffect == null) yield break;

        float duration = damagePopupDuration;
        float animationDuration = 0.5f; // 애니메이션 지속시간
        float elapsed = 0f;

        // 초기 위치 저장
        RectTransform hitRectTransform = hitEffect?.GetComponent<RectTransform>();
        RectTransform damageRectTransform = damagePopup?.GetComponent<RectTransform>();
        RectTransform criticalRectTransform = criticalEffect?.GetComponent<RectTransform>();

        Vector2 hitStartPos = hitRectTransform?.anchoredPosition ?? Vector2.zero;
        Vector2 damageStartPos = damageRectTransform?.anchoredPosition ?? Vector2.zero;
        Vector2 criticalStartPos = criticalRectTransform?.anchoredPosition ?? Vector2.zero;

        // 애니메이션 시작
        while (elapsed < duration)
        {
            if (hitEffect == null && damagePopup == null && criticalEffect == null) yield break;

            float t = elapsed / duration;
            float animationT = Mathf.Clamp01(elapsed / animationDuration); // 애니메이션용 시간 (0~1)
            
            // 물건을 던져서 올라갔다가 내려오는 듯한 포물선 운동
            // 포물선 공식: y = -4h * (t - 0.5)^2 + h (h는 최대 높이)
            float maxHeight = 40f; // 최대 높이
            float parabolaT = animationT; // 0~1 범위 (4배 빠른 애니메이션)
            float yOffset = -4f * maxHeight * (parabolaT - 0.5f) * (parabolaT - 0.5f) + maxHeight;
            
            // 음수 값 방지
            yOffset = Mathf.Max(0f, yOffset);
            
            // 히트 표시 애니메이션
            if (hitEffect != null && hitRectTransform != null)
            {
                hitRectTransform.anchoredPosition = new Vector2(hitStartPos.x, hitStartPos.y + yOffset);
            }

            // 크리티컬 이펙트 애니메이션
            if (criticalEffect != null && criticalRectTransform != null)
            {
                criticalRectTransform.anchoredPosition = new Vector2(criticalStartPos.x, criticalStartPos.y + yOffset);
            }

            // 데미지 팝업 애니메이션
            if (damagePopup != null && damageRectTransform != null)
            {
                damageRectTransform.anchoredPosition = new Vector2(damageStartPos.x, damageStartPos.y + yOffset);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 페이드 아웃 효과
        float fadeDuration = 0.3f;
        elapsed = 0f;

        CanvasGroup hitCanvasGroup = hitEffect?.GetComponent<CanvasGroup>();
        CanvasGroup damageCanvasGroup = damagePopup?.GetComponent<CanvasGroup>();
        CanvasGroup criticalCanvasGroup = criticalEffect?.GetComponent<CanvasGroup>();

        if (hitCanvasGroup == null && hitEffect != null)
        {
            hitCanvasGroup = hitEffect.AddComponent<CanvasGroup>();
        }
        if (damageCanvasGroup == null && damagePopup != null)
        {
            damageCanvasGroup = damagePopup.AddComponent<CanvasGroup>();
        }
        if (criticalCanvasGroup == null && criticalEffect != null)
        {
            criticalCanvasGroup = criticalEffect.AddComponent<CanvasGroup>();
        }

        while (elapsed < fadeDuration)
        {
            float alpha = 1f - (elapsed / fadeDuration);
            
            if (hitCanvasGroup != null) hitCanvasGroup.alpha = alpha;
            if (damageCanvasGroup != null) damageCanvasGroup.alpha = alpha;
            if (criticalCanvasGroup != null) criticalCanvasGroup.alpha = alpha;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 오브젝트 파괴
        if (hitEffect != null) Destroy(hitEffect);
        if (damagePopup != null) Destroy(damagePopup);
        if (criticalEffect != null) Destroy(criticalEffect);
    }

    /// <summary>
    /// BLOCK 이펙트를 애니메이션합니다
    /// </summary>
    private IEnumerator AnimateBlockEffect(GameObject blockEffect, Vector3 targetPosition)
    {
        if (blockEffect == null) yield break;

        float duration = 1.0f; // BLOCK 표시 지속시간
        float animationDuration = 0.5f; // 애니메이션 지속시간
        float elapsed = 0f;

        // 초기 위치 저장
        RectTransform blockRectTransform = blockEffect.GetComponent<RectTransform>();
        Vector2 blockStartPos = blockRectTransform?.anchoredPosition ?? Vector2.zero;

        // 애니메이션 시작
        while (elapsed < duration)
        {
            if (blockEffect == null) yield break;

            float t = elapsed / duration;
            float animationT = Mathf.Clamp01(elapsed / animationDuration);
            
            // BLOCK 텍스트가 위로 올라갔다가 내려오는 애니메이션
            float maxHeight = 50f; // 최대 높이
            float yOffset = -4f * maxHeight * (animationT - 0.5f) * (animationT - 0.5f) + maxHeight;
            yOffset = Mathf.Max(0f, yOffset);
            
            // BLOCK 표시 애니메이션
            if (blockRectTransform != null)
            {
                blockRectTransform.anchoredPosition = new Vector2(blockStartPos.x, blockStartPos.y + yOffset);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 페이드 아웃 효과
        float fadeDuration = 0.3f;
        elapsed = 0f;

        CanvasGroup blockCanvasGroup = blockEffect.GetComponent<CanvasGroup>();
        if (blockCanvasGroup == null)
        {
            blockCanvasGroup = blockEffect.AddComponent<CanvasGroup>();
        }

        while (elapsed < fadeDuration)
        {
            float alpha = 1f - (elapsed / fadeDuration);
            if (blockCanvasGroup != null) blockCanvasGroup.alpha = alpha;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 오브젝트 파괴
        if (blockEffect != null) Destroy(blockEffect);
    }

    /// <summary>
    /// 캐릭터 흔들림 효과를 실행합니다
    /// </summary>
    /// <param name="target">대상 캐릭터</param>
    /// <param name="duration">지속시간</param>
    /// <param name="intensity">흔들림 강도</param>
    private IEnumerator ShakeCharacter(CharacterStats target, float duration, float intensity)
    {
        Vector3 originalPosition = target.transform.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            // 캐릭터가 파괴되었는지 확인
            if (target == null || target.gameObject == null)
            {
                Debug.LogWarning($"[BattleEffectManager] ShakeCharacter: 캐릭터가 파괴되어 흔들림 효과 중단");
                yield break; // 코루틴 종료
            }

            float x = originalPosition.x + Random.Range(-intensity, intensity);
            float y = originalPosition.y + Random.Range(-intensity, intensity);
            target.transform.localPosition = new Vector3(x, y, originalPosition.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 원래 위치로 복귀 (파괴 여부 확인 후)
        if (target != null && target.gameObject != null)
        {
            target.transform.localPosition = originalPosition;
        }
        else
        {
            Debug.LogWarning($"[BattleEffectManager] ShakeCharacter: 원래 위치 복귀 실패 - 캐릭터가 파괴됨");
        }
    }

    /// <summary>
    /// 캐릭터 밀림 효과를 실행합니다 (스프라이트만 밀림)
    /// </summary>
    /// <param name="target">대상 캐릭터</param>
    /// <param name="attackerPosition">공격자 위치</param>
    /// <param name="distance">밀림 거리</param>
    /// <param name="duration">지속시간</param>
    private IEnumerator KnockbackCharacter(CharacterStats target, Vector3 attackerPosition, float distance, float duration)
    {
        // 캐릭터의 스프라이트를 정확히 찾기 (체력바가 아닌 캐릭터 스프라이트)
        SpriteRenderer spriteRenderer = null;
        
        // 먼저 "Sprite"라는 이름의 자식 오브젝트에서 찾기
        Transform spriteTransform = target.transform.Find("Sprite");
        if (spriteTransform != null)
        {
            spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
        }
        
        // "Sprite" 오브젝트를 찾지 못한 경우 직접 찾기
        if (spriteRenderer == null)
        {
            spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        }
        
        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[BattleEffectManager] {target.name}에서 SpriteRenderer를 찾을 수 없습니다.");
            yield break;
        }

        Vector3 originalSpritePosition = spriteRenderer.transform.localPosition;
        
        // attackerPosition을 로컬 좌표계로 변환
        Vector3 attackerLocalPosition = target.transform.parent.InverseTransformPoint(attackerPosition);
        
        // UI 캔버스 기준으로 밀림 방향 계산 (로컬 좌표계 사용)
        Vector3 knockbackDirection = (target.transform.localPosition - attackerLocalPosition).normalized;
        Vector3 targetSpritePosition = originalSpritePosition + knockbackDirection * distance;

        Debug.Log($"[BattleEffectManager] 스프라이트 밀림 시작: {target.name}, 원래스프라이트위치: {originalSpritePosition}, 목표스프라이트위치: {targetSpritePosition}, 방향: {knockbackDirection}, 공격자월드위치: {attackerPosition}, 공격자로컬위치: {attackerLocalPosition}");

        float elapsed = 0f;
        while (elapsed < duration)
        {
            // 캐릭터나 SpriteRenderer가 파괴되었는지 확인
            if (target == null || target.gameObject == null || spriteRenderer == null || spriteRenderer.gameObject == null)
            {
                Debug.LogWarning($"[BattleEffectManager] KnockbackCharacter: 캐릭터가 파괴되어 밀림 효과 중단");
                yield break; // 코루틴 종료
            }

            float t = elapsed / duration;
            // 이징 함수로 자연스러운 밀림 효과
            float easeOut = 1f - Mathf.Pow(1f - t, 3f);
            spriteRenderer.transform.localPosition = Vector3.Lerp(originalSpritePosition, targetSpritePosition, easeOut);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 밀린 위치에서 대기 (전투 루틴이 끝날 때까지)
        // 최종 위치 설정 전에 한 번 더 파괴 여부 확인
        if (target != null && target.gameObject != null && spriteRenderer != null && spriteRenderer.gameObject != null)
        {
            spriteRenderer.transform.localPosition = targetSpritePosition;
            Debug.Log($"[BattleEffectManager] 스프라이트 밀림 완료: {target.name}, 현재스프라이트위치: {spriteRenderer.transform.localPosition}");
        }
        else
        {
            Debug.LogWarning($"[BattleEffectManager] KnockbackCharacter: 최종 위치 설정 실패 - 캐릭터가 파괴됨");
        }
        
        // 전투 루틴이 끝나면 원래 위치로 복귀하는 것은 외부에서 호출
        // ResetCharacterPosition() 메서드를 통해 처리
    }

    // 위치 복귀는 TurnManager에서 CharacterMotionController.ResetPosition()을 직접 호출하므로 제거

    /// <summary>
    /// 데스 연출을 실행합니다
    /// </summary>
    /// <param name="target">사망한 캐릭터</param>
    public void PlayDeathEffect(CharacterStats target)
    {
        if (target == null) return;

        StartCoroutine(DeathEffectCoroutine(target));
    }

    /// <summary>
    /// 데스 연출 코루틴
    /// </summary>
    private IEnumerator DeathEffectCoroutine(CharacterStats target)
    {
        // target이 null인지 즉시 확인
        if (target == null)
        {
            Debug.LogWarning("[BattleEffectManager] DeathEffectCoroutine: target이 null입니다.");
            yield break;
        }

        // 1. 피격 상태로 고정 (죽는 모션 대신 피격 상태 유지)
        if (target != null && target.gameObject != null)
        {
            CharacterMotionController motionController = target.GetComponent<CharacterMotionController>();
            if (motionController != null)
            {
                motionController.PlayHitMotion();
                Debug.Log($"[BattleEffectManager] {target.name} 피격 상태로 고정");
            }
        }

        // 2. 데스 아이콘 생성
        // 카메라가 아직 줌인 상태면 잠깐 기다렸다가(최대 대기 제한) 아이콘을 표시해
        // "줌인 화면 위에 데스 아이콘이 겹쳐 보이는" 어색함을 줄인다.
        float waitElapsed = 0f;
        const float maxDeathIconWait = 1.2f;
        while (Camera.main != null && Camera.main.orthographicSize < 4.9f && waitElapsed < maxDeathIconWait)
        {
            waitElapsed += Time.deltaTime;
            yield return null;
        }

        // 데스 아이콘 생성은 SlotHandler(Character -> Slot -> UI) 경로로만 처리한다.

        // 3. 체력바와 상태이상 슬롯 즉시 숨기기
        if (target != null && target.gameObject != null)
        {
            // 체력바 숨기기 (미리 연결된 참조 사용)
            if (target.hpBarObject != null)
            {
                target.hpBarObject.SetActive(false);
            }
            // 기존 방식도 백업으로 유지
            else if (target.HpUI != null)
            {
                target.HpUI.gameObject.SetActive(false);
            }
            
            // 상태이상 슬롯들 숨기기 (미리 연결된 참조 사용)
            if (target.statusEffectArea != null)
            {
                target.statusEffectArea.gameObject.SetActive(false);
            }
            // 기존 방식도 백업으로 유지
            else
            {
                StatusEffectController statusController = target.GetComponent<StatusEffectController>();
                if (statusController != null)
                {
                    statusController.HideAllStatusEffects();
                }
            }
        }

        // 4. 잠시 대기 후 캐릭터 페이드 아웃 시작 (데스 아이콘 인지 시간 확보)
        yield return new WaitForSeconds(0.55f);

        // 5. 캐릭터 페이드 아웃 효과
        if (target != null && target.gameObject != null)
        {
            // 캐릭터의 실제 스프라이트를 찾기 위해 "Sprite" 자식 오브젝트를 찾음
            Transform spriteTransform = target.transform.Find("Sprite");
            SpriteRenderer spriteRenderer = null;
            
            if (spriteTransform != null)
            {
                spriteRenderer = spriteTransform.GetComponent<SpriteRenderer>();
            }
            
            // "Sprite" 자식이 없으면 기존 방식으로 찾기
            if (spriteRenderer == null)
            {
                spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
            }
            
            if (spriteRenderer != null && spriteRenderer.gameObject != null)
            {
                float elapsed = 0f;
                Color originalColor = spriteRenderer.color;

                while (elapsed < deathEffectDuration)
                {
                    // 매 프레임마다 target과 SpriteRenderer가 유효한지 확인
                    if (target == null || target.gameObject == null || spriteRenderer == null || spriteRenderer.gameObject == null)
                    {
                        Debug.LogWarning("[BattleEffectManager] DeathEffectCoroutine: 오브젝트가 파괴되어 페이드 아웃 중단");
                        break;
                    }

                    float alpha = Mathf.Lerp(1f, 0f, elapsed / deathEffectDuration);
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);

                    elapsed += Time.deltaTime;
                    yield return null;
                }

                // 최종 색상 설정 전에 한 번 더 확인
                if (target != null && target.gameObject != null && spriteRenderer != null && spriteRenderer.gameObject != null)
                {
                    spriteRenderer.color = new Color(originalColor.r, originalColor.g, originalColor.b, 0f);
                }
            }
        }

        // 6. 캐릭터 오브젝트 파괴 (페이드 아웃 완료 후)
        if (target != null && target.gameObject != null)
        {
            Debug.Log($"[BattleEffectManager] {target.name} 캐릭터 파괴");
            Destroy(target.gameObject);
            
            // 전투 종료 체크 수행
            if (TurnManager.Instance != null)
            {
                Debug.Log("[BattleEffectManager] 캐릭터 파괴 후 전투 종료 체크 호출");
                StartCoroutine(CheckBattleEndAfterDestroy());
            }
        }
    }

    /// <summary>
    /// 회복 연출을 실행합니다
    /// </summary>
    /// <param name="target">회복 대상 캐릭터</param>
    /// <param name="healAmount">회복량</param>
    public void PlayHealEffect(CharacterStats target, int healAmount)
    {
        if (target == null) return;

        // 회복 팝업 생성 (초록색) 및 애니메이션
        GameObject healPopup = CreatePopup(target.transform.position, healAmount, false, true);
        if (healPopup != null)
        {
            StartCoroutine(AnimateHitAndDamage(null, healPopup, null, target.transform.position));
        }
        
        // 회복 이펙트 (녹색 파티클 등) 추가 가능
        StartCoroutine(HealEffectCoroutine(target));
    }

    /// <summary>
    /// 버프 연출을 실행합니다
    /// </summary>
    /// <param name="target">버프 대상 캐릭터</param>
    /// <param name="buffValue">버프 수치</param>
    public void PlayBuffEffect(CharacterStats target, int buffValue)
    {
        if (target == null) return;

        // 새로운 시스템에서는 팝업을 생성하지 않음 (SkillManager에서 처리)
        // CreateNewBuffDebuffPopup(target.transform.position, "ATK", buffValue, true);
        
        // 버프 이펙트 (파란색 파티클 등) 추가 가능
        StartCoroutine(BuffEffectCoroutine(target));
    }

    /// <summary>
    /// 회복 연출 코루틴
    /// </summary>
    private IEnumerator HealEffectCoroutine(CharacterStats target)
    {
        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.gameObject != null)
        {
            Color originalColor = spriteRenderer.color;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                // SpriteRenderer가 파괴되었는지 확인
                if (spriteRenderer == null || spriteRenderer.gameObject == null)
                {
                    Debug.LogWarning("[BattleEffectManager] HealEffectCoroutine: SpriteRenderer가 파괴되어 회복 효과 중단");
                    break;
                }

                float t = elapsed / duration;
                // 녹색으로 깜빡이는 효과
                Color healColor = Color.Lerp(originalColor, Color.green, Mathf.Sin(t * Mathf.PI * 4) * 0.3f);
                spriteRenderer.color = healColor;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 최종 색상 복구 전에 한 번 더 확인
            if (spriteRenderer != null && spriteRenderer.gameObject != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// 버프 연출 코루틴
    /// </summary>
    private IEnumerator BuffEffectCoroutine(CharacterStats target)
    {
        SpriteRenderer spriteRenderer = target.GetComponentInChildren<SpriteRenderer>();
        if (spriteRenderer != null && spriteRenderer.gameObject != null)
        {
            Color originalColor = spriteRenderer.color;
            float elapsed = 0f;
            float duration = 0.5f;

            while (elapsed < duration)
            {
                // SpriteRenderer가 파괴되었는지 확인
                if (spriteRenderer == null || spriteRenderer.gameObject == null)
                {
                    Debug.LogWarning("[BattleEffectManager] BuffEffectCoroutine: SpriteRenderer가 파괴되어 버프 효과 중단");
                    break;
                }

                float t = elapsed / duration;
                // 연한 파란색으로 깜빡이는 효과
                Color buffColor = Color.Lerp(originalColor, new Color(0.5f, 0.5f, 1f, 1f), Mathf.Sin(t * Mathf.PI * 4) * 0.3f);
                spriteRenderer.color = buffColor;

                elapsed += Time.deltaTime;
                yield return null;
            }

            // 최종 색상 복구 전에 한 번 더 확인
            if (spriteRenderer != null && spriteRenderer.gameObject != null)
            {
                spriteRenderer.color = originalColor;
            }
        }
    }

    /// <summary>
    /// 스킬 사용 연출을 실행합니다
    /// </summary>
    /// <param name="caster">스킬 사용자</param>
    /// <param name="target">스킬 대상</param>
    /// <param name="skillName">스킬 이름</param>
    public void PlaySkillEffect(CharacterStats caster, CharacterStats target, string skillName)
    {
        if (caster == null) return;

        StartCoroutine(SkillEffectCoroutine(caster, target, skillName));
    }

    /// <summary>
    /// 스킬 연출 코루틴
    /// </summary>
    private IEnumerator SkillEffectCoroutine(CharacterStats caster, CharacterStats target, string skillName)
    {
        // 스킬 사용자 강조 효과
        SpriteRenderer casterSprite = caster.GetComponentInChildren<SpriteRenderer>();
        if (casterSprite != null && casterSprite.gameObject != null)
        {
            Color originalColor = casterSprite.color;
            casterSprite.color = Color.yellow; // 스킬 사용 시 노란색으로 강조

            yield return new WaitForSeconds(0.3f);

            // 색상 복구 전에 한 번 더 확인
            if (casterSprite != null && casterSprite.gameObject != null)
            {
                casterSprite.color = originalColor;
            }
        }

        // 추가 스킬별 특수 이펙트는 여기에 구현
        // 예: 화염 스킬이면 화염 이펙트, 얼음 스킬이면 얼음 이펙트 등
    }

    /// <summary>
    /// 캐릭터 파괴 후 전투 종료 체크를 수행하는 코루틴
    /// </summary>
    private IEnumerator CheckBattleEndAfterDestroy()
    {
        // 한 프레임 대기하여 Destroy가 완료되도록 함
        yield return null;
        
        // TurnManager의 CheckBattleEnd 직접 호출
        if (TurnManager.Instance != null)
        {
            Debug.Log("[BattleEffectManager] CheckBattleEndAfterDestroy 실행");
            // 전투 종료 체크만 수행 (EndTurn 호출하지 않음)
            TurnManager.Instance.CheckBattleEnd();
        }
    }

} 