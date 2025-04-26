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

    private void Awake() //이 녀석은 실제로 이 씬에서 자릴 지키면서 스테이지만 불러와줘야한다.
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else Destroy(gameObject);

        SettingStart();
    }

    private void Start()
    {
        SpawnStageBlocks(settingID);
    }


    public void SettingStart()
    {
        Debug.Log($"[StageSetting] StageManager에 등록된 키 수: {StageManager.Instance.stageDict.Count}");
        settingID = StageManager.Instance.SelectedStageID; //받아낸 ID를 실제로 쓸 변수쪽에 넣어주기
    }

    public void SpawnStageBlocks(string settingID)
    {

        if (!StageManager.Instance.stageDict.TryGetValue(settingID, out var stage))
        {
            Debug.LogError($"[StageObjectSpawner] 스테이지 ID '{settingID}'를 찾을 수 없습니다.");
            return;
        }
        Debug.Log($"[확인용] 블록 ID 수: {stage.BlockIDs.Count}");
        foreach (string blockID in stage.BlockIDs)
        {
            Debug.Log($"[확인용] 블록 ID: {blockID}");
        }
        foreach (string blockID in stage.BlockIDs)
        {
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
            
        }
    }
}

