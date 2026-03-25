using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    // Called by the stage start button: enters the party setup scene.
    public void LoadStage()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentScreenStateByValue((int)GameManager.ScreenState.PartySetting);
        }
        SceneManager.LoadScene("TestStage"); 
    }

    // Called by the return button: goes back to the world map scene.
    public void ReturntoWorldMap() 
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentScreenStateByValue((int)GameManager.ScreenState.WorldMap);
        }
        SceneManager.LoadScene("SampleScene");
    }
}
