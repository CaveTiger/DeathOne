using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public Dictionary<string, StageData> stageDict = new();
    public Dictionary<string, StageBlockData> stageBlockDict = new();

    public string SelectedStageID { get; private set; } = "";

    [Header("���õ� �������� (����׿�)")]
    [SerializeField] private string debugSelectedStageID;

    public void SelectStage(string id)
    {
        SelectedStageID = id;
        debugSelectedStageID = id; // �ν����Ϳ� �ݿ�
    }
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }
    public StageData GetStage(string id)
    {
        return stageDict.TryGetValue(id, out var stage) ? stage : null;
    }

    public StageBlockData GetBlock(string stageKey)
    {
        return stageBlockDict.TryGetValue(stageKey, out var block) ? block : null;
    }
    //public void SelectStage(string id)
    //{
    //    SelectedStageID = id;
    //    Debug.Log($"[StageManager] �������� ���õ�: {SelectedStageID}");
    //}

    public void ClearSelectedStage()
    {
        Debug.Log($"[StageManager] �������� �ʱ�ȭ");
        SelectedStageID = "";
    }
}
