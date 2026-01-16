using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

public class CharacterInventoryTab : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static CharacterInventoryTab Instance { get; private set; }
    [Header("UI 연결")]
    [SerializeField] public Transform characterListContainer;
    [SerializeField] private CharacterBlock characterBlockPrefab;

    private List<CharacterBlock> characterBlocks = new List<CharacterBlock>();
    public bool isPointerOver = false;
    private bool isInitialized = false;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        if (!isInitialized)
        {
            RefreshInventory();
            isInitialized = true;
        }
    }

    public void RefreshInventory()
    {
        Debug.Log("[Debug] 3. RefreshInventory: 캐릭터 블록 생성을 시작합니다.");
        
        try
        {
            if (GameProgressManager.Instance == null)
            {
                Debug.LogError("[Debug] RefreshInventory 실패: GameProgressManager를 찾을 수 없습니다!");
                return;
            }

            if (characterBlockPrefab == null)
            {
                Debug.LogError("[Debug] RefreshInventory 실패: characterBlockPrefab이 null입니다!");
                return;
            }

            if (characterListContainer == null)
            {
                Debug.LogError("[Debug] RefreshInventory 실패: characterListContainer가 null입니다!");
                return;
            }

            var inventory = GameProgressManager.Instance.CharacterInventory;
            if (inventory == null)
            {
                Debug.LogError("[Debug] RefreshInventory 실패: CharacterInventory가 null입니다!");
                return;
            }
            
            Debug.Log($"[Debug] 4. RefreshInventory: GameProgressManager로부터 총 {inventory.Count}개의 캐릭터 데이터를 가져왔습니다.");

            ClearCharacterList();

            if (inventory.Count == 0)
            {
                Debug.LogWarning("[Debug] RefreshInventory: 인벤토리가 비어있어, 캐릭터 블록을 생성하지 않았습니다.");
                return;
            }

            // 해금된 캐릭터만 블록 생성
            int createdCount = 0;
            foreach (var characterData in inventory)
            {
                if (characterData == null)
                {
                    Debug.LogWarning("[Debug] RefreshInventory: null 캐릭터 데이터 발견, 건너뜀");
                    continue;
                }
                
                if (!characterData.IsUnlocked) continue; // 해금된 캐릭터만 표시
                
                try
                {
                    CharacterBlock newBlock = Instantiate(characterBlockPrefab, characterListContainer);
                    if (newBlock != null)
                    {
                        newBlock.Initialize(characterData);
                        newBlock.name = $"Block_{characterData.Label}";
                        characterBlocks.Add(newBlock);
                        createdCount++;
                    }
                    else
                    {
                        Debug.LogError("[Debug] RefreshInventory: CharacterBlock 생성 실패");
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[Debug] RefreshInventory: 캐릭터 블록 생성 중 오류 - {characterData.Label}: {e.Message}");
                }
            }
            
            // 인벤토리 재정렬 (일관된 정렬 로직 사용)
            SortCharacterBlocks();
            
            Debug.Log($"[Debug] 7. RefreshInventory: 총 {createdCount}개의 해금된 캐릭터 블록을 생성하고 프로세스를 완료했습니다.");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[Debug] RefreshInventory: 전체 프로세스 중 오류 발생: {e.Message}");
        }
    }

    private void ClearCharacterList()
    {
        foreach (var block in characterBlocks)
        {
            if (block != null)
                Destroy(block.gameObject);
        }
        characterBlocks.Clear();
    }

    /// <summary>
    /// 캐릭터 블록을 인벤토리로 반환합니다.
    /// </summary>
    public void ReturnCharacterBlock(CharacterBlock block)
    {
        if (block == null) return;

        block.gameObject.SetActive(true);
        block.transform.SetParent(characterListContainer, true);
        
        // RectTransform 속성을 명시적으로 설정
        var rectTransform = block.GetComponent<RectTransform>();
        if (rectTransform != null)
        {
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            
            Debug.Log($"[Inventory] 블록 반환 RectTransform 설정 - anchorMin: {rectTransform.anchorMin}, anchorMax: {rectTransform.anchorMax}, pivot: {rectTransform.pivot}, anchoredPosition: {rectTransform.anchoredPosition}");
        }
        
        if (!characterBlocks.Contains(block))
        {
            characterBlocks.Add(block);
        }
        
        // 인벤토리 재정렬
        SortCharacterBlocks();
        
        Debug.Log($"[Inventory] 캐릭터 블록 반환: {block.characterData.Label}");
    }

    /// <summary>
    /// 인벤토리 블록들을 GameProgressManager의 CharacterInventory 순서에 맞게 재정렬합니다.
    /// </summary>
    private void SortCharacterBlocks()
    {
        if (GameProgressManager.Instance == null) return;
        
        var inventory = GameProgressManager.Instance.CharacterInventory;
        if (inventory == null) return;
        
        // GameProgressManager의 인벤토리 순서를 기준으로 정렬
        characterBlocks = characterBlocks
            .OrderBy(block => 
            {
                if (block?.characterData == null) return int.MaxValue;
                int index = inventory.FindIndex(data => data != null && data.ID == block.characterData.ID);
                return index >= 0 ? index : int.MaxValue;
            })
            .ToList();
        
        // 정렬된 순서대로 Transform 순서 변경
        for (int i = 0; i < characterBlocks.Count; i++)
        {
            if (characterBlocks[i] != null)
            {
                characterBlocks[i].transform.SetSiblingIndex(i);
            }
        }
        
        Debug.Log($"[Inventory] 인벤토리 블록 재정렬 완료 (총 {characterBlocks.Count}개)");
    }

    /// <summary>
    /// 인벤토리 리스트에서 특정 CharacterBlock을 제거합니다.
    /// </summary>
    public void RemoveBlockFromList(CharacterBlock block)
    {
        if (block == null)
        {
            Debug.LogWarning("[Inventory] 제거 시도: null 블록");
            return;
        }

        if (characterBlocks.Contains(block))
        {
            characterBlocks.Remove(block);
            Debug.Log($"[Inventory] 블록 제거 완료: {block.name}");
        }
        else
        {
            Debug.LogWarning($"[Inventory] 제거 시도: 인벤토리에 없는 블록 - {block.name}");
        }
    }

    /// <summary>
    /// 특정 캐릭터가 인벤토리에 있는지 확인합니다.
    /// </summary>
    public bool HasCharacter(string characterId)
    {
        if (GameProgressManager.Instance == null) return false;

        var inventory = GameProgressManager.Instance.CharacterInventory;
        return inventory.Any(c => c.ID == characterId && c.IsUnlocked);
    }

    /// <summary>
    /// 특정 캐릭터의 데이터를 반환합니다.
    /// </summary>
    public CharacterData GetCharacterData(string characterId)
    {
        if (GameProgressManager.Instance == null) return null;

        var inventory = GameProgressManager.Instance.CharacterInventory;
        return inventory.FirstOrDefault(c => c.ID == characterId && c.IsUnlocked);
    }

    /// <summary>
    /// 해금된 모든 캐릭터 데이터를 반환합니다.
    /// </summary>
    public List<CharacterData> GetUnlockedCharacters()
    {
        if (GameProgressManager.Instance == null) return new List<CharacterData>();

        var inventory = GameProgressManager.Instance.CharacterInventory;
        return inventory.Where(c => c.IsUnlocked).ToList();
    }

    /// <summary>
    /// 캐릭터 ID로 CharacterBlock을 찾아 반환합니다.
    /// </summary>
    public CharacterBlock GetCharacterBlockByID(string characterId)
    {
        return characterBlocks.FirstOrDefault(b => b.characterData != null && b.characterData.ID == characterId);
    }

    // 필요시 캐릭터 선택, 상세정보, 강화 등 이벤트/메서드 추가

    public void OnPointerEnter(PointerEventData eventData)
    {
        isPointerOver = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isPointerOver = false;
    }
}
