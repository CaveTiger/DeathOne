using System.Collections.Generic;
using UnityEngine;


public class StageSetting : MonoBehaviour
{
    //해당 스크립트는 스테이지 매니저에게 호출될 메서드를 작성한다.

    public static StageSetting Instance { get; private set; }
    [SerializeField, Tooltip("현재 스테이지의 ID")] string settingID = StageManager.Instance.SelectedStageID;

    [SerializeField] private GameObject stageBlock;

    // 내부 전용 클래스
    [System.Serializable]
    public class InStageData
    {
        public List<UnitHPData> hpDataList = new();

        public void AddOrUpdate(string id, int hp, int maxHp)
        {
            var unit = hpDataList.Find(x => x.characterID == id);
            int clampedHP = Mathf.Min(hp, maxHp);

            if (unit != null)
            {
                unit.hp = clampedHP;
                unit.maxHp = maxHp;
            }
            else
            {
                hpDataList.Add(new UnitHPData
                {
                    characterID = id,
                    hp = clampedHP,
                    maxHp = maxHp
                });
            }

            if (StageLoader.Instance == null) return;
        }

        public int GetHP(string id)
        {
            var unit = hpDataList.Find(x => x.characterID == id);
            return unit != null ? unit.hp : -1;
        }
        public int GetMaxHP(string id)
        {
            var unit = hpDataList.Find(x => x.characterID == id);
            return unit != null ? unit.maxHp : -1;
        }
    }

    [System.Serializable]
    public class UnitHPData
    {
        public string characterID;
        public int hp;
        public int maxHp;
    }

    public InStageData inStageData = new(); // 여기서 관리

    private StageBlockData stageData;
    private List<GameObject> spawnedBlocks = new List<GameObject>();

    private void Awake()
    {
        Debug.Log("[StageSetting] Awake 호출됨");
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);

        SettingStart();
    }

    private void Start()
    {
        Debug.Log("[StageSetting] Start 호출됨");
        SpawnStageBlocks(settingID);
    }


    public void SettingStart()
    {
        Debug.Log($"[StageSetting] SettingStart 호출, StageManager.SelectedStageID: {StageManager.Instance.SelectedStageID}");
        settingID = StageManager.Instance.SelectedStageID;
    }

    public void SpawnStageBlocks(string settingID)
    {
        Debug.Log($"[StageSetting] SpawnStageBlocks 호출, settingID: {settingID}");

        // 기존 블록들 비활성화
        foreach (var block in spawnedBlocks)
        {
            if (block != null)
            {
                Destroy(block);
            }
        }
        spawnedBlocks.Clear();

        if (!StageManager.Instance.stageDict.TryGetValue(settingID, out var stage))
        {
            Debug.LogError($"[StageObjectSpawner] 스테이지 ID '{settingID}'를 찾을 수 없습니다.");
            return;
        }
        Debug.Log($"[StageSetting] BlockIDs: {string.Join(",", stage.BlockIDs)}");

        // 1. 모든 블록 오브젝트 생성
        foreach (string blockID in stage.BlockIDs)
        {
            bool isCleared = StageManager.Instance.IsBlockCleared(blockID);
            bool isLocked = StageManager.Instance.IsBlockLocked(blockID);
            Debug.Log($"[StageSetting] 블록ID: {blockID}, Cleared: {isCleared}, Locked: {isLocked}");

            if (!StageManager.Instance.stageBlockDict.TryGetValue(blockID, out var blockData))
            {
                Debug.LogWarning($"[StageObjectSpawner] 블록 ID '{blockID}'를 찾을 수 없습니다.");
                continue;
            }

            // 위치 계산
            Vector3 spawnPos = new Vector3(blockData.Position.x, blockData.Position.y, 0f);

            // 프리팹 생성
            GameObject obj = Instantiate(stageBlock, spawnPos, Quaternion.identity);
            obj.name = $"Block_{blockData.ID}"; //프리팹 이름 바꾸기
            obj.GetComponent<StageBlockSelection>().blockID = blockData.ID; //그 프리팹에 블록 id 넣어주기
            spawnedBlocks.Add(obj);
        }
    }

    // 전투 종료 후 체력 업데이트
    public void UpdatePartyHP(Dictionary<string, (int hp, int maxHp)> partyHPData)
    {
        foreach (var data in partyHPData)
        {
            inStageData.AddOrUpdate(data.Key, data.Value.hp, data.Value.maxHp);
        }
        Debug.Log("[StageSetting] 파티 체력 데이터 업데이트 완료");
    }

    // 스테이지 시작 시 체력 데이터 초기화
    public void InitializePartyHP(Dictionary<string, (int hp, int maxHp)> initialHPData)
    {
        inStageData.hpDataList.Clear();
        foreach (var data in initialHPData)
        {
            inStageData.AddOrUpdate(data.Key, data.Value.hp, data.Value.maxHp);
        }
        Debug.Log("[StageSetting] 파티 체력 데이터 초기화 완료");
    }
}

