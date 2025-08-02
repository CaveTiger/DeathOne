using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Added for .Select()

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
        public List<string> unlockedSkills = new List<string>(); // 해금된 스킬 ID 목록
        public List<string> unlockedCharacters = new List<string>(); // 해금된 캐릭터 ID 목록
        public Dictionary<string, int> itemInventory = new Dictionary<string, int>();
        public Dictionary<string, bool> achievements = new Dictionary<string, bool>();
        public List<string> currentParty = new List<string>(); // 현재 파티 구성
        
        // 슬롯 기반 프리셋 저장용
        public string[] savedSkillPreset = new string[4] { "", "", "", "" };
        
        // 재화 필드 추가
        public int soulDust = 0; // 영혼먼지
        public int essence = 0; // 강자의 정수
    }

    private List<CharacterBlockData> partyData; // 캐릭터ID, skillIDs 등 구조체/클래스

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

    public int CurrentSlot
    {
        get { return currentSlot; }
    }

    private string GetSavePath(int slot)
    {
        return $"{Application.persistentDataPath}/save_slot_{slot}.json";
    }

    private const string GAME_VERSION = "1.0.0";

    private void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else Destroy(gameObject);
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
        // 현재 슬롯의 인벤토리 데이터를 가져오거나 새로 만듭니다.
        var inventory = CurrentSaveData.characterInventory;
        if (inventory == null)
        {
            inventory = new List<CharacterData>();
            CurrentSaveData.characterInventory = inventory;
        }
        
        // 기존 인벤토리를 비우지 않고, 테스트용 캐릭터가 없으면 추가
        var existingIDs = inventory.Select(c => c.ID).ToList();
        
        // 테스트용 캐릭터 추가 (ID, 수량) - 중복 방지
        if (!existingIDs.Contains("000001")) AddCharacterToInventory("000001", 1);
        if (!existingIDs.Contains("000002")) AddCharacterToInventory("000002", 1);
        if (!existingIDs.Contains("000005")) AddCharacterToInventory("000005", 1);
        
        // 테스트용 스킬 해금 (중복 방지)
        // 기본 스킬들
        UnlockSkill("010001"); // 단검베기
        UnlockSkill("010002"); // 발목 노리기
        UnlockSkill("010003"); // 여신의 축복
        UnlockSkill("010004"); // 주변 살피기
        
        // 테스트용 스킬들 (999xxx 시리즈)
        UnlockSkill("999001"); // 능동형 아이콘 테스트
        UnlockSkill("999002"); // 전체 회복
        UnlockSkill("999003"); // 전체 공격
        UnlockSkill("999004"); // 인접 공격
        UnlockSkill("999005"); // 인접 회복
        UnlockSkill("999006"); // 랜덤 공격
        UnlockSkill("999007"); // 약점 공격
        UnlockSkill("999008"); // 응급 치료
        UnlockSkill("999009"); // 호환성 테스트
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
            
            // StageManager가 null인 경우 처리
            if (StageManager.Instance != null)
            {
                data.totalClearCount = StageManager.Instance.GetTotalClearCount();
            }
            else
            {
                data.totalClearCount = 0; // 기본값 설정
                Debug.LogWarning("[GameProgressManager] StageManager.Instance가 null입니다. totalClearCount를 0으로 설정합니다.");
            }

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
        if (!saveSlots[currentSlot].unlockedCharacters.Contains(characterId))
        {
            saveSlots[currentSlot].unlockedCharacters.Add(characterId);
            SaveGameProgress(currentSlot);
        }
    }

    public bool IsCharacterUnlocked(string characterId)
    {
        return saveSlots[currentSlot].unlockedCharacters.Contains(characterId);
    }

    // 파티 구성 관련
    public void SavePartyData(List<CharacterBlock> blocks)
    {
        partyData = blocks.Select(b => new CharacterBlockData {
            characterID = b.characterData.ID,
            skillIDs = (string[])b.skillIDs.Clone()
        }).ToList();
    }

    public List<CharacterBlockData> GetPartyData()
    {
        return partyData;
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
                newCharacter.IsUnlocked = true; // 인벤토리에 들어오는 순간 해금 처리
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

    // 영혼먼지 관련 메서드
    public void AddSoulDust(int amount)
    {
        int beforeAmount = CurrentSaveData.soulDust;
        CurrentSaveData.soulDust += amount;
        SaveGameProgress(currentSlot);
        Debug.Log($"[영혼먼지] 추가: +{amount} (이전: {beforeAmount} → 현재: {CurrentSaveData.soulDust})");
    }

    public int GetSoulDust()
    {
        int currentAmount = CurrentSaveData.soulDust;
        Debug.Log($"[영혼먼지] 현재 보유량: {currentAmount}");
        return currentAmount;
    }

    public bool SpendSoulDust(int amount)
    {
        int beforeAmount = CurrentSaveData.soulDust;
        if (CurrentSaveData.soulDust >= amount)
        {
            CurrentSaveData.soulDust -= amount;
            SaveGameProgress(currentSlot);
            Debug.Log($"[영혼먼지] 소모: -{amount} (이전: {beforeAmount} → 현재: {CurrentSaveData.soulDust})");
            return true;
        }
        Debug.LogWarning($"[영혼먼지] 부족: 필요 {amount}, 보유 {CurrentSaveData.soulDust}");
        return false;
    }

    /// <summary>
    /// 영혼먼지 상태를 상세히 출력하는 디버그 메서드
    /// </summary>
    public void DebugSoulDustStatus()
    {
        Debug.Log($"[영혼먼지][상태] 현재 보유량: {CurrentSaveData.soulDust}");
        Debug.Log($"[영혼먼지][상태] 현재 슬롯: {currentSlot}");
        Debug.Log($"[영혼먼지][상태] 세이브 데이터 존재: {CurrentSaveData != null}");
        if (CurrentSaveData != null)
        {
            Debug.Log($"[영혼먼지][상태] 세이브 데이터 영혼먼지: {CurrentSaveData.soulDust}");
        }
    }

    // 강자의 정수 관련 메서드
    public void AddEssence(int amount)
    {
        CurrentSaveData.essence += amount;
        SaveGameProgress(currentSlot);
        Debug.Log($"[GameProgressManager] 강자의 정수 추가: +{amount} (총 {CurrentSaveData.essence})");
    }

    public int GetEssence()
    {
        return CurrentSaveData.essence;
    }

    public bool SpendEssence(int amount)
    {
        if (CurrentSaveData.essence >= amount)
        {
            CurrentSaveData.essence -= amount;
            SaveGameProgress(currentSlot);
            Debug.Log($"[GameProgressManager] 강자의 정수 소모: -{amount} (남은 {CurrentSaveData.essence})");
            return true;
        }
        Debug.LogWarning($"[GameProgressManager] 강자의 정수 부족: 필요 {amount}, 보유 {CurrentSaveData.essence}");
        return false;
    }
} 

public class CharacterBlockData
{
    public string characterID;
    public string[] skillIDs;
} 