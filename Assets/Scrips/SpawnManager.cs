using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class SpawnManager : MonoBehaviour
{
    public static SpawnManager Instance { get; private set; }

    public GameObject characterPrefab;
    public CharacterLoader loader; // XML 파싱하는 컴포넌트
    public CharacterData data;

    [Header("테스트용 캐릭터 ID")]
    public string targetID = "000001";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }
        Instance = this;
    }
    public void CharacterSpawnOn()
    {


        var loader = CharacterLoader.Instance;
        if (loader == null)
        {
            Debug.LogError("CharacterController 싱글톤이 초기화되지 않았습니다!");
        }
        else
        {
            //var data = loader.LoadCharacters();
        }
        Debug.Log($"[Spawn] 요청된 ID: '{targetID}'");
        if (!CharacterData.characterDict.TryGetValue(targetID, out data))
        {
            Debug.LogError($"ID {targetID} 캐릭터를 찾을 수 없습니다.");
            return;
        }
        CharacterSpawnOnLoad();
    }

    public void CharacterSpawnOnLoad()
    {
        // 1. 데이터가 로딩되었는지 확인
        if (CharacterData.characterDict.Count == 0)
        {
            Debug.LogError("CharacterDict가 비어 있습니다. LoadAllCharacters가 호출되지 않았을 수 있습니다.");
            return;
        }

        // 2. ID 기준으로 생성할 캐릭터 선택
        string targetID = "000001";
        if (!CharacterData.characterDict.TryGetValue(targetID, out var data))
        {
            Debug.LogError($"ID {targetID} 캐릭터를 찾을 수 없습니다.");
            return;
        }

        // 3. 캐릭터 프리팹 인스턴스 생성
        GameObject obj = Instantiate(characterPrefab, Vector3.zero, Quaternion.identity);
        obj.name = $"Character_{targetID}";

        // 4. 데이터 주입
        var unit = obj.GetComponent<CharacterStats>();
        if (unit != null)
            unit.SetData(data);
        else
            Debug.LogWarning("CharacterUnit 컴포넌트가 프리팹에 붙어있지 않습니다.");
    }
    public GameObject SpawnCharacter(string id, Vector3 position)
    {
        if (!CharacterData.characterDict.TryGetValue(id, out var data))
        {
            Debug.LogError($"[Spawn] ID {id} 캐릭터를 찾을 수 없습니다.");
            return null;
        }

        GameObject obj = Instantiate(characterPrefab, position, Quaternion.identity);
        obj.name = $"Character_{id}";

        var stats = obj.GetComponent<CharacterStats>();
        if (stats != null)
            stats.SetData(data);
        else
            Debug.LogWarning("CharacterStats 컴포넌트가 프리팹에 붙어있지 않습니다.");

        return obj;

    }
}
