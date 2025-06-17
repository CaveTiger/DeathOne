using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }

    [System.Serializable]
    public class GameProgressData
    {
        public string gameVersion;
        public string lastPlayedDate;
        public int totalPlayTime;
        public int totalClearCount;
        public List<string> unlockedCharacters = new List<string>();
        public List<string> unlockedSkills = new List<string>();
        public Dictionary<string, int> itemInventory = new Dictionary<string, int>();
        public Dictionary<string, bool> achievements = new Dictionary<string, bool>();
    }

    private const int MAX_SAVE_SLOTS = 3;
    private GameProgressData[] saveSlots = new GameProgressData[MAX_SAVE_SLOTS];
    private int currentSlot = 0;

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
} 