/// <summary>
/// 패시브를 데이터·타입·효과로 반복 확장할 때의 작업 순서 (이 타입은 코드에서 참조하지 않음).
/// <para>
/// 1) <see cref="PassiveData"/> — 런타임에 쓸 필드 추가.<br/>
/// 2) <see cref="PassiveLoader.ClonePassiveData"/> — 부모 상속 시 복사할 필드에 동일하게 추가 (누락 시 자식 패시브가 잘못된 기본값을 물려받음).<br/>
/// 3) <see cref="PassiveLoader.ParsePassiveFields"/> — XML 노드 이름과 파싱 규칙 추가.<br/>
/// 4) 매핑형 효과면 — <see cref="PassiveType"/> enum 값 추가, <see cref="PassiveEffectLibrary"/> 딕셔너리에 효과 인스턴스 등록.<br/>
/// 5) <see cref="PassiveEffectBase"/> 상속 클래스 작성. 턴 시작 훅이 필요하면 <c>OnOwnerTurnStart</c> 오버라이드 (호출은 <see cref="CharacterStats.InvokePassivesOnOwnerTurnStart"/>).<br/>
/// 6) 타입별 필수 XML/필드 검사를 로드 직후에 두고 싶으면 <see cref="PassiveLoader"/>의 검증 분기에 케이스 추가.<br/>
/// </para>
/// <para>
/// <b>규칙</b><br/>
/// · <b>스탯 부스트</b>만: 반드시 <c>Type=None</c> + TargetStat/Value(·FloatValue). <see cref="CharacterData.GetFinalStatValue"/>만 사용. 전용 enum/효과 클래스를 두지 않는다.<br/>
/// · <b>TurnIntervalGrantStatus</b>: 유즈+1 후 <see cref="PassiveData.useCount"/> 이상이면 발동·유즈 0. <c>StartCount</c>=첫 누적 전 시작값. + Duration, statusEffectID 등.<br/>
/// · <b>StatusEffectImmunity</b>: <c>ImmuneStatusEffectIDs</c>(CSV)에 적힌 EffectID를 <see cref="CharacterStats.AddStatusEffectPrefab"/> 단계에서 차단.<br/>
/// · <b>그 외 효과</b>: <c>Type=None</c>이 아니면 반드시 (1) <see cref="PassiveEffectLibrary"/>에 매핑된 enum 이름이거나 (2) <c>Type=CustomScript</c> + <c>ScriptClass</c>(리플렉션 가능한 타입 전체 이름).<br/>
/// · XML <c>&lt;Type&gt;</c>은 <see cref="PassiveType"/> enum 이름과 동일한 영문이어야 한다. 표시용 이름은 <c>&lt;Name&gt;</c>.
/// </para>
/// </summary>
public static class PassiveSystemExtensionGuide { }
