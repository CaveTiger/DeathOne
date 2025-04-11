using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance;

    public Dictionary<string, StageData> stageDict = new();
    public Dictionary<string, StageBlockData> stageBlockDict = new();

    public string SelectedStageID { get; private set; } = "";

    [Header("선택된 스테이지 (디버그용)")]
    [SerializeField] private string debugSelectedStageID;

    public void SelectStage(string id)
    {
        SelectedStageID = id;
        debugSelectedStageID = id; // 인스펙터에 반영
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
    //    Debug.Log($"[StageManager] 스테이지 선택됨: {SelectedStageID}");
    //}

    public void ClearSelectedStage()
    {
        Debug.Log($"[StageManager] 스테이지 초기화");
        SelectedStageID = "";
    }
}
