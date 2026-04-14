using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.EventSystems;

/// <summary>
/// 파티 세팅에서 플레이어블 캐릭터 한 명의 슬롯 역할을 담당.
/// 캐릭터 이미지와 데이터 참조를 보유.
/// </summary>
public class BattleSettingCharacterSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    
    [Header("참조")]
    [SerializeField] private CharacterInventoryTab inventoryTab; // Inspector에서 연결

    [Header("UI")]
    [SerializeField] private Image characterImage; // 캐릭터 이미지를 표시할 UI
    [SerializeField] private int slotNumber;       // 슬롯 번호 (Inspector에서 1~N)
    [SerializeField] private Image slotBackground; // 슬롯 배경 이미지
    [SerializeField] public Collider2D slotCollider; // 슬롯 콜라이더 (슬롯 상호작용 용도)
    [SerializeField] private SlotVisualState slotVisualState; // 이곳을 중심으로 슬롯의 상태를 체크크

    public CharacterBlock currentCharacterBlock;
    private bool isLocked = false; // 슬롯 고정 여부
    private CharacterData currentCharacterData; // 현재 슬롯에 배치된 캐릭터 데이터

    // 마우스 오버 상태 플래그
    public bool isPointerOver = false;

    // 이벤트 시스템
    public static event Action<BattleSettingCharacterSlot, CharacterData> OnSlotChanged;

    public enum SlotVisualState
    {
        Empty,          // 빈 슬롯 (기본 상태)
        Occupied,       // 캐릭터가 배치된 상태
        DragOver,       // 드래그 중인 블록이 위에 있을 때
        InvalidDrop     // 드롭할 수 없는 상태
    }

    private void Awake()
    {
        // 슬롯 자체의 위치는 변경하지 않음 (기존 레이아웃 유지)
        // 블록만 중앙에 배치되도록 PlaceCharacterBlock에서 처리
        Debug.Log($"[Slot{slotNumber}] 슬롯 초기화 완료");
    }

    private void Update()
    {
        if (slotCollider == null) return;

        // 마우스 위치가 콜라이더 안에 있는지 체크
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (slotCollider.OverlapPoint(mouseWorldPos))
        {
            Debug.Log($"[Slot{slotNumber}] 마우스가 콜라이더 안에 있음");
        }

        // 드래그 중인 블록이 콜라이더 안에 있는지 체크
        Collider2D[] hits = Physics2D.OverlapBoxAll(slotCollider.bounds.center, slotCollider.bounds.size, 0f);
        foreach (var hit in hits)
        {
            CharacterBlock block = hit.GetComponent<CharacterBlock>();
            if (block != null)
            {
                Debug.Log($"[Slot{slotNumber}] 블록 '{block.characterData?.Label}'이 콜라이더 안에 있음");
            }
        }
    }

    /// <summary>
    /// 슬롯에 캐릭터 블록을 배치합니다.
    /// </summary>
    public void PlaceCharacterBlock(CharacterBlock block)
    {
        if (block == null) return;

        // 이미 슬롯에 같은 블록이 있으면 무시
        if (currentCharacterBlock == block) return;

        // 슬롯이 잠겨있으면 배치 불가
        if (isLocked) return;

        if (CharacterInventoryTab.Instance != null)
        {
            // 인벤토리 리스트에 남아 있는 참조를 제거해 슬롯/인벤토리 UI 이중 상태를 방지.
            CharacterInventoryTab.Instance.RemoveBlockFromList(block);
        }

        // 기존 블록이 있으면 인벤토리로 반환
        if (currentCharacterBlock != null)
        {
            // 인벤토리로 반환
            CharacterInventoryTab.Instance.ReturnCharacterBlock(currentCharacterBlock);
            // 기존 블록의 anchor도 인벤토리로 갱신
            currentCharacterBlock.SetAnchorToSlot(CharacterInventoryTab.Instance.characterListContainer);
        }

        // 새 블록을 슬롯에 배치
        block.transform.SetParent(this.transform);
        block.SetAnchorToSlot(this.transform); // anchor 갱신
        
        // RectTransform 속성을 명시적으로 설정하여 중앙 배치 보장
        var rectTransform = block.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            
            // 디버그 로그로 실제 적용 여부 확인
            Debug.Log($"[Slot{slotNumber}] 블록 RectTransform 설정 - anchorMin: {rectTransform.anchorMin}, anchorMax: {rectTransform.anchorMax}, pivot: {rectTransform.pivot}, anchoredPosition: {rectTransform.anchoredPosition}");
        }
        
        currentCharacterBlock = block;
        currentCharacterData = block.characterData;
        slotVisualState = SlotVisualState.Occupied;

        // UI 업데이트
        UpdateSlotVisual();

        // 이벤트 발생
        OnSlotChanged?.Invoke(this, currentCharacterData);

        // 매니저에 알림
        if (BattleSettingManager.Instance != null)
        {
            // BattleSettingManager.Instance.HandleCharacterPlacedInSlot(block, this); // 임시 주석처리
        }

        Debug.Log($"[슬롯] 블록 배치: {currentCharacterData.Label}, HP: {currentCharacterData.Hp}/{currentCharacterData.MaxHp}, Atk: {currentCharacterData.Atk}, Def: {currentCharacterData.Def}, Speed: {currentCharacterData.Speed}, Evasion: {currentCharacterData.EvasionRate}, Accuracy: {currentCharacterData.Accuracy}, KDP: {currentCharacterData.KDP}/{currentCharacterData.MaxKDP}, Rarity: {currentCharacterData.Rarity}");
    }

    /// <summary>
    /// 슬롯에서 캐릭터 블록을 제거합니다.
    /// </summary>
    public void RemoveCharacterBlock()
    {
        if (currentCharacterBlock != null)
        {
            // 인벤토리로 반환
            CharacterInventoryTab.Instance.ReturnCharacterBlock(currentCharacterBlock);
            
            var removedData = currentCharacterData;
            currentCharacterBlock = null;
            currentCharacterData = null;
            slotVisualState = SlotVisualState.Empty;

            // UI 업데이트
            UpdateSlotVisual();

            // 이벤트 발생
            OnSlotChanged?.Invoke(this, null);

            Debug.Log($"[Slot] 슬롯 {slotNumber}에서 캐릭터 '{removedData?.Label}' 제거하고 인벤토리로 반환 완료");
        }
    }

    /// <summary>
    /// 현재 슬롯 자식/참조를 기준으로 UI를 강제 동기화합니다.
    /// 데이터는 정상인데 슬롯 비주얼만 비는 현상 방지용.
    /// </summary>
    public void ForceSyncVisualFromCurrentBlock()
    {
        if (currentCharacterBlock == null)
        {
            currentCharacterBlock = GetComponentInChildren<CharacterBlock>(true);
        }

        if (currentCharacterBlock != null)
        {
            currentCharacterData = currentCharacterBlock.characterData;
            slotVisualState = SlotVisualState.Occupied;
        }
        else
        {
            currentCharacterData = null;
            slotVisualState = SlotVisualState.Empty;
        }

        UpdateSlotVisual();
    }

    /// <summary>
    /// 외부에서 캐릭터 데이터를 직접 설정합니다 (주인공 자동 배치 등).
    /// </summary>
    public void SetCharacter(CharacterData data, Sprite sprite)
    {
        if (data == null) return;

        currentCharacterData = data;
        slotVisualState = SlotVisualState.Occupied;

        // UI 업데이트
        if (characterImage != null && sprite != null)
        {
            characterImage.sprite = sprite;
            characterImage.color = Color.white;
        }

        // 이벤트 발생
        OnSlotChanged?.Invoke(this, currentCharacterData);

        Debug.Log($"[Slot] 슬롯 {slotNumber}에 캐릭터 '{data.Label}' 직접 설정 완료");
    }

    /// <summary>
    /// 슬롯을 잠급니다 (주인공 슬롯 등).
    /// </summary>
    public void LockSlot()
    {
        isLocked = true;
        Debug.Log($"[Slot] 슬롯 {slotNumber} 잠금 완료");
    }

    /// <summary>
    /// 슬롯의 잠금을 해제합니다.
    /// </summary>
    public void UnlockSlot()
    {
        isLocked = false;
        Debug.Log($"[Slot] 슬롯 {slotNumber} 잠금 해제 완료");
    }

    /// <summary>
    /// 현재 슬롯에 배치된 캐릭터 데이터를 반환합니다.
    /// </summary>
    public CharacterData GetCharacterData()
    {
        return currentCharacterData;
    }

    /// <summary>
    /// 슬롯의 시각적 상태를 업데이트합니다.
    /// </summary>
    private void UpdateSlotVisual()
    {
        if (characterImage == null) return;

        switch (slotVisualState)
        {
            case SlotVisualState.Empty:
                characterImage.sprite = null;
                characterImage.color = new Color(0.8f, 0.8f, 0.8f, 0.5f);
                break;
            case SlotVisualState.Occupied:
                if (currentCharacterData != null && !string.IsNullOrEmpty(currentCharacterData.Sprite))
                {
                    string standPath = $"{currentCharacterData.Sprite}/Stand";
                    Sprite loaded = Resources.Load<Sprite>(standPath);
                    if (loaded == null)
                        loaded = Resources.Load<Sprite>(currentCharacterData.Sprite); // 구형 단일 경로 호환
                    characterImage.sprite = loaded;
                    characterImage.color = Color.white;
                }
                break;
            case SlotVisualState.DragOver:
                // 드래그 오버 시 시각적 피드백
                break;
            case SlotVisualState.InvalidDrop:
                // 무효한 드롭 시 시각적 피드백
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
        Debug.Log($"[Slot{slotNumber}] 마우스 진입");
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
        Debug.Log($"[Slot{slotNumber}] 마우스 이탈");
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 슬롯이 잠겨있지 않고 캐릭터가 배치되어 있으면 제거
        if (!isLocked && currentCharacterBlock != null)
        {
            Debug.Log($"[Slot{slotNumber}] 슬롯 클릭: 캐릭터 제거");
            RemoveCharacterBlock();
        }
        else if (isLocked)
        {
            Debug.Log($"[Slot{slotNumber}] 슬롯 클릭: 잠긴 슬롯이므로 제거 불가");
        }
        else
        {
            Debug.Log($"[Slot{slotNumber}] 슬롯 클릭: 빈 슬롯");
        }
    }
}
