using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq; // Added for .Select()
using System.Reflection;

public class GameProgressManager : MonoBehaviour
{
    public static GameProgressManager Instance { get; private set; }
    private const string MAIN_CHARACTER_ID = "000001";

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
        // 파티 세팅 화면의 슬롯 배치(1~4)를 그대로 저장. 비어있는 슬롯은 빈 문자열.
        public string[] savedPartySlots = new string[4] { "", "", "", "" };
        
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

        // 업그레이드 패널에서 마지막으로 선택한 캐릭터 ID
        public string lastSelectedUpgradeCharacterId = "";
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
        return Path.Combine(GetSlotDirectoryPath(slot), "game_progress.json");
    }

    private string GetLegacySavePath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"save_slot_{slot}.json");
    }

    private string GetSlotDirectoryPath(int slot)
    {
        return Path.Combine(Application.persistentDataPath, $"slot_{slot}");
    }

    private void EnsureSlotDirectory(int slot)
    {
        string slotDirectory = GetSlotDirectoryPath(slot);
        if (!Directory.Exists(slotDirectory))
        {
            Directory.CreateDirectory(slotDirectory);
        }
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
        // 모든 데이터(CharacterData 포함) 로딩 완료 이후 주인공 보장을 최종 적용한다.
        EnsureMainCharacterForSlot(currentSlot, saveImmediately: true);

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
        // 보상 검증 시에는 임시 스킬 자동 해금을 사용하지 않음.
        // 필요한 테스트 스킬은 수동으로만 해금한다.
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
            // 저장 방침:
            // - 게임 진행/스테이지 진행은 항상 같은 슬롯 컨텍스트를 공유한다.
            // - 슬롯 전환 시 StageManager도 즉시 같은 슬롯 데이터를 로드한다.
            // - 유저 설정(UserSettings)은 슬롯과 분리된 별도 저장소로 유지한다.
            currentSlot = slot;
            // 슬롯 전환 시 스테이지 진행도도 같은 슬롯 파일을 로드한다.
            if (StageManager.Instance != null)
            {
                StageManager.Instance.LoadStageProgress();
            }
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
            EnsureSlotDirectory(slot);
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
            string legacyPath = GetLegacySavePath(slot);
            bool loadedFromLegacy = false;

            if (!File.Exists(path) && File.Exists(legacyPath))
            {
                path = legacyPath;
                loadedFromLegacy = true;
            }

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

                if (saveSlots[slot].savedSkillPreset == null || saveSlots[slot].savedSkillPreset.Length != 4)
                {
                    saveSlots[slot].savedSkillPreset = new string[4] { "", "", "", "" };
                }
                if (saveSlots[slot].savedPartySlots == null || saveSlots[slot].savedPartySlots.Length != 4)
                {
                    saveSlots[slot].savedPartySlots = new string[4] { "", "", "", "" };
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

                // 구 경로 파일을 읽은 경우 새 슬롯 폴더 경로로 1회 마이그레이션 저장
                if (loadedFromLegacy)
                {
                    EnsureSlotDirectory(slot);
                    File.WriteAllText(GetSavePath(slot), json);
                    Debug.Log($"[GameProgressManager] 구 경로 세이브를 슬롯 폴더 경로로 마이그레이션 완료: {GetSavePath(slot)}");
                }
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

            // 로드 직후 1차 보정: 주인공 해금 상태는 즉시 보장한다.
            // (인벤토리 주입은 CharacterData가 아직 없으면 Initialize()에서 2차 보정)
            bool changed = EnsureMainCharacterForSlot(slot, saveImmediately: false);
            if (changed)
            {
                SaveGameProgress(slot);
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameProgressManager] 로드 중 오류 발생: {e.Message}");
            saveSlots[slot] = new GameProgressData();
        }
    }

    private bool EnsureMainCharacterForSlot(int slot, bool saveImmediately)
    {
        if (slot < 0 || slot >= MAX_SAVE_SLOTS)
            return false;

        if (saveSlots[slot] == null)
            saveSlots[slot] = new GameProgressData();

        var data = saveSlots[slot];
        bool changed = false;

        if (data.unlockedCharacters == null)
        {
            data.unlockedCharacters = new List<string>();
            changed = true;
        }

        if (data.characterInventory == null)
        {
            data.characterInventory = new List<CharacterData>();
            changed = true;
        }

        if (!data.unlockedCharacters.Contains(MAIN_CHARACTER_ID))
        {
            data.unlockedCharacters.Add(MAIN_CHARACTER_ID);
            changed = true;
        }

        bool hasMainCharacterInInventory = data.characterInventory.Exists(c => c != null && c.ID == MAIN_CHARACTER_ID);
        if (!hasMainCharacterInInventory)
        {
            if (CharacterData.characterDict != null &&
                CharacterData.characterDict.TryGetValue(MAIN_CHARACTER_ID, out var baseCharacter))
            {
                CharacterData mainCharacter = baseCharacter.Clone();
                mainCharacter.IsUnlocked = true;
                data.characterInventory.Add(mainCharacter);
                changed = true;
            }
            else
            {
                Debug.LogWarning($"[GameProgressManager] 주인공 데이터({MAIN_CHARACTER_ID})를 아직 찾지 못해 인벤토리 보정은 보류합니다.");
            }
        }

        if (changed && saveImmediately)
        {
            SaveGameProgress(slot);
        }

        return changed;
    }

    public bool HasSaveData(int slot)
    {
        return File.Exists(GetSavePath(slot)) || File.Exists(GetLegacySavePath(slot));
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

    /// <summary>
    /// 캐릭터의 업그레이드 보너스를 강제로 초기화합니다. (환불 없음)
    /// </summary>
    public static void ResetCharacterUpgradeBonuses(CharacterData character)
    {
        if (character == null) return;

        character.upgradeHpBonus = 0;
        character.upgradeMaxHpBonus = 0;
        character.upgradeAtkBonus = 0;
        character.upgradeDefBonus = 0;
        character.upgradeSpeedBonus = 0;
        character.upgradeEvasionBonus = 0f;
        character.upgradeAccuracyBonus = 0f;
        character.totalSoulDustSpent = 0;
        character.Hp = character.MaxHp;
    }

    /// <summary>
    /// 업그레이드 패널의 마지막 선택 캐릭터 ID를 저장합니다.
    /// </summary>
    public void SetLastSelectedUpgradeCharacterId(string characterId, bool saveImmediately = true)
    {
        if (CurrentSaveData == null) return;
        CurrentSaveData.lastSelectedUpgradeCharacterId = characterId ?? "";
        if (saveImmediately)
            SaveGameProgress(currentSlot);
    }

    /// <summary>
    /// 업그레이드 패널의 마지막 선택 캐릭터 ID를 가져옵니다.
    /// </summary>
    public string GetLastSelectedUpgradeCharacterId()
    {
        if (CurrentSaveData == null) return "";
        return CurrentSaveData.lastSelectedUpgradeCharacterId ?? "";
    }

    /// <summary>
    /// 캐릭터를 사용불가로 전환할 때 호출:
    /// - IsUnlocked를 false로 설정
    /// - 업그레이드 보너스를 강제 초기화(환불 없음)
    /// - 마지막 업그레이드 선택 대상이면 선택 ID를 비움
    /// </summary>
    public void MarkCharacterUnavailable(string characterId, bool saveImmediately = true)
    {
        if (string.IsNullOrEmpty(characterId) || CurrentSaveData == null) return;

        var target = CurrentSaveData.characterInventory.Find(c => c != null && c.ID == characterId);
        if (target == null) return;

        // 강제 초기화 시 영혼먼지 환불 (투자한 양을 다른 곳에 재투자 가능하게 하려는 목적)
        int refundAmount = target.totalSoulDustSpent;

        target.IsUnlocked = false;
        ResetCharacterUpgradeBonuses(target);

        if (CurrentSaveData.lastSelectedUpgradeCharacterId == characterId)
            CurrentSaveData.lastSelectedUpgradeCharacterId = "";

        if (refundAmount > 0)
        {
            CurrentSaveData.soulDust += refundAmount;
        }

        if (saveImmediately)
            SaveGameProgress(currentSlot);
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
    /// 특정 캐릭터에 영혼먼지를 투자하려고 시도합니다.
    /// - 등급별 최대 투자 한도(CharacterData.GetRemainingSoulDustCapacity)를 먼저 검사합니다.
    /// - 한도 내라면 실제 보유 영혼먼지(SpendSoulDust)로 결제합니다.
    /// - 둘 다 통과하면 CharacterData.totalSoulDustSpent를 증가시킵니다.
    /// </summary>
    /// <param name="character">투자 대상 캐릭터</param>
    /// <param name="amount">투자할 영혼먼지 양</param>
    /// <returns>투자 성공 여부</returns>
    public bool TryInvestSoulDust(CharacterData character, int amount)
    {
        if (character == null)
        {
            Debug.LogWarning("[영혼먼지] TryInvestSoulDust 호출 시 character가 null입니다.");
            return false;
        }

        if (amount <= 0)
        {
            Debug.LogWarning($"[영혼먼지] TryInvestSoulDust amount가 0 이하입니다: {amount}");
            return false;
        }

        int remainingCapacity = character.GetRemainingSoulDustCapacity();
        if (amount > remainingCapacity)
        {
            Debug.LogWarning($"[영혼먼지] 등급 한도 초과: 시도 {amount}, 남은 한도 {remainingCapacity}, 등급 {character.Rarity}");
            return false;
        }

        // 실제 보유 영혼먼지로 결제
        if (!SpendSoulDust(amount))
        {
            // SpendSoulDust 내부에서 부족 로그 출력
            return false;
        }

        // 투자량 누적
        character.totalSoulDustSpent += amount;
        return true;
    }

    /// <summary>
    /// 스탯 다운 등으로 투자를 되돌릴 때: 보유 영혼먼지를 돌려주고 totalSoulDustSpent를 감소시킵니다.
    /// 기록보다 많이 환급 요청되면 기록분까지만 차감합니다.
    /// </summary>
    public void RefundSoulDustInvestment(CharacterData character, int amount)
    {
        if (character == null)
        {
            Debug.LogWarning("[영혼먼지] RefundSoulDustInvestment: character가 null입니다.");
            return;
        }

        if (amount <= 0) return;

        int refund = Mathf.Min(amount, character.totalSoulDustSpent);
        if (refund <= 0) return;

        character.totalSoulDustSpent -= refund;
        AddSoulDust(refund);
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