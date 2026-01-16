using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Added for .Select()
using System.Reflection;

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
        public int essence = 0; // 강자의 정수 (현재 보유량)
        public int totalEssence = 0; // 강자의 정수 총 획득량 (획득 누적, 환불 시에는 증가하지 않음)
        
        // 축복 시스템 필드 추가 (라인별 비트마스크 방식)
        // 각 라인(0~4)별로 비트마스크 문자열로 저장
        // 예: ["10010", "01000", "000", "101", "0"] = 라인 0에 1번과 4번, 라인 1에 2번, 라인 3에 1번과 3번 축복 선택됨
        public string[] activeBlessingByLine = new string[5] { "", "", "", "", "" };
        
        // 전술 축복(라인 3)의 선택된 대상 저장
        // Dictionary는 JsonUtility에서 직렬화가 안 되므로, 리스트로 변환하여 저장
        // 형식: ["축복ID:슬롯번호", "축복ID:슬롯번호", ...]
        // 예: ["030001:2", "030002:3"] = "하나를 위한 모두" 축복의 대상이 2번 슬롯, "모두를 위한 하나" 축복의 대상이 3번 슬롯
        // 슬롯 번호: 1(주인공), 2, 3, 4
        public List<string> tacticalBlessingTargets = new List<string>();
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

            // BlessingPanel에서 활성화된 축복 정보를 라인별로 저장 (비트마스크 방식 - 확장형)
            // BlessingPanel이 프리팹으로 관리되므로 리플렉션을 사용하여 동적으로 찾기
            // 저장 포인트 #2: GameProgressManager.SaveGameProgress()에서 BlessingPanel의 activeBlessingByLine 가져오기
            // [수정] BlessingPanel이 없거나 전투 씬인 경우 기존 저장된 축복 정보를 유지 (덮어쓰지 않음)
            
            // 전투 씬인지 확인 (BattleManager가 존재하면 전투 씬)
            bool isBattleScene = false;
            try
            {
                Type battleManagerType = Type.GetType("BattleManager");
                if (battleManagerType != null)
                {
                    var instanceProperty = battleManagerType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static);
                    if (instanceProperty != null)
                    {
                        var battleManagerInstance = instanceProperty.GetValue(null);
                        isBattleScene = (battleManagerInstance != null);
                    }
                }
            }
            catch { }
            
            if (isBattleScene)
            {
                // 전투 씬에서는 축복 정보를 업데이트하지 않고 기존 데이터 유지
                Debug.Log($"[GameProgressManager] 저장 포인트 #2: 전투 씬 감지 - 축복 정보 업데이트 건너뜀 (기존 데이터 유지)");
                
                // activeBlessingByLine이 null이거나 길이가 맞지 않으면 초기화만 수행
                if (data.activeBlessingByLine == null || data.activeBlessingByLine.Length != 5)
                {
                    data.activeBlessingByLine = new string[5] { "", "", "", "", "" };
                }
            }
            else
            {
                // 월드맵/스테이지 씬에서만 BlessingPanel에서 축복 정보 가져오기
                Debug.Log($"[GameProgressManager] 저장 포인트 #2: SaveGameProgress() 호출됨 (슬롯 {slot})");
                
                // 리플렉션을 사용하여 BlessingPanel 타입 찾기 (컴파일 순서 문제 회피)
                Type blessingPanelType = Type.GetType("BlessingPanel");
                UnityEngine.Object[] allObjects = FindObjectsByType(typeof(MonoBehaviour), FindObjectsSortMode.None);
                MonoBehaviour blessingPanel = null;
                
                foreach (var obj in allObjects)
                {
                    if (obj != null && obj.GetType() == blessingPanelType)
                    {
                        blessingPanel = obj as MonoBehaviour;
                        break;
                    }
                }
                
                if (blessingPanel != null)
                {
                    // 리플렉션을 사용하여 GetActiveBlessingByLine 메서드 호출
                    MethodInfo getActiveBlessingMethod = blessingPanelType.GetMethod("GetActiveBlessingByLine");
                    string[] panelActiveBlessings = null;
                    
                    if (getActiveBlessingMethod != null)
                    {
                        panelActiveBlessings = getActiveBlessingMethod.Invoke(blessingPanel, null) as string[];
                    }
                    
                    Debug.Log($"[GameProgressManager] BlessingPanel에서 activeBlessingByLine 가져옴: {panelActiveBlessings != null}, 길이: {panelActiveBlessings?.Length ?? 0}");
                    
                    if (panelActiveBlessings != null && panelActiveBlessings.Length == 5)
                    {
                        // 각 라인의 비트마스크 문자열을 복사하여 저장
                        data.activeBlessingByLine = new string[5];
                        
                        for (int i = 0; i < 5; i++)
                        {
                            data.activeBlessingByLine[i] = panelActiveBlessings[i] ?? "";
                        }
                        
                        // 저장된 축복 정보 상세 로그
                        string blessingList = "";
                        int totalCount = 0;
                        for (int i = 0; i < 5; i++)
                        {
                            if (!string.IsNullOrEmpty(data.activeBlessingByLine[i]))
                            {
                                // 비트마스크에서 1의 개수 세기
                                int count = 0;
                                foreach (char c in data.activeBlessingByLine[i])
                                {
                                    if (c == '1') count++;
                                }
                                if (count > 0)
                                {
                                    blessingList += $"라인{i}:\"{data.activeBlessingByLine[i]}\"({count}개) ";
                                    totalCount += count;
                                }
                            }
                        }
                        Debug.Log($"[GameProgressManager] 축복 저장 (라인별 비트마스크): 총 {totalCount}개 - {blessingList}");
                    }
                    else
                    {
                        // GetActiveBlessingByLine()이 null이거나 길이가 맞지 않으면 기존 데이터 유지
                        if (data.activeBlessingByLine == null || data.activeBlessingByLine.Length != 5)
                        {
                            data.activeBlessingByLine = new string[5] { "", "", "", "", "" };
                        }
                        Debug.LogWarning("[GameProgressManager] BlessingPanel.GetActiveBlessingByLine()가 null이거나 길이가 5가 아닙니다. 기존 데이터를 유지합니다.");
                    }
                }
                else
                {
                    // BlessingPanel이 없으면 기존 저장된 축복 정보 유지 (덮어쓰지 않음)
                    if (data.activeBlessingByLine == null || data.activeBlessingByLine.Length != 5)
                    {
                        data.activeBlessingByLine = new string[5] { "", "", "", "", "" };
                    }
                    Debug.LogWarning("[GameProgressManager] ⚠️ BlessingPanel을 찾을 수 없습니다. 기존 축복 정보를 유지합니다.");
                }
            }

            // 전술 축복 대상 정보 저장 (BlessingManager에서 가져와서 리스트로 변환)
            if (BlessingManager.Instance != null)
            {
                var tacticalTargets = BlessingManager.Instance.GetAllTacticalBlessingTargets();
                data.tacticalBlessingTargets.Clear();
                foreach (var entry in tacticalTargets)
                {
                    // "축복ID:슬롯번호" 형식으로 저장
                    data.tacticalBlessingTargets.Add($"{entry.Key}:{entry.Value}");
                }
                Debug.Log($"[GameProgressManager] 전술 축복 대상 정보 저장: {data.tacticalBlessingTargets.Count}개");
            }
            else
            {
                // BlessingManager가 없으면 기존 데이터 유지
                if (data.tacticalBlessingTargets == null)
                {
                    data.tacticalBlessingTargets = new List<string>();
                }
                Debug.LogWarning("[GameProgressManager] BlessingManager를 찾을 수 없습니다. 기존 전술 축복 대상 정보를 유지합니다.");
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

                // activeBlessingByLine 배열 초기화 (null이거나 길이가 5가 아니면)
                if (saveSlots[slot].activeBlessingByLine == null || saveSlots[slot].activeBlessingByLine.Length != 5)
                {
                    saveSlots[slot].activeBlessingByLine = new string[5] { "", "", "", "", "" };
                }
                
                // 각 라인의 문자열이 null이면 빈 문자열로 초기화
                for (int i = 0; i < 5; i++)
                {
                    if (saveSlots[slot].activeBlessingByLine[i] == null)
                    {
                        saveSlots[slot].activeBlessingByLine[i] = "";
                    }
                }

                // 총량 필드(totalEssence)가 이전 버전 세이브에서 비어 있을 수 있으므로 보정
                // 규칙: totalEssence = 현재 보유 정수 + 활성화된 축복 수 합 (비트마스크에서 1의 개수)
                if (saveSlots[slot].totalEssence <= 0)
                {
                    int activeBlessingCount = 0;
                    if (saveSlots[slot].activeBlessingByLine != null)
                    {
                        for (int i = 0; i < saveSlots[slot].activeBlessingByLine.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(saveSlots[slot].activeBlessingByLine[i]))
                            {
                                // 비트마스크 문자열에서 1의 개수 세기
                                foreach (char c in saveSlots[slot].activeBlessingByLine[i])
                                {
                                    if (c == '1') activeBlessingCount++;
                                }
                            }
                        }
                    }

                    saveSlots[slot].totalEssence = saveSlots[slot].essence + activeBlessingCount;
                    Debug.Log($"[GameProgressManager] totalEssence 보정 완료: {saveSlots[slot].totalEssence} (essence: {saveSlots[slot].essence}, activeBlessings: {activeBlessingCount})");
                }

                // BlessingPanel에 활성화된 축복 정보 설정 (라인별 비트마스크 방식)
                // BlessingPanel은 OnEnable에서 자동으로 로드하므로 여기서는 로그만 남김
                if (saveSlots[slot].activeBlessingByLine != null)
                {
                    int blessingCount = 0;
                    for (int i = 0; i < saveSlots[slot].activeBlessingByLine.Length; i++)
                    {
                        if (!string.IsNullOrEmpty(saveSlots[slot].activeBlessingByLine[i]))
                        {
                            // 비트마스크 문자열에서 1의 개수 세기
                            foreach (char c in saveSlots[slot].activeBlessingByLine[i])
                            {
                                if (c == '1') blessingCount++;
                            }
                        }
                    }
                    
                    if (blessingCount > 0)
                    {
                        Debug.Log($"[GameProgressManager] 축복 로드 예정: {blessingCount}개 (BlessingPanel의 OnEnable에서 자동 로드됨)");
                    }
                    else
                    {
                        Debug.Log("[GameProgressManager] 저장된 축복 정보가 없습니다.");
                    }
                }
                else
                {
                    Debug.Log("[GameProgressManager] activeBlessingByLine이 null입니다.");
                }

                // 전술 축복 대상 정보 로드 (리스트를 Dictionary로 변환하여 BlessingManager에 설정)
                if (saveSlots[slot].tacticalBlessingTargets != null && saveSlots[slot].tacticalBlessingTargets.Count > 0)
                {
                    Dictionary<string, int> tacticalTargets = new Dictionary<string, int>();
                    foreach (var entry in saveSlots[slot].tacticalBlessingTargets)
                    {
                        // "축복ID:슬롯번호" 형식에서 분리
                        string[] parts = entry.Split(':');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int slotNumber))
                        {
                            // 슬롯 번호 유효성 검사 (1~4)
                            if (slotNumber >= 1 && slotNumber <= 4)
                            {
                                tacticalTargets[parts[0]] = slotNumber;
                            }
                            else
                            {
                                Debug.LogWarning($"[GameProgressManager] 유효하지 않은 슬롯 번호: {slotNumber} (1~4 범위여야 함)");
                            }
                        }
                    }

                    if (BlessingManager.Instance != null)
                    {
                        BlessingManager.Instance.SetTacticalBlessingTargets(tacticalTargets);
                        Debug.Log($"[GameProgressManager] 전술 축복 대상 정보 로드 완료: {tacticalTargets.Count}개");
                    }
                    else
                    {
                        Debug.LogWarning("[GameProgressManager] BlessingManager를 찾을 수 없습니다. 전술 축복 대상 정보는 나중에 로드됩니다.");
                    }
                }
                else
                {
                    // tacticalBlessingTargets가 null이면 초기화
                    if (saveSlots[slot].tacticalBlessingTargets == null)
                    {
                        saveSlots[slot].tacticalBlessingTargets = new List<string>();
                    }
                    Debug.Log("[GameProgressManager] 저장된 전술 축복 대상 정보가 없습니다.");
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

    /// <summary>
    /// 강자의 정수를 추가합니다.
    /// increaseTotal이 true이면 총량(totalEssence)에도 반영되고,
    /// false이면 현재 보유량(essence)에만 반영됩니다. (예: 축복 비활성화 환불 등)
    /// </summary>
    public void AddEssence(int amount, bool increaseTotal = true)
    {
        if (amount == 0) return;

        if (increaseTotal)
        {
            CurrentSaveData.totalEssence += amount;
        }

        CurrentSaveData.essence += amount;
        SaveGameProgress(currentSlot);
        Debug.Log($"[GameProgressManager] 강자의 정수 추가: +{amount} (현재: {CurrentSaveData.essence}, 총량: {CurrentSaveData.totalEssence}, increaseTotal: {increaseTotal})");
    }

    /// <summary>
    /// 현재 보유 강자의 정수를 반환합니다.
    /// </summary>
    public int GetEssence()
    {
        int currentAmount = CurrentSaveData.essence;
        Debug.Log($"[강자의 정수] 현재 보유량: {currentAmount}");
        return currentAmount;
    }

    /// <summary>
    /// 지금까지 획득한 강자의 정수 총량을 반환합니다.
    /// </summary>
    public int GetTotalEssence()
    {
        return CurrentSaveData.totalEssence;
    }

    /// <summary>
    /// 강자의 정수를 소모합니다. (총량은 감소하지 않음)
    /// </summary>
    public bool SpendEssence(int amount)
    {
        if (CurrentSaveData.essence >= amount)
        {
            CurrentSaveData.essence -= amount;
            SaveGameProgress(currentSlot);
            Debug.Log($"[GameProgressManager] 강자의 정수 소모: -{amount} (남은 {CurrentSaveData.essence}, 총량: {CurrentSaveData.totalEssence})");
            return true;
        }
        Debug.LogWarning($"[GameProgressManager] 강자의 정수 부족: 필요 {amount}, 보유 {CurrentSaveData.essence}");
        return false;
    }

    /// <summary>
    /// [테스트용] 강자의 정수를 직접 설정합니다.
    /// </summary>
    /// <param name="amount">설정할 정수 개수</param>
    public void SetEssenceForTest(int amount)
    {
        CurrentSaveData.essence = amount;
        // 총량도 최소한 현재 보유량만큼은 되도록 설정
        if (CurrentSaveData.totalEssence < amount)
        {
            CurrentSaveData.totalEssence = amount;
        }
        SaveGameProgress(currentSlot);
        Debug.Log($"[GameProgressManager] [테스트] 강자의 정수 설정: {amount} (총량: {CurrentSaveData.totalEssence})");
    }
} 

public class CharacterBlockData
{
    public string characterID;
    public string[] skillIDs;
} 