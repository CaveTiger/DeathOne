using UnityEngine;
using System.Collections.Generic;

public class StatusEffectSlot : MonoBehaviour
{
    // 상태이상 아이콘 프리팹
    public GameObject statusEffectIconPrefab;

    // 현재 슬롯에 표시 중인 상태이상 아이콘 리스트
    private List<StatusEffectInstance> activeInstances = new List<StatusEffectInstance>();

    /// <summary>
    /// 상태이상 추가
    /// </summary>
    public void AddStatusEffect(StatusEffectData effectData, int duration, int value, CharacterStats owner)
    {
        GameObject icon = Instantiate(statusEffectIconPrefab, transform);
        var instance = icon.GetComponent<StatusEffectInstance>();
        if (instance != null)
        {
            instance.Initialize(effectData, duration, value, owner);
            activeInstances.Add(instance);
        }
        else
        {
            Debug.LogWarning("StatusEffectInstance 컴포넌트가 프리팹에 없습니다.");
        }
    }

    /// <summary>
    /// 상태이상 제거
    /// </summary>
    public void RemoveStatusEffect(StatusEffectData effectData)
    {
        for (int i = activeInstances.Count - 1; i >= 0; i--)
        {
            if (activeInstances[i].EffectData == effectData)
            {
                Destroy(activeInstances[i].gameObject);
                activeInstances.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 모든 상태이상 초기화
    /// </summary>
    public void ClearAll()
    {
        foreach (var instance in activeInstances)
            Destroy(instance.gameObject);
        activeInstances.Clear();
    }
}
