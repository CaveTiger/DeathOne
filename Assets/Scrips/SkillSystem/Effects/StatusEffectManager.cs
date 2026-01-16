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
        var effects = Resources.LoadAll<StatusEffectData>("Data/ScriptableObject_StatusEffect");
        
        foreach (var effect in effects)
        {
            if (effect != null && !string.IsNullOrEmpty(effect.EffectID))
            {
                effectDict[effect.EffectID] = effect;
            }
        }
        
        Debug.Log($"[StatusEffectManager] 상태이상 데이터 로드 완료: {effectDict.Count}개");
    }

    public StatusEffectData GetById(string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        string trimmedId = id.Trim();

        if (!effectDict.TryGetValue(trimmedId, out var data))
        {
            Debug.LogWarning($"[StatusEffectManager] 상태이상을 찾을 수 없음: '{trimmedId}'");
            return null;
        }

        return data;
    }
}
