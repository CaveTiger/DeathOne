using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BattleSettingManager : MonoBehaviour
{
    public static BattleSettingManager Instance { get; private set; }
    private static readonly string[] DefaultSkillIDs = { "010001", "010002", "010003", "010004" };

    [Header("파티 슬롯 직접 등록")]
    public BattleSettingCharacterSlot slot1;
    public BattleSettingCharacterSlot slot2;
    public BattleSettingCharacterSlot slot3;
    public BattleSettingCharacterSlot slot4;

    [SerializeField] private CharacterInventoryTab inventoryTab;

    [Header("주인공 스킬 세팅")]
    [SerializeField] private GameObject skillSettingPanel; // 스킬 세팅 패널
    [SerializeField] private Transform skillSlotContainer; // 스킬 슬롯 컨테이너
    [SerializeField] private GameObject skillSlotPrefab; // 스킬 슬롯 프리팹
    
    // 직접 스킬ID 관리 (단순화된 데이터 흐름)
    private string[] playerSkillIDs = new string[4] { "", "", "", "" };

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
        BattleSettingCharacterSlot.OnSlotChanged += HandleSlotChanged;
    }

    private void OnDisable()
    {
        BattleSettingCharacterSlot.OnSlotChanged -= HandleSlotChanged;
    }

    private void Start()
    {
        // CharacterInventoryTab 초기화 대기 후 실행
        StartCoroutine(InitializeBattleSetting());
    }
    
    private System.Collections.IEnumerator InitializeBattleSetting()
    {
        // CharacterInventoryTab이 초기화될 때까지 대기
        while (CharacterInventoryTab.Instance == null)
        {
            yield return null;
        }
        
        // 추가로 한 프레임 더 대기하여 완전히 초기화되도록 보장
        yield return null;
        
        if (inventoryTab != null)
        {
            inventoryTab.RefreshInventory();
            // 인벤토리 블럭 생성 완료 대기
            yield return new WaitForSeconds(0.1f);
            yield return null;
            
            // 인벤토리 블럭 생성 후 주인공 블럭을 1번 슬롯에 자동 배치
            PlaceMainCharacterBlockToSlot1();
        }
        
        // 초기 기초스킬 설정
        InitializeDefaultSkills();
        
        // 나머지 슬롯 초기화 및 파티 정보 동기화
        UpdateSpawnManagerParty();
        if (skillSettingPanel != null)
        {
            skillSettingPanel.SetActive(false);
        }
    }

    // 주인공 블록을 1번 슬롯에 자동 배치 (보장시스템)
    private void PlaceMainCharacterBlockToSlot1()
    {
        if (CharacterInventoryTab.Instance == null || slot1 == null) return;

        // 주인공 슬롯이 잠겨 있다면 해제 (향후 사용자 조작 허용)
        slot1.UnlockSlot();

        var mainBlock = CharacterInventoryTab.Instance.GetCharacterBlockByID("000001");
        if (mainBlock != null && slot1.currentCharacterBlock != mainBlock)
        {
            CharacterInventoryTab.Instance.RemoveBlockFromList(mainBlock);
            mainBlock.gameObject.SetActive(true);
            slot1.PlaceCharacterBlock(mainBlock);
            Debug.Log("[BattleSetting] 1번 슬롯에 주인공 블록 자동 배치");
        }
    }

    /// <summary>
    /// 초기 기초스킬 설정 (BaseCharacter.xml의 기본 스킬들)
    /// </summary>
    private void InitializeDefaultSkills()
    {
        // playerSkillIDs 배열에 기초스킬 설정
        for (int i = 0; i < DefaultSkillIDs.Length && i < playerSkillIDs.Length; i++)
        {
            playerSkillIDs[i] = DefaultSkillIDs[i];
        }
        
        Debug.Log($"[SlotBased] 초기 기초스킬 설정 완료: {string.Join(",", playerSkillIDs)}");
    }

    private void HandleSlotChanged(BattleSettingCharacterSlot slot, CharacterData data)
    {
        UpdateSpawnManagerParty();
    }

    /// <summary>
    /// 슬롯 기반 단방향 전달: 현재 슬롯 상태를 SpawnManager에 전달
    /// </summary>
    public void UpdateSpawnManagerParty()
    {
        Debug.Log("[SlotBased] UpdateSpawnManagerParty() 호출됨");
        if (SpawnManager.Instance == null)
        {
            Debug.LogWarning("[SlotBased] SpawnManager를 찾을 수 없습니다.");
            return;
        }

        // 현재 슬롯에 배치된 캐릭터들의 ID/데이터를 수집
        List<string> partyIDs = new List<string>();
        List<CharacterData> partyData = new List<CharacterData>();
        
        // 각 슬롯의 상태 확인
        Debug.Log("[SlotBased] === 슬롯 상태 확인 ===");
        Debug.Log($"[SlotBased] slot1: {(slot1 != null ? "존재" : "null")}, GetCharacterData: {(slot1?.GetCharacterData() != null ? slot1.GetCharacterData().Label : "null")}");
        Debug.Log($"[SlotBased] slot2: {(slot2 != null ? "존재" : "null")}, GetCharacterData: {(slot2?.GetCharacterData() != null ? slot2.GetCharacterData().Label : "null")}");
        Debug.Log($"[SlotBased] slot3: {(slot3 != null ? "존재" : "null")}, GetCharacterData: {(slot3?.GetCharacterData() != null ? slot3.GetCharacterData().Label : "null")}");
        Debug.Log($"[SlotBased] slot4: {(slot4 != null ? "존재" : "null")}, GetCharacterData: {(slot4?.GetCharacterData() != null ? slot4.GetCharacterData().Label : "null")}");
        Debug.Log("[SlotBased] ======================");
        
        if (slot1 != null && slot1.GetCharacterData() != null)
        {
            partyIDs.Add(slot1.GetCharacterData().ID);
            partyData.Add(NormalizePartyCharacterForBattle(slot1.GetCharacterData()));
        }
        if (slot2 != null && slot2.GetCharacterData() != null)
        {
            partyIDs.Add(slot2.GetCharacterData().ID);
            partyData.Add(NormalizePartyCharacterForBattle(slot2.GetCharacterData()));
        }
        if (slot3 != null && slot3.GetCharacterData() != null)
        {
            partyIDs.Add(slot3.GetCharacterData().ID);
            partyData.Add(NormalizePartyCharacterForBattle(slot3.GetCharacterData()));
        }
        if (slot4 != null && slot4.GetCharacterData() != null)
        {
            partyIDs.Add(slot4.GetCharacterData().ID);
            partyData.Add(NormalizePartyCharacterForBattle(slot4.GetCharacterData()));
        }

        // 주인공이 존재한다면 1번 인덱스로 정렬
        if (partyIDs.Contains("000001"))
        {
            int mainIndex = partyIDs.IndexOf("000001");
            if (mainIndex > 0)
            {
                var mainID = partyIDs[mainIndex];
                var mainData = partyData[mainIndex];
                partyIDs.RemoveAt(mainIndex);
                partyData.RemoveAt(mainIndex);
                partyIDs.Insert(0, mainID);
                partyData.Insert(0, mainData);
                Debug.Log("[BattleSetting] 주인공을 첫 번째 위치로 이동했습니다.");
            }
        }
        else
        {
            Debug.LogWarning("[BattleSetting] 주인공이 슬롯에서 제거된 상태입니다. 전투에는 현재 슬롯 구성 그대로 전달됩니다.");
        }

        // SpawnManager에 파티 정보 업데이트
        SpawnManager.Instance.allyIDs = partyIDs;
        SpawnManager.Instance.allyPartyData = partyData;
        
        // 슬롯 기반 스킬ID 추출 및 전달
        string[] partySkillIDs = GetPartySkillIDsFromSlots();
        Debug.Log($"[Debug][UpdateSpawnManagerParty] 전달 skillIDs: {string.Join(",", partySkillIDs)}");
        SpawnManager.Instance.partySkillIDs = partySkillIDs;
        
        Debug.Log($"[SlotBased] SpawnManager에 전달: partySkillIDs = {string.Join(",", partySkillIDs)}");
    }

    /// <summary>
    /// 전투 전달용 캐릭터 데이터 정규화.
    /// 세이브/인벤토리 경로에서 누락될 수 있는 XML 원본 스케일을 ID 기준으로 보정한다.
    /// </summary>
    private CharacterData NormalizePartyCharacterForBattle(CharacterData source)
    {
        if (source == null)
            return null;

        CharacterData normalized = source.Clone();
        if (CharacterData.characterDict.TryGetValue(source.ID, out var canonical) && canonical != null)
        {
            normalized.Scale = canonical.Scale;
        }

        return normalized;
    }

    /// <summary>
    /// 현재 스킬 슬롯 UI에서 세팅된 스킬ID 배열을 추출 (단순화된 데이터 흐름)
    /// </summary>
    public string[] GetPartySkillIDsFromSlots()
    {
        Debug.Log($"[SlotBased] GetPartySkillIDsFromSlots 결과: {string.Join(",", playerSkillIDs)}");
        return (string[])playerSkillIDs.Clone();
    }

    /// <summary>
    /// 슬롯 기반 프리셋 저장: 현재 슬롯 상태를 GameProgressManager에 저장
    /// </summary>
    public void SaveSkillPreset()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[SlotBased] GameProgressManager를 찾을 수 없습니다.");
            return;
        }

        string[] currentSkillIDs = GetPartySkillIDsFromSlots();
        GameProgressManager.Instance.CurrentSaveData.savedSkillPreset = (string[])currentSkillIDs.Clone();
        GameProgressManager.Instance.SaveGameProgress(0); // 현재 슬롯 0으로 고정
        Debug.Log($"[SlotBased] 프리셋 저장 완료: {string.Join(",", currentSkillIDs)}");
    }

    /// <summary>
    /// 프리셋 로드: GameProgressManager에서 슬롯에 복원
    /// </summary>
    public void LoadSkillPreset()
    {
        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[SlotBased] GameProgressManager를 찾을 수 없습니다.");
            return;
        }

        string[] savedSkillIDs = GameProgressManager.Instance.CurrentSaveData.savedSkillPreset;
        if (savedSkillIDs != null && savedSkillIDs.Length == 4)
        {
            // 슬롯에 저장된 프리셋 복원
            RestoreSkillSlots(savedSkillIDs);
            Debug.Log($"[SlotBased] 프리셋 로드 완료: {string.Join(",", savedSkillIDs)}");
        }
        else
        {
            Debug.LogWarning("[SlotBased] 저장된 프리셋이 없거나 형식이 올바르지 않습니다.");
        }
    }

    /// <summary>
    /// 저장된 프리셋을 슬롯에 복원
    /// </summary>
    private void RestoreSkillSlots(string[] skillIDs)
    {
        if (skillSlotContainer == null) return;

        try
        {
            // 안전한 방식으로 SkillSlot 컴포넌트 찾기
            var allComponents = skillSlotContainer.GetComponentsInChildren<MonoBehaviour>();
            var skillSlots = new List<MonoBehaviour>();
            
            foreach (var component in allComponents)
            {
                if (component != null && component.GetType().Name == "SkillSlot")
                {
                    skillSlots.Add(component);
                }
            }
            
            for (int i = 0; i < skillSlots.Count && i < skillIDs.Length; i++)
            {
                var skillSlot = skillSlots[i];
                if (skillSlot != null)
                {
                    try
                    {
                        var setSkillMethod = skillSlot.GetType().GetMethod("SetSkill");
                        if (setSkillMethod != null)
                        {
                            setSkillMethod.Invoke(skillSlot, new object[] { skillIDs[i] });
                            Debug.Log($"[SlotBased] SkillSlot_{i}에 스킬 복원: {skillIDs[i]}");
                        }
                        else
                        {
                            Debug.LogWarning($"[SlotBased] SkillSlot_{i}에서 SetSkill 메서드를 찾을 수 없습니다.");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[SlotBased] SkillSlot_{i} 복원 중 오류: {e.Message}");
                    }
                }
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SlotBased] 프리셋 복원 중 오류 발생: {e.Message}");
        }
    }

    public List<CharacterBlock> GetCurrentPartyBlocks(bool debugLog = false)
    {
        List<CharacterBlock> partyBlocks = new List<CharacterBlock>();
        if (slot1 != null && slot1.currentCharacterBlock != null)
            partyBlocks.Add(slot1.currentCharacterBlock);
        if (slot2 != null && slot2.currentCharacterBlock != null)
            partyBlocks.Add(slot2.currentCharacterBlock);
        if (slot3 != null && slot3.currentCharacterBlock != null)
            partyBlocks.Add(slot3.currentCharacterBlock);
        if (slot4 != null && slot4.currentCharacterBlock != null)
            partyBlocks.Add(slot4.currentCharacterBlock);
        
        if (debugLog)
        {
            foreach (var block in partyBlocks)
                Debug.Log($"[SlotBased] GetCurrentPartyBlocks: {block.characterData.Label}");
        }
        return partyBlocks;
    }

    public bool IsPartyValid()
    {
        return GetCurrentPartyBlocks().Count > 0;
    }

    public void RemoveCharacterFromSlot(int slotNumber)
    {
        BattleSettingCharacterSlot targetSlot = null;
        
        switch (slotNumber)
        {
            case 1: targetSlot = slot1; break;
            case 2: targetSlot = slot2; break;
            case 3: targetSlot = slot3; break;
            case 4: targetSlot = slot4; break;
        }

        if (targetSlot != null)
        {
            targetSlot.RemoveCharacterBlock();
        }
    }

    public void ClearAllSlots()
    {
        if (slot1 != null) slot1.RemoveCharacterBlock();
        if (slot2 != null) slot2.RemoveCharacterBlock();
        if (slot3 != null) slot3.RemoveCharacterBlock();
        if (slot4 != null) slot4.RemoveCharacterBlock();
        
        Debug.Log("[SlotBased] 모든 슬롯에서 캐릭터 제거 완료");
    }

    [ContextMenu("Debug Party Info")]
    public void DebugPartyInfo()
    {
        Debug.Log("=== 현재 파티 정보 ===");
        var partyBlocks = GetCurrentPartyBlocks();
        for (int i = 0; i < partyBlocks.Count; i++)
        {
            var block = partyBlocks[i];
            Debug.Log($"슬롯 {i + 1}: {block.characterData?.Label} (ID: {block.characterData?.ID})");
        }
        Debug.Log($"파티 유효성: {IsPartyValid()}");
        
        // 슬롯 기반 스킬ID 디버그
        string[] skillIDs = GetPartySkillIDsFromSlots();
        Debug.Log($"현재 스킬 설정: {string.Join(",", skillIDs)}");
        Debug.Log("=====================");
    }

    /// <summary>
    /// 전투 시작 버튼 클릭 시 호출되는 메서드입니다.
    /// </summary>
    public void StartBattle()
    {
        Debug.Log("[SlotBased] StartBattle() 진입");
        
        // 파티 유효성 검사
        if (!IsPartyValid())
        {
            Debug.LogWarning("[SlotBased] 파티에 캐릭터가 없습니다. 전투를 시작할 수 없습니다.");
            return;
        }
        
        // 전투 진입 직전 최종 파티 정보 업데이트
        UpdateSpawnManagerParty();
        
        // 전투 진입 직전 allyPartyData 상태 출력
        Debug.Log($"[SlotBased] 전투 진입 직전 allyPartyData.Count: {SpawnManager.Instance.allyPartyData.Count}");
        for (int i = 0; i < SpawnManager.Instance.allyPartyData.Count; i++)
        {
            var data = SpawnManager.Instance.allyPartyData[i];
            Debug.Log($"[SlotBased] allyPartyData[{i}]: {data.Label} (ID: {data.ID})");
        }
        
        // 최종 스킬ID 확인
        Debug.Log($"[SlotBased] 전투 진입 직전 partySkillIDs: {string.Join(",", SpawnManager.Instance.partySkillIDs)}");
        
        // 씬 전환
        UnityEngine.SceneManagement.SceneManager.LoadScene("TestBattle");
    }

    // ===== 주인공 스킬 세팅 관련 메서드들 =====

    /// <summary>
    /// 1번 슬롯(주인공) 클릭 시 스킬 세팅 UI를 활성화합니다.
    /// </summary>
    public void OnSlot1Clicked()
    {
        if (skillSettingPanel != null)
        {
            skillSettingPanel.SetActive(true);
            RefreshSkillSlots();
            Debug.Log("[SlotBased] 주인공 스킬 세팅 UI 활성화");
        }
    }

    /// <summary>
    /// 스킬 세팅 UI를 닫습니다.
    /// </summary>
    public void CloseSkillSetting()
    {
        if (skillSettingPanel != null)
        {
            skillSettingPanel.SetActive(false);
            Debug.Log("[SlotBased] 주인공 스킬 세팅 UI 비활성화");
        }
    }

    /// <summary>
    /// 스킬 슬롯들을 새로고침합니다.
    /// </summary>
    private void RefreshSkillSlots()
    {
        if (skillSlotContainer == null) 
        {
            Debug.LogWarning("[SlotBased] skillSlotContainer가 null입니다.");
            return;
        }

        Debug.Log("[SlotBased] RefreshSkillSlots 시작");

        try
        {
            // 안전한 방식으로 SkillSlot 컴포넌트 찾기
            var allComponents = skillSlotContainer.GetComponentsInChildren<MonoBehaviour>();
            var skillSlots = new List<MonoBehaviour>();
            
            foreach (var component in allComponents)
            {
                if (component != null && component.GetType().Name == "SkillSlot")
                {
                    skillSlots.Add(component);
                }
            }
            
            Debug.Log($"[SlotBased] 발견된 SkillSlot 개수: {skillSlots.Count}");

            // 기존 슬롯들을 초기화 (기초스킬로 설정)
            for (int i = 0; i < skillSlots.Count && i < 4; i++)
            {
                var skillSlot = skillSlots[i];
                if (skillSlot != null)
                {
                    try
                    {
                        var initializeMethod = skillSlot.GetType().GetMethod("Initialize");
                        if (initializeMethod != null)
                        {
                            // 기초스킬로 초기화
                            string skillID = GetDefaultSkillForSlot(i);
                            initializeMethod.Invoke(skillSlot, new object[] { i, skillID });
                            
                            // playerSkillIDs 배열도 업데이트
                            if (i < playerSkillIDs.Length)
                            {
                                playerSkillIDs[i] = skillID;
                            }
                            
                            Debug.Log($"[SlotBased] SkillSlot_{i} 초기화 완료 (스킬: {skillID})");
                        }
                        else
                        {
                            Debug.LogWarning($"[SlotBased] SkillSlot_{i}에서 Initialize 메서드를 찾을 수 없습니다.");
                        }
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[SlotBased] SkillSlot_{i} 초기화 중 오류: {e.Message}");
                    }
                }
                else
                {
                    Debug.LogWarning($"[SlotBased] SkillSlot_{i}가 null입니다.");
                }
            }
            
            // SpawnManager에 업데이트된 스킬 정보 반영
            UpdateSpawnManagerParty();
            Debug.Log($"[SlotBased] 기초스킬 설정 완료: {string.Join(",", playerSkillIDs)}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SlotBased] 스킬 슬롯 초기화 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 특정 슬롯에 스킬을 설정합니다 (단순화된 데이터 흐름).
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스 (0-3)</param>
    /// <param name="skillID">스킬 ID</param>
    public void SetPlayerSkill(int slotIndex, string skillID)
    {
        if (slotIndex >= 0 && slotIndex < 4)
        {
            string resolvedSkillID = string.IsNullOrEmpty(skillID) ? GetDefaultSkillForSlot(slotIndex) : skillID;
            playerSkillIDs[slotIndex] = resolvedSkillID;
            Debug.Log($"[SlotBased] 주인공 스킬 슬롯 {slotIndex + 1}에 스킬 {skillID} 설정");
            Debug.Log($"[SlotBased] 현재 playerSkillIDs: {string.Join(",", playerSkillIDs)}");
            UpdateSpawnManagerParty(); // SpawnManager에 즉시 반영
        }
    }

    public string GetDefaultSkillForSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= DefaultSkillIDs.Length)
            return "";
        return DefaultSkillIDs[slotIndex];
    }
} 