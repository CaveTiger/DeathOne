using System.Collections.Generic;
using UnityEngine;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public Dictionary<string, StageData> stageDict = new();
    public Dictionary<string, StageBlockData> stageBlockDict = new();

    public string SelectedStageID { get; private set; } = "";

    [Header("선택된 스테이지 (디버그용)")]
    [SerializeField] private string debugSelectedStageID;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    public bool AddBlock(StageBlockData block)
    {
        if (string.IsNullOrEmpty(block.ID))
        {
            Debug.LogError("[StageManager] 블록 ID가 없습니다!");
            return false;
        }

        if (stageBlockDict.ContainsKey(block.ID))
        {
            Debug.LogWarning($"[StageManager] 이미 존재하는 블록 ID입니다: {block.ID}");
            return false;
        }

        stageBlockDict[block.ID] = block;
        return true;
    }

    public bool AddStage(StageData stage)
    {
        if (string.IsNullOrEmpty(stage.ID))
        {
            Debug.LogError("[StageManager] 스테이지 ID가 없습니다!");
            return false;
        }

        if (stageDict.ContainsKey(stage.ID))
        {
            Debug.LogWarning($"[StageManager] 이미 존재하는 스테이지 ID입니다: {stage.ID}");
            return false;
        }

        stageDict[stage.ID] = stage;
        return true;
    }

    public StageData GetStage(string id)
    {
        return stageDict.TryGetValue(id, out var stage) ? stage : null;
    }

    public StageBlockData GetBlock(string stageKey)
    {
        return stageBlockDict.TryGetValue(stageKey, out var block) ? block : null;
    }

    public void ClearSelectedStage()
    {
        Debug.Log($"[StageManager] 스테이지 초기화");
        SelectedStageID = "";
    }

    public void SelectStage(string id) //얜 스테이지 아이콘을 눌렀을때 호출돼서 ID 정보를 인스펙터에 넣어 이후 쓸 수 있게 함
    {
        SelectedStageID = id;
        debugSelectedStageID = id; // 인스펙터에 반영
    }
}
