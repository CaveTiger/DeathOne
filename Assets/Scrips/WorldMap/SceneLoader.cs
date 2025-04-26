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
    public void LoadStage() //�������� ID�� �������� ������ ��ư�ʿ��� �־�����
    {
        SceneManager.LoadScene("TestStage"); // ���������� �̵��� ������ �� ���Ĵ� �� �������� Start���� ������ ������
    }
    public void ReturntoWorldMap() //��������� ���ư���, ������ Ŭ���� �� �ʿ��� �����ʹ� ���� �׾����״� �길 �Ŀ� ȣ���ؼ� ��������� ���� ��
    {
        SceneManager.LoadScene("SampleScene");
    }
}
