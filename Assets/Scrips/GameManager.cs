using UnityEditor.SceneManagement;
using UnityEngine;

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

        CharacterLoader.Instance.LoadAllCharacters();
        StageLoader.Instance.Initialize();
        SkillLoader.Instance.Initialize();
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
