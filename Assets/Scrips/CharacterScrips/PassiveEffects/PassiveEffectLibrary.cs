using System.Collections.Generic;

/// <summary>
/// 패시브 효과 클래스를 <see cref="PassiveType"/> enum 기준으로 매핑하는 정적 라이브러리.
/// 이 구조를 잊지 말 것 — XML/데이터와의 연결 방식이 여러 갈래로 나뉜다.
///
/// 1) 이 라이브러리(매핑형)
///    - XML의 &lt;Type&gt;에는 **C# 클래스 이름이 아니라** <see cref="PassiveType"/>과 동일한 문자열을 적는다.
///      예: ManaBoost, TurnIntervalGrantStatus (스탯만 올릴 때는 Type=None, 여기에 등록하지 않음)
///    - &lt;ScriptClass&gt;는 이 경로에서 사용하지 않는다. 아래 딕셔너리가 Type → 인스턴스를 연결한다.
///    - 적용 시점: <see cref="CharacterStats"/>의 <c>ApplyPassives</c>에서 <c>TryGetEffect</c> 호출.
///
/// 2) 커스텀 리플렉션형
///    - XML: &lt;Type&gt;CustomScript&lt;/Type&gt;, &lt;ScriptClass&gt;에 <c>Type.GetType</c>이 해석할 수 있는 타입 문자열.
///    - Unity에서는 어셈블리 한정 이름(예: Namespace.ClassName, Assembly-CSharp)이 필요할 수 있다.
///    - 클래스는 반드시 <see cref="PassiveEffectBase"/>를 상속해야 한다.
///
/// 3) 비클래스형 스탯 부스트
///    - XML: &lt;Type&gt;None&lt;/Type&gt; + TargetStat/Value 등.
///    - 전투 인스턴스의 라이브러리 Apply가 아니라 <see cref="CharacterData.GetFinalStatValue"/>에서 상시 반영된다.
///
/// --- English ---
/// Static registry mapping <see cref="PassiveType"/> enum values to passive effect instances.
/// Do not confuse this with other XML wiring — there are several parallel paths.
///
/// 1) This library (enum-mapped)
///    - In XML, &lt;Type&gt; must be the **enum name** (e.g. ManaBoost, TurnIntervalGrantStatus). Stat-only passives use None, not a mapped type.
///    - &lt;ScriptClass&gt; is not used on this path; the dictionary below binds Type → effect instance.
///    - Applied from <see cref="CharacterStats"/> <c>ApplyPassives</c> via <c>TryGetEffect</c>.
///
/// 2) Custom reflection path
///    - XML: &lt;Type&gt;CustomScript&lt;/Type&gt;, &lt;ScriptClass&gt; = string resolvable by <c>Type.GetType</c>.
///    - In Unity you may need an assembly-qualified name (e.g. Namespace.ClassName, Assembly-CSharp).
///    - Types must inherit <see cref="PassiveEffectBase"/>.
///
/// 3) Data-driven stat boost (no effect class)
///    - XML: &lt;Type&gt;None&lt;/Type&gt; plus TargetStat/Value, etc.
///    - Always-on stat bonuses are applied in <see cref="CharacterData.GetFinalStatValue"/>, not via this library’s Apply.
///
/// 4) 턴 훅
///    - <see cref="PassiveEffectBase.OnOwnerTurnStart"/> — <see cref="CharacterStats.InvokePassivesOnOwnerTurnStart"/>가 전투 턴 시작 시 호출.
///
/// 새 매핑형 타입 추가 절차: <see cref="PassiveSystemExtensionGuide"/>.
/// </summary>
public static class PassiveEffectLibrary
{
    private static readonly Dictionary<PassiveType, PassiveEffectBase> effectByType =
        new Dictionary<PassiveType, PassiveEffectBase>
        {
            { PassiveType.ManaBoost, new PassiveEffectManaBoost() },
            { PassiveType.TurnIntervalGrantStatus, new PassiveEffectTurnIntervalGrantStatus() }
        };

    public static bool TryGetEffect(PassiveType type, out PassiveEffectBase effect)
    {
        return effectByType.TryGetValue(type, out effect);
    }
}
