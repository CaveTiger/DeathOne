using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneLoader : MonoBehaviour
{
    public static SceneLoader Instance;
    public string StageToLoad;
    public StageData StageData;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void LoadStage() //스테이지 ID는 스테이지 아이콘 버튼쪽에서 넣었으니
    {
        SceneManager.LoadScene("TestStage"); // 스테이지로 이동만 시켜줌 그 이후는 그 스테이지 Start에서 세팅을 시작함
    }
    public void ReturntoWorldMap() //월드맵으로 돌아가기, 스테이 클리어 후 필요한 데이터는 따로 쌓아줄테니 얘만 후에 호출해서 월드맵으로 가면 됨
    {
        SceneManager.LoadScene("SampleScene");
    }
}
