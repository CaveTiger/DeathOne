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
        // 파티 세팅 화면에서 나가는 경우를 대비해 전환 직전 저장을 우선 수행한다.
        if (BattleSettingManager.Instance != null)
        {
            bool committed = BattleSettingManager.Instance.CommitSetupBeforeTransition();
            Debug.Log($"[스킬세팅추적] ReturntoWorldMap CommitSetupBeforeTransition result={committed}");
        }
        else
        {
            Debug.LogWarning("[스킬세팅추적] ReturntoWorldMap 시점 BattleSettingManager.Instance == null");
        }

        if (GameManager.Instance != null)
        {
            GameManager.Instance.SetCurrentScreenStateByValue((int)GameManager.ScreenState.WorldMap);
        }
        Debug.Log("[스킬세팅추적] ReturntoWorldMap -> SampleScene 로드");
        SceneManager.LoadScene("SampleScene");
    }
}
