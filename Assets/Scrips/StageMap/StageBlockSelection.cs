using System.Collections.Generic;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StageBlockSelection : MonoBehaviour
{
    private Renderer rend;
    private Color originalColor;
    private bool isInteractable = true;

    public Color hoverColor = new Color(1f, 1f, 0.6f);
    public Color clickColor = Color.red;

    public GameObject StageBlock;

    public string blockType;

    [Header("이 오브젝트에 대응하는 블록 ID")]
    public string blockID;

    [Header("이 블록에 배치될 적 리스트")]
    public List<string> enemyID = new List<string>();

    [Header("컷신 앞/뒤 ID")]
    public string frontCutID;
    public string backCutID;

    private void Start()
    {
        rend = GetComponent<Renderer>();
        originalColor = rend.material.color;

        if (string.IsNullOrEmpty(blockID)) return;

        bool isLocked = StageManager.Instance.IsBlockLocked(blockID);
        if (isLocked)
        {
            rend.material.color = Color.gray;
            isInteractable = false;
        }

        if (StageManager.Instance.stageBlockDict.TryGetValue(blockID, out var data))
        {
            blockType = data.BlockType;
            enemyID = new List<string>(data.EnemyIDs);
            frontCutID = data.FrontCutID;
            backCutID = data.BackCutID;
        }
    }

    private void OnMouseEnter()
    {
        if (!isInteractable) return;
        rend.material.color = hoverColor;
    }

    private void OnMouseExit()
    {
        if (!isInteractable) return;
        rend.material.color = originalColor;
    }

    private void OnMouseDown()
    {
        if (!isInteractable) return;
        rend.material.color = clickColor;
        Debug.Log($"스테이지블록 클릭: {blockID}");
        GetInstance().GetBlock(blockID);
    }

    private void OnMouseUp()
    {
        if (!isInteractable) return;
        rend.material.color = hoverColor;
        Debug.Log($"스테이지블록 클릭 완수: {blockID}");

        if (blockID != null)
        {
            SpawnManager.Instance.enemyIDs = this.enemyID;
            SpawnManager.Instance.currentBlockID = this.blockID; // 현재 선택된 블록 ID 저장
            
            // blockID가 '060001'일 때만 컷신 출력, 그 외에는 바로 전투 시작
            // if (blockID == "060001" && CutsceneManager.Instance != null)
            // {
            //     CutsceneManager.Instance.PlayCutscene("튜토리얼", () => {
            //         // 컷신 종료 후 전투 시작
            //         SceneManager.LoadScene("TestBattle");
            //     });
            // }
            // else
            // {
                SceneManager.LoadScene("TestBattle");
            // }
        }
        else
        {
            Debug.LogWarning("스테이지 ID가 설정되지 않음");
        }
    }

    private static StageManager GetInstance()
    {
        return StageManager.Instance;
    }
}
