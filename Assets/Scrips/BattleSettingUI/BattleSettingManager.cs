using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
    [SerializeField] private MonoBehaviour skillPresetHandlerRef; // SkillPresetHandler (리플렉션 접근)
    [Header("스킬 슬롯 직접 등록(권장)")]
    [SerializeField] private MonoBehaviour skillSlot1Ref;
    [SerializeField] private MonoBehaviour skillSlot2Ref;
    [SerializeField] private MonoBehaviour skillSlot3Ref;
    [SerializeField] private MonoBehaviour skillSlot4Ref;
    
    // 슬롯별 스킬ID를 필드로 관리한다. (배열 인덱스 매핑 이슈 회피)
    [SerializeField] private string slot1SkillID = "";
    [SerializeField] private string slot2SkillID = "";
    [SerializeField] private string slot3SkillID = "";
    [SerializeField] private string slot4SkillID = "";
    private bool isRestoringPartySlots = false;
    private bool isApplyingSkillPresetToUi = false;

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
        EnsureSkillSlotBindings();
    }

    private void OnDisable()
    {
        CommitSetupBeforeTransition();
        BattleSettingCharacterSlot.OnSlotChanged -= HandleSlotChanged;
    }

    private void Start()
    {
        EnsureSkillSlotBindings();
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

            // 마지막 세팅 복원 시도. 복원 실패 시에만 주인공 자동 배치로 폴백.
            bool restored = TryRestorePartySlotsFromSave();
            if (!restored)
            {
                PlaceMainCharacterBlockToSlot1();
            }
        }
        
        // 초기 기초스킬 설정
        InitializeDefaultSkills();
        TryRestoreSkillPresetFromSave();
        
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

    private string GetSkillIdAt(int slotIndex)
    {
        switch (slotIndex)
        {
            case 0: return slot1SkillID;
            case 1: return slot2SkillID;
            case 2: return slot3SkillID;
            case 3: return slot4SkillID;
            default: return "";
        }
    }

    private void SetSkillIdAt(int slotIndex, string skillId)
    {
        switch (slotIndex)
        {
            case 0: slot1SkillID = skillId; break;
            case 1: slot2SkillID = skillId; break;
            case 2: slot3SkillID = skillId; break;
            case 3: slot4SkillID = skillId; break;
        }
    }

    private string[] BuildSkillIdArray()
    {
        return new[]
        {
            string.IsNullOrEmpty(slot1SkillID) ? GetDefaultSkillForSlot(0) : slot1SkillID,
            string.IsNullOrEmpty(slot2SkillID) ? GetDefaultSkillForSlot(1) : slot2SkillID,
            string.IsNullOrEmpty(slot3SkillID) ? GetDefaultSkillForSlot(2) : slot3SkillID,
            string.IsNullOrEmpty(slot4SkillID) ? GetDefaultSkillForSlot(3) : slot4SkillID
        };
    }

    /// <summary>
    /// 초기 기초스킬 설정 (BaseCharacter.xml의 기본 스킬들)
    /// </summary>
    private void InitializeDefaultSkills()
    {
        SetSkillIdAt(0, GetDefaultSkillForSlot(0));
        SetSkillIdAt(1, GetDefaultSkillForSlot(1));
        SetSkillIdAt(2, GetDefaultSkillForSlot(2));
        SetSkillIdAt(3, GetDefaultSkillForSlot(3));
        
        Debug.Log($"[SlotBased] 초기 기초스킬 설정 완료: {string.Join(",", BuildSkillIdArray())}");
    }

    private void HandleSlotChanged(BattleSettingCharacterSlot slot, CharacterData data)
    {
        RefreshAllSlotVisuals();
        if (!isRestoringPartySlots)
        {
            CommitSetupBeforeTransition();
        }
        else
        {
            Debug.Log("[스킬세팅추적] 복원 중 슬롯 변경 이벤트 저장 스킵");
        }
        UpdateSpawnManagerParty();
    }

    private void RefreshAllSlotVisuals()
    {
        slot1?.ForceSyncVisualFromCurrentBlock();
        slot2?.ForceSyncVisualFromCurrentBlock();
        slot3?.ForceSyncVisualFromCurrentBlock();
        slot4?.ForceSyncVisualFromCurrentBlock();
    }

    private void SavePartySlotsToCurrentSlot()
    {
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentSaveData == null)
            return;

        var data = GameProgressManager.Instance.CurrentSaveData;
        if (data.savedPartySlots == null || data.savedPartySlots.Length != 4)
            data.savedPartySlots = new string[4] { "", "", "", "" };

        data.savedPartySlots[0] = slot1?.GetCharacterData()?.ID ?? "";
        data.savedPartySlots[1] = slot2?.GetCharacterData()?.ID ?? "";
        data.savedPartySlots[2] = slot3?.GetCharacterData()?.ID ?? "";
        data.savedPartySlots[3] = slot4?.GetCharacterData()?.ID ?? "";

        if (data.currentParty == null) data.currentParty = new List<string>();
        data.currentParty.Clear();
        for (int i = 0; i < 4; i++)
        {
            if (!string.IsNullOrEmpty(data.savedPartySlots[i]))
                data.currentParty.Add(data.savedPartySlots[i]);
        }


    }

    private bool TryRestorePartySlotsFromSave()
    {
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentSaveData == null)
            return false;
        if (CharacterInventoryTab.Instance == null)
            return false;

        var data = GameProgressManager.Instance.CurrentSaveData;
        // 복원 중 이벤트 저장이 savedPartySlots를 덮어써도 영향 없도록 로컬 복사본을 사용.
        var saved = data.savedPartySlots != null ? (string[])data.savedPartySlots.Clone() : null;
        if (saved == null || saved.Length != 4)
            return false;
        if (!saved.Any(id => !string.IsNullOrEmpty(id)))
            return false;

        var slots = new[] { slot1, slot2, slot3, slot4 };
        if (slots.Any(s => s == null))
            return false;

        isRestoringPartySlots = true;
        try
        {
            // 이전 배치가 남아 있어도 동일 로직으로 덮어쓴다.
            for (int i = 0; i < 4; i++)
            {
                string id = saved[i];
                if (string.IsNullOrEmpty(id))
                    continue;

                var block = CharacterInventoryTab.Instance.GetCharacterBlockByID(id);
                if (block == null)
                {
                    Debug.LogWarning($"[BattleSetting] 저장된 슬롯 복원 실패: 캐릭터 블록 없음 id={id}, slot={i + 1}");
                    continue;
                }

                CharacterInventoryTab.Instance.RemoveBlockFromList(block);
                slots[i].PlaceCharacterBlock(block);
            }
        }
        finally
        {
            isRestoringPartySlots = false;
        }

        RefreshAllSlotVisuals();
        return true;
    }

    private void TryRestoreSkillPresetFromSave()
    {
        string[] saved = null;
        saved = TryReadRecentPresetFromPresetManager();
        if (saved == null && GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentSaveData != null)
        {
            saved = GameProgressManager.Instance.CurrentSaveData.savedSkillPreset;
        }

        Debug.Log($"[스킬세팅추적] TryRestoreSkillPreset raw={((saved != null) ? string.Join(",", saved) : "null")}");
        if (saved == null || saved.Length != 4) return;

        for (int i = 0; i < 4; i++)
        {
            SetSkillIdAt(i, string.IsNullOrEmpty(saved[i]) ? GetDefaultSkillForSlot(i) : saved[i]);
        }
        NormalizeUniquePlayerSkills();
        ApplyPlayerSkillIdsToUiSlots();
        RefreshSkillInventoryVisualState();
        Debug.Log($"[스킬세팅추적] TryRestoreSkillPreset 적용 playerSkillIDs={string.Join(",", BuildSkillIdArray())}");
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
        SpawnManager.Instance.SetPartySkillIDs(partySkillIDs);
        
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
        SyncPlayerSkillsFromUiSlots();
        var result = BuildSkillIdArray();
        Debug.Log($"[SlotBased] GetPartySkillIDsFromSlots 결과: {string.Join(",", result)}");
        return result;
    }

    /// <summary>
    /// 슬롯 기반 프리셋 저장: 현재 슬롯 상태를 GameProgressManager에 저장
    /// </summary>
    public void SaveSkillPreset()
    {
        string[] currentSkillIDs = GetPartySkillIDsFromSlots();

        if (!TryWriteRecentPresetToPresetManager(currentSkillIDs) && GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentSaveData != null)
        {
            GameProgressManager.Instance.CurrentSaveData.savedSkillPreset = (string[])currentSkillIDs.Clone();
        }

        if (GameProgressManager.Instance == null)
        {
            Debug.LogWarning("[SlotBased] GameProgressManager를 찾을 수 없습니다.");
            return;
        }

        GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
        Debug.Log($"[SlotBased] 프리셋 저장 완료: {string.Join(",", currentSkillIDs)}");
    }

    /// <summary>
    /// 프리셋 로드: GameProgressManager에서 슬롯에 복원
    /// </summary>
    public void LoadSkillPreset()
    {
        string[] savedSkillIDs = null;
        savedSkillIDs = TryReadRecentPresetFromPresetManager();
        if (savedSkillIDs == null && GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentSaveData != null)
            savedSkillIDs = GameProgressManager.Instance.CurrentSaveData.savedSkillPreset;

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
        try
        {
            var skillSlots = GetOrderedSkillSlots();

            for (int i = 0; i < 4 && i < skillIDs.Length; i++)
            {
                if (i >= skillSlots.Count) continue;
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
        Debug.Log("[스킬세팅추적] StartBattle 전환 직전 저장 시도");
        
        // 파티 유효성 검사
        if (!IsPartyValid())
        {
            Debug.LogWarning("[SlotBased] 파티에 캐릭터가 없습니다. 전투를 시작할 수 없습니다.");
            return;
        }
        
        // 전투 진입 직전 최종 파티 정보 업데이트
        CommitSetupBeforeTransition();
        UpdateSpawnManagerParty();
        
        // 전투 진입 직전 allyPartyData 상태 출력
        Debug.Log($"[SlotBased] 전투 진입 직전 allyPartyData.Count: {SpawnManager.Instance.allyPartyData.Count}");
        for (int i = 0; i < SpawnManager.Instance.allyPartyData.Count; i++)
        {
            var data = SpawnManager.Instance.allyPartyData[i];
            Debug.Log($"[SlotBased] allyPartyData[{i}]: {data.Label} (ID: {data.ID})");
        }
        
        // 최종 스킬ID 확인
        Debug.Log($"[SlotBased] 전투 진입 직전 partySkillIDs: {string.Join(",", SpawnManager.Instance.GetPartySkillIDs())}");
        
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
            // UI 오브젝트(실제 SkillBlock) 배치 기준으로 슬롯을 구성한다.
            ApplyPlayerSkillIdsToUiSlots();
            SyncPlayerSkillsFromUiSlots();

            // SpawnManager에 업데이트된 스킬 정보 반영
            UpdateSpawnManagerParty();
            RefreshSkillInventoryVisualState();
            Debug.Log($"[SlotBased] 스킬 슬롯 UI 동기화 완료: {string.Join(",", BuildSkillIdArray())}");
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[SlotBased] 스킬 슬롯 UI 동기화 중 오류 발생: {e.Message}");
        }
    }

    /// <summary>
    /// 특정 슬롯에 스킬을 설정합니다 (단순화된 데이터 흐름).
    /// </summary>
    /// <param name="slotIndex">슬롯 인덱스 (0-3)</param>
    /// <param name="skillID">스킬 ID</param>
    public void SetPlayerSkill(int slotIndex, string skillID)
    {
        if (isApplyingSkillPresetToUi)
            return;

        if (slotIndex >= 0 && slotIndex < 4)
        {
            string resolvedSkillID = string.IsNullOrEmpty(skillID) ? GetDefaultSkillForSlot(slotIndex) : skillID;
            SetSkillIdAt(slotIndex, resolvedSkillID);
            NormalizeUniquePlayerSkills();
            Debug.Log($"[스킬세팅추적] SetPlayerSkill slot={slotIndex + 1} input={skillID} resolved={resolvedSkillID}");
            Debug.Log($"[스킬세팅추적] 현재 playerSkillIDs={string.Join(",", BuildSkillIdArray())}");

            CommitSetupBeforeTransition();
            RefreshSkillInventoryVisualState();

            UpdateSpawnManagerParty(); // SpawnManager에 즉시 반영
        }
    }

    /// <summary>
    /// 현재 파티 슬롯 + 스킬 프리셋을 같은 슬롯 세이브에 즉시 반영한다.
    /// </summary>
    /// <summary>
    /// 화면 전환 직전에 호출해 현재 파티/스킬 세팅을 우선 저장한다.
    /// </summary>
    public bool CommitSetupBeforeTransition()
    {
        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentSaveData == null)
        {
            Debug.LogWarning("[스킬세팅추적] Commit 중단: GameProgressManager 또는 CurrentSaveData null");
            return false;
        }

        // 저장 직전에는 UI가 진실(source of truth)이다.
        SyncPlayerSkillsFromUiSlots();
        SavePartySlotsToCurrentSlot();
        NormalizeUniquePlayerSkills();

        string[] normalizedSkills = new string[4];
        for (int i = 0; i < 4; i++)
        {
            string v = GetSkillIdAt(i);
            normalizedSkills[i] = string.IsNullOrEmpty(v) ? GetDefaultSkillForSlot(i) : v;
        }

        var data = GameProgressManager.Instance.CurrentSaveData;
        if (TryWriteRecentPresetToPresetManager(normalizedSkills))
        {
            normalizedSkills = TryNormalizeSkillIdsWithPresetManager(normalizedSkills);
        }
        else
        {
            data.savedSkillPreset = (string[])normalizedSkills.Clone();
        }

        GameProgressManager.Instance.SaveGameProgress(GameProgressManager.Instance.CurrentSlot);
        Debug.Log($"[스킬세팅추적] Commit 완료 slot={GameProgressManager.Instance.CurrentSlot} skills={string.Join(",", normalizedSkills)}");
        Debug.Log($"[BattleSetting] 전환 직전 세팅 저장 완료 (slot={GameProgressManager.Instance.CurrentSlot})");
        return true;
    }

    /// <summary>
    /// 파티 스킬 슬롯에서 중복 스킬 ID를 제거한다. 중복 발견 슬롯은 해당 슬롯 기본기로 치환.
    /// </summary>
    private void NormalizeUniquePlayerSkills()
    {
        HashSet<string> seen = new HashSet<string>();
        for (int i = 0; i < 4; i++)
        {
            string current = GetSkillIdAt(i);
            string id = string.IsNullOrEmpty(current) ? GetDefaultSkillForSlot(i) : current;
            if (seen.Contains(id))
            {
                string fallback = GetDefaultSkillForSlot(i);
                SetSkillIdAt(i, fallback);
                Debug.Log($"[스킬세팅추적] 중복 스킬 제거 slot={i + 1} duplicated={id} -> fallback={fallback}");
                seen.Add(fallback);
            }
            else
            {
                SetSkillIdAt(i, id);
                seen.Add(id);
            }
        }
    }

    /// <summary>
    /// 현재 스킬 슬롯 UI에서 스킬ID를 읽어 playerSkillIDs에 동기화한다.
    /// </summary>
    private void SyncPlayerSkillsFromUiSlots()
    {
        if (skillSlotContainer == null)
            return;

        var skillSlots = GetOrderedSkillSlots();

        for (int i = 0; i < 4; i++)
        {
            string fallback = GetDefaultSkillForSlot(i);
            string resolved = fallback;

            if (i < skillSlots.Count && skillSlots[i] != null)
            {
                // 1순위: SkillSlot이 자체 보관하는 현재 ID를 사용한다(블록 참조 유무와 무관).
                var getSkillIdMethod = skillSlots[i].GetType().GetMethod("GetSkillID");
                if (getSkillIdMethod != null)
                {
                    var idObj = getSkillIdMethod.Invoke(skillSlots[i], null);
                    string slotSkillId = idObj as string;
                    if (!string.IsNullOrEmpty(slotSkillId))
                    {
                        resolved = slotSkillId;
                    }
                }

                // 2순위(폴백): 블록 참조에서 ID를 역추출.
                if (resolved == fallback)
                {
                    var currentSkillBlockField = skillSlots[i].GetType().GetField("currentSkillBlock");
                    var currentSkillBlock = currentSkillBlockField != null ? currentSkillBlockField.GetValue(skillSlots[i]) : null;
                    if (currentSkillBlock != null)
                    {
                        var skillDataField = currentSkillBlock.GetType().GetField("skillData");
                        var skillDataObj = skillDataField != null ? skillDataField.GetValue(currentSkillBlock) : null;
                        if (skillDataObj != null)
                        {
                            var idProp = skillDataObj.GetType().GetProperty("ID");
                            string rawId = idProp != null ? idProp.GetValue(skillDataObj) as string : "";
                            if (!string.IsNullOrEmpty(rawId))
                                resolved = rawId;
                        }
                    }
                }
            }

            SetSkillIdAt(i, resolved);
        }
    }

    /// <summary>
    /// playerSkillIDs를 스킬 슬롯 UI에 강제로 반영한다.
    /// </summary>
    private void ApplyPlayerSkillIdsToUiSlots()
    {
        if (skillSlotContainer == null)
            return;

        var skillSlots = GetOrderedSkillSlots();

        MonoBehaviour inventoryTab = null;
        var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb != null && mb.GetType().Name == "SkillInventoryTab")
            {
                inventoryTab = mb;
                break;
            }
        }
        isApplyingSkillPresetToUi = true;
        try
        {
            // 기존 슬롯 블록은 모두 반환하여 UI 기준 상태를 초기화.
            for (int i = 0; i < 4 && i < skillSlots.Count; i++)
            {
                if (skillSlots[i] == null) continue;
                var removeMethod = skillSlots[i].GetType().GetMethod("RemoveSkillBlock");
                if (removeMethod != null)
                    removeMethod.Invoke(skillSlots[i], null);
            }

            for (int i = 0; i < 4 && i < skillSlots.Count; i++)
            {
                if (skillSlots[i] == null) continue;
                string id = string.IsNullOrEmpty(GetSkillIdAt(i)) ? GetDefaultSkillForSlot(i) : GetSkillIdAt(i);
                object block = null;
                if (inventoryTab != null)
                {
                    var getBlockMethod = inventoryTab.GetType().GetMethod("GetSkillBlockByID");
                    if (getBlockMethod != null)
                        block = getBlockMethod.Invoke(inventoryTab, new object[] { id });
                }

                // 원하는 블록이 인벤토리에 없으면 슬롯 기본기 블록으로 폴백
                if (block == null && inventoryTab != null)
                {
                    string fallback = GetDefaultSkillForSlot(i);
                    var getBlockMethod = inventoryTab.GetType().GetMethod("GetSkillBlockByID");
                    if (getBlockMethod != null)
                        block = getBlockMethod.Invoke(inventoryTab, new object[] { fallback });
                    id = fallback;
                }

                if (block != null)
                {
                    var placeMethod = skillSlots[i].GetType().GetMethod("PlaceSkillBlock");
                    if (placeMethod != null)
                        placeMethod.Invoke(skillSlots[i], new object[] { block });
                }
                else
                {
                    var initializeMethod = skillSlots[i].GetType().GetMethod("Initialize");
                    if (initializeMethod != null)
                        initializeMethod.Invoke(skillSlots[i], new object[] { i, id });
                }
            }
        }
        finally
        {
            isApplyingSkillPresetToUi = false;
        }
    }

    /// <summary>
    /// 스킬 슬롯을 고정 순서(1~4)로 수집한다.
    /// 인스펙터 직결 슬롯을 우선 사용하고, 누락 시에만 기존 탐색 로직으로 폴백한다.
    /// </summary>
    private List<MonoBehaviour> GetOrderedSkillSlots()
    {
        EnsureSkillSlotBindings();

        // 최우선: SkillPresetHandler의 skillSlots(인스펙터 순서 고정)를 그대로 사용.
        if (skillPresetHandlerRef == null || skillPresetHandlerRef.GetType().Name != "SkillPresetHandler")
            skillPresetHandlerRef = FindSkillPresetHandler();
        if (TryGetSkillSlotsFromPresetHandler(skillPresetHandlerRef, out var presetSlots))
        {
            return presetSlots;
        }

        List<MonoBehaviour> slots = new List<MonoBehaviour>();
        MonoBehaviour[] arr = new MonoBehaviour[4];
        MonoBehaviour[] directRefs = { skillSlot1Ref, skillSlot2Ref, skillSlot3Ref, skillSlot4Ref };

        for (int i = 0; i < 4; i++)
        {
            var mb = directRefs[i];
            if (mb != null && mb.GetType().Name == "SkillSlot")
                arr[i] = mb;
        }

        bool hasMissing = false;
        for (int i = 0; i < 4; i++)
        {
            if (arr[i] == null)
            {
                hasMissing = true;
                break;
            }
        }

        if (hasMissing && skillSlotContainer != null)
        {
            MonoBehaviour[] all = skillSlotContainer.GetComponentsInChildren<MonoBehaviour>(true);

            foreach (var mb in all)
            {
                if (mb == null || mb.GetType().Name != "SkillSlot")
                    continue;

                int idx = -1;
                var prop = mb.GetType().GetProperty("SlotIndex");
                if (prop != null)
                {
                    object v = prop.GetValue(mb);
                    if (v is int pidx) idx = pidx;
                }

                if (idx < 0 || idx >= 4)
                {
                    string n = mb.gameObject.name;
                    string digits = new string(n.Where(char.IsDigit).ToArray());
                    if (int.TryParse(digits, out int nameNum))
                        idx = nameNum - 1;
                }

                if (idx >= 0 && idx < 4 && arr[idx] == null)
                    arr[idx] = mb;
            }

            // 마지막 폴백: 이름 직탐색
            for (int i = 0; i < 4; i++)
            {
                if (arr[i] != null) continue;
                string slotName = $"SkillSlot{i + 1}";
                Transform tr = skillSlotContainer.Find(slotName);
                if (tr == null) continue;
                var inChildren = tr.GetComponentsInChildren<MonoBehaviour>(true);
                foreach (var mb in inChildren)
                {
                    if (mb != null && mb.GetType().Name == "SkillSlot")
                    {
                        arr[i] = mb;
                        break;
                    }
                }
            }
        }

        for (int i = 0; i < 4; i++)
            slots.Add(arr[i]);
        return slots;
    }

    private void EnsureSkillSlotBindings()
    {
        if (skillPresetHandlerRef == null || skillPresetHandlerRef.GetType().Name != "SkillPresetHandler")
            skillPresetHandlerRef = FindSkillPresetHandler();
        if (TryGetSkillSlotsFromPresetHandler(skillPresetHandlerRef, out _))
        {
            // SkillPresetHandler가 유효하면 자동 재바인딩으로 덮어쓰지 않는다.
            return;
        }

        if (skillSlotContainer == null)
            return;

        MonoBehaviour[] refs = { skillSlot1Ref, skillSlot2Ref, skillSlot3Ref, skillSlot4Ref };
        bool needsRebind = false;
        Scene activeScene = SceneManager.GetActiveScene();

        for (int i = 0; i < refs.Length; i++)
        {
            var mb = refs[i];
            if (mb == null || mb.GetType().Name != "SkillSlot" || mb.gameObject.scene != activeScene)
            {
                needsRebind = true;
                break;
            }
        }

        if (!needsRebind)
            return;

        MonoBehaviour[] found = new MonoBehaviour[4];
        var all = skillSlotContainer.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var mb in all)
        {
            if (mb == null || mb.GetType().Name != "SkillSlot")
                continue;

            int idx = -1;
            var prop = mb.GetType().GetProperty("SlotIndex");
            if (prop != null)
            {
                object v = prop.GetValue(mb);
                if (v is int pidx) idx = pidx;
            }

            if (idx < 0 || idx >= 4)
            {
                string n = mb.gameObject.name;
                string digits = new string(n.Where(char.IsDigit).ToArray());
                if (int.TryParse(digits, out int nameNum))
                    idx = nameNum - 1;
            }

            if (idx >= 0 && idx < 4 && found[idx] == null)
                found[idx] = mb;
        }

        skillSlot1Ref = found[0];
        skillSlot2Ref = found[1];
        skillSlot3Ref = found[2];
        skillSlot4Ref = found[3];
    }

    private MonoBehaviour FindSkillPresetHandler()
    {
        var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb != null && mb.GetType().Name == "SkillPresetHandler")
                return mb;
        }
        return null;
    }

    private bool TryGetSkillSlotsFromPresetHandler(MonoBehaviour presetHandler, out List<MonoBehaviour> orderedSlots)
    {
        orderedSlots = null;
        if (presetHandler == null || presetHandler.GetType().Name != "SkillPresetHandler")
            return false;

        var field = presetHandler.GetType().GetField("skillSlots");
        if (field == null)
            return false;

        var value = field.GetValue(presetHandler);
        if (!(value is System.Collections.IEnumerable enumerable))
            return false;

        List<MonoBehaviour> slots = new List<MonoBehaviour>();
        foreach (var item in enumerable)
        {
            if (item is MonoBehaviour mb && mb != null && mb.GetType().Name == "SkillSlot")
                slots.Add(mb);
            if (slots.Count >= 4)
                break;
        }

        if (slots.Count < 4)
            return false;

        orderedSlots = slots;
        return true;
    }

    private MonoBehaviour FindPresetManagerReflective()
    {
        var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb != null && mb.GetType().Name == "PresetManager")
                return mb;
        }
        return null;
    }

    private bool TryWriteRecentPresetToPresetManager(string[] skillIDs)
    {
        var presetManager = FindPresetManagerReflective();
        if (presetManager == null) return false;

        var method = presetManager.GetType().GetMethod("WriteRecentPresetToCurrentSave");
        if (method == null) return false;

        method.Invoke(presetManager, new object[] { skillIDs });
        return true;
    }

    private string[] TryReadRecentPresetFromPresetManager()
    {
        var presetManager = FindPresetManagerReflective();
        if (presetManager == null) return null;

        var method = presetManager.GetType().GetMethod("ReadRecentPresetFromCurrentSaveOrDefault");
        if (method == null) return null;

        return method.Invoke(presetManager, null) as string[];
    }

    private string[] TryNormalizeSkillIdsWithPresetManager(string[] skillIDs)
    {
        var presetManager = FindPresetManagerReflective();
        if (presetManager == null) return skillIDs;

        var method = presetManager.GetType().GetMethod("NormalizeSkillIDs");
        if (method == null) return skillIDs;

        return method.Invoke(presetManager, new object[] { skillIDs }) as string[] ?? skillIDs;
    }

    private void RefreshSkillInventoryVisualState()
    {
        var all = FindObjectsByType<MonoBehaviour>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        foreach (var mb in all)
        {
            if (mb == null) continue;
            if (mb.GetType().Name != "SkillInventoryTab") continue;
            var method = mb.GetType().GetMethod("RefreshUsedSkillsFromParty");
            if (method != null)
            {
                method.Invoke(mb, null);
            }
            break;
        }
    }

    public string GetDefaultSkillForSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= DefaultSkillIDs.Length)
            return "";
        return DefaultSkillIDs[slotIndex];
    }
} 