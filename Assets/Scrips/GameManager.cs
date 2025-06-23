using UnityEditor.SceneManagement;
using UnityEngine;

// 이 스크립트는 모든 데이터 로더보다 먼저 실행되어야 합니다.
// 하지만 다른 매니저(예: GameProgressManager)의 Awake()보다는 나중에 실행될 수 있습니다.
// 따라서 실행 순서는 GameManger -> 다른 데이터 로더 -> GameProgressManager 순으로 제어합니다.
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public bool IsPaused => Time.timeScale == 0f;

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
        
        // 2. 기본 데이터 로딩이 끝난 후, 이 데이터를 사용하는 다른 매니저를 초기화합니다.
        GameProgressManager.Instance.Initialize();
    }

    public void PauseGame()
    {
        Time.timeScale = 0f;
        // 필요시: AudioListener.pause = true;
        Debug.Log("게임 일시정지");
    }

    public void ResumeGame()
    {
        Time.timeScale = 1f;
        // 필요시: AudioListener.pause = false;
        Debug.Log("게임 재개");
    }

    private void Start()
    {
        
    }
}
