using System.Collections.Generic;
using UnityEngine;

public class BattleSettingManager : MonoBehaviour
{
    public static BattleSettingManager Instance { get; private set; }

    [SerializeField] private List<BattleSttingCharacterSlot> playerSlots;
    [SerializeField] private CharacterInventoryTab inventoryTab;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnEnable()
    {
        BattleSttingCharacterSlot.OnSlotChanged += HandleSlotChanged;
    }

    private void OnDisable()
    {
        BattleSttingCharacterSlot.OnSlotChanged -= HandleSlotChanged;
    }

    private void Start()
    {
        // 초기화
        inventoryTab.RefreshInventory();
        UpdateSpawnManagerParty();
    }

    private void HandleSlotChanged(BattleSttingCharacterSlot slot, CharacterData data)
    {
        UpdateSpawnManagerParty();
    }

    public void UpdateSpawnManagerParty()
    {
        if (SpawnManager.Instance == null)
        {
            Debug.LogError("SpawnManager 인스턴스를 찾을 수 없습니다!");
            return;
        }

        SpawnManager.Instance.allyIDs.Clear();

        foreach (var slot in playerSlots)
        {
            CharacterData charData = slot.GetCharacterData();
            if (charData != null)
            {
                // playerID는 고정값이므로 allyIDs에 추가하지 않음
                if (charData.Label != SpawnManager.Instance.playerID)
                {
                    SpawnManager.Instance.allyIDs.Add(charData.Label);
                }
            }
        }
        
        // 디버그 로그
        Debug.Log("스폰매니저 파티 업데이트: " + string.Join(", ", SpawnManager.Instance.allyIDs));
    }
} 