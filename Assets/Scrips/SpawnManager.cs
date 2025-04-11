using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public GameObject characterPrefab;
    public CharacterLoader loader; // XML �Ľ��ϴ� ������Ʈ
    public CharacterData data;

    [Header("�׽�Ʈ�� ĳ���� ID")]
    public string targetID = "000001";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // �ߺ� ����
            return;
        }
        Instance = this;
    }
    public void CharacterSpawnOn()
    {


        var loader = CharacterLoader.Instance;
        if (loader == null)
        {
            Debug.LogError("CharacterController �̱����� �ʱ�ȭ���� �ʾҽ��ϴ�!");
        }
        else
        {
            //var data = loader.LoadCharacters();
        }
        Debug.Log($"[Spawn] ��û�� ID: '{targetID}'");
        if (!CharacterData.characterDict.TryGetValue(targetID, out data))
        {
            Debug.LogError($"ID {targetID} ĳ���͸� ã�� �� �����ϴ�.");
            return;
        }
        CharacterSpawnOnLoad();
    }

    public void CharacterSpawnOnLoad()
    {
        // 1. �����Ͱ� �ε��Ǿ����� Ȯ��
        if (CharacterData.characterDict.Count == 0)
        {
            Debug.LogError("CharacterDict�� ��� �ֽ��ϴ�. LoadAllCharacters�� ȣ����� �ʾ��� �� �ֽ��ϴ�.");
            return;
        }

        // 2. ID �������� ������ ĳ���� ����
        string targetID = "000001";
        if (!CharacterData.characterDict.TryGetValue(targetID, out var data))
        {
            Debug.LogError($"ID {targetID} ĳ���͸� ã�� �� �����ϴ�.");
            return;
        }

        // 3. ĳ���� ������ �ν��Ͻ� ����
        GameObject obj = Instantiate(characterPrefab, Vector3.zero, Quaternion.identity);
        obj.name = $"Character_{targetID}";

        // 4. ������ ����
        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
            unit.SetData(data);
        else
            Debug.LogWarning("CharacterUnit ������Ʈ�� �����տ� �پ����� �ʽ��ϴ�.");
    }
    public GameObject SpawnCharacter(string id, Vector3 position)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} ĳ���͸� ã�� �� �����ϴ�.");
            return null;
        }

        GameObject obj = Instantiate(characterPrefab, position, Quaternion.identity);
        obj.name = $"Character_{id}";

        var stats = obj.GetComponent<CharacterStats>();
        if (stats != null)
            stats.SetData(data);
        else
            Debug.LogWarning("CharacterStats ������Ʈ�� �����տ� �پ����� �ʽ��ϴ�.");

        return obj;

    }
}
