using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

public class StageManager : MonoBehaviour
{
    public static StageManager Instance { get; private set; }

    public Dictionary<string, StageData> stageDict = new();
    public Dictionary<string, StageBlockData> stageBlockDict = new();
    private Dictionary<string, StageProgressData> stageProgressDict = new();
    
    private string SavePath => $"{Application.persistentDataPath}/stage_progress.json";
    private const string GAME_VERSION = "1.0.0"; // 게임 버전 관리

    // 현재 선택된 스테이지 관련
    public string SelectedStageID { get; private set; } = "";
    public StageData CurrentStage => !string.IsNullOrEmpty(SelectedStageID) ? GetStage(SelectedStageID) : null;
    public bool HasSelectedStage => !string.IsNullOrEmpty(SelectedStageID);

    [Header("선택된 스테이지 (디버그용)")]
    [SerializeField] private string debugSelectedStageID;

    // 스테이지 진입/퇴장 관련 이벤트
    public event Action<string> OnStageEnter;    // 스테이지 진입 시
    public event Action<string> OnStageExit;     // 스테이지 퇴장 시
    public event Action<string> OnStageClear;    // 스테이지 클리어 시

    private Dictionary<string, BlockState> blockStates = new();

    [System.Serializable]
    public class BlockStateDebugView
    {
        public string blockID;
        public bool Cleared;
        public bool Locked;
    }

    public List<BlockStateDebugView> blockStatesDebugView = new List<BlockStateDebugView>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            LoadStageProgress(); // 게임 시작시 진행 상태 로드
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
        Debug.Log($"[StageManager] 블록 {block.ID} 추가 완료 (Last: {block.Last})");
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

    public void SelectStage(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[StageManager] 스테이지 ID가 비어있습니다.");
            return;
        }

        if (!stageDict.ContainsKey(id))
        {
            Debug.LogError($"[StageManager] 존재하지 않는 스테이지 ID입니다: {id}");
            return;
        }

        // 이전 스테이지가 있었다면 퇴장 처리
        if (!string.IsNullOrEmpty(SelectedStageID))
        {
            OnStageExit?.Invoke(SelectedStageID);
        }

        SelectedStageID = id;
        debugSelectedStageID = id; // 인스펙터에 반영
        OnStageEnter?.Invoke(id);

