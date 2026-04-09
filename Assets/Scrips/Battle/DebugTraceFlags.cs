/// <summary>
/// 전투/패시브 디버그 출력 스위치. 필요 시 여기만 수정.
/// </summary>
public static class DebugTraceFlags
{
    /// <summary> 패시브 유즈 → AddStatusEffectPrefab → 버프 생성 추적 로그 </summary>
    public const bool PassiveStatusEffectFlow = true;

    /// <summary> StatusEffectInstanceBuff의 UI/지속턴/아이콘 성공 로그(스탯·UI 잡음) </summary>
    public const bool StatBuffDetailLogs = false;

    /// <summary> Mora(001003) GetFinalStatValue·패시브 스탯 보너스 추적 </summary>
    public const bool PassiveStatTraceMora = false;
}
