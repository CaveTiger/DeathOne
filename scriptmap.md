# 스크립트맵

> **경로 안내**: 실제 C# 스크립트 루트는 `Assets/Scrips/` 입니다. (과거 문서의 `Assets/Scripts/`, `CharacterScripts` 표기는 동일 폴더를 가리키지 않으므로 이 문서에서는 `Scrips` / `CharacterScrips` 로 통일합니다.)

---

## Assets/Scrips/ (루트)

- **GameManager.cs**: 씬·일시정지·전역 흐름 관리
- **GameProgressManager.cs**: 진행·세이브 데이터
- **SpawnManager.cs**: 전투 유닛 스폰
- **StageManager.cs**: 스테이지 진행·전투 진입 연계
- **RewardManager.cs**: 전투 보상 처리
- **SoundManager.cs**: BGM/SFX 재생·볼륨
- **UserSettingsManager.cs**: 사용자 설정(사운드 등) 저장/로드
- **Debuger.cs**: 디버그용 유틸

---

## Assets/Scrips/Battle/

- **BattleManager.cs**: 전투 루프, 유닛 생성, 전투 상태
- **TurnManager.cs**: 턴 순서, 턴 진행, 종료 조건
- **TurnTransitionSkipManager.cs**: 턴 전환 대기 스킵
- **TurnBlockHandler.cs**: 턴 블록(행동 제한) 처리
- **BattleUIManager.cs**: 전투 HUD·슬롯·입력 UI 연동
- **BattleEffectManager.cs**: 피격·이동·고스트 프록시 등 전투 연출
- **BattleCamera.cs**: 전투 카메라
- **BattleSnapshotManager.cs**: 전투 스냅샷/복원 등
- **SlotHandler.cs**: 슬롯 UI·사망 아이콘 등 슬롯 단위 처리
- **BackGroundHandler.cs**: 전투 배경
- **DebugTraceFlags.cs**: 전투 디버그 로그 플래그
- **EnemyAIController.cs**: 적 AI 기본(추상)
- **DefaultEnemyAIController.cs**: 기본 랜덤 AI
- **AdeliaEnemyAIController.cs**: 아델리아 보스 전용 AI
- **BattleResultData.cs** / **StageRewardData.cs** / **TimelineData.cs**: 전투 결과·보상·타임라인 데이터
- **TimelineManager.cs**: 전투 타임라인 연출

---

## Assets/Scrips/BattleSettingUI/

- **BattleSettingManager.cs**: 출전 파티·스킬 세팅 UI 총괄
- **BattleSettingButton.cs** / **PageOutButton.cs**: 세팅 화면 버튼
- **BattleSettingCharacterSlot.cs**: 파티 슬롯
- **CharacterInventoryTab.cs** / **SkillInventoryTab.cs**: 캐릭터·스킬 인벤 탭
- **CharacterBlock.cs** / **SkillBlock.cs**: 드래그 블록
- **CharacterInfoPopup.cs** / **SkillInfoPopup.cs**: 상세 팝업
- **CharacterInfoPassiveBlock.cs**: 패시브 표시 블록
- **SkillSlot.cs**: 주인공 스킬 슬롯 UI
- **DragTool.cs**: 드래그 앤 드롭 공통
- **SkillPresetHandler.cs** / **PresetManager.cs** / **CharacterPartyPreset.cs**: 프리셋 저장·로드

---

## Assets/Scrips/CharacterScrips/

- **CharacterStats.cs**: 스탯·피해·버프 적용
- **CharacterData.cs** / **CharacterLoader.cs**: 캐릭터 데이터·XML 로드
- **CharacterMotionController.cs**: 애니·스킬 모션 길이 등
- **MotionData.cs**: 모션 메타데이터
- **CharacterClickHandler.cs**: 캐릭터 클릭 입력
- **StatusEffectController.cs**: 캐릭터 단위 상태이상 부착/해제 연계
- **PassiveData.cs** / **PassiveLoader.cs** / **PassiveManager.cs**: 패시브 데이터·로드·런타임
- **PassiveSystemExtensionGuide.cs**: 패시브 확장 가이드(참고용)
- **BlessingData.cs** / **BlessingManager.cs** / **BlessingPanel.cs**: 축복 데이터·관리·UI