        Debug.Log($"[StageManager] 스테이지 선택됨: {id}");
    }

    public void ClearSelectedStage()
    {
        if (!string.IsNullOrEmpty(SelectedStageID))
        {
            OnStageExit?.Invoke(SelectedStageID);
            Debug.Log($"[StageManager] 스테이지 초기화: {SelectedStageID}");
        }
        SelectedStageID = "";
        debugSelectedStageID = "";
    }

    // 스테이지 진행 상태 저장
    public void SaveStageProgress()
    {
        try
        {
            var wrapper = new StageProgressWrapper
            {
                progressData = stageProgressDict.Values.ToList(),
                lastPlayedDate = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                gameVersion = GAME_VERSION,
                totalPlayTime = (int)(Time.time - Time.timeSinceLevelLoad), // 임시로 현재 세션 시간만 저장
                totalClearCount = stageProgressDict.Values.Count(x => x.isCleared)
            };

            string json = JsonUtility.ToJson(wrapper, true); // true로 설정하여 가독성 있는 JSON 생성
            File.WriteAllText(SavePath, json);
            Debug.Log($"[StageManager] 스테이지 진행 데이터 저장 완료: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"[StageManager] 저장 중 오류 발생: {e.Message}");
        }
    }

    // 스테이지 진행 상태 로드
    public void LoadStageProgress()
    {
        try
        {
            stageProgressDict.Clear();

            if (File.Exists(SavePath))
            {
                string json = File.ReadAllText(SavePath);
                var wrapper = JsonUtility.FromJson<StageProgressWrapper>(json);

                // 버전 체크 및 마이그레이션 로직
                if (wrapper.gameVersion != GAME_VERSION)
                {
                    Debug.Log($"[StageManager] 게임 버전 변경 감지: {wrapper.gameVersion} -> {GAME_VERSION}");
                    // TODO: 버전별 데이터 마이그레이션 로직 추가
                }

                foreach (var progress in wrapper.progressData)
                {
                    stageProgressDict[progress.stageId] = progress;
                }

                Debug.Log($"[StageManager] 마지막 플레이: {wrapper.lastPlayedDate}");
                Debug.Log($"[StageManager] 총 클리어 수: {wrapper.totalClearCount}");
            }
            else
            {
                // 초기 데이터 생성
                foreach (var stage in stageDict)
                {
                    stageProgressDict[stage.Key] = new StageProgressData 
                    { 
                        stageId = stage.Key,
                        isCleared = false,
                        isLocked = true,
                        clearCount = 0,
                        lastClearTime = DateTime.MinValue
                    };
                }
                SaveStageProgress(); // 초기 데이터 저장
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"[StageManager] 로드 중 오류 발생: {e.Message}");
            // 오류 발생 시 초기 데이터로 리셋
            ResetProgressData();
        }
    }

    // 진행 데이터 초기화
    private void ResetProgressData()
    {
        stageProgressDict.Clear();
        foreach (var stage in stageDict)
        {
            stageProgressDict[stage.Key] = new StageProgressData 
            { 
                stageId = stage.Key,
                isCleared = false,
                isLocked = true,
                clearCount = 0,
                lastClearTime = DateTime.MinValue
            };
        }
        SaveStageProgress();
    }

    // 스테이지 클리어 처리
    public void SetStageCleared(string stageId)
    {
        if (!stageProgressDict.ContainsKey(stageId))
        {
            stageProgressDict[stageId] = new StageProgressData { stageId = stageId };
        }
        
        stageProgressDict[stageId].isCleared = true;
        stageProgressDict[stageId].clearCount++;
        stageProgressDict[stageId].lastClearTime = DateTime.Now;
        
        // 현재 스테이지가 클리어된 경우 이벤트 발생
        if (stageId == SelectedStageID)
        {
            OnStageClear?.Invoke(stageId);
            
            // 다음 스테이지 블록 잠금 해제
            if (stageDict.TryGetValue(stageId, out var currentStage))
            {
                foreach (var nextBlockId in currentStage.NextBlockIDs)
                {
                    UnlockBlock(stageId, nextBlockId);
                    Debug.Log($"[StageManager] 다음 스테이지 블록 잠금 해제: {nextBlockId}");
                }
            }
        }
        
        SaveStageProgress();
    }

    // 스테이지 잠금 해제
    public void UnlockStage(string stageId)
    {
        if (!stageProgressDict.ContainsKey(stageId))
        {
            stageProgressDict[stageId] = new StageProgressData { stageId = stageId };
        }
        
        stageProgressDict[stageId].isLocked = false;
        SaveStageProgress();
    }

    // 스테이지 클리어 여부 확인
    public bool IsStageCleared(string stageId)
    {
        return stageProgressDict.TryGetValue(stageId, out var progress) && progress.isCleared;
    }

    // 스테이지 잠금 여부 확인
    public bool IsStageLocked(string stageId)
    {
        return stageProgressDict.TryGetValue(stageId, out var progress) && progress.isLocked;
    }

    // 스테이지 진입 가능 여부 확인
    public bool CanEnterStage(string stageId)
    {
        if (!stageDict.ContainsKey(stageId)) return false;
        
        var stage = stageDict[stageId];
        if (string.IsNullOrEmpty(stage.ParentID)) return true; // 부모가 없는 경우 (첫 스테이지)
        
        // 부모 스테이지가 클리어되어 있는지 확인
        return IsStageCleared(stage.ParentID);
    }

    // 블록 클리어 처리
    public void SetBlockCleared(string stageId, string blockId)
    {
        if (!stageProgressDict.ContainsKey(stageId))
        {
            stageProgressDict[stageId] = new StageProgressData { stageId = stageId };
        }

        var stageProgress = stageProgressDict[stageId];
        var blockProgress = stageProgress.blockProgress.FirstOrDefault(b => b.blockId == blockId);
        
        if (blockProgress == null)
        {
            blockProgress = new BlockProgressData { blockId = blockId };
            stageProgress.blockProgress.Add(blockProgress);
        }

        blockProgress.isCleared = true;
        blockProgress.lastClearTime = DateTime.Now;
        
        // Last 속성이 true인 블록을 클리어한 경우 스테이지도 클리어 처리
        var block = GetBlock(blockId);
        if (block != null && block.Last)
        {
            SetStageCleared(stageId);
        }

        SaveStageProgress();
    }

    // 블록 잠금 해제
    public void UnlockBlock(string stageId, string blockId)
    {
        if (!stageProgressDict.ContainsKey(stageId))
        {
            stageProgressDict[stageId] = new StageProgressData { stageId = stageId };
        }

        var stageProgress = stageProgressDict[stageId];
        var blockProgress = stageProgress.blockProgress.FirstOrDefault(b => b.blockId == blockId);
        
        if (blockProgress == null)
        {
            blockProgress = new BlockProgressData { blockId = blockId };
            stageProgress.blockProgress.Add(blockProgress);
        }

        blockProgress.isLocked = false;
        SaveStageProgress();
    }

    // 블록 클리어 여부 확인
    public bool IsBlockCleared(string blockID)
    {
        return blockStates.TryGetValue(blockID, out var state) && state.Cleared;
    }

    // 블록 잠금 여부 확인
    public bool IsBlockLocked(string blockID)
    {
        return blockStates.TryGetValue(blockID, out var state) && state.Locked;
    }

    public void SetBlockCleared(string blockID, bool value)
    {
        if (!blockStates.ContainsKey(blockID))
            blockStates[blockID] = new BlockState();
        blockStates[blockID].Cleared = value;
    }

    public void SetBlockLocked(string blockID, bool value)
    {
        if (!blockStates.ContainsKey(blockID))
            blockStates[blockID] = new BlockState();
        blockStates[blockID].Locked = value;
    }

    public void ClearBlock(string blockID)
    {
        SetBlockCleared(blockID, true);
        SetBlockLocked(blockID, true); // 클리어된 블록은 잠금

        // 다음 블록 해금
        if (stageBlockDict.TryGetValue(blockID, out var blockData))
        {
            foreach (var nextBlockID in blockData.NextBlockIDs)
                SetBlockLocked(nextBlockID, false);
        }
    }

    public void RegisterStageBlocks(List<string> blockIDs)
    {
        Debug.Log($"[RegisterStageBlocks] 호출됨. blockIDs: {string.Join(",", blockIDs)}");
        foreach (var blockID in blockIDs)
        {
            if (!blockStates.ContainsKey(blockID))
            {
                blockStates[blockID] = new BlockState
                {
                    Cleared = false,
                    Locked = true // 기본값: 잠금
                };
                Debug.Log($"[RegisterStageBlocks] blockStates에 등록: {blockID}");
            }
        }
        // 시작 블록만 해금
        if (blockIDs.Count > 0)
        {
            blockStates[blockIDs[0]].Locked = false;
            Debug.Log($"[RegisterStageBlocks] 시작 블록 해금: {blockIDs[0]}");
        }
    }

    // blockStates 딕셔너리를 리스트로 변환해서 인스펙터에 노출
    public void RefreshBlockStatesDebugView()
    {
        blockStatesDebugView.Clear();
        foreach (var kvp in blockStates)
        {
            blockStatesDebugView.Add(new BlockStateDebugView
            {
                blockID = kvp.Key,
                Cleared = kvp.Value.Cleared,
                Locked = kvp.Value.Locked
            });
        }
    }

    private void Update()
    {
        RefreshBlockStatesDebugView();
    }

    public void UnregisterStageBlocks(List<string> blockIDs)
    {
        foreach (var blockID in blockIDs)
        {
            if (blockStates.ContainsKey(blockID))
            {
                blockStates.Remove(blockID);
                Debug.Log($"[UnregisterStageBlocks] blockStates에서 제거: {blockID}");
            }
        }
    }

    public void ClearAllBlockStates()
    {
        blockStates.Clear();
    }

    public int GetTotalClearCount()
    {
        // 예시: 클리어된 스테이지 개수 반환
        int count = 0;
        foreach (var stage in stageProgressDict.Values)
        {
            if (stage.isCleared) count++;
        }
        return count;
    }
}

public class BlockState
{
    public bool Cleared;
    public bool Locked;
}
