using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class CharacterInventoryTab : MonoBehaviour
{
    [Header("UI 연결")]
    [SerializeField] private Transform characterListContainer;
    [SerializeField] private CharacterBlock characterBlockPrefab;

    private List<CharacterBlock> characterBlocks = new List<CharacterBlock>();

    private void OnEnable()
    {
        RefreshInventory();
    }

    public void RefreshInventory()
    {
        Debug.Log("[Debug] 3. RefreshInventory: 캐릭터 블록 생성을 시작합니다.");
        if (GameProgressManager.Instance == null)
        {
            Debug.LogError("[Debug] RefreshInventory 실패: GameProgressManager를 찾을 수 없습니다!");
            return;
        }

        var inventory = GameProgressManager.Instance.CharacterInventory;
        Debug.Log($"[Debug] 4. RefreshInventory: GameProgressManager로부터 총 {inventory.Count}개의 캐릭터 데이터를 가져왔습니다.");

        ClearCharacterList();

        if (inventory.Count == 0)
        {
            Debug.LogWarning("[Debug] RefreshInventory: 인벤토리가 비어있어, 캐릭터 블록을 생성하지 않았습니다.");
            return;
        }

        // 1. 순정(IsCustomized == false) 캐릭터 그룹화 및 수량 계산
        var standardCharacters = inventory
            .Where(c => !c.IsCustomized)
            .GroupBy(c => c.ID)
            .ToDictionary(g => g.Key, g => g.Count());

        // 2. 강화(IsCustomized == true) 캐릭터 리스트
        var customizedCharacters = inventory.Where(c => c.IsCustomized).ToList();

        int createdCount = 0;

        // 3. 순정 캐릭터 블록 생성
        foreach (var kvp in standardCharacters)
        {
            string characterId = kvp.Key;
            int count = kvp.Value;

            if (CharacterData.characterDict.TryGetValue(characterId, out var characterData))
            {
                CharacterBlock newBlock = Instantiate(characterBlockPrefab, characterListContainer);
                newBlock.Initialize(characterData, count);
                newBlock.name = $"Block_{characterData.Label} (x{count})";
                characterBlocks.Add(newBlock);
                createdCount++;
            }
        }

        // 4. 강화된 개별 캐릭터 블록 생성
        foreach (var characterData in customizedCharacters)
        {
            CharacterBlock newBlock = Instantiate(characterBlockPrefab, characterListContainer);
            newBlock.Initialize(characterData, 1);
            newBlock.name = $"Block_{characterData.Label} (Customized)";
            characterBlocks.Add(newBlock);
            createdCount++;
        }

        Debug.Log($"[Debug] 7. RefreshInventory: 총 {createdCount}개의 캐릭터 블록을 생성하고 프로세스를 완료했습니다.");
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

    public void ReturnCharacterBlock(CharacterBlock block)
    {
        if (block == null) return;

        block.gameObject.SetActive(true);
        block.transform.SetParent(characterListContainer);
        if (!characterBlocks.Contains(block))
        {
            characterBlocks.Add(block);
        }
    }

    // 필요시 캐릭터 선택, 상세정보, 강화 등 이벤트/메서드 추가
}
