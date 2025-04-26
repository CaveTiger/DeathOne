using UnityEditor.SceneManagement;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }


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
    private void Start()
    {

        
    }
}
