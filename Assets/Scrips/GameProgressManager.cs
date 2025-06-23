using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [Header("디버그 옵션")]
    [SerializeField] private bool isTestMode = false; // true로 설정 시 테스트용 데이터를 로드합니다.

    [System.Serializable]
    public class GameProgressData
    {
        public string gameVersion;
        public string lastPlayedDate;
        public int totalPlayTime;
        public int totalClearCount;
        public List<CharacterData> characterInventory = new List<CharacterData>(); // 모든 캐릭터 데이터 관리
        public List<string> unlockedSkills = new List<string>();
        public Dictionary<string, int> itemInventory = new Dictionary<string, int>();
        public Dictionary<string, bool> achievements = new Dictionary<string, bool>();
        public List<string> currentParty = new List<string>(); // 현재 파티 구성
    }

    private const int MAX_SAVE_SLOTS = 3;
    private GameProgressData[] saveSlots = new GameProgressData[MAX_SAVE_SLOTS];
    private int currentSlot = 0;

    // 현재 활성화된 세이브 슬롯의 데이터에 접근하기 위한 프로퍼티
    public GameProgressData CurrentSaveData
    {
        get
        {
            if (saveSlots[currentSlot] == null)
            {
                LoadGameProgress(currentSlot); // 데이터가 없으면 로드 시도
            }
            return saveSlots[currentSlot];
        }
    }

    private string GetSavePath(int slot)
    {
        return $"{Application.persistentDataPath}/save_slot_{slot}.json";
    }

    private const string GAME_VERSION = "1.0.0";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        LoadAllSaveSlots();
    }

    /// <summary>
    /// GameManager가 모든 데이터 로딩을 마친 후 호출할 초기화 메서드.
    /// </summary>
    public void Initialize()
    {
        if (isTestMode)
        {
            SetupTestInventory();
        }
    }

    /// <summary>
    /// 테스트 모드가 활성화되었을 때, 인벤토리를 테스트용 데이터로 채웁니다.
    /// </summary>
    private void SetupTestInventory()
    {
        Debug.LogWarning("[Debug] 1. SetupTestInventory: 테스트 인벤토리 생성을 시작합니다.");
        
        // 현재 슬롯의 인벤토리 데이터를 가져오거나 새로 만듭니다.
        var inventory = CurrentSaveData.characterInventory;
        if (inventory == null)
        {
            inventory = new List<CharacterData>();
            CurrentSaveData.characterInventory = inventory;
        }
        inventory.Clear();

        // 테스트용 캐릭터 추가 (ID, 수량)
        AddCharacterToInventory("000001", 1);
        AddCharacterToInventory("000002", 2);
        AddCharacterToInventory("000005", 1);
        
        Debug.LogWarning($"[Debug] 2. SetupTestInventory: 인벤토리 생성이 완료되었습니다. 인벤토리에 있는 캐릭터 수: {CurrentSaveData.characterInventory.Count}개");
    }

    private void OnApplicationQuit()
    {
        SaveGameProgress(currentSlot);
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            SaveGameProgress(currentSlot);
        }
    }

    public void SetCurrentSlot(int slot)
    {
        if (slot >= 0 && slot < MAX_SAVE_SLOTS)
        {
            currentSlot = slot;
        }
    }

    public void SaveGameProgress(int slot)
    {
        try
        {
            if (saveSlots[slot] == null)
            {
                saveSlots[slot] = new GameProgressData();
            }

            var data = saveSlots[slot];
            data.gameVersion = GAME_VERSION;
            data.lastPlayedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            data.totalPlayTime = (int)(Time.time - Time.timeSinceLevelLoad);
            data.totalClearCount = StageManager.Instance.GetTotalClearCount();

            string json = JsonUtility.ToJson(data, true);
            File.WriteAllText(GetSavePath(slot), json);
            Debug.Log($"[GameProgressManager] 게임 진행 데이터 저장 완료: {GetSavePath(slot)}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameProgressManager] 저장 중 오류 발생: {e.Message}");
        }
    }

    public void LoadAllSaveSlots()
    {
        for (int i = 0; i < MAX_SAVE_SLOTS; i++)
        {
            LoadGameProgress(i);
        }
    }

    public void LoadGameProgress(int slot)
    {
        try
        {
            string path = GetSavePath(slot);
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                saveSlots[slot] = JsonUtility.FromJson<GameProgressData>(json);

                // 버전 체크 및 마이그레이션
                if (saveSlots[slot].gameVersion != GAME_VERSION)
                {
                    Debug.Log($"[GameProgressManager] 게임 버전 변경 감지: {saveSlots[slot].gameVersion} -> {GAME_VERSION}");
                    // TODO: 버전별 마이그레이션 로직 구현
                }

                Debug.Log($"[GameProgressManager] 게임 진행 데이터 로드 완료: {path}");
            }
            else
            {
                saveSlots[slot] = new GameProgressData
                {
                    gameVersion = GAME_VERSION,
                    lastPlayedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    totalPlayTime = 0,
                    totalClearCount = 0
                };
                Debug.Log($"[GameProgressManager] 새로운 게임 진행 데이터 생성 (슬롯 {slot})");
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameProgressManager] 로드 중 오류 발생: {e.Message}");
            saveSlots[slot] = new GameProgressData();
        }
    }

    public bool HasSaveData(int slot)
    {
        return File.Exists(GetSavePath(slot));
    }

    public string GetSaveSlotInfo(int slot)
    {
        if (saveSlots[slot] == null) return "빈 슬롯";
        return $"마지막 저장: {saveSlots[slot].lastPlayedDate}\n" +
               $"플레이 시간: {saveSlots[slot].totalPlayTime}초\n" +
               $"클리어 수: {saveSlots[slot].totalClearCount}";
    }

    // 캐릭터 해금 관련
    public void UnlockCharacter(string characterId)
    {
        if (!saveSlots[currentSlot].unlockedSkills.Contains(characterId))
        {
            saveSlots[currentSlot].unlockedSkills.Add(characterId);
            SaveGameProgress(currentSlot);
        }
    }

    public bool IsCharacterUnlocked(string characterId)
    {
        return saveSlots[currentSlot].unlockedSkills.Contains(characterId);
    }

    // 파티 구성 관련
    public void SavePartyConfiguration(List<string> partyMemberIds)
    {
        saveSlots[currentSlot].currentParty = new List<string>(partyMemberIds);
        SaveGameProgress(currentSlot);
    }

    public List<string> GetCurrentPartyConfiguration()
    {
        return new List<string>(saveSlots[currentSlot].currentParty);
    }

    // 스킬 해금 관련
    public void UnlockSkill(string skillId)
    {
        if (!saveSlots[currentSlot].unlockedSkills.Contains(skillId))
        {
            saveSlots[currentSlot].unlockedSkills.Add(skillId);
            SaveGameProgress(currentSlot);
        }
    }

    public bool IsSkillUnlocked(string skillId)
    {
        return saveSlots[currentSlot].unlockedSkills.Contains(skillId);
    }

    // 아이템 인벤토리 관련
    public void AddItem(string itemId, int count = 1)
    {
        if (saveSlots[currentSlot].itemInventory.ContainsKey(itemId))
        {
            saveSlots[currentSlot].itemInventory[itemId] += count;
        }
        else
        {
            saveSlots[currentSlot].itemInventory[itemId] = count;
        }
        SaveGameProgress(currentSlot);
    }

    public int GetItemCount(string itemId)
    {
        return saveSlots[currentSlot].itemInventory.ContainsKey(itemId) 
            ? saveSlots[currentSlot].itemInventory[itemId] 
            : 0;
    }

    // 업적 관련
    public void UnlockAchievement(string achievementId)
    {
        if (!saveSlots[currentSlot].achievements.ContainsKey(achievementId) || 
            !saveSlots[currentSlot].achievements[achievementId])
        {
            saveSlots[currentSlot].achievements[achievementId] = true;
            SaveGameProgress(currentSlot);
        }
    }

    public bool IsAchievementUnlocked(string achievementId)
    {
        return saveSlots[currentSlot].achievements.ContainsKey(achievementId) && 
               saveSlots[currentSlot].achievements[achievementId];
    }

    // 캐릭터 인벤토리 관련
    public void AddCharacterToInventory(string characterId, int count = 1)
    {
        if (CharacterData.characterDict.TryGetValue(characterId, out var originalData))
        {
            for (int i = 0; i < count; i++)
            {
                // 원본 데이터를 복제하여 고유한 인스턴스를 인벤토리에 추가
                CharacterData newCharacter = originalData.Clone();
                CurrentSaveData.characterInventory.Add(newCharacter);
            }
            SaveGameProgress(currentSlot);
        }
        else
        {
            Debug.LogError($"[GameProgressManager] ID '{characterId}'에 해당하는 캐릭터를 찾을 수 없습니다.");
        }
    }

    public CharacterData GetCharacterData(string characterId)
    {
        // 단순히 원본 데이터의 복제본만 반환 (스탯 조절 없음)
        if (CharacterData.characterDict.TryGetValue(characterId, out CharacterData originalData))
        {
            return originalData.Clone();
        }
        return null;
    }

    /// <summary>
    /// 특정 ID의 캐릭터 수를 반환합니다. IsCustomized가 false인 캐릭터만 카운트합니다.
    /// </summary>
    public int GetCharacterCount(string characterId)
    {
        return CurrentSaveData.characterInventory.FindAll(c => c.ID == characterId && !c.IsCustomized).Count;
    }

    /// <summary>
    /// 특정 ID의 캐릭터를 소유하고 있는지 확인합니다.
    /// </summary>
    public bool HasCharacter(string characterId)
    {
        return CurrentSaveData.characterInventory.Exists(c => c.ID == characterId);
    }

    /// <summary>
    /// 인벤토리에서 순정 상태의 캐릭터를 제거합니다.
    /// </summary>
    public void RemoveCharacterFromInventory(string characterId, int count = 1)
    {
        for (int i = 0; i < count; i++)
        {
            // 순정 상태(IsCustomized == false)인 캐릭터만 찾아서 제거
            CharacterData toRemove = CurrentSaveData.characterInventory.Find(c => c.ID == characterId && !c.IsCustomized);
            if (toRemove != null)
            {
                CurrentSaveData.characterInventory.Remove(toRemove);
            }
            else
            {
                Debug.LogWarning($"[GameProgressManager] 제거할 캐릭터(ID: {characterId}, 순정)가 인벤토리에 부족합니다.");
                break; // 제거할 캐릭터가 더 이상 없으면 중단
            }
        }
        SaveGameProgress(currentSlot);
    }

    public List<CharacterData> CharacterInventory
    {
        get { return CurrentSaveData.characterInventory; }
    }
} 