using UnityEngine;
using System.Collections.Generic;

public class StatusEffectController : MonoBehaviour
{
    [Header("상태이상 프리팹")]
    [SerializeField] private GameObject cDamageEffectPrefab;
    [SerializeField] private GameObject reactionEffectPrefab;
    [SerializeField] private GameObject buffEffectPrefab;
    [SerializeField] private GameObject deBuffEffectPrefab;

    [Header("상태이상 UI 생성 위치")]
    [SerializeField] private Transform statusEffectArea; // Inspector에서 StatusEffectSlot 연결

    [SerializeField] private List<GameObject> activeEffectPrefabs = new List<GameObject>();

    // 적용 예시
    public void AddCDamageEffect(StatusEffectData data, int duration, int value)
    {
        if (cDamageEffectPrefab == null || statusEffectArea == null)
        {
            Debug.LogWarning("[StatusEffectController] 프리팹 또는 생성 위치가 할당되지 않았습니다.");
            return;
        }
        GameObject effectObj = Instantiate(cDamageEffectPrefab, statusEffectArea);
        effectObj.name = data.effectName;
        activeEffectPrefabs.Add(effectObj);
        Debug.Log("[StatusEffectController] 프리팹 인스턴스 생성됨: " + effectObj);

        var instance = effectObj.GetComponent<StatusEffectInstance>();
        if (instance != null)
        {
            Debug.Log("[StatusEffectController] StatusEffectInstance 컴포넌트 발견, Initialize 호출");
            instance.Initialize(data, duration, value, GetComponent<CharacterStats>());
        }
        else
        {
            Debug.LogWarning("[StatusEffectController] 프리팹에 StatusEffectInstance 스크립트가 없습니다.");
        }
    }

    public void AddBuffEffect(StatusEffectData data, int duration, int value)
    {
        if (buffEffectPrefab == null || statusEffectArea == null)
        {
            Debug.LogWarning("[StatusEffectController] 버프 프리팹 또는 생성 위치가 할당되지 않았습니다.");
            return;
        }
        GameObject effectObj = Instantiate(buffEffectPrefab, statusEffectArea);
        effectObj.name = data.effectName;
        var buff = effectObj.GetComponent<StatusEffectInstanceBuff>();
        if (buff != null)
        {
            buff.Initialize(data, duration, value, GetComponent<CharacterStats>());
        }
        else
        {
            Debug.LogWarning("[StatusEffectController] 프리팹에 StatusEffectInstanceBuff 스크립트가 없습니다.");
        }
    }

    // 기타 상태이상 관련 메서드도 이쪽으로
    public void ApplyStatusEffectsOnTurnStart()
    {
        // 1. 파괴된 오브젝트(null) 정리
        activeEffectPrefabs.RemoveAll(e => e == null);

        // 2. 순회하며 null 체크
        foreach (var effect in activeEffectPrefabs)
        {
            if (effect == null) continue;

            // 일반 상태이상
            var instance = effect.GetComponent<StatusEffectInstance>();
            if (instance != null)
            {
                bool effectApplied = instance.OnTurnStart();
                if (effectApplied)
                {
                    instance.OnTurnEnd();
                }
                continue;
            }

            // 버프 전용
            var buffInstance = effect.GetComponent<StatusEffectInstanceBuff>();
            if (buffInstance != null)
            {
                buffInstance.OnTurnEnd();
            }
        }
    }
}