### Assets/Scrips/CharacterScrips/PassiveEffects/

- **PassiveEffectBase.cs**: 패시브 효과 베이스
- **PassiveEffectLibrary.cs**: 효과 타입 등록·생성
- **PassiveEffectManaBoost.cs**: 마나 관련 패시브
- **PassiveEffectTurnIntervalGrantStatus.cs**: 턴 간격 상태 부여 패시브

### Assets/Scrips/CharacterScrips/BlessingEffects/

- **BlessingEffectBase.cs**: 축복 효과 베이스
- **BlessingEffectCriticalChance.cs**: 치명타 확률 축복 예시

---

## Assets/Scrips/SkillSystem/

- **SkillData.cs** / **SkillLoader.cs**: 스킬 정의·XML 파싱 (`GhostProxyScale` 등 확장 필드 포함)
- **SkillManager.cs**: 스킬 실행·타겟·연출·고스트 프록시 등
- **SkillInstance.cs**: 스킬 버튼/UI 인스턴스
- **SkillSlotData.cs**: 슬롯에 올라간 스킬 데이터

### Assets/Scrips/SkillSystem/Effects/

- **StatusEffectData.cs**: 상태이상 SO/데이터 베이스
- **StatusEffectManager.cs**: 전역 상태이상 적용·틱
- **StatusEffectInstance.cs** / **StatusEffectInstanceBase.cs**: 런타임 인스턴스
- **StatusEffectInstanceBuff.cs** / **StatusEffectInstanceReaction.cs**: 버프·리액션 변형

### Assets/Scrips/SkillSystem/Effects/EffectsScriptable/

- **StatusEffectBleedData.cs** (020001): 출혈
- **StatusEffectPoisonData.cs** (020002): 중독
- **StatusEffectBurnData.cs** (020003): 화상
- **StatusEffectNoDamageBuffData.cs**: 피해무시 등
- **StatusEffectHpReductionData.cs**: 체력 감소(지속 피해 계열)

---

## Assets/Scrips/UI/

- **CharacterInfo.cs** / **CharacterInfoPlayer.cs**: 캐릭터 정보 패널
- **HpUIHandler.cs**: 체력바 UI
- **StatusEffectSlot.cs**: 상태이상 슬롯 UI(아이콘·턴·팝업 연동)
- **VirtualMouseStEfPanel.cs**: 상태이상 호버 수치·지속 표시 (`VirtualMouse`가 직접 사용). 레거시 `StatusPopupHandler`/`StatusEffectIconHandler`는 제거됨.
- **StatusEffectDescriptionPanel.cs**: 아이콘·설명
- **StatusValueBlockManager.cs**: 스탯 팝업·수치 블록
- **BuffDebuffPopup.cs** / **DamagePopup.cs**: 버프/디버프·데미지 팝업
- **NextTurnIndicatorUI.cs** / **TurnIndicatorHandler.cs**: 턴 표시
- **TargetSelector.cs** / **TacticalBlessingTargetSelector.cs**: 타겟·전술 축복 선택
- **SkillStarter.cs** / **SkillSlider.cs** / **SkillButtonDrag.cs**: 스킬 UI 조작
- **VirtualMouse.cs** 및 **VirtualMouseCanvas.cs** / **VirtualMouseUIPanel.cs** / **VirtualMouseSkillPanel.cs** / **VirtualMouseStEfPanel.cs** / **VirtualMouseWorldObject.cs**: 가상 마우스·패널 분기
- **CharacterUpgradePannel.cs** / **UpgradeCharacterButton.cs** / **UpgradeStat.cs**: 캐릭터 강화 UI
- **RewardUIManager.cs** / **RewardItemUI.cs**: 보상 화면
- **TimeLineList.cs** / **TimeLineBlock.cs** / **TimeLinePopupUI.cs**: 타임라인 UI

---

## Assets/Scrips/WorldMap/

