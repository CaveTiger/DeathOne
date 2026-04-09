using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class SlotHandler : MonoBehaviour
{
    public CharacterStats currentCharacter;
    public int slotIndex; // 슬롯 순서 저장 (성능 최적화용)

    [Header("Death Icon Relay (Slot -> UI)")]
    [SerializeField] private bool relayDeathIconToUI = true;
    [SerializeField] private Canvas deathOverlayCanvas;
    [SerializeField] private GameObject deathIconPrefab;
    [SerializeField] private Vector3 deathIconWorldOffset = new Vector3(0f, 0.12f, 0f);
    [SerializeField, Range(0f, 1f)] private float deathIconHeightNormalized = 0.56f; // 0=발, 0.5=중앙, 1=머리
    [SerializeField] private float deathIconLifetime = 2.8f;

    private CharacterStats subscribedCharacter;
    private bool hasShownDeathIcon;

    private void OnEnable()
    {
        FindUnit();
    }

    private void OnDisable()
    {
        UnbindDeathEvent();
    }

    public void FindUnit()
    {
        UnbindDeathEvent();
        currentCharacter = GetComponentInChildren<CharacterStats>();
        hasShownDeathIcon = false;
        BindDeathEvent();

        if (currentCharacter != null)
        {
            //Debug.Log($"[슬롯 {name}] 캐릭터 로드 완료: {currentCharacter.name}");
        }
        else
        {
            //Debug.LogWarning($"[슬롯 {name}] 캐릭터가 자식에 존재하지 않음!");
        }
    }
    public CharacterStats SlotCharacterLoad()
    {
        FindUnit();
        return currentCharacter;
    }

    // 턴 시작 직전 상태이상 정산 (슬롯 컨테이너 중심 설계 대비용)
    public void SettleStatusEffectsAtTurnStart()
    {
        if (currentCharacter == null)
            FindUnit();

        if (currentCharacter == null)
            return;

        var controller = currentCharacter.GetComponent<StatusEffectController>();
        if (controller != null)
        {
            controller.ApplyStatusEffectsOnTurnStart();
        }
    }

    /// <summary>
    /// 상태이상 정산 + 연출을 함께 처리하는 메서드
    /// </summary>
    public IEnumerator SettleStatusEffectsWithAnimation(CharacterStats character)
    {
        // 매개변수로 받은 캐릭터를 currentCharacter로 설정 (동기화 보장)
        if (currentCharacter != character)
        {
            UnbindDeathEvent();
        }
        currentCharacter = character;
        hasShownDeathIcon = false;
        BindDeathEvent();
        
        if (currentCharacter == null)
        {
            Debug.LogWarning($"[SlotHandler] {name}: currentCharacter가 null입니다.");
            yield break;
        }

        Debug.Log($"[SlotHandler] {name}: 상태이상 정산 시작 - 캐릭터: {currentCharacter.Label}");

        // StatusEffectSlot을 통해 상태이상 정산 + 연출 처리
        var statusEffectSlot = currentCharacter.transform.Find("StatusEffectSlot")?.GetComponent<StatusEffectSlot>();
        if (statusEffectSlot != null)
        {
            yield return StartCoroutine(statusEffectSlot.SettleStatusEffectsWithAnimation(currentCharacter));
        }
        else
        {
            Debug.LogWarning($"[SlotHandler] {name}: StatusEffectSlot을 찾을 수 없습니다.");
            // StatusEffectSlot이 없는 경우 기존 방식 백업
            SettleStatusEffectsAtTurnStart();
        }
    }

    private void BindDeathEvent()
    {
        if (currentCharacter == null)
            return;

        currentCharacter.OnDeathEvent += HandleCharacterDeathForSlotUI;
        subscribedCharacter = currentCharacter;
    }

    private void UnbindDeathEvent()
    {
        if (subscribedCharacter == null)
            return;

        subscribedCharacter.OnDeathEvent -= HandleCharacterDeathForSlotUI;
        subscribedCharacter = null;
    }

    private void HandleCharacterDeathForSlotUI(CharacterStats deadCharacter)
    {
        if (!relayDeathIconToUI || deadCharacter == null)
            return;

        if (currentCharacter == null || deadCharacter != currentCharacter)
            return;

        if (hasShownDeathIcon)
            return;

        hasShownDeathIcon = true;
        SpawnDeathIcon(deadCharacter);
    }

    private void SpawnDeathIcon(CharacterStats deadCharacter)
    {
        if (deathOverlayCanvas == null)
        {
            Debug.LogWarning($"[SlotHandler] {name}: deathOverlayCanvas가 비어 있습니다.");
            return;
        }

        if (deathIconPrefab == null)
        {
            Debug.LogWarning($"[SlotHandler] {name}: deathIconPrefab이 비어 있습니다.");
            return;
        }

        if (!IsValidDeathIconPrefab(deathIconPrefab))
        {
            Debug.LogWarning($"[SlotHandler] {name}: deathIconPrefab에 표시 가능한 UI 요소가 없습니다. 생성 건너뜀.");
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogWarning($"[SlotHandler] {name}: Main Camera를 찾을 수 없습니다.");
            return;
        }

        RectTransform canvasRect = deathOverlayCanvas.GetComponent<RectTransform>();
        if (canvasRect == null)
            return;

        Vector3 anchorPos = GetDeathIconAnchorWorldPosition(deadCharacter);
        Vector3 worldPos = new Vector3(
            anchorPos.x + deathIconWorldOffset.x,
            anchorPos.y,
            anchorPos.z + deathIconWorldOffset.z
        );
        Vector3 screenPos = mainCamera.WorldToScreenPoint(worldPos);

        Camera uiCamera = deathOverlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay
            ? null
            : deathOverlayCanvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, screenPos, uiCamera, out Vector2 localPoint))
            return;

        GameObject icon = Instantiate(deathIconPrefab, deathOverlayCanvas.transform);
        RectTransform iconRect = icon.GetComponent<RectTransform>();
        if (iconRect != null)
        {
            // 프리팹 앵커/피벗이 제각각이면 anchoredPosition만으로 위치가 어긋난다. 월드→스크린 변환 지점을
            // 아이콘 중심에 맞춘다.
            iconRect.anchorMin = Vector2.one * 0.5f;
            iconRect.anchorMax = Vector2.one * 0.5f;
            iconRect.pivot = Vector2.one * 0.5f;
            iconRect.anchoredPosition = localPoint;
        }

        Destroy(icon, deathIconLifetime);
    }

    private Vector3 GetDeathIconAnchorWorldPosition(CharacterStats deadCharacter)
    {
        if (deadCharacter == null)
            return Vector3.zero;

        SpriteRenderer spriteRenderer = deadCharacter.spriteRenderer;
        if (spriteRenderer == null)
        {
            Transform spriteRoot = deadCharacter.transform.Find("Sprite");
            if (spriteRoot != null)
                spriteRenderer = spriteRoot.GetComponent<SpriteRenderer>();
        }
        if (spriteRenderer == null)
            spriteRenderer = deadCharacter.GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer != null)
        {
            Bounds b = spriteRenderer.bounds;
            float y = Mathf.Lerp(b.min.y, b.max.y, deathIconHeightNormalized);
            return new Vector3(b.center.x, y, deadCharacter.transform.position.z);
        }

        return deadCharacter.transform.position;
    }

    private bool IsValidDeathIconPrefab(GameObject prefab)
    {
        if (prefab == null) return false;

        // 스프라이트 UI
        Image image = prefab.GetComponentInChildren<Image>(true);
        if (image != null && image.sprite != null) return true;

        // 월드 스프라이트
        SpriteRenderer spriteRenderer = prefab.GetComponentInChildren<SpriteRenderer>(true);
        if (spriteRenderer != null && spriteRenderer.sprite != null) return true;

        // 텍스트 기반 아이콘(예: TMP 숫자/심볼)
        TMP_Text tmp = prefab.GetComponentInChildren<TMP_Text>(true);
        if (tmp != null) return true;

        Text legacyText = prefab.GetComponentInChildren<Text>(true);
        if (legacyText != null) return true;

        return false;
    }

}
