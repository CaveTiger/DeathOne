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
        // 프리팹의 원본 크기 그대로 사용 (스케일 변경 없음)
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
        // 프리팹의 원본 크기 그대로 사용 (스케일 변경 없음)
        effectObj.name = data.effectName;
        activeEffectPrefabs.Add(effectObj);
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

    public void AddReactionEffect(StatusEffectData data, int duration, int value)
    {
        if (reactionEffectPrefab == null || statusEffectArea == null)
        {
            Debug.LogWarning("[StatusEffectController] 반응 프리팹 또는 생성 위치가 할당되지 않았습니다.");
            return;
        }
        GameObject effectObj = Instantiate(reactionEffectPrefab, statusEffectArea);
        // 프리팹의 원본 크기 그대로 사용 (스케일 변경 없음)
        effectObj.name = data.effectName;
        activeEffectPrefabs.Add(effectObj);
        
        Debug.Log($"[StatusEffectController] 반응 효과 생성: {data.effectName} (ID: {data.EffectID}) - activeEffectPrefabs 개수: {activeEffectPrefabs.Count}");
        
        var reaction = effectObj.GetComponent<StatusEffectInstanceReaction>();
        if (reaction != null)
        {
            reaction.Initialize(data, duration, value, GetComponent<CharacterStats>());
            Debug.Log($"[StatusEffectController] StatusEffectInstanceReaction 초기화 완료 - TriggerCount: {reaction.triggerCount}");
        }
        else
        {
            Debug.LogWarning("[StatusEffectController] 프리팹에 StatusEffectInstanceReaction 스크립트가 없습니다.");
        }
    }

    // 기타 상태이상 관련 메서드도 이쪽으로
    public void ApplyStatusEffectsOnTurnStart()
    {
        Debug.Log("[AI개선] ApplyStatusEffectsOnTurnStart 시작");
        float startTime = Time.realtimeSinceStartup;
        
        var characterStats = GetComponent<CharacterStats>();
        if (characterStats == null || !characterStats.IsMyTurn)
        {
            Debug.Log("[AI개선] ApplyStatusEffectsOnTurnStart - 턴이 아니거나 CharacterStats가 null");
            return;
        }

        // 현재 적용된 모든 상태이상 효과를 순회
        for (int i = activeEffectPrefabs.Count - 1; i >= 0; i--)
        {
            var effect = activeEffectPrefabs[i];
            if (effect == null) continue;

            // StatusEffectInstance (지속피해) 처리
            var instance = effect.GetComponent<StatusEffectInstance>();
            if (instance != null && instance.ApplyEffect())
            {
                Debug.Log($"[AI개선] ApplyStatusEffectsOnTurnStart - 상태이상 적용: {effect.name}");
                // 상태이상 적용 후 지속 턴 감소
                instance.ReduceDuration();
            }
            
            // StatusEffectInstanceBuff (버프) 처리
            var buffInstance = effect.GetComponent<StatusEffectInstanceBuff>();
            if (buffInstance != null)
            {
                // 버프도 지속 턴 감소
                buffInstance.ReduceDuration();
            }
            
            // StatusEffectInstanceReaction (반응) 처리
            var reactionInstance = effect.GetComponent<StatusEffectInstanceReaction>();
            if (reactionInstance != null)
            {
                // 반응 효과도 지속 턴 감소
                reactionInstance.ReduceDuration();
            }
        }
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[AI개선] ApplyStatusEffectsOnTurnStart 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms");
    }

    /// <summary>
    /// 특정 타입의 상태이상이 이미 적용되어 있는지 확인합니다.
    /// </summary>
    public bool HasStatusEffect(string effectId)
    {
        foreach (var effect in activeEffectPrefabs)
        {
            if (effect == null) continue;

            var instance = effect.GetComponent<StatusEffectInstance>();
            if (instance != null && instance.EffectData != null && instance.EffectData.EffectID == effectId)
            {
                return true;
            }

            var buffInstance = effect.GetComponent<StatusEffectInstanceBuff>();
            if (buffInstance != null && buffInstance.EffectData != null && buffInstance.EffectData.EffectID == effectId)
            {
                return true;
            }

            var reactionInstance = effect.GetComponent<StatusEffectInstanceReaction>();
            if (reactionInstance != null && reactionInstance.EffectData != null && reactionInstance.EffectData.EffectID == effectId)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// 모든 상태이상을 제거합니다.
    /// </summary>
    public void ClearAllStatusEffects()
    {
        foreach (var effect in activeEffectPrefabs)
        {
            if (effect != null)
            {
                Destroy(effect);
            }
        }
        activeEffectPrefabs.Clear();
    }

    /// <summary>
    /// 모든 상태이상 UI를 숨깁니다 (오브젝트 파괴 없이).
    /// </summary>
    public void HideAllStatusEffects()
    {
        foreach (var effect in activeEffectPrefabs)
        {
            if (effect != null)
            {
                effect.SetActive(false);
            }
        }
    }

    /// <summary>
    /// 턴이 끝날 때 상태이상 지속시간을 감소시키고 만료된 상태이상을 제거합니다.
    /// </summary>
    public void ApplyStatusEffectsOnTurnEnd()
    {
        Debug.Log("[StatusEffectController] ApplyStatusEffectsOnTurnEnd 시작");
        float startTime = Time.realtimeSinceStartup;
        
        var characterStats = GetComponent<CharacterStats>();
        if (characterStats == null || !characterStats.IsMyTurn)
        {
            Debug.Log("[StatusEffectController] ApplyStatusEffectsOnTurnEnd - 턴이 아니거나 CharacterStats가 null");
            return;
        }

        // 현재 적용된 모든 상태이상 효과를 순회 (역순으로 순회하여 삭제 시 인덱스 문제 방지)
        for (int i = activeEffectPrefabs.Count - 1; i >= 0; i--)
        {
            var effect = activeEffectPrefabs[i];
            if (effect == null) 
            {
                // null인 오브젝트는 리스트에서 제거
                activeEffectPrefabs.RemoveAt(i);
                continue;
            }

            // StatusEffectInstance (지속피해) 처리 - Unity 생명주기 제거
            var instance = effect.GetComponent<StatusEffectInstance>();
            if (instance != null)
            {
                // OnTurnEnd() 제거 - 수동으로 ReduceDuration() 호출
                // instance.OnTurnEnd(); // ← 이 부분 제거!
                if (!instance.isActive)
                {
                    Debug.Log($"[StatusEffectController] 상태이상 만료로 제거: {effect.name}");
                    activeEffectPrefabs.RemoveAt(i);
                }
                continue;
            }

            // StatusEffectInstanceBuff (버프) 처리 - Unity 생명주기 제거
            var buffInstance = effect.GetComponent<StatusEffectInstanceBuff>();
            if (buffInstance != null)
            {
                // OnTurnEnd() 제거 - 수동으로 ReduceDuration() 호출
                // buffInstance.OnTurnEnd(); // ← 이 부분 제거!
                if (!buffInstance.isActive)
                {
                    Debug.Log($"[StatusEffectController] 버프 만료로 제거: {effect.name}");
                    activeEffectPrefabs.RemoveAt(i);
                }
                continue;
            }

            // StatusEffectInstanceReaction (반응) 처리 - Unity 생명주기 제거
            var reactionInstance = effect.GetComponent<StatusEffectInstanceReaction>();
            if (reactionInstance != null)
            {
                // OnTurnEnd() 제거 - 수동으로 ReduceDuration() 호출
                // reactionInstance.OnTurnEnd(); // ← 이 부분 제거!
                if (!reactionInstance.isActive)
                {
                    Debug.Log($"[StatusEffectController] 반응 효과 만료로 제거: {effect.name}");
                    activeEffectPrefabs.RemoveAt(i);
                }
                continue;
            }
        }
        
        float endTime = Time.realtimeSinceStartup;
        Debug.Log($"[StatusEffectController] ApplyStatusEffectsOnTurnEnd 완료 - 소요시간: {(endTime - startTime) * 1000:F2}ms, 남은 상태이상: {activeEffectPrefabs.Count}개");
    }

    /// <summary>
    /// activeEffectPrefabs 리스트를 반환합니다.
    /// </summary>
    public List<GameObject> GetActiveEffectPrefabs()
    {
        return activeEffectPrefabs;
    }
}