- **WorldMapRoot.cs** / **WorldMapUiHub.cs**: 월드맵 루트·UI 허브
- **WorldMapStageSelection.cs**: 스테이지 선택
- **WorldMapCurrencyUI.cs**: 재화 표시
- **WorldMapReturnButton.cs**: 복귀 버튼
- **WorldMapDebugConsole.cs**: 월드맵 디버그 콘솔
- **SceneLoader.cs**: 씬 전환
- **StageCameraUI.cs**: 스테이지 카메라 UI

---

## Assets/Scrips/StageMap/

- **StageSetting.cs** / **StageData.cs** / **StageLoader.cs**: 스테이지 설정·데이터·로드
- **StageBlockSelection.cs**: 스테이지 블록 선택
- **StageProgressData.cs** / **InStageData.cs**: 진행·인스테이지 데이터
- **CutsceneData.cs** / **CutsceneLoader.cs** / **CutsceneManager.cs**: 컷신

---

## XML 데이터 파일

- **Assets/Resources/Data/Characters/**: 캐릭터
- **Assets/Resources/Data/Skill/**: 스킬 (`BossSkills.xml` 등)
- **Assets/Resources/Data/Effect/**: 상태이상
- **Assets/Resources/Data/Passive/**: 패시브
- **Assets/Resources/Data/Stage/**: 스테이지

---

## 스킬 목록 (PlayerSkills.xml)

- 010000: 스킬 (기본 템플릿)
- 010001: 단검베기 (단일 타겟 공격)
- 010002: 발목 노리기 (단일 타겟 공격 + 상태이상)
- 010003: 여신의 축복 (자신에게 버프 + 피해무시)
- 010004: 주변 살피기 (자신에게 버프)

### 테스트용 스킬 (999xxx 시리즈)

- 999001: 능동형 아이콘 테스트 (다양한 스탯 변화 테스트)
- 999002: 전체 회복 (모든 아군 회복 + 방어력 버프)
- 999003: 전체 공격 (모든 적 공격 + 출혈 상태이상)
- 999004: 인접 공격 (인접 적 공격)
- 999005: 인접 회복 (자신과 인접 아군 회복)
- 999006: 랜덤 공격 (랜덤 적 1명 공격)
- 999007: 약점 공격 (방어력이 가장 낮은 적 공격)
- 999008: 응급 치료 (체력이 가장 낮은 아군 회복)

## 스킬 목록 (EnemyMobSkills.xml)

- 010005: 기초적인 검격 (적 몹 기본 공격)
- 010006: 방어 (적 몹 방어)
- 010007: 참수 (적 몹 단일 강공격 + 상태이상)
- 010008: 지목 (적 몹 버프/디버프)
- 010009: 성실히 갈아낸 날 (적 몹 공격 + 디버프)
- 010010: 녹슨 날 (적 몹 공격 + 디버프)

## 스킬 목록 (BossSkills.xml)

- 011001: 어설픈 다루기 (보스 단일 공격)
- 011002: 버서크 임펙트 (보스 강공격 + 체력 감소 디버프)
- 011003: 상황 관찰 (보스 자기 버프)
- 011004: 세계 뭉개기 (보스 전체 적 공격/회복)

## 상태이상 목록

**ID 규칙(확정)**: 스턴·행동불가 등 **제어 계열** 상태이상은 **`023`으로 시작** (`023001`~). 빙결·스택형 제어 등도 동일 구간에서 번호만 분리하면 됨.

- **020001**: 출혈 (지속 피해)
- **020002**: 중독 (턴 간격 피해 등)
- **020003**: 화상 (주변/지속 피해)
- **021001**: 방어력 변화 (능동형 아이콘)
- **021002**: 공격력 변화 (능동형 아이콘) — 별도 **피해무시** 리액션 타입과 ID가 겹칠 수 있으므로 데이터/XML 기준으로 구분
- **021003**: 속도 변화 (능동형 아이콘)
- **021004**: 체력 변화 (능동형 아이콘)
- **021005**: 회피율 변화 (능동형 아이콘)
- **022001**: 체력 감소 디버프 등 (`StatusEffectHpReductionData` 예시)
- **023001~**: 스턴·행동불가 (`StatusEffectType.Stun`, 턴 스킵 연동) — **023 구간 확정**
- **024001**: 피해무시 등 토큰·리액션 (`StatusEffectType.Token`)
