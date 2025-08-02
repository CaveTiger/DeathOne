# 스크립트맵

## Assets/Scripts/Battle/
- **BattleManager.cs**: 전투 관리, 유닛 생성, 스킬 UI 관리
- **TurnManager.cs**: 턴 관리, 턴 순서 결정, 전투 종료 체크
- **SkillManager.cs**: 스킬 사용, 스킬 효과 적용, 스킬 연출
- **EnemyAIController.cs**: 적 AI 기본 클래스 (추상)
- **DefaultEnemyAIController.cs**: 기본 랜덤 AI 구현
- **AdeliaEnemyAIController.cs**: 아델리아 보스 전용 AI (턴 기반 패턴)
- **BattleEffectManager.cs**: 전투 연출 관리 (피격, 데미지 팝업, 밀림 효과)
- **DamagePopup.cs**: 데미지 수치 표시 UI
- **TurnTransitionSkipManager.cs**: 턴 전환 대기 시간 스킵 시스템

## Assets/Scripts/BattleSettingUI/
- **BattleSettingManager.cs**: 파티/스킬 세팅 UI 관리
- **CharacterInventoryTab.cs**: 캐릭터 인벤토리 탭
- **SkillInventoryTab.cs**: 스킬 인벤토리 탭
- **CharacterBlock.cs**: 캐릭터 블록 (드래그 앤 드롭)
- **SkillBlock.cs**: 스킬 블록 (드래그 앤 드롭)
- **BattleSttingCharacterSlot.cs**: 파티 캐릭터 슬롯
- **SkillSlot.cs**: 주인공 스킬 슬롯
- **CharacterInfoPopup.cs**: 캐릭터 상세 정보 팝업
- **SkillInfoPopup.cs**: 스킬 상세 정보 팝업
- **PageOutButton.cs**: 팝업 닫기 버튼
- **DragTool.cs**: 드래그 앤 드롭 도구
- **SkillPresetHandler.cs**: 스킬 프리셋 관리
- **PresetManager.cs**: 프리셋 저장/로드 시스템

## Assets/Scripts/CharacterScripts/
- **CharacterStats.cs**: 캐릭터 스탯 관리, 상태이상 적용
- **CharacterMotionController.cs**: 캐릭터 모션 관리
- **CharacterData.cs**: 캐릭터 데이터 구조
- **CharacterLoader.cs**: 캐릭터 XML 파싱
- **PassiveEffectBase.cs**: 패시브 효과 기본 클래스 (추상)
- **PassiveEffectMaxHpBoost.cs**: 최대 체력 증가 패시브
- **PassiveEffectAtkBoost.cs**: 공격력 증가 패시브
- **PassiveEffectDefBoost.cs**: 방어력 증가 패시브
- **PassiveEffectManaBoost.cs**: 마나 시스템 활성화 패시브

## Assets/Scripts/SkillSystem/
- **SkillData.cs**: 스킬 데이터 구조
- **SkillLoader.cs**: 스킬 XML 파싱
- **SkillInstance.cs**: 스킬 인스턴스 (UI 버튼)
- **SkillButton.cs**: 스킬 버튼 UI

## Assets/Scripts/SkillSystem/Effects/
- **StatusEffectData.cs**: 상태이상 데이터 기본 클래스
- **StatusEffectInstance.cs**: 상태이상 인스턴스 (버프/디버프)
- **StatusEffectInstanceBuff.cs**: 버프 타입 상태이상
- **StatusEffectInstanceReaction.cs**: 리액션 타입 상태이상
- **StatusEffectController.cs**: 상태이상 컨트롤러
- **StatusEffectSlot.cs**: 상태이상 UI 슬롯
- **StatusPopupHandler.cs**: 상태이상 팝업 핸들러

## Assets/Scripts/SkillSystem/Effects/EffectsScriptable/
- **StatusEffectBleedData.cs**: 출혈 상태이상 (020001)
- **StatusEffectPoisonData.cs**: 중독 상태이상 (020002)
- **StatusEffectBurnData.cs**: 화상 상태이상 (020003)
- **StatusEffectGuardPowerBuffData.cs**: 방어력 변화 (021001) - 능동형 아이콘
- **StatusEffectAttackPowerBuffData.cs**: 공격력 변화 (021002) - 능동형 아이콘
- **StatusEffectSpeedBuffData.cs**: 속도 변화 (021003) - 능동형 아이콘
- **StatusEffectHpChangeData.cs**: 체력 변화 (021004) - 능동형 아이콘
- **StatusEffectNoDamageBuffData.cs**: 피해무시 (021002)
- **StatusEffectHpReductionData.cs**: 체력 감소

## Assets/Scripts/UI/
- **CharacterInfo.cs**: 캐릭터 정보 UI 기본 클래스
- **CharacterInfoPlayer.cs**: 플레이어 캐릭터 정보 UI
- **CharacterInfoEnemy.cs**: 적 캐릭터 정보 UI
- **HpUI.cs**: 체력바 UI
- **StatusEffectIconHandler.cs**: 상태이상 아이콘 핸들러
- **NextTurnIndicatorUI.cs**: 다음 턴 표시 UI
- **TurnIndicatorHandler.cs**: 턴 표시 핸들러
- **BattleUIManager.cs**: 전투 UI 관리
- **WorldMapCurrencyUI.cs**: 월드맵 재화 UI

## Assets/Scripts/WorldMap/
- **SceneLoader.cs**: 씬 전환 관리
- **StageCameraUI.cs**: 스테이지 카메라 UI
- **WorldMapStageSelection.cs**: 월드맵 스테이지 선택
- **WorldMapReturnButton.cs**: 월드맵 복귀 버튼

## Assets/Scripts/StageMap/
- **StageManager.cs**: 스테이지 진행 관리
- **StageSetting.cs**: 스테이지 설정
- **StageData.cs**: 스테이지 데이터 구조
- **StageLoader.cs**: 스테이지 XML 파싱
- **StageBlockSelection.cs**: 스테이지 블록 선택
- **CutsceneData.cs**: 컷신 데이터
- **CutsceneLoader.cs**: 컷신 로더
- **CutsceneManager.cs**: 컷신 매니저

## Assets/Scripts/
- **GameManager.cs**: 게임 전체 관리
- **GameProgressManager.cs**: 게임 진행 데이터 관리
- **SpawnManager.cs**: 유닛 생성 관리
- **RewardManager.cs**: 전투 보상 관리
- **RewardUIManager.cs**: 보상 UI 관리
- **RewardPopupUI.cs**: 보상 팝업 UI
- **RewardItemUI.cs**: 보상 아이템 UI
- **PassiveManager.cs**: 패시브 시스템 관리
- **PassiveData.cs**: 패시브 데이터 구조
- **PassiveLoader.cs**: 패시브 XML 파싱

## XML 데이터 파일
- **Assets/Resources/Data/Characters/**: 캐릭터 데이터
- **Assets/Resources/Data/Skill/**: 스킬 데이터
- **Assets/Resources/Data/Effect/**: 상태이상 데이터
- **Assets/Resources/Data/Passive/**: 패시브 데이터
- **Assets/Resources/Data/Stage/**: 스테이지 데이터

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
- **020001**: 출혈 (지속 피해)
- **020002**: 중독 (2턴당 피해)
- **020003**: 화상 (주변 피해)
- **021001**: 방어력 변화 (능동형 아이콘)
- **021002**: 공격력 변화 (능동형 아이콘)
- **021003**: 속도 변화 (능동형 아이콘)
- **021004**: 체력 변화 (능동형 아이콘)
- **021002**: 피해무시 (리액션 타입) 