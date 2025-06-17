# 스크립트 맵 (Script Map)

> 이 파일은 프로젝트 내 주요 스크립트의 위치와 역할을 한눈에 파악하기 위한 지도입니다.
> 폴더 구조, 파일명, 간단한 역할/책임을 정리해두고, 변경/추가/삭제 시마다 업데이트합니다.

---

## Assets/Scrips/Battle
- **BattleManager.cs** : 전투 전체 흐름 관리, 유닛 소환, 전투 시작/종료, 파티원 HP 저장 등
- **TurnManager.cs** : 턴 순서 관리, 턴 종료/시작, 전투 종료 판정
- **EnemyAIController.cs** : 적 AI 행동 제어
- **TimelineManager.cs** : 전투 타임라인(행동 기록, 롤백 등) 관리
- **TimelineData.cs** : 타임라인 데이터 구조 정의
- **BattleUIManager.cs** : 전투 UI 관리
- **BattleCamera.cs** : 전투 카메라 연출, 줌 등
- **BackGroundHandler.cs** : 전투 배경 관리
- **TurnBlockHandler.cs** : 턴 UI 블록 관리
- **SlotHandler.cs** : 유닛 슬롯 관리

## Assets/Scrips/BattleSettingUI
- **BattleSettingUI.cs** : 전투 시작 전 파티/스킬 세팅 UI 관리
- **SkillSetRoot.cs** : 스킬 세트 UI 루트 관리
- **SkillSlot1~4.cs** : 개별 스킬 슬롯 UI 관리
- **CharacterInfoPassiveBlock.cs** : 캐릭터 패시브 정보 UI 블록
- **BattleSettingCharacterSlot.cs** : 전투 세팅용 캐릭터 슬롯 UI
- **PageOutButton.cs** : UI 페이지 닫기 버튼

## Assets/Scrips/StageMap
- **StageManager.cs** : 스테이지 데이터/진행 관리, 저장/로드, 클리어/잠금 처리
- **StageSetting.cs** : 스테이지 블록 배치, 파티 상태 관리, 씬 진입 시 초기화
- **StageBlockSelection.cs** : 스테이지 블록(버튼) 선택/상호작용 처리
- **StageData.cs** : 스테이지/블록 데이터 구조 정의
- **StageProgressData.cs** : 스테이지 진행상황(클리어, 잠금 등) 데이터
- **StageLoader.cs** : 스테이지 데이터(XML 등) 파싱/로드
- **InStageData.cs** : 인게임 파티원 HP 등 임시 데이터 관리
- **CutsceneManager.cs** : 컷신 재생/관리 (글로벌 매니저로 이동 예정)
- **CutsceneData.cs** : 컷신 데이터 구조 정의
- **CutsceneLoader.cs** : 컷신 데이터 파싱/로드

## Assets/Scrips/SkillSystem
- **SkillManager.cs** : 스킬 사용, 효과 적용, 연출 관리
- **SkillInstance.cs** : 스킬 인스턴스(버튼 등) 관리
- **SkillLoader.cs** : 스킬 데이터 파싱/로드
- **SkillData.cs** : 스킬 데이터 구조 정의
- **Effects/** : 상태이상/버프/디버프 등 효과 관련 스크립트

## Assets/Scrips/CharacterScrips
- **CharacterStats.cs** : 캐릭터 스탯, 데미지/회복/사망 처리, 상태이상 관리, KDP(넉다운) 시스템
- **StatusEffectController.cs** : 캐릭터별 상태이상 효과 관리
- **CharacterLoader.cs** : 캐릭터 데이터 파싱/로드
- **CharacterData.cs** : 캐릭터 데이터 구조 정의
- **CharacterMotionController.cs** : 캐릭터 모션/애니메이션 관리
- **MotionData.cs** : 모션 데이터 구조 정의
- **CharacterClickHandler.cs** : 캐릭터 클릭/상호작용 처리

## Assets/Scrips/WorldMap
- **WorldMapStageSelection.cs** : 월드맵 스테이지 버튼 선택/상호작용
- **StageCameraUI.cs** : 월드맵 카메라 연출, 줌 등
- **WorldMapReturnButton.cs** : 월드맵 복귀 버튼 처리
- **SceneLoader.cs** : 씬 전환 관리

## Assets/Scrips/UI
- **BattleUIManager.cs** : 전투 UI 관리
- **CharacterInfoPlayer.cs** : 플레이어 캐릭터 정보 UI
- **CharacterInfoEnemy.cs** : 적 캐릭터 정보 UI
- **CharacterInfo.cs** : 캐릭터 정보 UI 베이스
- **TimeLinePopupUI.cs** : 타임라인 상세 팝업 UI
- **TimeLineBlock.cs** : 타임라인 블록 UI
- **TimeLineList.cs** : 타임라인 리스트 UI
- **TurnIndicatorHandler.cs** : 턴 인디케이터 UI
- **NextTurnIndicatorUI.cs** : 다음 턴 인디케이터 UI
- **StatusEffectIconHandler.cs** : 상태이상 아이콘 UI
- **StatusPopupHandler.cs** : 상태이상 팝업 UI
- **StatusEffectSlot.cs** : 상태이상 슬롯 UI
- **SkillStarter.cs** : 스킬 사용 버튼 UI
- **SkillSlider.cs** : 스킬 쿨타임/게이지 UI
- **SkillButtonDrag.cs** : 스킬 드래그 UI
- **HpUIHandler.cs** : HP 바 UI
- **CharacterInfoPassiveBlock.cs** : 캐릭터 패시브 정보 UI 블록
- **BattleSettingCharacterSlot.cs** : 전투 세팅용 캐릭터 슬롯 UI
- **PageOutButton.cs** : UI 페이지 닫기 버튼

## Assets/Scrips (루트)
- **GameManager.cs** : 글로벌 게임 상태/매니저
- **SpawnManager.cs** : 유닛/오브젝트 스폰 관리
- **Debuger.cs** : 디버그/로그 유틸리티

---

## 주요 연결 구조/참조 관계

- (예시) BattleManager → TurnManager, StageSetting
- (예시) StageManager → StageProgressData, StageBlockData
- (예시) WorldMapStageSelection → StageManager

---

> 각 항목에 실제 역할/책임, 연결 구조 등을 자유롭게 추가/수정해 주세요. 