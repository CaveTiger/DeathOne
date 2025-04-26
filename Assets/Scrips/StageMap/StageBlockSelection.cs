using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StageBlockSelection : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;

    public Color hoverColor = new Color(1f, 1f, 0.6f);
    public Color clickColor = Color.red;

    public GameObject StageBlock;

    public string blockType;

    [Header("이 오브젝트에 대응하는 블록 ID")]
    public string blockID;

    [Header("이 전투 블록의 적 목록")]
    public List<string> enemyID = new List<string>();

    [Header("전투 컷신")]
    public string frontCutID;
    public string backCutID;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;

        //생성되고 알맞는 정보를 넣기
        if (string.IsNullOrEmpty(blockID)) return;
        //ID를 감지해 하위 데이터를 불러온다.
        if (StageManager.Instance.stageBlockDict.TryGetValue(blockID, out var data))
        {
            blockType = data.BlockType;
            enemyID = new List<string>(data.EnemyIDs);
            frontCutID = data.FrontCutID;
            backCutID = data.BackCutID;

            Debug.Log($"[{blockID}] 블록 데이터 적용 완료");
        }
        else
        {
            Debug.LogWarning($"[{blockID}] 에 해당하는 블록 데이터를 찾을 수 없습니다.");
        }
    }
    private void OnMouseEnter()
    {
        rend.material.color = hoverColor; //마우스가 위치에 들어감
    }
    private void OnMouseExit()
    {
        rend.material.color = originalColor; //마우스가 위치를 빠져나옴
    }

    void OnMouseDown()
    {
        rend.material.color = clickColor;
        Debug.Log($"스테이지블록 클릭됨: {blockID}");
        GetInstance().GetBlock(blockID);
    }

    private static StageManager GetInstance()
    {
        return StageManager.Instance;
    }

    private void OnMouseUp()
    {
        rend.material.color = hoverColor; // 클릭 후 다시 hover 상태
        Debug.Log($"스테이지블록 클릭 완수: {blockID}");

        if (blockID != null)
        {
            SpawnManager.Instance.enemyIDs = this.enemyID;
            SceneManager.LoadScene("TestBattle");
        }
        else
        {
            Debug.LogWarning("스테이지 ID가 들어오지 않음");
        }
    }
}
