using UnityEngine;
using System.Collections.Generic;

public class StatusEffectManager : MonoBehaviour
{
    public static StatusEffectManager Instance { get; private set; }
    private Dictionary<string, StatusEffectData> effectDict = new Dictionary<string, StatusEffectData>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        LoadAllEffects();

        // 딕셔너리 상태 디버깅
        if (effectDict.Count == 0)
        {
            Debug.LogWarning("[StatusEffectManager] 상태이상 데이터가 로드되지 않았습니다.");
        }
        else
        {
            Debug.Log($"[StatusEffectManager] effectDict에 등록된 상태이상 개수: {effectDict.Count}");
            foreach (var kvp in effectDict)
            {
                Debug.Log($"[StatusEffectManager] 등록: {kvp.Key} - {kvp.Value.effectName}");
            }
        }
    }

    private void LoadAllEffects()
    {
        var effects = Resources.LoadAll<StatusEffectData>("Data/ScriptableObject");
        foreach (var effect in effects)
        {
            if (effect != null && !string.IsNullOrEmpty(effect.EffectID))
            {
                effectDict[effect.EffectID] = effect;
            }
        }
    }

    public StatusEffectData GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
        {
            Debug.LogWarning("[StatusEffectManager] 유효하지 않은 상태이상 ID");
            return null;
        }

        if (!effectDict.TryGetValue(id, out var data))
        {
            Debug.LogWarning($"[StatusEffectManager] 상태이상을 찾을 수 없음: {id}");
            return null;
        }

        return data;
    }
}
