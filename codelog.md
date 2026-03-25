# 코드 로그 (Code Log)

각 파일의 상세 스펙과 의도를 기록하는 문서입니다. 코드 손실 방지 및 복구를 위한 참고 자료입니다.

**작성 원칙:**
- 각 파일의 목적과 역할 명시
- 주요 메서드와 기능 설명
- 중요한 로직과 의도 기록
- 다른 파일과의 연관성 명시
- 주의사항 및 특이사항 기록

---

## 목차
1. [핵심 시스템](#핵심-시스템)
2. [전투 시스템](#전투-시스템)
3. [캐릭터 시스템](#캐릭터-시스템)
4. [스킬 시스템](#스킬-시스템)
5. [축복 시스템](#축복-시스템)
6. [AI 시스템](#ai-시스템)
7. [UI 시스템](#ui-시스템)
8. [데이터 관리](#데이터-관리)

---

## 핵심 시스템

### GameManager.cs
**경로:** `Assets/Scrips/GameManager.cs`

**목적:** 게임 전체 초기화 및 매니저 실행 순서 관리

**주요 기능:**
- 싱글톤 패턴 (DontDestroyOnLoad)
- 데이터 로더 초기화 순서 제어
  - CharacterLoader, StageLoader, SkillLoader 먼저 실행
  - 그 다음 GameProgressManager 초기화
- 게임 일시정지/재개 기능

**중요 로직:**
- `Awake()`: 모든 데이터 로더를 먼저 실행한 후 GameProgressManager 초기화
- 실행 순서가 중요: 데이터 로드 → 세이브 데이터 로드

**연관 파일:**
- CharacterLoader.cs
- StageLoader.cs
- SkillLoader.cs
- GameProgressManager.cs

---

### GameProgressManager.cs
**경로:** `Assets/Scrips/GameProgressManager.cs`

**목적:** 게임 진행 데이터 저장/로드 (세이브 시스템)

**주요 데이터 구조:**
```csharp
public class GameProgressData
{
    public string gameVersion;
    public List<CharacterData> characterInventory; // 모든 캐릭터 데이터
    public List<string> unlockedSkills; // 해금된 스킬 ID
    public List<string> unlockedCharacters; // 해금된 캐릭터 ID
    public int soulDust; // 영혼먼지
    public int essence; // 강자의 정수 (현재 보유량)
    public int totalEssence; // 강자의 정수 총 획득량
    public string[] activeBlessingByLine = new string[5]; // 라인별 비트마스크 (0~4)
    public List<string> tacticalBlessingTargets; // 전술 축복 대상 ("축복ID:슬롯번호")
    public string[] savedSkillPreset = new string[4]; // 주인공 스킬 프리셋
}
```

**주요 메서드:**
- `SaveGameProgress(int slot)`: 세이브 슬롯에 저장
- `LoadGameProgress(int slot)`: 세이브 슬롯에서 로드
- `GetEssence()`: 현재 정수 반환
- `GetTotalEssence()`: 총 획득 정수 반환
- `SpendEssence(int amount)`: 정수 소모 (총량 변화 없음)
- `AddEssence(int amount, bool increaseTotal)`: 정수 추가 (총량 증가 여부 선택)

**중요 로직:**
- **정수 시스템**: `essence`는 현재 보유량, `totalEssence`는 총 획득량
  - 정수 소모 시: `essence`만 감소, `totalEssence`는 유지
  - 정수 획득 시: `essence` 증가, `increaseTotal=true`면 `totalEssence`도 증가
- **축복 비트마스크**: `activeBlessingByLine[0~4]`에 문자열로 저장 (예: "10010" = 1번과 4번 활성화)
- **전술 축복 대상**: `tacticalBlessingTargets`에 "축복ID:슬롯번호" 형식으로 저장

**연관 파일:**
- BlessingPanel.cs (축복 비트마스크 읽기/쓰기)
- BlessingManager.cs (전술 축복 대상 읽기/쓰기)

**주의사항:**
- `CurrentSaveData` 프로퍼티는 자동으로 로드 시도 (null이면 LoadGameProgress 호출)
- 세이브 파일 경로: `Application.persistentDataPath/save_slot_{slot}.json`

---

## 전투 시스템

### BattleManager.cs
**경로:** `Assets/Scrips/Battle/BattleManager.cs`

**목적:** 전투 씬 관리, 유닛 생성, 축복 적용

**주요 필드:**
- `public Transform playerSlot`: 주인공 슬롯 (1번)
- `public Transform[] allySlots`: 아군 슬롯 (2~4번)
- `public Transform[] enemySlots`: 적 슬롯
- `public List<CharacterStats> allCharacters`: 전투 참여 모든 캐릭터

**주요 메서드:**
- `StartBattle()`: 전투 시작 (유닛 생성, 축복 적용)
- `SpawnAllUnits()`: 아군/적 유닛 생성
- `SpawnUnitEnemy(string id, Transform slot)`: 적 유닛 생성 및 AI 할당
- `ApplyBlessingsToMainCharacter()`: 전투 시작 시 축복 적용
- `AssignAIComponent(GameObject obj, CharacterStats characterStats)`: 패턴에 따라 AI 할당
- `SubscribeEmergencyMeasureBlessing()`: 응급 조치 축복 이벤트 구독

**중요 로직:**
- **아군 생성**: `SpawnManager.allyPartyData`에서 가져와서 생성
  - 0번 인덱스: `playerSlot`에 배치 (주인공)
  - 1~3번 인덱스: `allySlots[0~2]`에 배치
  - 주인공(000001)이 아니면 `flipX = true`
- **적 생성**: `SpawnManager.enemyIDs`에서 가져와서 생성
  - `AssignAIComponent()`에서 `Pattern`에 따라 AI 할당
    - `PatternType.Adelia` → `AdeliaEnemyAIController`
    - `PatternType.Default` → `DefaultEnemyAIController`
- **축복 적용**: `ApplyBlessingsToMainCharacter()`에서 활성화된 축복 적용
  - `applyToAllAllies == true`: 모든 아군에게 적용
  - `applyToAllAllies == false`: 주인공에게만 적용
- **응급 조치 축복**: 전투 시작 시 모든 캐릭터에 `OnCollapseCrisisEvent` 구독

**연관 파일:**
- SpawnManager.cs (파티 데이터)
- TurnManager.cs (턴 관리)
- BlessingManager.cs (축복 적용)
- EnemyAIController.cs (AI 할당)

**주의사항:**
- 싱글톤이 아님 (씬마다 새로 생성)
- `allCharacters`는 전투 시작 시 초기화됨

---

### TurnManager.cs
**경로:** `Assets/Scrips/Battle/TurnManager.cs`

**목적:** 턴 순서 관리, 턴 전환, 전투 종료 체크

**주요 필드:**
- `private List<SlotHandler> turnQueue`: 턴 가능한 캐릭터 슬롯 리스트
- `public List<SlotHandler> allSlots`: 모든 슬롯 (Pslot, Eslot)
- `public CharacterStats currentCaster`: 현재 턴 캐릭터
- `public List<SlotHandler> playerSlots`: 아군 슬롯 리스트
- `public List<SlotHandler> enemySlots`: 적 슬롯 리스트

**주요 메서드:**
- `ResetTurn()`: 턴 리셋 (모든 생존 캐릭터의 `TurnChanse = true`)
- `TurnDecider()`: 다음 턴 캐릭터 결정 (속도순 정렬)
- `StartTurn(CharacterStats character)`: 턴 시작 처리
- `EndTurn()`: 턴 종료 처리
- `CheckBattleEnd()`: 전투 종료 체크
- `GetNextTurnCharacter()`: 속도순으로 다음 턴 캐릭터 반환

**중요 로직:**
- **턴 리셋**: `ResetTurn()`에서 모든 생존 캐릭터를 `turnQueue`에 추가하고 `TurnChanse = true`
  - 속도값 변동을 잡아내기 위해 턴 주기 시작 시 속도 조회
- **턴 결정**: `GetNextTurnCharacter()`에서 속도 내림차순 정렬, 같으면 슬롯 인덱스 오름차순
- **턴 시작**: `StartTurn()`에서 캐릭터 타입에 따라 분기
  - `IsPlayer == true && EnemyAIController 있음`: 아군 AI 턴 → `EnemyActionRoutine()` 호출
  - `IsPlayer == true && EnemyAIController 없음`: 플레이어 턴 → 타겟 선택 UI 표시
  - `IsPlayer == false`: 적 턴 → `EnemyActionRoutine()` 호출
- **전투 종료 체크**: `CheckBattleEnd()`에서 체크
  - 1번 슬롯(주인공) 사망 → 즉시 게임오버
  - 모든 아군 사망 → 패배
  - 모든 적 사망 → 승리

**연관 파일:**
- SlotHandler.cs (슬롯 관리)
- EnemyAIController.cs (AI 턴)
- BattleManager.cs (전투 종료 처리)

**주의사항:**
- `TurnChanse`가 `false`가 되면 해당 턴 주기에서 제외됨
- 모든 캐릭터의 `TurnChanse`가 `false`가 되면 `ResetTurn()` 호출

---

## 캐릭터 시스템

### CharacterStats.cs
**경로:** `Assets/Scrips/CharacterScrips/CharacterStats.cs`

**목적:** 캐릭터 스탯 관리, 피해/힐 처리, 붕괴 상태 시스템

**주요 필드:**
```csharp
public string Label; // 캐릭터 이름
public int Hp, MaxHp, Atk, Def; // 체력, 최대체력, 공격력, 방어력
public float Evasion, Accuracy, CollapseChance; // 회피율, 명중률, 붕괴 확률
public int Speed; // 속도 (턴 순서 결정)
public string[] Skills = new string[4]; // 스킬 ID 배열
public bool IsDead = false; // 사망 상태
public bool IsActive = true; // 행동 가능 여부 (false면 스킬로 인한 행동불가)
public bool IsMyTurn = false; // 현재 턴 여부
public bool IsPlayer = true; // 아군 여부
public bool TurnChanse = false; // 턴 가능 여부
public PatternType Pattern; // AI 패턴 타입
public RarityList Rarity; // 등급
public string CharacterId; // 캐릭터 ID (예: "000001")
```

**주요 이벤트:**
- `OnTakeDamageEvent`: 피해 받을 때 발생
- `OnHealEvent`: 힐 받을 때 발생
- `OnDeathEvent`: 사망할 때 발생
- `OnBuffEvent`: 버프 받을 때 발생
- `OnCollapseCrisisEvent`: 붕괴 위기 상태 진입 시 발생

**주요 메서드:**
- `SetData(CharacterData data)`: 캐릭터 데이터 설정
  - `Pattern`, `Rarity` 설정 (AI 할당에 필요)
- `TakeDamage(int dmg, float attackerAccuracy, SkillData skillData, Vector3? attackerPosition)`: 피해 처리
  - 크리티컬 확률 계산 (회피율 - 명중률 기반)
  - 피해무시 효과 체크 (StatusEffectController)
  - 체력 감소 후 `Deathcheck()` 호출
- `Heal(int amount)`: 힐 처리
- `Deathcheck()`: 사망 체크 및 붕괴 상태 처리
- `TryCollapse()`: 붕괴 확률 체크
- `Update()`: 안전장치 (TakeDamage를 거치지 않은 사망 체크)

**중요 로직:**
- **붕괴 상태 시스템** (아군 전용, 주인공 제외):
  - `Hp <= 0 && !IsDead`일 때 `TryCollapse()` 호출
  - 붕괴 확률 체크: `Random.value < CollapseChance`
  - 붕괴 실패 시: `CollapseChance += 0.2f` (최대 1.0), `IsDead = false` 유지, `OnCollapseCrisisEvent` 발생
  - 붕괴 성공 시: `IsDead = true` 설정
- **Update() 안전장치**: 
  - 아군(주인공 제외)의 빈사 상태(`Hp <= 0 && !IsDead`)는 정상 상태이므로 재체크하지 않음
  - 주인공 또는 적의 경우에만 `Deathcheck()` 호출
- **피해 처리 흐름**:
  1. 크리티컬 확률 계산
  2. 피해무시 효과 체크 (StatusEffectController)
  3. 체력 감소
  4. `OnTakeDamageEvent` 발생
  5. `Deathcheck()` 호출
  6. `DeathAction()` 호출

**연관 파일:**
- CharacterData.cs (데이터 구조)
- StatusEffectController.cs (상태이상 처리)
- BattleEffectManager.cs (이벤트 구독)

**주의사항:**
- `CollapseChance`는 캐릭터 데이터에 없음, 스테이지 기준으로 관리
- 아군의 빈사 상태는 `Update()`에서 재체크하지 않음 (반복 체크 방지)

---

## 스킬 시스템

### SkillManager.cs
**경로:** `Assets/Scrips/SkillSystem/SkillManager.cs`

**목적:** 스킬 사용, 스킬 효과 적용, 스킬 연출

**주요 메서드:**
- `UseSkill(SkillData skill, CharacterStats caster, CharacterStats target, SkillData motionData)`: 스킬 사용
- `UseRangeSkill()`: 범위 스킬 사용
- `UseAllTargetSkill()`: 전체 타겟 스킬 사용
- `GetRangeTargets()`: 범위 스킬 타겟 리스트 반환

**중요 로직:**
- **스킬 타입 분기**:
  - `IsAllTargetSkill()`: AllEnemies, AllAllies → `UseAllTargetSkill()`
  - `IsRangeSkill()`: All, Adjacent, Random 등 → `UseRangeSkill()`
  - 그 외: 단일 타겟 스킬 → `PlaySkillEffect()`
- **타겟 검증**:
  - 공격 스킬: 아군 타겟 불가
  - 버프/힐 스킬: 적 타겟 불가 (단, "Me"는 본인만)
- **안전장치**: 범위/전체 타겟 스킬에서 타겟이 없으면 `TurnManager.Instance.EndTurn()` 호출 (게임 멈춤 방지)

**연관 파일:**
- SkillData.cs (스킬 데이터)
- TurnManager.cs (턴 종료)

**주의사항:**
- 스킬 사용 시 `skill.CurrentCooldown = skill.Cooldown` 설정
- 스킬 연출은 코루틴으로 처리

---

## 축복 시스템

### BlessingManager.cs
**경로:** `Assets/Scrips/CharacterScrips/BlessingManager.cs`

**목적:** 축복 데이터 관리, 축복 효과 적용, 활성화 상태 관리

**주요 필드:**
- `private Dictionary<string, BlessingData> blessingDict`: 모든 축복 데이터 (ID → 데이터)
- `private Dictionary<string, int> activeBlessings`: 활성화된 축복 (ID → 칸 수)
- `private Dictionary<string, int> tacticalBlessingTargets`: 전술 축복 대상 (ID → 슬롯 번호)

**주요 메서드:**
- `LoadAllBlessings()`: Resources에서 모든 축복 ScriptableObject 로드
- `ApplyBlessing(CharacterStats character, BlessingData blessingData)`: 축복 효과 적용
- `ApplySpecialEffect()`: 리플렉션으로 특수 효과 클래스 실행
- `ApplyStatEffect()`: 단순 스탯 효과 적용
- `GetTacticalBlessingTarget(string blessingID)`: 전술 축복 대상 조회
- `SetTacticalBlessingTarget(string blessingID, int slotNumber)`: 전술 축복 대상 설정

**중요 로직:**
- **리플렉션 시스템**: `scriptClass` 문자열로 클래스 찾기
  - `Type.GetType(blessingData.scriptClass)` 시도
  - 실패 시 `Type.GetType(blessingData.scriptClass + ", Assembly-CSharp")` 시도
  - `BlessingEffectBase`를 상속받은 클래스인지 확인
  - `Activator.CreateInstance()`로 인스턴스 생성 후 `Apply()` 호출
- **스탯 효과**: `targetStat`과 `value`로 직접 스탯 수정
  - Hp/MaxHp: 체력 비율 유지하며 증가
  - Atk/Def/Speed: 정수로 반올림하여 증가
  - Evasion/Accuracy: 실수로 증가
- **전술 축복 대상**: 슬롯 번호로 저장 (1=주인공, 2~4=아군)

**연관 파일:**
- BlessingData.cs (축복 데이터 구조)
- BlessingEffectBase.cs (축복 효과 기본 클래스)
- BlessingPanel.cs (활성화 상태 관리)
- GameProgressManager.cs (전술 축복 대상 저장/로드)

**주의사항:**
- `ApplyBlessing()`은 정수를 소모하지 않음 (효과만 적용)
- 정수 소모는 `BlessingPanel.ToggleBlessingInLine()`에서만 발생

---

### BlessingPanel.cs
**경로:** `Assets/Scrips/CharacterScrips/BlessingPanel.cs`

**목적:** 축복 UI 관리, 활성화/비활성화, 비트마스크 저장/로드

**주요 필드:**
- `private string[] activeBlessingByLine = new string[5]`: 라인별 비트마스크 (0~4)

**주요 메서드:**
- `LoadActiveBlessingsFromSave()`: 세이브에서 비트마스크 로드 및 축복 활성화
- `ToggleBlessingInLine(string blessingID, int lineIndex)`: 축복 토글 및 비트마스크 업데이트
- `GetActiveBlessingByLine()`: 비트마스크 배열 반환

**중요 로직:**
- **비트마스크 형식**: 문자열로 저장 (예: "10010" = 1번과 4번 축복 활성화)
- **로드 시**: 비트마스크 파싱하여 `BlessingManager.Instance.ActivateBlessing()` 호출
- **저장 시**: `GameProgressManager`에 비트마스크 배열 전달

**연관 파일:**
- GameProgressManager.cs (비트마스크 저장/로드)
- BlessingManager.cs (축복 활성화)

---

### BlessingEffectBase.cs
**경로:** `Assets/Scrips/CharacterScrips/BlessingEffects/BlessingEffectBase.cs`

**목적:** 모든 축복 효과의 기본 추상 클래스

**주요 메서드:**
- `abstract void Apply(CharacterStats character, BlessingData blessingData)`: 효과 적용 (상속받아 구현)

**상속 클래스:**
- `BlessingEffectCriticalChance`: 크리티컬 확률 증가 (예시)
- `BlessingEffectEmergencyMeasure`: 응급 조치 (붕괴 위기 시 자동 힐)
- `BlessingEffectOneForAll`: 하나를 위한 모두 (파티 3명의 ATK/DEF를 대상에게 전달)
- `BlessingEffectAllForOne`: 모두를 위한 하나 (대상의 ATK/DEF를 파티 전체에 분배)
- `BlessingEffectAuthorityDelegation`: 권한대행 (AI 할당 + 스탯 강화)

---

### BlessingEffectEmergencyMeasure.cs
**경로:** `Assets/Scrips/CharacterScrips/BlessingEffects/BlessingEffectEmergencyMeasure.cs`

**목적:** 붕괴 위기 상태 진입 시 자동 힐 제공

**주요 로직:**
- `Apply()`: `OnCollapseCrisisEvent` 구독 (정적 변수로 중복 구독 방지)
- `OnCollapseCrisis()`: 붕괴 위기 시 최대 체력의 30% 힐, `CollapseChance` 초기화
- `SubscribeToCharacter()`: 전투 시작 시 새로 생성된 캐릭터에 구독 (BattleManager에서 호출)

**연관 파일:**
- CharacterStats.cs (`OnCollapseCrisisEvent`)
- BattleManager.cs (`SubscribeEmergencyMeasureBlessing()`)

---

### BlessingEffectOneForAll.cs
**경로:** `Assets/Scrips/CharacterScrips/BlessingEffects/BlessingEffectOneForAll.cs`

**목적:** 파티 3명(주인공 제외)의 ATK, DEF를 각각 1씩 빼서 대상 하나에게 추가

**주요 로직:**
- `BlessingManager.GetTacticalBlessingTarget()`로 대상 슬롯 번호 가져오기
- 파티 멤버 중 대상 제외한 3명 선택
- 각 멤버의 ATK, DEF를 1씩 감소
- 대상에게 감소한 만큼 추가

**연관 파일:**
- BlessingManager.cs (전술 축복 대상 조회)
- BattleManager.cs (슬롯으로 캐릭터 찾기)

---

### BlessingEffectAllForOne.cs
**경로:** `Assets/Scrips/CharacterScrips/BlessingEffects/BlessingEffectAllForOne.cs`

**목적:** 지정 대상의 ATK, DEF를 3씩 깎고 파티 전체에 나눠주기

**주요 로직:**
- 대상의 ATK, DEF를 3씩 감소 (최소 0)
- 감소한 스탯을 파티 전체 멤버 수로 나눠서 분배
- 나머지는 첫 번째 멤버에게 추가

---

### BlessingEffectAuthorityDelegation.cs
**경로:** `Assets/Scrips/CharacterScrips/BlessingEffects/BlessingEffectAuthorityDelegation.cs`

**목적:** 아군을 AI화 시키고 스탯 강화

**주요 로직:**
- 대상 캐릭터에 `AllyAIController` 추가
- 주인공(000001)은 AI화 불가
- 스탯 강화: ATK +5, DEF +5, Speed +3 (기본값, 축복 데이터의 value 사용 가능)

**연관 파일:**
- AllyAIController.cs (아군 AI)
- BattleManager.cs (슬롯으로 캐릭터 찾기)

---

## 3라인 축복 시스템 (전술 축복)

**관련 파일:**
- `BlessingEffectOneForAll.cs` - 하나를 위한 모두
- `BlessingEffectAllForOne.cs` - 모두를 위한 하나
- `BlessingEffectAuthorityDelegation.cs` - 권한대행

**현재 상태:**
- 모든 3라인 축복 효과 클래스 구현 완료 (2024-12-XX 복구)
- 전술 축복 대상 선택 시스템 (`TacticalBlessingTargetSelector`) 구현 완료
- `BlessingManager`의 전술 축복 대상 관리 시스템 구현 완료

**테스트 버전 관련:**
- 테스트 버전에서는 3라인 축복을 활성화할 정수가 충분하지 않으므로 별도의 제외 로직 불필요
- 정상 버전에서는 정수 획득 후 3라인 축복 사용 가능

**추후 계획:**
- 테스트 버전에서 3라인 축복 버튼 비활성화/숨김 처리 (필요 시)
- 3라인 축복 밸런스 조정 (필요 시)

---

## AI 시스템

### EnemyAIController.cs
**경로:** `Assets/Scrips/Battle/EnemyAIController.cs`

**목적:** 모든 AI 컨트롤러의 기본 추상 클래스

**주요 메서드:**
- `IEnumerator EnemyActionRoutine(CharacterStats enemy)`: AI 행동 루틴
  - 스킬 선택 → 타겟 선택 → 스킬 사용
  - 아군 AI와 적 AI의 타겟 리스트 자동 분기
- `abstract string ChooseSkillID()`: 스킬 선택 (상속받아 구현)
- `abstract CharacterStats ChooseTarget(List<CharacterStats> targets)`: 타겟 선택 (상속받아 구현)
- `UseSkill(string skillID, CharacterStats caster, CharacterStats target)`: 스킬 사용

**중요 로직:**
- **타겟 리스트 분기**:
  - `enemy.IsPlayer == true`: 아군 AI → 적 리스트 선택
  - `enemy.IsPlayer == false`: 적 AI → 아군 리스트 선택

**상속 클래스:**
- `DefaultEnemyAIController`: 기본 랜덤 AI
- `AdeliaEnemyAIController`: 아델리아 보스 전용 AI (턴 기반 패턴)
- `AllyAIController`: 아군 전용 AI

**연관 파일:**
- TurnManager.cs (턴 시작 시 호출)
- SkillManager.cs (스킬 사용)

---

### DefaultEnemyAIController.cs
**경로:** `Assets/Scrips/Battle/DefaultEnemyAIController.cs`

**목적:** 기본 랜덤 AI 구현

**주요 로직:**
- `ChooseSkillID()`: 사용 가능한 스킬 중 랜덤 선택
- `ChooseTarget()`: 주인공 연속 공격 방지 시스템 포함
  - 주인공이 3회 연속 공격받으면 다른 플레이어로 강제 타겟팅

---

### AdeliaEnemyAIController.cs
**경로:** `Assets/Scrips/Battle/AdeliaEnemyAIController.cs`

**목적:** 아델리아 보스 전용 AI (턴 기반 패턴)

**주요 로직:**
- `ChooseSkillID()`: 턴 카운트 기반 패턴
  - 1~4턴: 스킬 1 (어설픈 다루기)
  - 5턴: 스킬 3 (상황 관찰) - 버프
  - 6턴: 스킬 2 (버서크 임펙트) - 연계 공격
  - 7턴 이후: 6턴 패턴 순환
- `ChooseTarget()`: 아군 우선 타겟팅
  - 아군(주인공 제외)이 살아있으면 아군 중 랜덤 선택
  - 아군이 모두 죽으면 주인공 타겟팅

**연관 파일:**
- BattleManager.cs (`PatternType.Adelia`로 할당)

---

### AllyAIController.cs
**경로:** `Assets/Scrips/Battle/AllyAIController.cs`

**목적:** 아군 전용 AI 컨트롤러

**주요 로직:**
- `ChooseSkillID()`: 기본 랜덤 선택 (추후 고도화 예정)
- `ChooseTarget()`: 적 리스트에서 랜덤 선택
  - `players` 파라미터는 무시하고 `TurnManager.allSlots`에서 적 찾기

**연관 파일:**
- BlessingEffectAuthorityDelegation.cs (AI 할당)
- TurnManager.cs (아군 AI 턴 처리)

**주의사항:**
- 추후 고도화 예정: 힐러/버퍼/딜러 포지션 판단, 턴 패턴, 조건부 스킬 등

---

## UI 시스템

### TacticalBlessingTargetSelector.cs
**경로:** `Assets/Scrips/UI/TacticalBlessingTargetSelector.cs`

**목적:** 전술 축복의 대상 선택 UI

**주요 기능:**
- 파티 멤버 선택 버튼 생성
- 축복 타입에 따른 제목 텍스트 자동 변경
- 선택된 캐릭터 ID를 `BlessingManager.SetTacticalBlessingTarget()`에 전달

**연관 파일:**
- BlessingManager.cs (대상 저장)
- BattleSettingManager.cs (파티 멤버 정보)

---

## 데이터 관리

### CharacterLoader.cs
**경로:** `Assets/Scrips/CharacterScrips/CharacterLoader.cs`

**목적:** 캐릭터 XML 파싱 및 데이터 로드

**주요 로직:**
- XML에서 `Pattern` 필드 파싱 (`PatternType` enum)
- `OverrideCharacter()`: 부모-자식 관계 처리
  - `ParentID`가 있으면 부모 데이터 클로닝 후 자식 데이터로 오버라이드
  - `Pattern != PatternType.Default`면 오버라이드

**중요 필드:**
- `PatternType` enum: Default, Attaker, Defender, Tactician, Adelia, Gadian, Hunter

**연관 파일:**
- CharacterData.cs (데이터 구조)
- BattleManager.cs (AI 할당에 Pattern 사용)

---

### SkillLoader.cs
**경로:** `Assets/Scrips/SkillSystem/SkillLoader.cs`

**목적:** 스킬 XML 파싱 및 데이터 로드

**주요 로직:**
- XML에서 `UseCountYes`, `UseCount` 필드 파싱
- `OverrideSkill()`: 부모-자식 관계 처리
  - `UseCountYes`가 `true`면 오버라이드
  - `UseCount`가 0이 아니면 오버라이드

**중요 필드:**
- `UseCountYes` (bool): 횟수 제한 여부
- `UseCount` (int): 사용 가능 횟수

**연관 파일:**
- SkillData.cs (데이터 구조)

---

## 특수 시스템

### 붕괴 상태 시스템
**관련 파일:** `CharacterStats.cs`

**의도:** 아군이 죽을 때 즉시 사망하지 않고 빈사 상태로 유지, 회생 기회 제공

**동작 방식:**
1. 아군(주인공 제외)이 `Hp <= 0`이 되면 `TryCollapse()` 호출
2. `Random.value < CollapseChance`로 붕괴 확률 체크
3. 붕괴 실패 시:
   - `IsDead = false` 유지 (빈사 상태)
   - `CollapseChance += 0.2f` (최대 1.0)
   - `OnCollapseCrisisEvent` 발생
4. 붕괴 성공 시: `IsDead = true` 설정

**주의사항:**
- `Update()`에서 아군의 빈사 상태를 재체크하지 않음 (반복 체크 방지)
- `CollapseChance`는 스테이지 기준으로 관리 (캐릭터 데이터에 없음)

---

### 전술 축복 시스템
**관련 파일:** `BlessingManager.cs`, `TacticalBlessingTargetSelector.cs`, 각종 `BlessingEffect*.cs`

**의도:** 대상 선택이 필요한 축복 효과 구현

**동작 방식:**
1. 축복 활성화 시 `TacticalBlessingTargetSelector`로 대상 선택 UI 표시
2. 선택된 슬롯 번호를 `BlessingManager.SetTacticalBlessingTarget()`에 저장
3. 전투 시작 시 `BlessingManager.GetTacticalBlessingTarget()`로 대상 조회
4. 축복 효과 클래스에서 대상 캐릭터 찾아서 효과 적용

**저장 방식:**
- `GameProgressManager.tacticalBlessingTargets`: `List<string>` 형식
- 형식: `["축복ID:슬롯번호", ...]` (예: `["030001:2"]`)

---

### 아군 AI 시스템
**관련 파일:** `AllyAIController.cs`, `TurnManager.cs`, `BlessingEffectAuthorityDelegation.cs`

**의도:** 축복을 통해 아군을 AI화하여 자동 행동

**동작 방식:**
1. "권한대행" 축복 활성화 시 대상 선택
2. 전투 시작 시 `BlessingEffectAuthorityDelegation.Apply()` 호출
3. 대상 캐릭터에 `AllyAIController` 컴포넌트 추가
4. `TurnManager.StartTurn()`에서 `IsPlayer == true && EnemyAIController 있음` 체크
5. 아군 AI 턴으로 처리하여 `EnemyActionRoutine()` 호출

**주의사항:**
- 주인공(000001)은 AI화 불가
- 아군 AI는 적을 타겟으로 선택

---

## 수정 금지 규칙

### 승리/패배 로직 수정 금지
**파일:**
- `TurnManager.CheckBattleEnd()` - 수정 금지
- `TurnManager.EndBattle()` - 수정 금지
- `BattleManager.EndBattle()` - 수정 금지
- `RewardManager.ProcessBattleReward()` - 수정 금지

**이유:** 전투 종료 관련 씬 전환 로직이 복잡하게 얽혀있음

**예외:** 직접적인 수정 요구가 있을 때만 접근 가능

---

## 데이터 구조 참고

### CharacterStats 주요 변수명
```csharp
public string Label; // 이름
public int Hp, MaxHp, Atk, Def; // 체력, 최대체력, 공격력, 방어력
public float Evasion, Accuracy; // 회피율, 명중률
public int Speed; // 속도
public string[] Skills = new string[4]; // 스킬 ID 배열
public bool IsDead = false; // 사망 상태
public bool IsActive = true; // 행동 가능 여부
public bool IsMyTurn = false; // 현재 턴 여부
public bool IsPlayer = true; // 아군 여부
public bool TurnChanse = false; // 턴 가능 여부
public PatternType Pattern; // AI 패턴
public RarityList Rarity; // 등급
```

**주의:** 이미 작성된 변수명은 변경하지 않음

---

## 네이밍 컨벤션

- **스킬 관련**: `Skill` 접미사 (예: `FireballSkill`)
- **상태이상 관련**: `Effect` 접미사 (예: `PoisonEffect`)
- **UI 관련**: `UI` 접미사 (예: `BattleUI`)
- **매니저 클래스**: `Manager` 접미사 (예: `BattleManager`)
- **스크립테이블 오브젝트**: `Data` 접미사 (예: `SkillData`)

---

## 파일 구조 규칙

- 스크립트: `Assets/Scrips/` 폴더
- 프리팹: `Assets/Prefabs/` 폴더
- 리소스: `Assets/Resources/` 폴더
- UI 관련: `Assets/Scrips/UI/` 폴더
- 새로운 스크립트 생성 시: `scriptmap.md`에 추가

---

## XML 파싱 규칙

- XML 파일: `Assets/Resources/Data/` 하위 폴더
- 파싱 단계: 파싱 → 클로닝 → 오버라이드 → 딕셔너리 저장
- 클로닝: 파싱한 데이터를 복사하여 새로운 데이터 생성 (모두 오버라이드 후 딕셔너리에 저장)
- 파싱 코드 수정 시: 클로닝 오버라이드 방식을 고려하여 변경
- 오류 발생 시: 가장 먼저 클로닝 오버라이드 문제 확인

---

**마지막 업데이트:** 2024-12-XX
**작성 목적:** 코드 손실 방지 및 복구를 위한 상세 스펙 기록

