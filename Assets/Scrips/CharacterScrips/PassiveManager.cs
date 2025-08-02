using UnityEngine;
using System.Collections.Generic;

public class PassiveManager : MonoBehaviour
{
    public static PassiveManager Instance { get; private set; }
    private Dictionary<string, PassiveData> passiveDict = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadAllPassives();
    }

    private void LoadAllPassives()
    {
        var allPassives = PassiveLoader.GetAllPassives(); // PassiveLoader에서 전체 PassiveData 리스트 반환
        passiveDict.Clear();
        foreach (var pair in allPassives)
        {
            if (pair.Value != null && !string.IsNullOrEmpty(pair.Value.passiveID))
                passiveDict[pair.Value.passiveID] = pair.Value;
        }
    }

    public PassiveData GetById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        passiveDict.TryGetValue(id, out var data);
        return data;
    }

    // 캐릭터에 패시브 효과 적용(예시, 실제 구현은 추후)
    public void ApplyPassivesToCharacter(CharacterStats character, List<string> passiveIDs)
    {
        // TODO: 패시브 효과 적용 로직 구현
    }
} 