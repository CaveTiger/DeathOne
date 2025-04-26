using System.Collections.Generic;
using UnityEngine;

public class BattleManager : MonoBehaviour
{
    [Header("�÷��̾� ����")]
    public Transform playerSlot;

    [Header("�Ʊ� ����")]
    public Transform[] allySlots;

    [Header("�ۼ��� �� ���")]
    public Transform[] enemySlots;


    //�� �̱����� ���� �ʴ´�. ��Ŵ� ������ ������ ���Ҹ��� �ô´�.
    void Start()
    {
        Debug.Log($"[Check] ĳ���� ��ųʸ� Count: {CharacterData.characterDict.Count}");
        SpawnAllUnits();
    }

    void SpawnAllUnits()
    {
        var spawn = SpawnManager.Instance;
        var dict = CharacterData.characterDict;

        // 1. �÷��̾� ����
        if (playerSlot == null)
        {
            Debug.LogError("[Spawn] playerSlot�� �Ҵ���� �ʾҽ��ϴ�!");
        }
        else if (!CharacterData.characterDict.ContainsKey(spawn.playerID))
        {
            Debug.LogError($"[Spawn] ĳ���� ��ųʸ��� {spawn.playerID} ����");
        }
        else
        {
            Debug.Log("[Spawn] �÷��̾� ���� �õ� ��");
            SpawnUnit(spawn.playerID, playerSlot);
        }

        // 2. �Ʊ� ����
        for (int i = 0; i < spawn.allyIDs.Count && i < allySlots.Length; i++)
        {
            SpawnUnit(spawn.allyIDs[i], allySlots[i]);
        }

        // 3. ���� ����
        for (int i = 0; i < spawn.enemyIDs.Count && i < enemySlots.Length; i++)
        {
            SpawnUnitEnemy(spawn.enemyIDs[i], enemySlots[i]);
        }
        Debug.Log($"[Spawn] �÷��̾� ID: {spawn.playerID}");
    }

    void SpawnUnit(string id, Transform slot)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} �� �ش��ϴ� ĳ���� ������ ����.");
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
            Debug.LogWarning("CharacterStats ������Ʈ�� �����տ� �����ϴ�.");
        }
    }

    void SpawnUnitEnemy(string id, Transform slot)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} �� �ش��ϴ� ĳ���� ������ ����.");
            return;
        }

        GameObject obj = Instantiate(SpawnManager.Instance.characterPrefab, slot.position, Quaternion.identity);
        obj.name = $"Unit_{id}";
        obj.transform.SetParent(slot);
        obj.transform.localPosition = Vector3.zero;
        obj.tag = "Enemy";

        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
        {
            unit.SetData(data);
        }
        else
        {
            Debug.LogWarning("CharacterStats ������Ʈ�� �����տ� �����ϴ�.");
        }
    }
}
