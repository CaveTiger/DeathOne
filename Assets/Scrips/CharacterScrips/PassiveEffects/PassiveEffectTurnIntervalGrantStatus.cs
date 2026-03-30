using UnityEngine;

/// <summary>
/// 자기 턴 시작마다 유즈 +1, <see cref="PassiveData.useCount"/> 이상이면 <see cref="PassiveData.grantStatusEffectId"/> 부여 후 누적 0. useCount==0이면 매 턴 부여(상시).
/// 첫 누적 전 시작값 <see cref="PassiveData.startUseCount"/> (XML StartCount).
/// 지속·수치는 <see cref="PassiveData.grantStatusDuration"/>, <see cref="PassiveData.grantStatusValue"/> (XML: UseCount, StartCount, Duration, statusEffectID 등).
/// 유사 타입 추가 절차: <see cref="PassiveSystemExtensionGuide"/>.
/// </summary>
public class PassiveEffectTurnIntervalGrantStatus : PassiveEffectBase
{
    public override void Apply(CharacterStats character, PassiveData data)
    {
        // 런타임 스탯 변경 없음. 턴 시작 시 <see cref="OnOwnerTurnStart"/>에서 처리.
    }

    public override void OnOwnerTurnStart(CharacterStats character, PassiveData data)
    {
        if (character == null || data == null || !character.IsActive || character.IsDead)
            return;

        if (DebugTraceFlags.PassiveStatusEffectFlow)
            Debug.Log($"[StatusFxTrace] TurnInterval OnOwnerTurnStart unit={character.Label} passive={data.passiveID} useCount={data.useCount} startUseCount={data.startUseCount} grantId={data.grantStatusEffectId}");

        if (string.IsNullOrWhiteSpace(data.grantStatusEffectId))
        {
            Debug.LogWarning($"[TurnIntervalGrantStatus] {character.Label} 패시브 {data.passiveID}: grantStatusEffectId가 비어 있습니다.");
            return;
        }

        bool fire = character.TryTickOwnerTurnPassiveUseAndShouldFire(data.passiveID, data.useCount, data.startUseCount);
        if (DebugTraceFlags.PassiveStatusEffectFlow)
            Debug.Log($"[StatusFxTrace] TurnInterval TryTick passive={data.passiveID} threshold={data.useCount} → shouldFire={fire}");
        if (!fire)
            return;

        if (StatusEffectManager.Instance == null)
        {
            Debug.LogWarning("[TurnIntervalGrantStatus] StatusEffectManager.Instance가 없습니다.");
            return;
        }

        StatusEffectData effectData = StatusEffectManager.Instance.GetById(data.grantStatusEffectId.Trim());
        if (effectData == null)
        {
            if (DebugTraceFlags.PassiveStatusEffectFlow)
                Debug.LogWarning($"[StatusFxTrace] GetById miss id={data.grantStatusEffectId.Trim()}");
            return;
        }

        int duration = data.grantStatusDuration >= 1 ? data.grantStatusDuration : 1;
        if (DebugTraceFlags.PassiveStatusEffectFlow)
            Debug.Log($"[StatusFxTrace] TurnInterval → AddStatusEffectPrefab id={effectData.EffectID} type={effectData.effectType} dur={duration} val={data.grantStatusValue}");
        character.AddStatusEffectPrefab(effectData, duration, data.grantStatusValue);
    }
}
