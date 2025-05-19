using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("플레이어 슬롯")]
    public Transform playerSlot;

    [Header("아군 슬롯")]
    public Transform[] allySlots;

    [Header("작성될 적 목록")]
    public Transform[] enemySlots;

    public List<CharacterStats> allCharacters; // 전투에 참여하는 모든 캐릭터
    public Transform skillSlotParent; // 스킬 슬롯 UI 부모 오브젝트
    public GameObject skillInstancePrefab; // SkillInstance 프리팹

    //얜 싱글톤을 하지 않는다. 배매는 오로지 실행자 역할만을 맡는다.
    void Start()
    {
        Debug.Log($"[Check] 캐릭터 딕셔너리 Count: {CharacterData.characterDict.Count}");
        SpawnAllUnits();

        Debug.Log($"이게 전투 시작부분  SkillDict 현재 등록된 스킬 수: {SkillData.skillDict.Count}");

        foreach (var kvp in SkillData.skillDict)
        {
            Debug.Log($"SkillDict Key: {kvp.Key} / Skill 이름: {kvp.Value.Name}");
        }
        
        SetupSkillSlots();
    }

    void SpawnAllUnits()
    {
        var spawn = SpawnManager.Instance;
        var dict = CharacterData.characterDict;

        // 1. 플레이어 유닛
        if (playerSlot == null)
        {
            Debug.LogError("[Spawn] playerSlot이 할당되지 않았습니다!");
        }
        else if (!CharacterData.characterDict.ContainsKey(spawn.playerID))
        {
            Debug.LogError($"[Spawn] 캐릭터 딕셔너리에 {spawn.playerID} 없음");
        }
        else
        {
            Debug.Log("[Spawn] 플레이어 생성 시도 중");
            SpawnUnit(spawn.playerID, playerSlot);
        }

        // 2. 아군 유닛
        for (int i = 0; i < spawn.allyIDs.Count && i < allySlots.Length; i++)
        {
            SpawnUnit(spawn.allyIDs[i], allySlots[i]);
        }

        // 3. 적군 유닛
        for (int i = 0; i < spawn.enemyIDs.Count && i < enemySlots.Length; i++)
        {
            SpawnUnitEnemy(spawn.enemyIDs[i], enemySlots[i]);
        }
        Debug.Log($"[Spawn] 플레이어 ID: {spawn.playerID}");
    }

    void SpawnUnit(string id, Transform slot)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} 에 해당하는 캐릭터 데이터 없음.");
            return;
        }

        GameObject obj = Instantiate(SpawnManager.Instance.characterPrefab, slot.position, Quaternion.identity);
        obj.name = $"Unit_{id}";
        obj.transform.SetParent(slot);
        obj.transform.localPosition = Vector3.zero;

        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
        {
            unit.SetData(data);
        }
        else
        {
            Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 없습니다.");
        }
    }

    void SpawnUnitEnemy(string id, Transform slot)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} 에 해당하는 캐릭터 데이터 없음.");
            return;
        }

        GameObject obj = Instantiate(SpawnManager.Instance.characterPrefab, slot.position, Quaternion.identity);
        obj.name = $"Unit_{id}";
        obj.transform.SetParent(slot);
        obj.transform.localPosition = Vector3.zero;
        obj.tag = "Enemy";

        if (obj.GetComponent<EnemyAIController>() == null)
        {
            obj.AddComponent<EnemyAIController>();
        }

        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
        {
            unit.IsPlayer = false;
            unit.SetData(data);
        }
        else
        {
            Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 없습니다.");
        }
    }

    public void StartBattle()
    {
        // ... 캐릭터 생성 등 기타 초기화 ...

        SetupSkillSlots();
    }

    private void SetupSkillSlots()
    {
        foreach (var character in allCharacters)
        {
            for (int i = 0; i < character.Skills.Length; i++)
            {
                string skillId = character.Skills[i];
                if (SkillData.skillDict.TryGetValue(skillId, out var skillData))
                {
                    var skillInstanceObj = Instantiate(skillInstancePrefab, skillSlotParent);
                    var skillInstance = skillInstanceObj.GetComponent<SkillInstance>();
                    skillInstance.SetSkillData(skillData);
                    skillInstance.SetCaster(character);
                }
            }
        }
    }
}
