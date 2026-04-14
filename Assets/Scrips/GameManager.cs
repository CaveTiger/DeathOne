using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections.Generic;
// RewardManager 사용

// 이 스크립트는 모든 데이터 로더보다 먼저 실행되어야 합니다.
// 하지만 다른 매니저(예: GameProgressManager)의 Awake()보다는 나중에 실행될 수 있습니다.
// 따라서 실행 순서는 GameManger -> 다른 데이터 로더 -> GameProgressManager 순으로 제어합니다.
public class GameManager : MonoBehaviour
{
    public enum ScreenState
    {
        None = 0,
        WorldMap = 1,
        StageSelected = 2,
        CharacterUpgrade = 3,
        PartySetting = 4,
        Blessing = 5,
        Battle = 6,
        Pause = 7
    }

    public static GameManager Instance { get; private set; }

    public bool IsPaused => Time.timeScale == 0f;
    public ScreenState CurrentScreenState => currentScreenState;

    [Header("현재 화면 상태")]
    [SerializeField] private ScreenState currentScreenState = ScreenState.None;

    private void Awake()
    {
        Debug.Log("GameManager Awake");

        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("GameManager 중복! 삭제됨");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        // 1. 모든 기본 데이터를 먼저 로드합니다.
        CharacterLoader.Instance.LoadAllCharacters();
        StageLoader.Instance.Initialize();
        SkillLoader.Instance.Initialize();
        if (PassiveLoader.Instance != null)
            PassiveLoader.Instance.Initialize();
        else
            Debug.LogWarning("[GameManager] PassiveLoader.Instance가 없어 패시브 로더 초기화를 건너뜁니다.");
        
        // 2. 기본 데이터 로딩이 끝난 후, 이 데이터를 사용하는 다른 매니저를 초기화합니다.
        GameProgressManager.Instance.Initialize();

        // 유저 공용 설정(사운드 등) 초기 적용
        if (UserSettingsManager.Instance != null)
            UserSettingsManager.Instance.ApplySettingsToSoundManager();
        else
            Debug.LogWarning("[GameManager] UserSettingsManager.Instance가 없어 유저 설정 적용을 건너뜁니다.");
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += HandleSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= HandleSceneLoaded;
    }

    private void HandleSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "SampleScene")
        {
            SetCurrentScreenState(ScreenState.WorldMap);
            
            // 월드맵 복귀 = 체인 전투 끊김 → 턴 되돌리기 + 전투 종료 이어하기 스냅샷 모두 폐기
            // 스냅샷은 CharacterStats 수치만 다루며 VirtualMouse/UI와 무관함
            if (BattleSnapshotManager.Instance != null)
                BattleSnapshotManager.Instance.ClearAllSnapshots();

            if (VirtualMouse.Instance != null)
                VirtualMouse.Instance.DismissAllHoverUi();

            // DDOL VirtualMouseCanvas는 씬 재로드 시 Start가 다시 호출되지 않을 수 있음 → 스킬 인포 등 가시성 자가 복구
            if (VirtualMouseCanvas.Instance != null)
                VirtualMouseCanvas.Instance.EnsureVisibleAfterWorldMapLoad();
        }
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetPaused(true);
        Debug.Log("게임 일시정지");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        if (SoundManager.Instance != null)
            SoundManager.Instance.SetPaused(false);
        Debug.Log("게임 재개");
    }

    private void Start()
    {
        Debug.Log("[GameManager] Start() 실행됨");
        SetCurrentScreenState(ScreenState.WorldMap);
        
        // 보상 처리는 TurnManager에서 씬 전환 전에 완료됨
        // GameManager에서는 추가 보상 처리가 필요하지 않음
        Debug.Log("[GameManager] 보상 처리는 TurnManager에서 이미 완료됨");
    }

    /// <summary>
    /// 현재 화면 상태를 변경합니다.
    /// </summary>
    public void SetCurrentScreenState(ScreenState state)
    {
        currentScreenState = state;
    }

    /// <summary>
    /// 인스펙터/버튼 이벤트용: 정수값으로 화면 상태를 변경합니다.
    /// enum 값이 유효하지 않으면 변경하지 않습니다.
    /// </summary>
    public void SetCurrentScreenStateByValue(int stateValue)
    {
        if (!System.Enum.IsDefined(typeof(ScreenState), stateValue))
        {
            Debug.LogWarning($"[GameManager] 유효하지 않은 ScreenState 값: {stateValue}");
            return;
        }

        currentScreenState = (ScreenState)stateValue;
    }

    /// <summary>
    /// 현재 화면 상태가 특정 값인지 확인합니다.
    /// </summary>
    public bool IsCurrentScreenState(ScreenState state)
    {
        return currentScreenState == state;
    }
}
