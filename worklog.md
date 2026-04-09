# 작업 로그


## 2024-05-25
### 상태이상 시스템 버그 수정
- 파일 위치: 
  - Assets/Scripts/StatusEffect/StatusEffectInstanceReaction.cs
- 작업 내용: 무적(피해무시) 효과 버그 수정
- 변경 사항:
  - Initialize 메서드에 triggerCount 초기화 로직 추가
  - 무적 효과 정상 작동 확인
- 참고 사항: 
  - 리액션 타입 상태이상의 triggerCount 초기화 중요성 확인
  - 상태이상 시스템의 확장성 검증

## 2024-05-26
### 모션 시스템 분석
- 파일 위치: 
  - Assets/Scripts/CharacterScrips/CharacterMotionController.cs
  - Assets/Scripts/Battle/MotionManager.cs
- 작업 내용: 모션 시스템 구조 분석
- 변경 사항:
  - 모션 시스템 구조 파악
  - 모션 관련 리소스 위치 확인
  - 모션 타입 및 시퀀스 시스템 분석
- 참고 사항: 
  - 스프라이트 기반 모션 시스템
  - 모드 지원 구조 확인
  - 모션 시퀀스 시스템 구조 파악
  - Attack, Hit, Buff, Stand 등 기본 모션 타입 확인

# 2024-05-26: 모션 시스템 개선

## 변경 사항
1. **경로 기반 스프라이트 로드 방식 채택**
   - `Resources.Load<Sprite>("UnitSprite/Player/Stand")` 형태로 직접 로드
   - MotionManager 의존성 제거
   - 폴더 구조 기반으로 스프라이트 관리

2. **CharacterMotionController 개선**
   - `currentMotion` 필드 추가로 현재 모션 상태 추적
   - 모션 시퀀스에서 이전/현재 모션 관리
   - 경로 기반 스프라이트 로드 구현

3. **스킬 시스템과 모션 연동**
   - SkillData의 Motion 필드 활용
   - SkillManager에서 모션 컨트롤러 호출 구조 확인
   - 모션 컨트롤러 null 체크 필요성 발견

## 발견된 문제점
- 캐릭터 오브젝트에 CharacterMotionController 컴포넌트 누락
- 모션 연출 중 다음 스킬 사용 가능한 상태
- 모션 컨트롤러 null 체크 필요

## 향후 작업 계획
1. 캐릭터 오브젝트에 CharacterMotionController 자동 추가
2. 모션 완료 대기 로직 구현
3. 모션 컨트롤러 null 체크 강화

## 2025-05-28 종료 전투 모션 시스템 개선 및 구조 정비

- SkillManager, CharacterMotionController 등 전투 모션 연출 구조 전면 리팩터링
- 공격/피격 모션을 코루틴 흐름에 맞춰 동기화
- 스프라이트 동적 변경 및 Stand 복귀 로직 구현
- 캐릭터 프리팹 구조 점검 및 모션 컴포넌트 연결 문제 해결
- 스킬 연출, 피해, 효과, 턴 관리의 전체 흐름을 일관성 있게 정비
- 디버깅 및 로그 추가로 문제 원인 신속 파악
- 전투 연출이 자연스럽고 안정적으로 동작하도록 개선

## 2025-07-29 턴 전환 스킵 시스템 구현 및 NullReferenceException 해결

### 턴 전환 스킵 시스템 구현
- 파일 위치: Assets/Scrips/Battle/TurnTransitionSkipManager.cs
- 작업 내용: 턴 전환 시의 답답한 대기 시간만 스킵할 수 있는 시스템 구현
- 변경 사항:
  - TurnTransitionSkipManager 클래스 생성
  - 스킵 가능한 대기 시간: 죽는 연출(1.5초), 적 턴 시작 전(0.8초), 적 턴 종료 후(0.5초)
  - 스페이스바 또는 마우스 클릭으로 스킵 가능
  - Inspector에서 개별 대기 시간 타입별 스킵 허용/금지 설정 가능
  - 전투 연출은 그대로 유지하면서 턴 전환 대기만 스킵
- 참고 사항: 
  - 스킵 타입별 개별 제어 가능 ("death", "enemyStart", "enemyEnd")
  - 확장 가능한 구조로 설계 (새로운 스킵 타입 추가 용이)
  - TurnManager, EnemyAIController에서 스킵 시스템 적용

### NullReferenceException 해결
- 파일 위치: 
  - Assets/Scrips/GameProgressManager.cs
  - Assets/Scrips/StageMap/StageSetting.cs
- 작업 내용: 게임 종료/일시정지 시 발생하는 NullReferenceException 해결
- 변경 사항:
  - GameProgressManager.SaveGameProgress()에서 StageManager.Instance null 체크 추가
  - StageSetting에서 StageManager.Instance null 체크 추가
  - 초기값 설정 및 안전장치 구현
- 참고 사항:
  - OnApplicationQuit(), OnApplicationPause() 시점에서 StageManager가 초기화되지 않은 상태일 수 있음
  - null 체크 후 기본값 설정으로 안전성 확보

### 스킵 시스템 시행착오
- 작업 내용: 전체 전투 스킵 시스템 구현 시도 후 제거
- 변경 사항:
  - BattleSkipManager.cs 생성 후 삭제
  - 모든 대기 시간을 스킵 가능하게 수정 후 원상복구
  - 턴 전환 대기만 스킵하는 방향으로 재설계
- 참고 사항:
  - 전체 전투 스킵은 게임 밸런스에 문제 발생
  - 턴 전환 대기만 스킵하는 것이 적절한 수준
  - 사용자 요구사항 정확히 파악의 중요성

## 내일 작업 계획 (2025-07-30)

### 1. 보스 패턴 구현
- 작업 내용: AI 패턴 시스템에 보스 전용 패턴 추가
- 예상 난이도: 쉬움 (기존 AI 구조 활용)
- 구현 방향:
  - PatternType.Boss 패턴 구현
  - 보스 특유의 전략적 행동 로직 설계
  - 기존 Default 패턴과 차별화된 AI 행동

### 2. 미구현 스킬 시스템 개선
- 작업 내용: 현재 구현되지 않은 스킬 기능들 구현
- 개선 영역:
  - 스킬별 특수 효과 (상태이상, 연계 등)
  - 스킬 연출 개선 (이펙트, 사운드)
  - 스킬 밸런싱 조정
  - 새로운 스킬 타입 추가
- 우선순위: 보스 패턴 구현 후 진행

### 3. 기타 고려사항
- UI/UX 개선: 스킬 선택, 전투 화면 레이아웃
- 성능 최적화: 메모리 사용량, 프레임 레이트
- 버그 수정: 발견되는 문제들 해결

## 2025-06-01 월드맵 UI/버튼 입력 충돌 완전 차단 및 복귀 기능 구현
- 월드맵 스테이지 버튼 중첩 클릭 문제를 Collider 일괄 비활성화 방식으로 완전히 차단
- UI가 열릴 때 모든 월드맵 버튼의 Collider를 꺼주고, 닫힐 때 다시 켜주는 static 메서드 구조로 개선
- ESC 키 및 UI 내 "월드맵으로" 버튼을 통한 월드맵 복귀(줌아웃, 선택 해제, UI 닫기, 버튼 활성화) 기능 구현
- 복귀 버튼용 WorldMapReturnButton 스크립트 제작 및 동적 참조 연결 방식 적용
- 버튼 클릭 시 targetStageSelection의 ReturnToWorldMap() 호출로 안전하게 복귀 처리
- Inspector 접근 제한자 문제(public) 해결
- 현재 아트 작업 병행 중

## 2025-06-02 월드맵 카메라 이동 및 줌 연출 개선
- StageCameraUI에서 카메라 이동 시 x축으로 0.75만큼 이동하여 파란 영역 중앙에 월드스테이지 아이콘이 오도록 조정
- 줌인/줌아웃 시 카메라 위치와 orthographicSize를 자연스럽게 보간
- ResetCamera에서 카메라 위치를 원래대로 복귀하도록 개선
- Inspector 값과 코드 기본값 차이로 인한 카메라 사이즈 적용 문제 해결
- 내일은 스테이지 진입 컷신(연출) 임시작업 예정

## 2025-06-03 컷신 시스템 구현
- 파일 위치: 
  - Assets/Scripts/StageMap/CutsceneData.cs
  - Assets/Scripts/Cutscene/CutsceneData.cs
  - Assets/Scripts/Cutscene/CutsceneLoader.cs
  - Assets/Scripts/Cutscene/CutsceneManager.cs
- 작업 내용: 컷신 시스템 기본 구조 설계 및 구현
- 변경 사항:
  - 컷신 데이터 구조 설계
  - 컷신 로더 구현
  - 컷신 매니저 구현
  - 스테이지 진입 시 컷신 재생 시스템 구축
- 참고 사항: 
  - 스테이지 진입 시 컷신 재생을 통한 스토리 연출
  - 컷신 시스템의 기본 구조 설계 완료
  - 향후 컷신 데이터 및 연출 개선 예정

## 2025-06-04 상태이상 중복 처리 개선 및 컷신 임시 비활성화
- 파일 위치: 
  - Assets/Scrips/CharacterScrips/CharacterStats.cs
  - Assets/Scrips/StageMap/StageBlockSelection.cs
  - Assets/Scrips/SkillSystem/Effects/EffectsScriptable/StatusEffectBleedData.cs
- 작업 내용:
  - 상태이상 중복 적용 시 기존 프리팹의 value/duration에 새 값의 절반만큼 누적하는 로직 구현
  - StatusEffectSlot 하위에서 중복 체크 방식으로 구조 개선
  - 출혈 등 효과의 value/duration 전달 및 ScriptableObject 값 점검
  - 컷신 시스템 임시 비활성화(테스트 목적)
- 변경 사항:
  - CharacterStats.cs의 AddStatusEffectPrefab 중복 처리 로직 개선 및 주석 보강
  - StageBlockSelection.cs에서 컷신 호출부 주석 처리
  - StatusEffectBleedData 등 ScriptableObject 값 확인 및 적용 방식 점검
- 참고 사항:
  - 상태이상 효과 적용 시 value, duration이 0이 되는 문제는 호출부 파라미터 전달 방식에서 발생할 수 있음
  - 컷신 연동은 추후 복구 예정

## 2025-06-05 상태이상 시스템 구조 개선 및 정상 작동 확인

- StatusEffectController를 캐릭터 본체에 부착, UI 생성 위치(statusEffectArea) 분리
- 상태이상 프리팹 생성 시 activeEffectPrefabs에 추가하여 턴마다 효과 정상 적용
- 지속피해형(StatusEffectInstance) 코드 정리: 오로지 ContinuousDamage만 담당하도록 리팩터링
- TurnManager에서 character.GetComponent<StatusEffectController>()로 상태이상 효과 적용
- 디버그 로그로 정상 작동 확인, UI 및 효과 적용 모두 정상
- 불필요한 코드/필드 정리 및 구조 일원화

## 2025-06-06 스테이지 연계 및 전투 종료 구조 개선 준비
- 전투 종료 후 스테이지(월드맵)로 정상 복귀 확인
- 빈 슬롯/캐릭터 없는 슬롯 처리 방식 개선
- 스테이지가 여러 개일 때 진행 정보 저장 필요성 인지
- StageManager 구조 점검 및 확장 방향 논의
- 내일: 스테이지 클리어 정보, 상태 복구, UI 연동 등 구체적 설계 예정

## 2025-06-06 타임라인 시스템 설계/구현 실패 및 교훈

- **실패 요약**
  - 런타임/저장용 데이터 구조 분리 미흡으로 변수명 충돌 및 대량 컴파일 오류 발생
  - 변수명 일괄 변경만으로는 구조적 문제 해결 불가
  - 복원/저장 정책(언제, 무엇을, 어떻게 저장/복원할지) 미정으로 인한 혼란
  - 실제 게임 오브젝트와 저장 데이터의 동기화 문제(상태 꼬임, 참조 무결성 등)
  - 행동 로그와 스냅샷의 분리 기준 불명확

- **교훈 및 향후 개선 방향**
  1. 런타임/저장용 데이터 구조를 명확히 분리하고, 변수명/클래스명에 접두사 등으로 혼동 방지
  2. 타임라인 시스템 설계(데이터 흐름, 저장/복원 시점, 변수명 규칙, 예외처리 등) 문서화 필요
  3. 복원/되돌리기 시 참조 무결성, 오브젝트 풀링 등 런타임 상태와 저장 데이터의 동기화 정책 수립
  4. 행동 로그와 스냅샷의 역할/시점 분리 및 정책 명확화
  5. 실패한 구조/접근법, 시행착오를 worklog.md에 반드시 기록하여 재발 방지

- **실제 경험**
  - 변수명 충돌, 구조 불일치로 인한 대량 오류 경험
  - 설계 미흡 시 단순 변수명 변경만으로는 문제 해결이 불가능함을 체감
  - 복잡한 시스템일수록 설계와 기록의 중요성 재확인

## 2025-06-07 스테이지 진행/저장/잠금 시스템 설계 및 커서룰 정비

- 월드맵 > 월드맵 버튼 > 스테이지 > 스테이지블록 > 전투 구조로 전체 흐름 및 데이터 계층 정리
- StageProgressData에 isLocked(잠금 여부) 필드 추가, 변수명 일치화로 오류 해결
- 진행/클리어/잠금 정보는 StageProgressData에서 통합 관리, 전투 자체는 이어하기 미지원
- 정적 정보(StageData/StageBlockData)와 동적 정보(StageProgressData) 분리 설계 확정
- 커서룰 144번에 월드맵 폴더 경로(Assets/Scripts/WorldMap) 명시, 스테이지 폴더 경로도 추가
- 커서룰에 월드맵/스테이지 씬 관리 규칙 및 데이터 흐름 상세 기술
- 오늘 작업을 통해 스테이지 저장/진행 시스템의 구조적 혼선 해소 및 확장성 기반 마련

## 2025-06-08 스테이지 블록 Last 속성 파싱 구현
- 파일 위치: 
  - Assets/Scrips/StageMap/StageData.cs
  - Assets/Scrips/StageMap/StageLoader.cs
  - Assets/Resources/Data/Stage/BaseBlock.xml
- 작업 내용: 스테이지 블록의 클리어 가능 여부를 나타내는 Last 속성 구현
- 변경 사항:
  - StageBlockData 클래스에 Last 속성 추가
  - StageLoader의 XML 파싱 코드에 Last 속성 파싱 로직 추가
  - CloneBlock 메서드에 Last 속성 복제 로직 추가
  - BaseBlock.xml에 보스 블록(060002)에 Last 속성 추가
  - 파싱 검증을 위한 디버그 로그 추가
- 참고 사항: 
  - Last 속성이 true인 블록은 스테이지 클리어가 가능한 블록으로 판정
  - XML에서 <Last>true</Last> 태그로 클리어 가능 여부 지정
  - 파싱 및 저장 과정 검증 완료

## 2025-06-09 스크립트 구조 리팩터링 및 역할 분리, 스크립트맵 도입
- 스크립트맵(scriptmap.md) 신규 작성 및 폴더별/기능별 스크립트 구조 정리
- StageManager와 StageSetting의 역할 분리 원칙 확립
    - StageManager: 스테이지/블록의 클리어, 잠금 등 진행상태의 영구적 관리만 담당
    - StageSetting: 인게임 임시 데이터(파티 체력 등) 및 실제 오브젝트/블록 배치, 전투 진입/복귀 등 실질적 실행 담당
- 월드맵에서 스테이지 ID 전달 → StageManager.SelectedStageID 저장 → StageSetting이 참조하여 세팅하는 구조 재확인
- 진행상태(클리어/잠금 등)는 StageManager가 소유, StageSetting은 참조만 하도록 설계
- StageManager의 블록/스테이지 정보 등록 및 관리 방식 점검(딕셔너리, XML 파싱 등)
- XML을 세이브/로드용 파일 포맷으로도 활용 가능함을 확인, 마스터 데이터와 세이브 데이터 분리 관리 원칙 논의
- 전체 구조/책임 분리로 인한 유지보수성, 확장성, 버그 방지 효과 기대
- 기타: 스크립트맵/커서룰/워크로그 3종을 작업 시작 루틴으로 확립

### 2025-06-10 내일 할 일(계획)
- 스테이지 클리어 정보를 StageManager/StageSetting 구조에 맞게 실제로 적용 및 테스트
- 스테이지 과정(진행, 클리어, 잠금/해제 등)이 모두 정상적으로 동작하는지 검증
- 스테이지 시스템이 완성되면 다음으로 집중할 시스템/기능(예: 보상, 컷신, 파티 관리 등) 선정 및 설계 방향 고민

## 2025-06-10

### 스테이지 블록 상태 관리 시스템 개선
- StageManager, StageData, StageSetting의 블록 상태 관련 변수명(NextBlockIDs, Cleared, Locked) 통일
- StageManager에 blockStates 딕셔너리 기반 중앙 집중 관리 구조 확립
- RegisterStageBlocks, UnregisterStageBlocks 메서드 구현 및 월드맵 진입/해제 시점에 연동
- blockStatesDebugView 리스트 및 RefreshBlockStatesDebugView 메서드로 인스펙터 실시간 상태 확인 기능 추가
- WorldMapStageSelection에서 UI 열고 닫을 때 블록 상태 등록/해제 로직 적용
- 전체 흐름 및 구조 점검, 디버깅 및 로그 추가로 상태 변화 추적

## 2025-06-11 전투 준비(파티/스킬 세팅) 시스템 설계 및 스킬 슬롯 동적 생성 구조 정비
- BattleSettingUI 폴더 신설, 전투 시작 전 파티/스킬 세팅 전용 UI 구조 설계
- 전체 구조: 좌측 파티 세팅, 우측 스킬/아군 탭으로 구분하는 레이아웃 논의
- 스킬 파싱 및 SkillData 딕셔너리 구조 점검(스킬 데이터는 이미 완비)
- 인게임(전투 씬)에서 파티원별 스킬 슬롯(버튼) 동적 생성 구조 설계
    - SkillSetRoot > SkillSlot1~4 > 각 슬롯에 파티원 수만큼 스킬버튼 생성 구조로 정리
    - SkillInstance 스크립트 구조 및 동작 방식 점검(동적 생성 시 ID/데이터 연동 확인)
- 아이콘 등 리소스 미구현 상태에서는 스킬 이름만 표시하도록 임시 처리
- 내일 할 일: BattleSettingUI 실제 구현(파티/스킬 세팅 UI, 데이터 연동, 전투 시작 연계 등)

## 2025-06-12
### BattleManager 전투 준비 구조 리팩터링 및 스킬 생성 구조 개선
- Start()에서 StartBattle()만 호출하도록 구조 변경
- StartBattle()에서 SpawnAllUnits, CreateAllSkillButtons, SetupSkillSlots 등 전투 준비를 통합적으로 처리하도록 리팩터링
- 스킬 버튼 UI 동적 생성(CreateAllSkillButtons) 로직을 전투 준비 과정에 통합
- 스킬 인스턴스 준비(SetupSkillSlots)와 UI 생성의 역할 분리 및 순서 정비
- 스킬 생성/연동 구조 점검 및 디버깅 방법 논의
- 전체적으로 전투 준비 흐름의 일관성 및 확장성 개선

## 2025-06-13 넉다운(KDP) 시스템 설계 및 구현
- CharacterData: NDP/MaxNDP → KDP/MaxKDP로 변수명 일괄 변경
- CharacterLoader: KDP/MaxKDP 파싱 및 오버라이드 반영
- 캐릭터 XML(NamedCharacterCh1 등): <NDP> → <KDP>로 태그명 변경
- SkillData: KnockdownMultiplier(넉다운 배율) 필드 추가 및 Clone 반영
- SkillLoader: <KnockdownMultiplier> 태그 파싱 및 오버라이드 반영
- CharacterStats: TakeDamage에서 피해량 * KnockdownMultiplier만큼 KDP 누적, MaxKDP 도달 시 TriggerKnockdown() 호출
- 플레이어 캐릭터에는 KDP 시스템 미적용(적 전용)
- KDP/MaxKDP가 없는 경우 무시, KnockdownMultiplier가 0 이하인 경우도 무시
- TriggerKnockdown()은 현재 Debug.Log만 출력(실제 스턴/연출 등은 추후 구현)

### 앞으로 적용해야 할 요소들
- TriggerKnockdown()에서 실제 스턴/행동불가, 연출, 이펙트, 턴 스킵 등 구체적 처리 구현
- KDP 상태를 UI/이펙트로 잠깐 보여주는 연출(유저는 평소엔 HP만 보게 설계)
- 스킬/상태이상 등에서 KnockdownMultiplier를 다양하게 활용할 수 있도록 데이터 설계 확장
- 보스/특수 몬스터 등 예외 처리(녹다운 면역 등)
- 튜토리얼/팁 등에서 녹다운 시스템 안내(선택사항)
- 기타: KDP가 일정 턴마다 감소/회복 등 추가 기획 반영 가능

## 2025-06-14
### Unity UI 설정 및 작업
- 파티 세팅 구간에서 캐릭터 정보창 UI 팝업 배치 및 레이아웃 조정
- HP, ATK, DEF, Evasion, Accuracy 등 주요 스탯 텍스트 UI 정렬 및 폰트/색상 수정
- 캐릭터 스프라이트와 정보창의 자연스러운 배치(좌측 캐릭터, 우측 정보)
- UI Canvas, Panel, Text, Image 등 오브젝트 계층 구조 정비
- Inspector에서 RectTransform, Anchor, Pivot 등 세부 위치/크기 조정
- UI 프리팹 저장 및 적용, 테스트 플레이로 배치 확인
- 기타: UI 팝업 활성/비활성 트리거 연결, 임시 더미 데이터로 동작 확인
- 코드 작업은 최소화, 대부분 에디터 상에서 UI 구성 및 시각적 피드백 위주로 진행

## 2025-06-15
### BattleSettingUI 및 팝업/패시브/캐릭터 슬롯 구조 개선 및 실전 적용 논의
- CharacterInfoPassiveBlock:  
  - TextMeshProUGUI 타입으로 필드 수정  
  - 임시 passive(string) 필드 추가로 패시브 정보 구조 확장 준비  
  - SetPassiveInfo(string name, string description) 방식의 UI 갱신 구조 논의  
  - 추후 마우스 오버 시 상세정보 팝업 등 확장성 고려
- BattleSttingCharacterSlot:  
  - 슬롯에 캐릭터 이미지와 데이터 보유  
  - 전투 진입 시 파티 정보로 활용할 구조 설계  
  - 실제 캐릭터 Sprite와 데이터 연동 구조 논의
- PageOutButton:  
  - 해당 페이지(Transform) 참조 방식으로 팝업 닫기 구조 설계  
  - X버튼 역할 단순화 및 Awake에서 1회 SetActive(false)로 관리하는 실전 팁 정리  
  - 팝업이 비활성화 상태여도 이벤트 연결이 유지되는 구조 논의
- 전체적으로 UI/팝업/슬롯 구조의 확장성과 실전 적용 방안 논의 및 설계  
- 워크로그, 커서룰, 스크립트맵 등 작업 시작 루틴 재확인 및 습관화

## 2025-06-16 BattleSettingUI 및 캐릭터 정보 연동 구조 설계

- ArrangeTab에 CharacterTab, SkillTab, PassiveTab 오브젝트 연결 및 탭 전환 로직 설계/테스트
- CharacterBlock 프리팹/스크립트 설계
  - 캐릭터 데이터 및 이미지 외부 세팅 구조 확정
  - 드래그 앤 드롭 기능(원래 위치 복귀 포함) 구현
  - 더블클릭 감지 로직 설계 및 CharacterInfoPopup 연동 구조 논의
- CharacterInfoPopup 스크립트와 CharacterBlock의 연계성 점검
  - 더블클릭 시 SetCharacterData로 정보 전달 및 팝업 활성화 구조 확정
- .cursorrules, worklog.md, scriptmap.md 파일의 위치/인코딩/자동 인식 구조 점검 및 정비
- 커서가 프로젝트 규칙/작업이력/구조를 자동 인식하도록 환경 정비 완료

### 내일 할 일
- CharacterBlock, CharacterInfoPopup 등 실제 오브젝트 배치 및 연동 테스트
- 캐릭터 리스트 생성, 더블클릭 시 상세 정보 팝업 정상 동작 확인
- 파티 슬롯 연동 및 실제 게임 흐름과의 연결 작업 예정

## 2025-06-17
### SSD 교체 및 개발 환경 점검
- 오늘은 개발 작업 없이 SSD 교체 및 시스템 환경 점검에 하루를 소요함
- Unity, Git, 커서룰, 작업 로그 등 개발 환경 정상 작동 확인
- 기존 데이터 및 프로젝트 파일 백업/복원 완료
- 내일부터 정상 개발 작업 재개 예정
- 아쉬운 점: M.2 슬롯에 SSD를 장착한 결과, 기존 SATA 케이블로 연결된 SSD가 인식되지 않아 무력화됨

## 2025-06-18 캐릭터 인벤토리 탭 설계 및 UI 구조 분리
- ArrangeTab.cs 스크립트 삭제 및 기능 분리 결정
- CharacterInventoryTab(캐릭터 인벤토리 탭) 설계 시작: 소지한 캐릭터 목록을 보여주는 역할만 담당하도록 명확화
- ScrollView(Content) 구조를 활용해 캐릭터 리스트를 동적으로 생성하는 구조로 전환
- characterListContainer에 ScrollView의 Content 오브젝트를 연결하여 프리팹이 스크롤 영역에 자동 배치되도록 설계
- Inspector에서 직접 연결 방식으로 NullReferenceException 등 오류 방지
- 향후 강화/합성/파티 등 다양한 UI에서 재사용 가능하도록 확장성 고려
- CharacterInfoPopup 등 UI 오브젝트 활성화 및 연결 문제 디버깅
- 코드 작업은 최소화, 대부분 에디터 상에서 UI 구성 및 시각적 피드백 위주로 진행

## 2025-06-21 전투 시작 시 스킬 슬롯 생성 로직 디버깅 및 문제 해결

- **문제 상황**: 전투 시작 시 모든 스킬 슬롯에 동일한 스킬이 표시되고, 버튼이 작동하지 않는 문제 발생.

- **원인 분석 및 해결 과정**:
  1.  **`BattleManager`의 캐릭터 리스트 누락**: `SpawnUnit` 시 `allCharacters` 리스트에 캐릭터가 추가되지 않는 문제를 발견하고, 리스트에 `Add`하는 코드를 추가함.
  2.  **데이터 참조 복사 문제**: `CharacterData.Clone()` 메서드가 스킬 리스트를 참조로 복사하여 모든 캐릭터 인스턴스가 동일한 스킬 리스트를 공유하는 근본적인 원인을 발견. `new List<string>(this.Skills)`로 수정하여 깊은 복사(deep copy)를 수행하도록 변경함.
  3.  **UI 업데이트 데이터 부재**: 데이터는 정상적으로 로드되나, UI가 갱신되지 않는 문제를 디버그 로그(`[SkillGen-Check]`)를 통해 확인. `PlayerSkills.xml` 파일의 모든 `<Icon>` 태그가 비어있는 것을 확인하고, 임시 아이콘 경로를 추가하여 UI가 정상적으로 스킬별 아이콘을 표시하도록 수정함.
  4.  **프리팹 이벤트 연결 누락**: 사용자가 직접 `SkillButton` 프리팹에서 `OnClick` 이벤트에 `UseSkill` 메서드가 연결되지 않은 문제를 발견하고 해결함.

- **결과**:
  - 전투 시작 시 각 캐릭터는 고유한 스킬 목록을 가지며, UI 스킬 슬롯에 4개의 서로 다른 스킬(이름, 아이콘)이 정상적으로 표시됨.
  - 스킬 버튼 클릭 시 `UseSkill` 메서드가 정상적으로 호출됨.

## 2025-06-22 파티 세팅 UI 드래그&드롭 RectTransform 앵커/피벗 문제 분석 로그

- **문제 상황**: `GameProgressManager`의 테스트 모드를 통해 인벤토리에 캐릭터 데이터를 추가했음에도 불구하고, UI에 캐릭터 블록이 전혀 생성되지 않는 문제가 발생.
- **디버깅 과정**:
  - `GameProgressManager`와 `CharacterInventoryTab`에 상세한 디버그 로그를 추가하여 데이터 흐름을 추적.
  - 로그 분석 결과, `GameProgressManager`가 캐릭터를 인벤토리에 추가하려는 시점(`Awake`)에, `CharacterLoader`가 아직 XML에서 캐릭터 정보를 읽어오지 않아 `CharacterData.characterDict`가 비어있는 것을 확인.
- **핵심 원인**: **스크립트 실행 순서(Script Execution Order)** 문제. 데이터 로더(`CharacterLoader`)가 데이터 소비자(`GameProgressManager`)보다 늦게 실행됨.
- **해결 과정**:
  - **1차 시도 (실패)**: `[DefaultExecutionOrder]` 속성을 이용해 실행 순서를 강제하려 했으나, 복잡성만 증가시키고 다른 접근성 문제를 야기함.
  - **2차 시도 (성공 및 최종 해결책)**:
    1. **`GameManager`를 초기화 총책임자로 지정**: 모든 데이터 로딩과 초기화 흐름을 `GameManager.Awake()`에서 명시적으로 제어하도록 구조 변경.
    2. **`GameProgressManager` 수정**: `Awake()`에 있던 초기화 로직(테스트 캐릭터 생성)을 새로운 `public void Initialize()` 메서드로 분리.
    3. **`GameManager` 수정**: `Awake()`에서 `CharacterLoader`, `SkillLoader` 등 모든 필수 데이터 로더를 먼저 실행시킨 후, 마지막에 `GameProgressManager.Instance.Initialize()`를 호출하도록 수정.
- **결과**: 스크립트 실행 순서에 의존하지 않는 명확하고 안정적인 초기화 파이프라인을 구축하여 문제를 완전히 해결함. 이제 캐릭터 블록이 정상적으로 생성됨.

## 2025-06-23 작업 중 모니터/컴퓨터 화면 무한 깜빡임 및 강제 재부팅, 그리고 전체 작업 내역

- **문제 상황**: 게임 종료 후 컴퓨터 전체 화면이 무한히 깜빡이는 현상 발생. 작업 내용 저장 불가 및 강제 재부팅 필요.
- **조치 및 기록**:
    - 작업 중단, 컴퓨터 강제 재부팅 후 복구.
    - 재부팅 후 프로젝트 및 작업 파일 무결성 확인.
    - 오늘 작업한 주요 내역을 아래와 같이 정리:
        1. 인벤토리 시스템 구조 개선: Dictionary → List<CharacterData>로 변경, 순정/강화 캐릭터 구분 및 수량 표시 기능 구현.
        2. CharacterData에 IsCustomized 플래그 추가 및 Clone 메서드 개선.
        3. CharacterBlock, CharacterInventoryTab, BattleSttingCharacterSlot 등 UI/드래그앤드롭/파티 슬롯 연동 구조 전면 개선.
        4. 드래그앤드롭 시 시각적 피드백 및 상태 복구 로직 개선, 슬롯에 드롭 시 블록 비활성화 및 슬롯 이미지 갱신 구조 완성.
        5. Hierarchy에서 캐릭터 블록 이름 자동 변경, 디버깅 편의성 향상.
        6. 각종 버그 수정 및 유니티 에디터 설정(Inspector 연결 등) 안내.
- **결과**: 강제 재부팅 후에도 작업 내용은 정상적으로 복구되었으며, 오늘의 구조 개선 및 UI/UX 작업은 모두 성공적으로 마무리됨.

## 2025-06-24 해야 할 일(TO-DO)

1. **캐릭터 블록 분할/합치기 연출 구현**
    - 인벤토리에서 동일 캐릭터가 2개 이상일 때, 드래그 시 클론 블록이 생성되고 원본 블록의 수량이 1 감소하도록 구현
    - 드래그 취소 시 원래 블록과 합쳐져 수량 복구, 슬롯에 배치 시 인벤토리 수량 감소
2. **캐릭터 슬롯 시스템 UX/구조 개선**
    - 슬롯에 캐릭터 배치/교체/제거 시 시각적 피드백 및 애니메이션 추가
    - 슬롯-인벤토리 간 데이터 동기화 및 예외 처리 강화
    - 슬롯에 이미 캐릭터가 있을 때의 교체/반환 로직 견고화
3. **스킬 슬롯 시스템 설계 및 구현**
    - 캐릭터 슬롯 시스템 경험을 바탕으로 스킬 슬롯 시스템 설계
    - 스킬은 중복 개념이 없으므로, 드래그/드롭 및 슬롯 배치 로직을 단순화
    - 스킬 슬롯 UI/UX를 캐릭터 슬롯과 일관성 있게 구현
4. **작업 우선순위 및 세부 일정 수립**
    - 각 작업별 예상 소요 시간 및 우선순위 지정
    - 하루 단위로 할 일 체크리스트 작성 및 점검

## 2025-06-25 - 인벤토리/파티 슬롯 구조 개편

### 주요 변경사항
1. **캐릭터 인벤토리 시스템 전면 개편**
   - Dictionary → List<CharacterData>로 변경
   - 해금 여부(IsUnlocked)로 관리하는 방식으로 전환
   - 수량 분할/합치기 기능 완전 제거

2. **파티 슬롯 시스템 개선**
   - 1번 슬롯(주인공) 자동 배치 및 고정
   - 나머지 슬롯 자유 세팅 가능
   - 드래그 앤 드롭 시스템 구현

3. **UI 구조 개선**
   - CharacterInventoryTab, CharacterBlock, BattleSttingCharacterSlot 연동
   - 해금된 캐릭터만 인벤토리에 표시

### 해결된 문제점
- 캐릭터 중복 소유 문제 해결
- 인벤토리 수량 관리 복잡성 제거
- 파티 구성의 일관성 확보

### 남은 작업
- 슬롯 연결/인식 문제 해결 필요
- 인벤토리 수량 관리 로직 정리 필요
- SpawnManager 연동 완성 필요

# 2025-06-26
- BattleSettingCharacterSlot, CharacterBlock 구조 완전 초기화 및 리팩토링 시작
- 슬롯의 Update()에서 콜라이더+마우스 위치로 블록 배치 판정 구현
- 블록이 슬롯에 들어가지 않으면 transformAnchor로 복귀하는 구조 설계
- 매니저에서 파티 정보 실시간 동기화, 블록 인스턴스 기준으로 관리하도록 변경
- CharacterBlock에서 매니저로 캐릭터 정보 전달용 메서드 준비 및 연결
- 코드 내 불필요/중복 기능 정리, 필드/메서드 접근성 조정
- 주요 설계 원칙 및 실시간 상태 관리 기준 확립

## 2025-06-27
- 드래그&드롭 UI의 RectTransform 앵커/피벗 문제 해결
- BattleSettingCharacterSlot의 PlaceCharacterBlock, RemoveCharacterBlock 메서드에서 SetParent 후 RectTransform 속성을 명시적으로 설정하도록 수정
- 슬롯 자체의 RectTransform도 Awake에서 중앙 앵커로 초기화하도록 개선
- CharacterBlock의 OnEndDrag, ReturnToAnchor 메서드에서도 RectTransform 설정 강화
- 디버그 로그 추가로 실제 적용 여부 확인 가능하도록 개선
- anchorMin/Max, pivot, anchoredPosition을 모두 (0.5, 0.5) 기준으로 통일하여 중앙 배치 보장

### 추가 문제 해결
- **드래그 고정 문제**: DragTool에 isDragging 상태 추적 기능 추가, Update에서 드래그 중일 때만 마우스 위치 추적하도록 수정
- **슬롯 교체 문제**: CharacterInventoryTab의 ReturnCharacterBlock 메서드에서 RectTransform 속성 설정 누락 문제 해결
- **드래그 중 마우스와 블록 분리 문제**: CharacterBlock의 OnDrag 메서드에서 직접 마우스 위치를 추적하여 블록이 마우스와 함께 움직이도록 수정
- **드래그 추적 방식 개선**: DragTool이 자식 오브젝트 존재 여부로 드래그 상태를 판단하여 isDragging 상태에 의존하지 않고 지속적으로 마우스 추적
- **마우스 고정 문제 해결**: 슬롯 감지 실패 시 ReturnToAnchor에서 DragTool 분리 확인 및 디버그 로그 추가로 원인 파악 및 해결
- 드래그 시작/종료 시 DragTool의 StartDragging/StopDragging 메서드 호출로 정확한 상태 관리

## 2025-06-30
- 드래그&드롭 UI의 RectTransform 앵커/피벗 문제 해결
- BattleSettingCharacterSlot의 PlaceCharacterBlock, RemoveCharacterBlock 메서드에서 SetParent 후 RectTransform 속성을 명시적으로 설정하도록 수정
- 슬롯 자체의 RectTransform도 Awake에서 중앙 앵커로 초기화하도록 개선
- CharacterBlock의 OnEndDrag, ReturnToAnchor 메서드에서도 RectTransform 설정 강화
- 디버그 로그 추가로 실제 적용 여부 확인 가능하도록 개선
- anchorMin/Max, pivot, anchoredPosition을 모두 (0.5, 0.5) 기준으로 통일하여 중앙 배치 보장

### 추가 문제 해결
- **드래그 고정 문제**: DragTool에 isDragging 상태 추적 기능 추가, Update에서 드래그 중일 때만 마우스 위치 추적하도록 수정
- **슬롯 교체 문제**: CharacterInventoryTab의 ReturnCharacterBlock 메서드에서 RectTransform 속성 설정 누락 문제 해결
- **드래그 중 마우스와 블록 분리 문제**: CharacterBlock의 OnDrag 메서드에서 직접 마우스 위치를 추적하여 블록이 마우스와 함께 움직이도록 수정
- **드래그 추적 방식 개선**: DragTool이 자식 오브젝트 존재 여부로 드래그 상태를 판단하여 isDragging 상태에 의존하지 않고 지속적으로 마우스 추적
- **마우스 고정 문제 해결**: 슬롯 감지 실패 시 ReturnToAnchor에서 DragTool 분리 확인 및 디버그 로그 추가로 원인 파악 및 해결
- 드래그 시작/종료 시 DragTool의 StartDragging/StopDragging 메서드 호출로 정확한 상태 관리

## 2025-07-01 작업로그

### 파티 세팅 → 전투 연동 및 생성 로직 개선
- BattleSettingManager 오브젝트 누락으로 인한 파티 연동 불가 이슈 진단 및 해결
- 슬롯 → BattleSettingManager → SpawnManager → BattleManager allyPartyData 흐름 전 구간 디버그 추적
- 파티 세팅 UI에서 allyPartyData가 정상적으로 저장되는지, 전투 씬에서 반영되는지 확인
- BattleManager에서 플레이어/아군 생성 시 allyPartyData 순서대로 playerSlot/allySlots에 배치되도록 구조 확정

### 아군 캐릭터 flipX(좌우 반전) 적용
- 아군(allySlots) 생성 시 SpriteRenderer의 flipX를 true로 적용하여 방향 연출 개선
- 플레이어(주인공)는 flipX 미적용(정방향 유지)
- 프리팹 구조상 "Sprite" 자식 오브젝트의 SpriteRenderer에 flipX 적용하도록 코드 수정
- flipX 적용 결과 시각적으로 정상 확인

### 기타
- BattleSettingManager, 슬롯, 인벤토리 등 Inspector 연결 상태 점검 및 재설정
- 디버그 로그로 allyPartyData, 슬롯 상태 등 실시간 추적
- 오브젝트 이름 변경/컴포넌트 누락 등 Unity 에디터 실수로 인한 연동 문제 경험 및 해결

---

## 2025-07-02 이후 해야 할 작업들

### 1. 주인공 생성 방식 변경
- **기존:** 1번슬롯엔 생성스크립트단에서 강제로 주인공생성성
- **변경:**
  - 파티세팅 슬롯 1번에는 주인공(메인 캐릭터)만 생성/배치
  - 주인공은 슬롯 1번에 강제 배치되며, 다른 캐릭터는 2~4번 슬롯에만 배치 가능
  - 파티 세팅 UI 및 SpawnManager, BattleManager 연동 시 이 규칙을 강제 적용
  - 그에따라 파티세팅쪽에서 이어진게 아닌 기존의 강제생성 로직을 해체하고 분석해서 필요한 부분을 절제해야함함

### 2. 전투 UI 스킬 프리팹 활성화 개선
- **문제점:**
  - 현재는 모든 캐릭터의 스킬 프리팹이 전투 UI에 한 번에 생성되어 겹쳐 보임
- **개선 방향:**
  - 턴이 온 캐릭터(행동 가능한 캐릭터)의 스킬 UI만 활성화
  - 나머지 캐릭터의 스킬 UI는 비활성화 또는 숨김 처리
  - BattleUIManager(또는 관련 UI 컨트롤러)에서 턴 변경 시 UI 활성/비활성 로직 추가

---

## 2025-07-03 전투 UI 스킬 프리팹 활성화 개선 및 주인공 생성 방식 정비

### 전투 UI 스킬 프리팹 활성화 개선
- **문제점**: 모든 캐릭터의 스킬 프리팹이 전투 UI에 한 번에 생성되어 겹쳐 보임
- **해결 방안**: 
  - BattleUIManager에 UpdateSkillUIForTurn(), DisableAllSkillUI() 메서드 추가
  - TurnManager의 StartTurn()에서 턴이 시작될 때 해당 캐릭터의 스킬만 활성화
  - 적 턴일 때는 모든 스킬 UI 비활성화
  - BattleManager의 CreateAllSkillButtons()에서 초기 상태를 비활성화로 설정
- **변경 사항**:
  - SkillInstance에 GetCaster() 메서드 추가
  - BattleUIManager에 skillSetRoot 필드 추가 및 스킬 UI 관리 기능 구현
  - TurnManager에서 턴 변경 시 스킬 UI 업데이트 로직 추가
  - BattleManager에서 BattleUIManager의 skillSetRoot 설정

### 주인공 생성 방식 정비
- **기존 구조**: 파티 세팅에서 1번 슬롯에 주인공 자동 배치 및 잠금 처리 (이미 잘 구현됨)
- **개선 사항**:
  - BattleManager의 SpawnAllUnits()에서 fallback 로직 제거
  - allyPartyData만 사용하여 파티 세팅 기반 캐릭터 생성
  - 1번 슬롯(주인공) → playerSlot, 2~4번 슬롯 → allySlots 구조 명확화
- **결과**: 파티 세팅에서 설정된 캐릭터들이 전투에서 정확히 반영되며, 주인공은 1번 슬롯에 고정

### 기타 개선사항
- 디버그 로그 개선으로 스킬 UI 활성화/비활성화 상태 추적 가능
- 전투 시작 시 모든 스킬 버튼이 비활성화 상태로 시작하여 UI 깔끔함 확보
- 턴 기반 스킬 UI 관리로 사용자 경험 개선

---  

## 2025-07-04 할 일(TO-DO)

### 파티세팅 → 주인공 스킬 생성 로직 설계/구현
- **목표**: 파티세팅 UI에서 주인공(1번 슬롯)만 스킬을 직접 세팅할 수 있도록 스킬 생성/연동 구조 설계 및 구현
- **구체적 계획**:
  - 파티세팅 UI에서 1번 슬롯(주인공) 선택 시, 스킬 슬롯/버튼이 동적으로 생성되도록 구현
  - 주인공의 스킬은 파티세팅에서 직접 선택/교체/세팅 가능하게 UI/로직 설계
  - 나머지 파티원(2~4번 슬롯)은 각자 고유 스킬 목록을 기준으로 전투 진입 시 자동 생성
  - 스킬 데이터/아이콘/이름 등 UI 연동 및 저장 구조 점검
  - 파티세팅에서 세팅한 주인공 스킬 정보가 전투 씬까지 정확히 연동되는지 검증
- **참고**: 파티세팅에서 주인공 외 파티원은 스킬 세팅 불가, 각자 고유 스킬만 사용

### 스킬 인벤토리(도감) 및 해금 조건 설계
- **스킬 인벤토리 구조**: 개별 인스턴스/수량/상태 관리 없이, 해금된 스킬ID 리스트만 관리 (ex: List<string> unlockedSkillIDs)
- **스킬 해금 조건**: 전투 종료 후 보상 정산 시, 사망한 캐릭터가 소지했던 모든 스킬ID를 인벤토리에 해금
- **해금 로직**:
  - deadCharacters 리스트의 각 캐릭터의 Skills 배열을 순회하며 SkillInventory.UnlockSkill(skillID) 호출
  - 이미 해금된 스킬은 중복 처리 없이 무시
- **UI/저장**: 해금된 스킬은 인벤토리 UI에 즉시 반영, 세이브 데이터에는 unlockedSkillIDs만 저장
- **추가 아이디어**: 해금 시 연출/팝업/이펙트, 한정 스킬 등도 기획 가능

### 스킬 아이콘 리소스 준비
- **목적**: 스킬 인벤토리/세팅/전투 UI에서 각 스킬을 직관적으로 구분할 수 있도록 아이콘 리소스 준비
- **계획**:
  - 스킬별로 고유 아이콘(임시/최종)을 준비
  - 아이콘 파일명/경로/포맷 통일(예: Resources/SkillIcon/스킬ID.png)
  - SkillData에 아이콘 경로 필드가 정확히 매칭되도록 점검
  - 인벤토리/세팅/전투 UI에서 아이콘이 정상적으로 표시되는지 테스트
- **기대 효과**:
  - 스킬 선택/세팅/사용 시 시각적 구분이 쉬워짐
  - 플레이어의 직관적 판단 및 UX 향상
  - 개발/디버깅 시 스킬 연동 상태 확인이 쉬워짐

### 스킬 세팅 정보 연동 구조 설계
- **목표**: 파티세팅에서 세팅한 주인공 스킬 정보가 BattleSettingManager → SpawnManager → BattleManager까지 매끄럽게 연동
- **구조 설계**:
  - BattleSettingManager에 playerSkillIDs[4] 필드 추가 (주인공 스킬 4개 슬롯)
  - SpawnManager에 playerSkillIDs 필드 추가하여 BattleSettingManager에서 받은 스킬 정보 저장
  - BattleManager의 SpawnPlayer()에서 SpawnManager.playerSkillIDs를 CharacterStats.Skills에 적용
  - 파티원(2~4번 슬롯)은 각자 고유 스킬 사용, 주인공만 세팅 가능
- **UI 구현**:
  - 1번 슬롯(주인공) 선택 시 스킬 슬롯 4개 동적 생성
  - 스킬 인벤토리에서 드래그&드롭으로 스킬 배치
  - 스킬 슬롯 변경 시 BattleSettingManager.playerSkillIDs 실시간 업데이트
- **데이터 흐름**: 파티세팅 UI → BattleSettingManager → SpawnManager → BattleManager → 전투

---  

## 2025-07-05 스킬 인벤토리 드래그&드롭 및 아이콘 오버라이드 개선

- SkillBlock에 inventoryContentTransform 필드 추가 및 Inspector에서 직접 연결
- 드래그 종료 시 슬롯/인벤토리 분기 처리(PlaceInSlot, ReturnToInventory)로 RectTransform 앵커/피벗/위치 자동 복구
- 인벤토리 복귀 시 좌상단, 슬롯 진입 시 중앙 기준으로 UI 정렬 문제 해결
- SkillLoader.cs의 OverrideSkill에 Icon 필드 오버라이드 코드 추가로 스킬 아이콘 상속/오버라이드 버그 수정
- 전체 스킬/파티/인벤토리 UI 연동 구조 점검 및 정상 동작 확인
- Inspector 연결 방식으로 유지보수성 및 디버깅 편의성 향상

### [미해결/추가 해야 할 일]
- 스킬 인벤토리 UI/UX 추가 개선(툴팁, 정렬, 필터 등) 
- 스킬 해금 시 연출/이펙트/팝업 구현
- 파티/스킬 세팅 UX 보완(슬롯 교체, 드래그 피드백 등)
- 스킬/캐릭터 데이터 저장/불러오기 구조 점검 및 리팩터링(캐릭터에 지정된 스킬은 잘 불러와짐)
- 전체 코드 리팩터링 및 주석/문서화
- 테스트 및 예외 상황(슬롯 중복, 잘못된 드래그 등) 처리 강화

## 2025-07-06 스킬 프리셋 시스템 설계 및 PresetManager 구현

- **SkillPreset, PresetManager.cs 신규 생성**
    - 항상 존재하는 디폴트 프리셋(최근 사용) 구조 설계 및 구현
    - 스킬 세팅이 바뀔 때마다 디폴트 프리셋에 자동 저장, 세팅 UI 오픈 시 자동 복원
    - PresetManager 싱글톤 구조, userPresets 리스트로 확장성 확보
- **프리셋 구조 및 UX 논의**
    - 디폴트 프리셋은 삭제/이름변경 불가, 항상 최신 상태 자동 반영
    - 유저 프리셋은 추가/삭제/이름변경 등 확장 가능 구조로 설계
    - 프리셋 시스템 도입으로 다양한 전략/상황별 세팅 전환 UX 개선
- **전체 데이터 흐름 점검 및 자동화**
    - 오드(주인공) 자동 배치, 스킬 디폴트값 보장, Progress 연동 등 전체 구조 일관성 확보
    - 디버깅/로그 루틴 정비, 데이터 꼬임 방지
- **향후 계획**
    - 프리셋 추가/삭제/불러오기/이름변경 등 UI 및 기능 확장 예정
    - 파티/장비/전체 세트 프리셋 등으로 확장 가능성 논의

---

## 2025-07-08: 슬롯 기반 단방향 전달 구조 리셋

### 작업 내용
- **핵심 구조 단순화**: 슬롯 → 스폰매니저 → 배틀매니저 순서의 완전 단순화 구조로 리셋
- **불필요한 과정 제거**: CharacterBlock 참조, mainCharacterBlock 연결, 딕셔너리/프리팹/중간 데이터, 복제본/참조 꼬임 등 모두 제거
- **데이터 흐름 정리**:
  - **전투용**: 슬롯 UI → SpawnManager.partySkillIDs → BattleManager → CharacterStats.Skills
  - **저장용**: 슬롯 UI → GameProgressManager.savedSkillPreset

### 변경된 파일들
1. **BattleSettingManager.cs**: 
   - 슬롯 기반 단방향 전달 구조로 완전 리셋
   - GetPartySkillIDsFromSlots()로 4개 슬롯의 skillID 추출
   - SaveSkillPreset()/LoadSkillPreset() 프리셋 저장/로드 기능 추가
   - 불필요한 CharacterBlock 참조, mainCharacterBlock 연결 로직 제거

2. **SkillSlot.cs**: 
   - currentSkillID만 관리하고 GetSkillID()로 반환하는 단순 구조
   - mainCharacterBlock, skillIDs 동기화 등 복잡한 참조 로직 제거
   - BattleSettingManager.SetPlayerSkill() 호출로 단방향 전달

3. **SpawnManager.cs**: 
   - partySkillIDs(string[4])만 사용하는 단순 구조
   - debugPartySkillIDs, allyPartySkillIDs, partyBlocks 등 복잡한 참조 제거

4. **BattleManager.cs**: 
   - SpawnManager.partySkillIDs를 받아 유닛 생성 시 CharacterStats.Skills에 복사
   - 플레이어(0번)인 경우에만 partySkillIDs 사용

5. **GameProgressManager.cs**: 
   - GameProgressData에 savedSkillPreset(string[4]) 필드 추가
   - 프리셋 저장/로드 기능 지원

### 핵심 개선사항
- **단방향 전달**: 슬롯 UI에서 선택한 skillID가 전투씬까지 1:1로 반영
- **참조 꼬임 해결**: CharacterBlock, 프리팹, 딕셔너리 등 중간 참조 제거
- **디버그 포인트**: 슬롯 UI, 전투 진입 직전, 전투씬 진입 직후, 유닛 생성 직후에 로그 추가
- **프리셋 시스템**: 슬롯 상태를 GameProgressManager에 저장/로드 가능

### 다음 단계
- 컴파일 오류 수정 (TimelineManager, SkillButton 등)
- 실제 테스트를 통한 데이터 흐름 검증
- 프리셋 저장/로드 UI 연동

## [2025-07-09] SkillSlot 스킬ID 전달 구조 점검 및 분석

### 현상
- 슬롯 UI에서 4개 스킬을 배치해도, 실제로는 1개만 전달되는 문제 발생.
- 콘솔 로그상 GetPartySkillIDsFromSlots에서 index 0만 반복적으로 출력됨.

### 구조 분석
- SkillSlot: slotIndex, currentSkillID, GetSkillID() 등으로 구성. Initialize에서 slotIndex를 세팅.
- BattleSettingManager: skillSlotContainer 하위에 4개 SkillSlot 프리팹을 Instantiate, 각 슬롯에 Initialize(i, "") 호출.
- GetPartySkillIDsFromSlots: skillSlotContainer 하위의 모든 SkillSlot을 순회하며 slotIndex, GetSkillID()로 배열을 채움.

### 원인 가능성
- skillSlotContainer 하위에 SkillSlot이 1개만 생성되었거나,
- 4개가 생성되어도 slotIndex가 모두 0으로 세팅되어 있을 가능성.
- 프리팹의 slotIndex가 0으로 고정되어 있어도, Initialize에서 덮어쓰므로 원칙적으로 문제 없음.
- Instantiate/Awake/Initialize 호출 순서, skillSlotContainer의 자식 관리 구조 점검 필요.

### 점검/수정 권장 사항
1. skillSlotContainer 하위에 SkillSlot이 4개 생성되는지, 각각의 slotIndex가 0~3으로 세팅되는지 Inspector에서 직접 확인.
2. SkillSlot 프리팹의 slotIndex 값은 Initialize에서 덮어쓰므로 Awake에서 0으로 찍히는 건 정상.
3. RefreshSkillSlots, SkillSlot.Initialize, GetPartySkillIDsFromSlots 등 주요 지점에 디버그 로그 추가하여 실제 데이터 흐름 추적.
4. 구조상 문제라면 Instantiate/Initialize 호출 구조, 프리팹 구조, skillSlotContainer의 자식 관리 로직을 꼼꼼히 점검할 것.

### 결론
- SkillSlot이 4개 생성되고, 각 slotIndex가 0~3으로 세팅되는지 확인이 핵심.
- Inspector에서 실제 상태를 반드시 직접 확인할 것.
- 구조적 문제라면 프리팹, Instantiate, Initialize, 자식 관리 구조를 집중 점검.

## [2025-07-09] SkillSlot 스킬ID 전달 구조 디버깅 및 개선

### 문제 해결 과정
1. **GetPartySkillIDsFromSlots 메서드 개선**
   - skillSlotContainer의 자식 오브젝트 개수와 이름을 상세히 로그로 출력
   - GetComponentsInChildren<MonoBehaviour>로 찾은 모든 컴포넌트 타입을 로그로 출력
   - SkillSlot 컴포넌트 발견 시 index와 skillID를 상세히 로그로 출력
   - 리플렉션 사용 시 메서드/필드 찾기 실패 시 경고 로그 추가

2. **RefreshSkillSlots 메서드 개선**
   - 기존 자식 개수와 제거할 자식 이름을 상세히 로그로 출력
   - 각 슬롯 생성 시 이름을 "SkillSlot_{i}" 형태로 명시적 설정
   - 슬롯 컴포넌트 타입과 Initialize 메서드 호출 결과를 상세히 로그로 출력
   - 최종 생성된 슬롯 개수를 로그로 출력

3. **디버깅 목표**
   - skillSlotContainer 하위에 실제로 4개의 SkillSlot이 생성되는지 확인
   - 각 SkillSlot의 slotIndex가 0~3으로 올바르게 설정되는지 확인
   - GetPartySkillIDsFromSlots에서 모든 슬롯의 정보를 올바르게 추출하는지 확인

### 다음 단계
- Unity에서 실제 테스트 실행하여 로그 확인
- Inspector에서 skillSlotContainer 하위 구조 직접 점검
- 필요시 추가 디버깅 및 수정 진행

## [2025-07-09] SkillSlot 스킬ID 전달 구조 문제 해결 완료

### 문제 해결 과정 (계속)
4. **SkillSlot 클래스 개선**
   - public SlotIndex 프로퍼티 추가로 slotIndex에 직접 접근 가능하도록 개선
   - 리플렉션 사용 시 더 안전하고 간단한 방식으로 수정

5. **GetPartySkillIDsFromSlots 메서드 최종 개선**
   - GetComponentsInChildren<MonoBehaviour>로 모든 컴포넌트를 찾고 SkillSlot 타입만 필터링
   - GetSkillID 메서드와 SlotIndex 프로퍼티를 리플렉션으로 안전하게 접근
   - 각 단계별 상세한 디버깅 로그 추가로 문제 원인 파악 가능

6. **RefreshSkillSlots 메서드 개선**
   - 기존 자식 개수와 제거할 자식 이름을 상세히 로그로 출력
   - 각 슬롯 생성 시 이름을 "SkillSlot_{i}" 형태로 명시적 설정
   - 슬롯 컴포넌트 타입과 Initialize 메서드 호출 결과를 상세히 로그로 출력
   - 최종 생성된 슬롯 개수를 로그로 출력

### 해결된 문제점
- 리플렉션 사용 시 복잡성과 오류 가능성 감소
- SkillSlot 컴포넌트 접근 방식 개선
- 상세한 디버깅 로그로 문제 원인 파악 가능
- 컴파일 오류 해결

### 다음 단계
- Unity에서 TestStage 씬 실행하여 실제 테스트
- 콘솔 로그를 통해 다음 사항 확인:
  1. RefreshSkillSlots에서 4개의 SkillSlot이 제대로 생성되는지
  2. 각 SkillSlot의 slotIndex가 0~3으로 올바르게 설정되는지
  3. GetPartySkillIDsFromSlots에서 모든 슬롯의 정보를 올바르게 추출하는지
- 필요시 추가 디버깅 및 수정 진행

## [2025-07-09] SkillSlot 프리팹 문제 발견 및 해결

### 발견된 핵심 문제
- **TestStage 씬에서 skillSlotPrefab이 할당되지 않음**: BattleSettingManager의 skillSlotPrefab 필드가 `{fileID: 0}`으로 설정되어 있어서 프리팹을 Instantiate할 수 없음
- **기존 수동 배치된 SkillSlot 활용**: TestStage 씬에 이미 4개의 SkillSlot 오브젝트가 수동으로 배치되어 있고, SkillPresetHandler에서 이들을 참조하고 있음

### 해결 방법
1. **RefreshSkillSlots 메서드 완전 리팩터링**
   - 프리팹 Instantiate 방식에서 기존 SkillSlot 재사용 방식으로 변경
   - GetComponentsInChildren<MonoBehaviour>로 모든 컴포넌트를 찾고 SkillSlot 타입만 필터링
   - 리플렉션을 사용하여 Initialize 메서드를 안전하게 호출

2. **기존 구조 활용**
   - TestStage 씬의 수동 배치된 4개 SkillSlot을 그대로 사용
   - 프리팹 할당 문제를 우회하여 안정적인 구조로 변경

### 최종 결과
- 컴파일 오류 해결
- 기존 수동 배치된 SkillSlot들을 정상적으로 초기화
- 4개 슬롯이 모두 정상적으로 작동하도록 구조 개선

### 다음 단계
- Unity에서 TestStage 씬 실행하여 실제 테스트
- 1번 슬롯(주인공) 클릭 시 스킬 세팅 UI가 정상적으로 열리는지 확인
- 4개 SkillSlot이 모두 정상적으로 초기화되는지 확인
- GetPartySkillIDsFromSlots에서 모든 슬롯의 정보를 올바르게 추출하는지 확인

## [2025-07-09] 데이터 흐름 단순화 및 직접 관리 방식 도입

### 문제 상황
- 여전히 4개 스킬 중 1개만 전달되는 문제 발생
- 복잡한 리플렉션과 컴포넌트 검색으로 인한 불안정성
- SkillSlot 컴포넌트 접근 시 타입 인식 문제

### 해결 방법: 직접 관리 방식 도입
1. **BattleSettingManager에 직접 스킬ID 배열 추가**
   - `private string[] playerSkillIDs = new string[4] { "", "", "", "" };` 필드 추가
   - 복잡한 컴포넌트 검색 대신 직접 배열 관리

2. **GetPartySkillIDsFromSlots 메서드 단순화**
   - 기존: 복잡한 리플렉션과 컴포넌트 검색
   - 변경: `return (string[])playerSkillIDs.Clone();`로 단순화

3. **SetPlayerSkill 메서드 개선**
   - 직접 `playerSkillIDs[slotIndex] = skillID;`로 배열 업데이트
   - 상세한 디버그 로그 추가로 데이터 흐름 추적 가능

4. **SkillSlot과의 연동**
   - SkillSlot에서 PlaceSkillBlock, RemoveSkillBlock, SetSkill 시 SetPlayerSkill 호출
   - 단방향 데이터 흐름: SkillSlot → BattleSettingManager → SpawnManager → BattleManager

### 기대 효과
- 복잡한 리플렉션 제거로 안정성 향상
- 직접 배열 관리로 데이터 일관성 보장
- 단순한 데이터 흐름으로 디버깅 용이성 증가
- 4개 스킬이 모두 정상적으로 전달될 것으로 예상

### 다음 단계
- Unity에서 TestStage 씬 실행하여 실제 테스트
- 스킬 슬롯에 스킬 배치 시 playerSkillIDs 배열이 올바르게 업데이트되는지 확인
- 전투 진입 시 4개 스킬이 모두 정상적으로 전달되는지 확인

## 2025-07-10 진행상황 및 구조 결정

### 1. 전투 준비 및 스킬 슬롯 구조 완성
- 파티 세팅 UI에서 주인공 스킬 4개가 정상적으로 전투에 전달되는 구조 완성
- 데이터 흐름을 단순화하여 BattleSettingManager에서 직접 배열로 스킬ID를 관리
- SkillSlot → BattleSettingManager → SpawnManager → BattleManager로 단방향 전달 구조 확립

### 2. 상태이상 UI 구조 개선
- 캐릭터 프리팹 하위에 StatusEffectSlot 오브젝트를 두고, 상태이상 아이콘을 그리드 정렬로 자동 배치
- 월드 오브젝트용 그리드 정렬 스크립트 적용 (한 줄 5개, 2줄, 9개 초과 시 ...아이콘 표시)
- Inspector에서 cellSize, maxPerRow, moreIconPrefab 등 세부 옵션 조정 가능

### 3. To-Do(주요 남은 과제) 정비
- 스킬 쿨타임 관련 항목은 기획상 삭제(존재하지 않음)
- "전투 연출(이펙트, 애니메이션, 타격감 등) 강화 및 세부 개선" 항목으로 변경
- "전투 종료 후 보상/스킬 해금/캐릭터 상태 저장" 항목은 계속 유지(중요)

### 4. 기타 결정 및 논의 사항
- 월드 스페이스 UI의 장단점, 해상도/카메라/퍼포먼스 이슈 등 실전 고민 및 설계 논의
- 상태이상 UI의 최대 표시 개수, 더보기(...) 아이콘 등 UX 세부 설계 확정
- 앞으로의 전투 시스템 개선 방향(연출 강화, 보상/스킬 해금 등) 명확화

---

이상, 2025-07-10 기준 전체 구조/진행상황 요약.

## 2025-07-11 전투 보상 시스템 구현 완료

### 구현된 기능
1. **BattleManager 전투 결과 데이터 수집**
   - BattleResultData 클래스로 전투 결과 정보 저장
   - 사망한 적/아군 캐릭터 ID 수집
   - 사망한 적이 소지했던 스킬 해금 처리
   - 경험치 및 골드 계산 (임시 값: 적 1명당 경험치 100, 골드 50)

2. **GameProgressManager 보상 관리 시스템**
   - 경험치/골드 필드 추가
   - AddExperience(), AddGold() 메서드 구현
   - GetExperience(), GetGold() 메서드 구현

3. **WorldMapStageSelection 전투 결과 처리**
   - 전투 종료 후 월드맵 복귀 시 결과 확인
   - 승리 시 보상 지급 및 스테이지 클리어 처리
   - 전투 결과 UI 표시 (승리/패배)
   - 스킬 해금, 경험치/골드 지급 자동화

4. **전투 종료 흐름 개선**
   - TurnManager에서 전투 종료 시 월드맵으로 복귀
   - BattleManager가 전투 결과 데이터 수집 후 정리
   - 월드맵에서 전투 결과 확인 및 보상 정산

### 게임 기본 사이클 완성
- **월드맵** → **파티세팅** → **전투** → **전투 결과** → **보상 정산** → **월드맵**
- 전투 보상 시스템을 통해 게임의 기본 진행 사이클이 완성됨
- 스킬 해금, 경험치/골드 획득, 스테이지 클리어 등 핵심 보상 요소 구현

### 다음 단계
- 전투 결과 UI 프리팹 제작 및 연결
- 보상 수치 조정 및 밸런싱
- 추가 보상 요소 (아이템, 업적 등) 구현
- 전투 연출 개선 및 이펙트 강화

## 2025-07-11 전투 보상 시스템 기초 구축

### 구현된 기능
1. **RewardPopupUI 스크립트 생성**
   - 월드맵에서 사용할 보상 팝업 UI 스크립트
   - 평소에는 SetActive(false) 상태로 유지
   - 전투 결과가 있을 때만 활성화되어 보상 표시
   - 승리/패배, 스테이지 클리어 여부, 보상 종류별 표시 기능

2. **RewardItemUI 스크립트 생성**
   - 개별 보상 아이템(스킬, 캐릭터) 표시 UI 스크립트
   - 스킬/캐릭터 데이터에서 정보 자동 로드
   - 아이콘, 이름, 설명 표시 기능
   - Resources 폴더에서 아이콘 자동 로드

3. **보상 시스템 기초 구조 완성**
   - BattleManager의 BattleResultData와 StageRewardData 구조 활용
   - 보상 시스템은 스킬 해금, 캐릭터 해금, 영혼먼지, 강자의 정수에 집중
   - 월드맵 → 파티세팅 → 전투 → 전투 결과 → 보상 정산 → 월드맵 게임 사이클 완성을 위한 기초 작업

4. **문서 업데이트**
   - scriptmap.md에 새 스크립트 정보 추가 완료

### 다음 단계
- 보상 팝업 프리팹 제작 및 UI 연결
- 전투 종료 후 월드맵에서 보상 팝업 자동 표시
- 실제 보상 데이터 연동 및 테스트

### 마나 시스템 고려사항 (추가 예정)
- **마나 시스템 설계**: 특수 캐릭터들만 최대 3의 마나 수치를 가짐
- **마나 스킬**: 마나를 소모하는 특별한 스킬 시스템 구현 예정
- **패시브 시스템 연동**: 마나는 패시브로 구성되어 있어, 패시브 커스텀을 통해 원래 마나가 없던 캐릭터도 마나 사용 가능
- **구현 방향**:
  - CharacterData에 Mana, MaxMana 필드 추가
  - 마나 보유 캐릭터 판별 로직 (예: 특정 캐릭터 ID 또는 플래그)
  - 마나 스킬과 일반 스킬 구분 시스템
  - 마나 소모/회복 로직 및 UI 표시
  - 전투 중 마나 관리 및 스킬 사용 제한 시스템
  - 패시브 시스템과 마나 시스템 연동 구조 설계
  - 패시브를 통한 마나 획득/증가 로직 구현

# 2025-07-14 중간작업 로그

## 보상(Reward) 시스템 및 UI 구조 개선

- **보상 시스템 전체 흐름 점검 및 구조 명확화**
    - 월드맵(씬) 복귀 시 GameManager에서 BattleManager 오브젝트 존재 여부를 체크하여 보상 처리 시작
    - BattleManager.LastBattleResult가 존재하면 RewardManager.ProcessBattleReward 호출, 이후 BattleManager 오브젝트 삭제
    - RewardManager에서 보상 정산(영혼먼지, 해금 스킬/캐릭터 등) 및 RewardUIManager.ShowReward로 UI 연동
    - RewardUIManager에서 캐릭터 슬롯/이미지, 보상 프리팹(스킬/캐릭터), 영혼먼지 등 UI 갱신

- **캐릭터 슬롯/이미지 출력 조건 개선**
    - 캐릭터 보상이 존재할 때만 CharacterSlot1~4에 이미지 출력, 없으면 슬롯 비활성화(null)
    - 캐릭터 데이터에서 Stand 스프라이트를 동적으로 로드하여 이미지에 할당
    - 슬롯 배경/출력 역할 분리 및 필드명/구조 정비

- **Reward 프리팹 분리 및 Inspector 구조 개선**
    - 캐릭터/스킬 보상 프리팹을 분리하여 각각 Inspector에 연결
    - UpdateUI에서 타입별로 프리팹 Instantiate

- **전체 보상 처리 흐름 실제 스크립트 기준으로 재점검**
    - BattleManager → (씬 전환) → GameManager → RewardManager → RewardUIManager 순으로 데이터/호출 흐름 명확히 파악
    - 보상 시스템의 진짜 시작점이 월드맵 복귀 시 BattleManager 오브젝트 체크임을 재확인

- **RewardManager 보상 지급 로직 개선**
    - GrantRewards에서 전투 결과의 해금 캐릭터, 해금 스킬, 영혼먼지 보상을 GameProgressManager(프로그레스)에 실제로 반영하도록 개선
    - 인벤토리/스킬탭 UI를 즉시 새로고침하여, 보상 결과가 바로 화면에 반영되도록 처리

    ```csharp
    // RewardManager.cs - 보상 지급/해금/저장 처리 핵심 로직
    private void GrantRewards(BattleResultData result)
    {
        // 1. (생략) deadEnemyIDs로 영혼먼지 정산

        // 2. 캐릭터 해금: 전투에서 해금된 캐릭터ID를 프로그레스에 저장
        foreach (var charID in result.unlockedCharacterIDs)
            GameProgressManager.Instance.UnlockCharacter(charID); // 중복시 무시

        // 3. 스킬 해금: 전투에서 해금된 스킬ID를 프로그레스에 저장
        foreach (var skillID in result.unlockedSkillIDs)
            GameProgressManager.Instance.UnlockSkill(skillID); // 중복시 무시

        // 4. 영혼먼지 등 재화 지급: 보상 수치만큼 인벤토리에 추가
        if (result.soulDustGained > 0)
            GameProgressManager.Instance.AddItem("SoulDust", result.soulDustGained);

        // 5. 인벤토리/스킬탭 UI 즉시 새로고침: 보상 결과가 바로 화면에 반영됨
        if (CharacterInventoryTab.Instance != null)
            CharacterInventoryTab.Instance.RefreshInventory();
        if (SkillInventoryTab.Instance != null)
            SkillInventoryTab.Instance.RefreshSkillInventory();
    }
    // (이 함수는 RewardManager.ProcessBattleReward에서 호출됨)
    ```

- **향후 계획/메모**
    - 보상 UI 연출/애니메이션, 추가 보상(아이템 등) 확장 예정
    - 보상 데이터 저장/불러오기, 예외 상황 처리 등 추가 점검 필요

# 2025-07-16 전투 보상 시스템 캐릭터 해금 문제 해결 작업

## 문제 상황
- 전투 후 보상으로 받아야 할 캐릭터가 인벤토리탭에 표시되지 않는 문제 발생
- 임시로 생성한 캐릭터는 인벤토리탭에 정상 표시되지만, 보상 캐릭터만 누락
- RewardManager에서 deadEnemyIDs가 빈 문자열로 들어와서 캐릭터 해금이 0개로 처리됨

## 원인 분석
1. **BattleManager에서 사망한 적 ID 수집 방식 문제**
   - 기존: allCharacters 리스트에서 사망한 적을 찾는 방식
   - 문제: CharacterStats.CharacterId가 비어있거나 올바르게 세팅되지 않음

2. **스테이지 클리어 상태 문제**
   - 이미 클리어된 스테이지에서는 보상 처리가 중단됨
   - ProcessVictoryRewards에서 isAlreadyCleared 체크로 return 처리

## 해결 과정

### 1. RewardManager 보상 지급 로직 개선
- AddCharacterToInventory 메서드 호출 방식으로 변경
- 중복 추가 방지 로직 추가
- 디버그 로그 강화

### 2. BattleManager 사망한 적 수집 방식 변경
- allCharacters 기반 → enemySlots 기반으로 변경
- 각 enemySlots의 자식 오브젝트에서 CharacterStats를 직접 추출
- IsDead가 true이고 CharacterId가 비어있지 않은 경우만 deadEnemyIDs에 추가

### 3. 디버그 로그 시스템 구축
- "[캐릭터리워드]" 접두사로 상세한 디버그 로그 추가
- enemySlots 상태, CharacterStats 정보, IsDead 상태, ID 수집 과정을 모두 로그로 출력
- 문제 발생 지점을 정확히 파악할 수 있도록 로그 체계 구축

## 현재 상태
- enemySlots 기반 사망한 적 수집 로직 구현 완료
- "[캐릭터리워드]" 디버그 로그 시스템 구축 완료
- 다음 테스트에서 실제 로그 결과를 확인하여 추가 문제점 파악 예정

## 다음 작업 계획
1. 새로운 스테이지에서 전투 테스트
2. "[캐릭터리워드]" 로그 분석으로 정확한 원인 파악
3. CharacterStats.CharacterId 세팅 문제 해결 (필요시)
4. 보상 지급 완료 후 인벤토리 UI 갱신 확인

## 기술적 개선사항
- enemySlots 기반 접근으로 실제 전투에 등장한 적만 보상 대상으로 삼음
- 디버그 로그 체계화로 문제 추적 능력 향상
- 중복 추가 방지로 데이터 무결성 확보

## ⚠️ 내일 작업 시 주의사항
**로그 읽고 작성할 때 반드시 BattleManager로부터 시작하는 흐름을 꼼꼼히 읽을 것!**
- BattleManager → TurnManager → RewardManager → GameProgressManager → CharacterInventoryTab
- 이 전체 흐름에서 어느 단계에서 문제가 발생하는지 정확히 파악 필요
- 특히 enemySlots 기반 사망한 적 수집 → deadEnemyIDs → 보상 지급 과정을 집중적으로 점검

## 2025-07-16 TPRG 계산기 서브프로젝트
### 오늘 하루 일정
- **프로젝트**: TPRG 계산기 개발
- **목적**: 턴제 RPG 게임의 다양한 계산 기능을 제공하는 도구 제작
- **기능 예정**:
  - 데미지 계산기 (공격력, 방어력, 크리티컬 등)
  - 상태이상 확률 계산기 (출혈, 화상, 독 등)
  - 스킬 효과 계산기 (버프, 디버프 등)
  - 턴 순서 계산기 (속도 기반)
  - 경험치/레벨 계산기
- **기술 스택**: 웹 기반 (HTML/CSS/JavaScript) 또는 데스크톱 앱
- **완료 예정**: 내일 (2025-07-17)
- **참고 사항**: 게임 개발 중단하고 재미있는 도구 제작으로 하루 휴식

## 2025-07-17 TPRG 계산기 서브프로젝트 완료
### 프로젝트 완료
- TPRG 계산기 서브프로젝트 성공적으로 완료
- 게임 개발 중단 없이 재미있는 도구 제작으로 하루 휴식 완료
- **참고**: 다른 프로젝트에서 작업하여 이 프로젝트의 worklog.md에서는 구체적인 기능 및 기술적 세부사항 확인 불가

## 2025-07-18 게임 개발 재개
### 오늘 작업 계획
- **전투 보상 시스템 캐릭터 해금 문제 해결 계속**
  - 어제 작업한 enemySlots 기반 사망한 적 수집 로직 테스트
  - "[캐릭터리워드]" 디버그 로그 분석으로 정확한 원인 파악
  - CharacterStats.CharacterId 세팅 문제 해결 (필요시)
  - 보상 지급 완료 후 인벤토리 UI 갱신 확인

### 우선순위 작업
1. **새로운 스테이지에서 전투 테스트**
   - BattleManager → TurnManager → RewardManager → GameProgressManager → CharacterInventoryTab 전체 흐름 점검
   - enemySlots 기반 사망한 적 수집 → deadEnemyIDs → 보상 지급 과정 집중 분석

2. **로그 분석 및 문제점 파악**
   - "[캐릭터리워드]" 로그를 통한 정확한 문제 지점 식별
   - CharacterStats.CharacterId 세팅 상태 확인

3. **보상 시스템 안정화**
   - 캐릭터 해금 로직 완전 수정
   - 인벤토리 UI 갱신 확인
   - 전체 보상 시스템 안정성 검증

### 참고 사항
- 어제 TPRG 계산기 개발로 게임 개발 중단
- 오늘부터 정상적인 게임 개발 작업 재개
- 전투 보상 시스템의 캐릭터 해금 문제를 우선적으로 해결하여 게임의 핵심 사이클 완성

### 디버그 로그 정리 완료
- **BattleManager**: 리워드 관련 디버그는 남기고 나머지 모든 디버그 로그 제거
- **RewardManager**: 리워드 관련 디버그는 남기고 나머지 모든 디버그 로그 제거
- **TurnManager**: 모든 디버그 로그 제거 (오류 관련 로그는 유지)
- **SpawnManager**: 모든 디버그 로그 제거
- **StageManager**: 모든 디버그 로그 제거 (오류 관련 로그는 유지)
- **CharacterStats**: 모든 디버그 로그 제거 (오류 관련 로그는 유지)
- **GameProgressManager**: 테스트 관련 디버그 로그 제거

### 정리된 디버그 로그 종류
- 일반적인 정보성 디버그 로그 (Debug.Log)
- 슬롯 기반 시스템 관련 디버그 로그
- 스킬 생성 관련 디버그 로그
- 스테이지 관리 관련 디버그 로그
- 캐릭터 생성/로딩 관련 디버그 로그

### 유지된 디버그 로그
- **리워드 관련**: "[캐릭터리워드]" 접두사로 시작하는 모든 로그
- **오류 관련**: Debug.LogError, Debug.LogWarning (빨간색, 노란색 로그)
- **중요한 경고**: 시스템 동작에 영향을 주는 경고 로그

### 결과
- 게임 콘솔의 디버그 로그 수 대폭 감소
- 리워드 관련 문제 해결에 집중할 수 있는 환경 조성
- 중요한 오류나 경고는 여전히 확인 가능

## 2025-07-19 전투 보상 시스템 중복 보상 문제 해결 및 영혼먼지 시스템 구축

### 전투 보상 시스템 중복 보상 문제 해결
- **문제 상황**: 같은 스테이지를 여러 번 클리어해도 중복으로 보상이 지급되는 문제 발생
- **해결 과정**:
  1. **BattleManager**: `CollectBattleResultData`에서 이미 클리어된 스테이지인지 먼저 확인하여 보상 수집 중단
  2. **RewardManager**: `ProcessBattleReward`에서 클리어된 스테이지의 보상 데이터를 완전히 제거하고 보상 지급 중단
  3. **GameProgressManager**: `SaveGameProgress`에서 중복 저장 방지 로직 추가

### 영혼먼지 시스템 구축
- **영혼먼지 보상 시스템**: 적 캐릭터의 등급에 따라 영혼먼지 보상량 차등 지급
  - Normal: 2, Rare: 5, Uniqu: 10, Legend: 25
  - 주인공 등급(One)은 영혼먼지 보상 제외
- **GameProgressManager**: 영혼먼지와 강자의 정수 필드 추가 및 관리 메서드 구현
  - `AddSoulDust()`, `GetSoulDust()`, `SpendSoulDust()` 메서드
  - `AddEssence()`, `GetEssence()`, `SpendEssence()` 메서드
- **RewardManager**: 기존 `AddItem` 방식에서 새로운 재화 관리 메서드 사용으로 변경

### 월드맵 재화 UI 시스템 구축
- **WorldMapCurrencyUI**: 월드맵에서 영혼먼지와 강자의 정수를 실시간 표시하는 UI 스크립트
  - 0.5초마다 자동 업데이트 (설정 가능)
  - UI 요소가 할당되지 않아도 자동으로 찾아서 연결
  - 외부에서 강제 업데이트 가능
- **RewardManager 연동**: 전투 보상 지급 후 자동으로 월드맵 UI 업데이트
  - 월드맵 씬에서만 작동하도록 씬 체크
  - 재화 변경 시 즉시 반영

### 3중 안전장치 완성
1. **BattleManager**: 클리어 여부를 먼저 체크
2. **RewardManager**: 이미 클리어된 스테이지의 보상 데이터를 완전히 제거
3. **GameProgressManager**: 중복 저장 방지

### 결과
- 같은 스테이지를 여러 번 클리어해도 중복 보상이 지급되지 않음
- 영혼먼지와 강자의 정수가 별도로 관리되며 월드맵에서 실시간 표시
- 전투 보상 시스템의 안정성과 확장성 확보

## 2025-07-19 ~ 2025-07-20 주말 휴식
- **2025-07-19 (토요일)**: 주말 휴식으로 게임 개발 작업 중단
- **2025-07-20 (일요일)**: 주말 휴식으로 게임 개발 작업 중단
- **재개 예정**: 2025-07-21 (월요일)부터 정상적인 게임 개발 작업 재개
- **참고**: 주말 휴식으로 인한 작업 중단은 정기적인 휴식 계획에 따른 것

## 2025-07-22 영혼먼지 시스템 디버그 로그 강화 및 테스트 기능 추가

### 영혼먼지 디버그 로그 시스템 구축
- **GameProgressManager**: 영혼먼지 관련 메서드에 상세한 디버그 로그 추가
  - `AddSoulDust()`: 이전/현재 수량 비교 로그 추가
  - `GetSoulDust()`: 현재 보유량 출력 로그 추가
  - `SpendSoulDust()`: 이전/현재 수량 비교 로그 추가
  - `DebugSoulDustStatus()`: 영혼먼지 상태 상세 출력 메서드 신규 추가

### 월드맵 UI 디버그 로그 강화
- **WorldMapCurrencyUI**: UI 업데이트 과정에 상세한 디버그 로그 추가
  - `UpdateCurrencyDisplay()`: UI 업데이트 시 영혼먼지 수량 로그 출력
  - null 체크 및 경고 로그 추가
  - `TestSoulDustStatus()`: 영혼먼지 상태 테스트 메서드 신규 추가
  - `AddTestSoulDust()`: 테스트용 영혼먼지 추가 메서드 신규 추가

### RewardManager 로그 개선
- **RewardManager**: 영혼먼지 보상 지급 과정에 상세한 로그 추가
  - 보상 지급 시작/완료 로그 분리
  - "[영혼먼지][보상]" 접두사로 로그 구분 명확화

### 디버그 로그 접두사 체계화
- **[영혼먼지]**: 기본 영혼먼지 관련 로그
- **[영혼먼지][UI]**: UI 관련 영혼먼지 로그
- **[영혼먼지][보상]**: 보상 지급 관련 영혼먼지 로그
- **[영혼먼지][테스트]**: 테스트 관련 영혼먼지 로그
- **[영혼먼지][상태]**: 상태 확인 관련 영혼먼지 로그

### 테스트 방법
1. **월드맵에서 영혼먼지 상태 확인**: `TestSoulDustStatus()` 메서드 호출
2. **테스트용 영혼먼지 추가**: `AddTestSoulDust()` 메서드 호출
3. **전투 후 보상 확인**: 전투 종료 시 콘솔에서 "[영혼먼지]" 로그 확인
4. **UI 업데이트 확인**: 월드맵에서 영혼먼지 UI 변경 시 로그 확인

### 결과
- 영혼먼지 시스템의 모든 단계에서 상세한 디버그 로그 출력
- UI 표시 문제와 데이터 문제를 구분하여 파악 가능
- 테스트 기능을 통한 영혼먼지 시스템 검증 가능

### 추가 수정 사항 (2025-07-22 오후)
- **UI 표시 개선**: 월드맵에서 영혼먼지와 강자의 정수를 숫자만 표시하도록 수정
  - 기존: "영혼먼지: 9" 형태
  - 변경: "9" 형태 (아이콘으로 대체 예정)
- **중복 계산 문제 해결**: RewardManager에서 HashSet을 사용하여 중복된 적 ID 제거
  - Normal 등급 3개 × 2 = 6개가 정상적으로 계산되도록 수정
  - 기존 9개에서 6개로 정확한 계산 결과 도출
- **디버그 로그 개선**: "[영혼먼지][보상]" 접두사로 보상 관련 로그 구분 명확화

## 2025-07-22 패시브 시스템 구조 설계 및 기반 코드 구축

- PassiveData ScriptableObject 구조 확정 및 확장 필드 추가 (레어리티, 코스트, 최대 패시브 코스트 등)
- 캐릭터별 패시브 슬롯 한도(maxPassiveCost) 구조 설계 및 XML/파싱/클론/오버라이드까지 일관성 있게 반영
- PassiveData에 패시브 효과 ScriptableObject(효과 스크립트) 연결 필드(effectScript) 추가
- PassiveEffectBase 추상 SO 생성, 효과별로 상속받아 구현하는 구조로 설계
- 패시브 효과의 데이터(정의)와 로직(효과) 완전 분리, 유지보수성과 확장성 극대화
- 실전 예시: 최대체력+10 효과 패시브 설계, 효과 스크립트로 분리 구현 준비
- 전체 구조가 "PassiveData(정의) ↔ PassiveEffectBase(로직) ↔ 캐릭터 인스턴스"로 명확하게 정립됨
- 다음 단계: 효과별 SO 구현, 적용 컨트롤러에서 effectScript.Apply 호출 구조로 확장 예정

## 2025-07-23 패시브 시스템 구현 완료

### 구현된 기능
1. **패시브 데이터 구조**
   - PassiveData 클래스: 패시브의 기본 정보 (ID, 이름, 설명, 타입, 수치 등)
   - PassiveType enum: 패시브 효과 타입 (None, ManaBoost 등)
   - XML 기반 데이터 관리 시스템

2. **패시브 효과 시스템**
   - PassiveEffectBase 추상 클래스: 모든 패시브 효과의 기본 클래스
   - 구체적 효과 클래스들:
     - PassiveEffectMaxHpBoost: 최대 체력 증가
     - PassiveEffectAtkBoost: 공격력 증가
     - PassiveEffectDefBoost: 방어력 증가
     - PassiveEffectManaBoost: 마나 시스템 활성화

3. **패시브 로더 시스템**
   - PassiveLoader: XML에서 패시브 데이터를 로드하는 정적 클래스
   - 상속/오버라이드 시스템: ParentID를 통한 데이터 상속
   - 파싱 및 검증 로직

4. **캐릭터 통합 시스템**
   - CharacterStats에 패시브 관리 기능 추가
   - SetData() 시 자동으로 패시브 효과 적용
   - 활성 패시브 ID 추적 및 관리

5. **XML 데이터**
   - BasePassive.xml: 기본 패시브 데이터 정의
   - 체력 강화, 공격력 강화, 방어력 강화, 마나 수련 등 기본 패시브 포함

### 기술적 특징
- **확장성**: 새로운 패시브 효과를 쉽게 추가할 수 있는 구조
- **유지보수성**: 데이터와 로직의 분리로 관리 용이
- **일관성**: 기존 시스템(캐릭터, 스킬, 상태이상)과 동일한 패턴 적용
- **안정성**: 컴파일 오류 해결 및 기본 동작 보장

### 다음 단계
- 패시브 UI 시스템 구현 (패시브 선택, 장착, 해제 등)
- 패시브 코스트 시스템과 캐릭터 maxPassiveCost 연동
- 패시브 해금 시스템 (전투 보상으로 패시브 획득)
- 마나 시스템과 패시브 시스템의 완전한 연동

## 2025-07-26 전투 연출 시스템 1차 구현 완료

### 구현된 기능
1. **BattleEffectManager 클래스**
   - 전투 연출을 관리하는 중앙 매니저 클래스
   - 피격 이펙트, 데미지 팝업, 캐릭터 밀림, 데스 애니메이션 등을 통합 관리
   - 이벤트 기반 구조로 CharacterStats와 연동

2. **DamagePopup 클래스**
   - 데미지 수치를 표시하는 팝업 UI 클래스
   - 스케일 애니메이션, 위로 이동, 페이드 아웃 효과
   - 크리티컬 데미지와 일반 데미지 구분 표시
   - 회복량 표시용 특별 애니메이션 지원

3. **전투 연출 시스템**
   - **피격 시 효과**: 타격 이펙트 생성, 캐릭터 흔들림, 밀림 효과
   - **데미지 팝업**: 피해량 숫자 표시, 크리티컬 구분, 애니메이션 효과
   - **캐릭터 밀림**: 공격자 방향으로 밀림 후 원래 위치 복귀
   - **데스 애니메이션**: 페이드 아웃 효과와 데스 이펙트

4. **이벤트 기반 구조**
   - CharacterStats에 OnTakeDamageEvent, OnDeathEvent 추가
   - BattleEffectManager가 자동으로 모든 캐릭터의 이벤트를 구독
   - 느슨한 결합으로 시스템 간 의존성 최소화

### 기술적 특징
- **확장성**: 새로운 연출 효과를 쉽게 추가할 수 있는 구조
- **성능**: 이벤트 기반 구조로 효율적인 연출 관리
- **유지보수성**: 연출 로직과 게임 로직의 분리
- **안정성**: 컴파일 오류 해결 및 기본 동작 보장

### 구현된 연출 효과
1. **피격 연출**: 이펙트 생성 → 데미지 팝업 → 캐릭터 흔들림 → 밀림 효과
2. **크리티컬 연출**: 특별한 크리티컬 이펙트와 색상 구분
3. **데스 연출**: 페이드 아웃 효과와 데스 이펙트
4. **회복 연출**: 녹색 깜빡임 효과와 특별한 팝업 애니메이션

### 다음 단계
- 이펙트 프리팹 제작 및 연결 (피격, 크리티컬, 데스 이펙트)
- 데미지 팝업 프리팹 제작 및 UI 연결
- 연출 효과 세부 조정 및 밸런싱
- 추가 연출 효과 구현 (스킬 이펙트, 상태이상 이펙트 등)

## 미래 계획 및 To-Do

- 전투 연출 시스템 1차 구현이 완료되었으므로, 다음 주요 작업은 **이펙트 프리팹 제작 및 실제 연출 테스트**로 진행 예정
- 프리팹 제작 후 실제 전투에서 연출이 정상적으로 작동하는지 검증하고, 필요시 세부 조정을 진행할 예정
- 추가 연출 효과(스킬별 특수 이펙트, 상태이상 이펙트, UI 연출 등)는 기본 시스템 완성 후 확장 예정

## 2025-07-23
### PassiveManager KeyValuePair 오류 해결 및 Unity 정상화
- PassiveManager에서 Dictionary 순회 시 KeyValuePair<string, PassiveData>의 .Value로 접근해야 함을 인지
- passiveID 접근 오류(KeyValuePair에 .passiveID 없음) 발생 → pair.Value.passiveID로 수정하여 해결
- Dictionary 순회 구조와 KeyValuePair의 의미(키-값 쌍, pair.Key/pair.Value) 학습
- Unity Safe Mode 진입 → 코드 수정 및 전체 빌드 후 정상화 확인
- 기타: KeyValuePair, Dictionary 순회 패턴에 대한 이해도 향상

## 2025-07-26 전투 연출 시스템 개선 및 루틴 가속 문제 해결

### 전투 연출 시스템 개선
1. **BattleEffectManager 연출 타이밍 조정**
   - 피격 연출과 공격 연출의 타이밍 동기화
   - 공격 모션이 완료된 후 피격 연출이 시작되도록 순서 조정
   - 데미지 팝업, 캐릭터 흔들림, 밀림 효과의 자연스러운 연출

2. **캐릭터 밀림 효과 개선**
   - 밀림 거리와 지속시간 조정으로 자연스러운 연출
   - 캐릭터 스프라이트만 밀리도록 대상 수정 (체력바 제외)
   - 턴 종료 시 밀린 캐릭터들의 위치 자동 복귀

3. **데스 애니메이션 개선**
   - 피격 상태로 고정 후 페이드 아웃 효과
   - 0.5초 대기 후 2초간 페이드 아웃 진행
   - 캐릭터 오브젝트 완전 파괴 전 연출 완료 보장

### 루틴 가속 문제 해결
1. **문제 상황**
   - 적 턴에서 적 턴으로 넘어갈 때 페이드 아웃이 완료되기 전에 다음 적의 행동이 시작
   - 전체 과정이 가속되어 부자연스러운 전투 흐름

2. **해결 방법**
   - TurnManager.EndTurn()을 코루틴으로 변경
   - WaitForDeathEffects() 메서드로 죽는 연출 완료까지 3초 대기
   - 죽는 연출과 다음 턴 시작을 순차적으로 처리

3. **구현된 구조**
   - EndTurnCoroutine(): 턴 종료 처리와 죽는 연출 대기를 순차적으로 수행
   - WaitForDeathEffects(): 3초간 대기하여 페이드 아웃 완료 보장
   - 자연스러운 턴 전환: 페이드 아웃 → 다음 적 행동 → 페이드인

### 기술적 개선사항
- **MissingReferenceException 해결**: 모든 코루틴에 null 체크 추가
- **재귀 호출 문제 해결**: TurnDecider() 직접 호출을 DelayedTurnDecider() 코루틴으로 변경
- **로그 시스템 개선**: TurnIndicatorHandler의 오류 로그를 경고 로그로 변경
- **이벤트 기반 구조**: CharacterStats의 이벤트를 통한 느슨한 결합 유지

### 결과
- 전투 연출의 자연스러운 타이밍과 시각적 효과 향상
- 루틴 가속 문제 완전 해결로 안정적인 전투 흐름 확보
- 전투 시스템의 안정성과 확장성 향상

### 다음 단계
- 이펙트 프리팹 제작 및 실제 연출 테스트
- 추가 연출 효과 구현 (스킬별 특수 이펙트, 상태이상 이펙트 등)
- 전투 UI 연출 개선 및 애니메이션 강화

## 2025-07-28 전투 이펙트 시스템 구축 및 버그 수정

### 전투 이펙트 시스템 구축
- BattleEffectManager 클래스 구현
- 히트 이펙트, 데미지 팝업, 크리티컬 이펙트, 데스 이펙트 구현
- 애니메이션 최적화 (속도, 이징 함수 조정)
- UI 좌표 변환 시스템 구현 (월드 → 스크린 → 캔버스)

### 기초 스킬 자동 배치 시스템
- BattleSettingManager에서 기초 스킬 자동 배치 로직 구현
- GameProgressManager에서 기초 스킬 언락 제거 (중복 방지)
- 플레이어 스킬 슬롯 빈 상태 문제 해결

### 데스 연출 시스템 강화
- 캐릭터 즉시 사라짐 문제 해결 (CharacterStats.DeathAction 수정)
- HP바와 상태이상 슬롯 즉시 숨김 처리
- 캐릭터 스프라이트 페이드아웃 연출 구현
- CharacterStats에 UI 참조 필드 추가

### 모션 버그 수정
- 적 캐릭터 죽을 때 다른 적들의 이상한 움직임 버그 해결
- TurnManager.ResetKnockedBackCharacters() 호출 타이밍 조정
- CharacterMotionController 좌표계 일관성 문제 해결
- IsInBattlePosition 프로퍼티 추가 및 활용

### 2연타 버그 수정
- BattleEffectManager와 SkillManager의 EndTurn 중복 호출 문제 해결
- TurnManager.CheckBattleEnd() public으로 변경
- 전투 종료 체크와 턴 종료 로직 분리

## 2025-07-29 예정: 턴 전환 최적화 작업

### 작업 목표
- 플레이어 턴 → 적 턴 전환 시 미묘한 지연 현상 개선
- 턴 전환 과정의 불안정한 점들 수정
- 전투 흐름의 부드러움과 반응성 향상

### 분석 대상 (수정 전 로그 분석 필수)
1. **TurnManager 턴 전환 로직**
   - TurnDecider() 메서드 성능 분석
   - ResetTurn() → TurnDecider() 흐름 분석
   - 불필요한 대기 시간이나 연산 확인

2. **EnemyAIController 턴 시작**
   - 적 턴 시작 시 지연 요소 분석
   - AI 결정 로직 성능 확인

3. **UI 전환 효과**
   - 턴 전환 시 UI 애니메이션 성능
   - NextTurnIndicatorUI 최적화 필요성

4. **전투 상태 체크**
   - CheckBattleEnd() 호출 타이밍 분석
   - 불필요한 반복 체크 확인

### 작업 원칙
- **절대 선 수정 금지**: 먼저 정확한 로그 분석 후 수정
- **단계별 접근**: 한 번에 하나씩 문제점 파악 후 수정
- **성능 측정**: 수정 전후 성능 비교 필수
- **안정성 우선**: 속도 개선보다 안정성 확보가 우선

결론 내일 루틴 시작할때 수정하면서 시작하지 말고 일고만 작업대상들을 읽음 완료후 내게 보고

### 예상 개선 방향
- 턴 전환 시 불필요한 대기 시간 제거
- UI 애니메이션 최적화
- AI 결정 로직 개선
- 전투 상태 체크 최적화

## 2025-07-29 턴 전환 최적화 디버그 로그 시스템 구축

### 작업 내용
- **턴 전환 최적화를 위한 디버그 로그 시스템 구축**
  - TurnManager: ResetTurn, TurnDecider, StartTurn, EndTurnCoroutine, WaitForDeathEffects, ResetKnockedBackCharacters, CheckBattleEnd 메서드에 성능 측정 로그 추가
  - EnemyAIController: EnemyActionRoutine, UseSkill 메서드에 턴 시작 및 AI 행동 성능 측정 로그 추가
  - NextTurnIndicatorUI: CreateTurnBlocks, RemoveCurrentTurnBlock 메서드에 UI 생성/삭제 성능 측정 로그 추가
  - TurnIndicatorHandler: SetIndicator 메서드에 UI 업데이트 성능 측정 로그 추가
  - BattleUIManager: UpdateSkillUIForTurn, DisableAllSkillUI 메서드에 스킬 UI 성능 측정 로그 추가
  - SkillManager: PlaySkillEffect 메서드에 스킬 연출 각 단계별 성능 측정 로그 추가
  - StatusEffectController: ApplyStatusEffectsOnTurnStart 메서드에 상태이상 효과 성능 측정 로그 추가

### 디버그 로그 특징
- **통일된 접두사**: "[턴최적화]" 접두사로 모든 턴 전환 관련 로그 구분
- **성능 측정**: Time.realtimeSinceStartup을 사용하여 각 메서드의 실행 시간을 밀리초 단위로 측정
- **단계별 추적**: 턴 전환의 각 단계별 시작/완료 로그로 정확한 병목 지점 파악 가능
- **상세 정보**: 캐릭터 이름, 스킬 정보, UI 상태 등 상세한 정보 포함

### 기존 로그 정리
- 기존의 방해되는 디버그 로그들을 주석 처리하거나 제거하여 턴 최적화 로그에 집중할 수 있도록 정리
- 오류 관련 로그(LogError, LogWarning)는 유지하여 시스템 안정성 확보

### 다음 단계
- 실제 게임에서 턴 전환 시 콘솔 로그 분석을 통한 정확한 병목 지점 파악
- 성능 측정 결과를 바탕으로 단계별 최적화 작업 진행
- 불필요한 대기 시간, 중복 연산, UI 성능 이슈 등 구체적 개선 방안 수립

### 예상 분석 대상
1. **대기 시간 분석**: EnemyAIController의 1.5초 대기, WaitForDeathEffects의 3초 대기 등
2. **UI 성능 분석**: NextTurnIndicatorUI의 블록 생성/삭제, TurnIndicatorHandler의 지속적 업데이트
3. **연산 성능 분석**: TurnDecider의 속도순 정렬, CheckBattleEnd의 생존자 체크 등
4. **스킬 연출 분석**: PlaySkillEffect의 각 단계별 소요 시간 및 불필요한 대기 시간

---

## 2025-07-30

### 완료된 작업
- 보스 전용 스킬 파일 생성 (BossSkills.xml)
- 스킬 ID 체계 규칙 정리 (DataMemo.md)
- 상태이상 효과 주석 추가
- priority 필드 제거
- **컴포넌트 기반 AI 시스템 구축**
  - EnemyAIController를 추상 클래스로 변경
  - DefaultEnemyAIController 구현 (랜덤 AI)
  - AdeliaEnemyAIController 구현 (턴 기반 패턴)
  - PatternType enum에서 Boss 제거, Adelia 추가
  - BattleManager에서 패턴에 따라 AI 컴포넌트 할당
  - 아델리아 XML 패턴을 Adelia로 변경
- **능동형 아이콘 시스템 구현**
  - StatusEffectData에 GetDynamicIcon 메서드 추가
  - 값에 따른 아이콘 동적 로드 (양수: Up, 음수: Down)
  - StatusEffectInstance에서 능동형 아이콘 적용
  - 통합 상태이상 데이터 생성 (방어력, 공격력, 속도, 체력)
  - DataMemo.md에 시스템 설명 추가

### 미래 계획 (연계 스킬 시스템)
- **공격 연계**: 예약 공격 → 주기 종료 시 동시 실행 (배수 시스템)
- **버프 연계**: 쌍둥이 스킬 → 즉시 동시 실행
- **지휘관-병사 시스템**: 명령-응답 구조
- **파티 구성별 상호작용**: 주인공 중심 vs 다중 상호작용
- **스택 시스템**: 아군 행동 횟수 기반 스택 쌓기

### 능동형 아이콘 시스템 완성
- **아이콘 로직 수정**: 0 이상 값은 Up 아이콘, 음수 값은 Down 아이콘 사용
- **통합 상태이상 완성**: 방어력, 공격력, 속도, 체력 변화를 하나의 상태이상으로 처리
- **컴파일 오류 해결**: StatusEffectData의 GetDynamicIcon 메서드를 virtual로 선언하여 오버라이드 가능하게 수정
- **아이콘 파일 준비 필요**: StatusEffect/GuardPowerUp, StatusEffect/GuardPowerDown 등 실제 아이콘 파일 생성 필요

### 내일 작업 계획 (2025-07-31)
- **템플릿 상태이상 효과 작동 테스트**: 새로 만든 통합 상태이상들이 모두 정상 작동하는지 체크
  - StatusEffectGuardPowerBuffData (021001): 방어력 변화 테스트
  - StatusEffectAttackPowerBuffData (021002): 공격력 변화 테스트
  - StatusEffectSpeedBuffData (021003): 속도 변화 테스트
  - StatusEffectHpChangeData (021004): 체력 변화 테스트
- **능동형 아이콘 시스템 검증**: 값에 따른 아이콘 변경이 올바르게 작동하는지 확인
- **기존 상태이상과의 호환성**: 기존 출혈, 중독, 화상 등과 새로운 통합 상태이상이 함께 작동하는지 테스트

## 2025-07-31 스킬 ID 중복 문제 해결 및 테스트용 ID 규칙 정립

### 스킬 ID 중복 문제 발견 및 해결
- **문제 상황**: PlayerSkills.xml과 EnemyMobSkills.xml에서 ID 010005~010010이 중복되어 충돌 발생
- **중복된 ID들**:
  - ID 010005: PlayerSkills.xml "능동형 아이콘 테스트" vs EnemyMobSkills.xml "기초적인 검격"
  - ID 010006: PlayerSkills.xml "전체 회복" vs EnemyMobSkills.xml "방어"
  - ID 010007: PlayerSkills.xml "전체 공격" vs EnemyMobSkills.xml "참수"
  - ID 010008: PlayerSkills.xml "인접 공격" vs EnemyMobSkills.xml "지목"
  - ID 010009: PlayerSkills.xml "인접 회복" vs EnemyMobSkills.xml "성실히 갈아낸 날"
  - ID 010010: PlayerSkills.xml "랜덤 공격" vs EnemyMobSkills.xml "녹슨 날"

### 해결 방법: 테스트용 ID 규칙 정립
- **테스트용 ID 규칙**: 999xxx 시리즈로 완전히 분리
  - 999001: 능동형 아이콘 테스트
  - 999002: 전체 회복
  - 999003: 전체 공격
  - 999004: 인접 공격
  - 999005: 인접 회복
  - 999006: 랜덤 공격
  - 999007: 약점 공격
  - 999008: 응급 치료

### 변경된 파일들
1. **PlayerSkills.xml**: 모든 테스트용 스킬 ID를 999xxx 시리즈로 변경
2. **scriptmap.md**: 스킬 목록 업데이트, 테스트용 스킬을 별도 섹션으로 분리
3. **EnemyMobSkills.xml**: 기존 ID 유지 (실제 적 몹 스킬)

### 결과
- 테스트용 스킬과 실제 스킬 간 ID 충돌 완전 해결
- 999xxx 시리즈로 테스트용 ID 규칙 정립
- 기존 적 몹 스킬들은 영향 없이 정상 작동
- 스킬 시스템의 안정성과 확장성 확보

### 향후 계획
- 새로운 테스트용 스킬 추가 시 999xxx 시리즈 사용
- 실제 스킬과 테스트용 스킬의 명확한 구분 유지
- 스킬 ID 관리 체계의 일관성 확보

## 2025-07-31 캐릭터/스킬 정보 팝업 시스템 구현

### 구현된 기능
1. **CharacterInfoPopup 개선**
   - CharacterData.Sprite 필드를 우선 사용하여 캐릭터 이미지 로드
   - ID 기반 백업 로직으로 안정성 확보
   - 캐릭터 스프라이트 경로: `{characterData.Sprite}/Stand`

2. **SkillInfoPopup 신규 구현**
   - 스킬 정보 표시 UI 시스템 구축
   - 데미지/힐량 통합 표시 (`damageOrHealText`)
   - 타겟 타입 간소화 (아군, 적군, 전체, 랜덤, 특정)
   - 상태이상 효과 정보 표시 (`effectContainer`, `effectItemPrefab`)
   - 스킬 자체 데미지/힐 + 상태이상 효과 통합 분석

3. **더블클릭 시스템 구현**
   - CharacterBlock, SkillBlock에 IPointerClickHandler 구현
   - OnPointerClick으로 더블클릭 감지 (0.3초 임계값)
   - FindObjectsOfType<T>(true).FirstOrDefault()로 비활성화된 팝업도 찾기
   - Inspector 연결 실패 시 자동 fallback 시스템

4. **테스트용 스킬 해금**
   - GameProgressManager에서 999xxx 시리즈 테스트 스킬들 해금
   - 기본 스킬 + 테스트용 스킬 모두 사용 가능하도록 설정

### 기술적 특징
- **UI 이벤트 처리**: OnMouseDown → IPointerClickHandler로 변경하여 UI 호환성 확보
- **동적 아이콘 로딩**: 스킬/캐릭터 아이콘 자동 로드 및 오류 처리
- **확장 가능한 구조**: 새로운 정보 타입 추가 용이
- **안정성**: null 체크, 예외 처리, fallback 시스템으로 견고한 구조

### 다음 단계
- 스킬 효과 겹침 문제 해결 (줄바꿈 또는 개별 UI 요소로 분리)
- 팝업 UI 스왑 버튼 구현으로 겹친 효과 관리
- 실제 프리팹 제작 및 UI 연결 완성

### ⚠️ 내일 작업 시 필수 확인 파일들
**작업 시작 전 반드시 다음 파일들을 꼼꼼히 읽고 시작할 것:**

1. **Assets/Scrips/BattleSettingUI/CharacterInfoPopup.cs**
   - 캐릭터 이미지 로딩 로직 (CharacterData.Sprite 우선 사용)
   - ID 기반 백업 로직 구조

2. **Assets/Scrips/BattleSettingUI/SkillInfoPopup.cs**
   - GetDamageHealInfo 메서드 (데미지/힐량 통합 분석)
   - UpdateEffectInfo 메서드 (상태이상 효과 표시)
   - GetSimplifiedTargetText 메서드 (타겟 타입 간소화)
   - effectItemPrefab 생성 시 RectTransform 좌표 설정

3. **Assets/Scrips/BattleSettingUI/CharacterBlock.cs**
   - IPointerClickHandler 구현
   - OnPointerClick 더블클릭 감지 로직
   - FindObjectsOfType fallback 시스템

4. **Assets/Scrips/BattleSettingUI/SkillBlock.cs**
   - IPointerClickHandler 구현
   - OnPointerClick 더블클릭 감지 로직
   - FindObjectsOfType fallback 시스템

5. **Assets/Scrips/GameProgressManager.cs**
   - SetupTestInventory 메서드 (999xxx 시리즈 스킬 해금)
   - 테스트용 스킬 목록 확인

**특히 SkillInfoPopup의 GetDamageHealInfo에서 string.Join(", ", effects)로 인한 겹침 문제가 핵심 해결 대상**

## 2025-08-01 오늘의 작업 완료 요약

### 1. 스킬 이펙트 뷰 전환 버튼 시스템 구현 완료
- **문제 해결**: 스킬 정보 팝업에서 여러 효과가 겹쳐 보이는 문제 완전 해결
- **구현 기능**: 이전/다음 버튼으로 효과 간 스왑, 페이지 표시 (예: "1/3")
- **UI 개선**: 개별 효과 분리 표시, 직관적인 인터페이스 제공
- **기술적 특징**: 확장성, 안정성, 사용성, 성능 최적화

### 2. 템플릿 상태이상 효과 ScriptableObject 생성 완료
- **생성된 파일들**:
  - StatusEffectAttackPowerBuffData.asset (021002) - 공격력 변화
  - StatusEffectSpeedBuffData.asset (021003) - 속도 변화  
  - StatusEffectHpChangeData.asset (021004) - 체력 변화
  - 관련 .meta 파일들 생성
- **기존 파일**: StatusEffectGuardPowerBuffData.asset (021001) - 방어력 변화

### 3. 템플릿 상태이상 효과 아이콘 시스템 완성
- **기존 아이콘**: ATKUp/ATKDown, DEFUp/DEFDown, Bleed/Poison/Burn ✅
- **임시 아이콘 할당**: 속도 변화(ATK 아이콘), 체력 변화(DEF 아이콘)
- **능동형 아이콘 시스템**: 값에 따른 아이콘 변경 로직 구현 완료

### 4. 호환성 테스트용 스킬 생성 완료
- **스킬 ID**: 999009 "호환성 테스트"
- **포함 효과**: 기존 출혈(020001) + 새로운 템플릿 상태이상(021001, 021002)
- **테스트 목적**: 기존/새로운 상태이상 동시 작동, 능동형 아이콘 시스템 검증
- **GameProgressManager 연동**: 테스트용 스킬 해금 로직 추가

### 완료된 테스트 준비
1. ✅ 모든 템플릿 상태이상 ScriptableObject 생성
2. ✅ 아이콘 시스템 연결 및 임시 아이콘 할당
3. ✅ 능동형 아이콘 시스템 구현
4. ✅ 호환성 테스트용 스킬 생성
5. ✅ GameProgressManager 연동

### 다음 단계
- Unity에서 실제 게임 실행하여 모든 테스트 진행
- 템플릿 상태이상 효과 작동 확인
- 능동형 아이콘 시스템 검증
- 기존 상태이상과의 호환성 테스트
- 전용 아이콘 파일 생성 (SpeedUp/SpeedDown, HpUp/HpDown)

## 2025-08-01 템플릿 상태이상 효과 ScriptableObject 생성 완료

### 생성된 파일들
1. **StatusEffectAttackPowerBuffData.asset** (021002)
   - 공격력 변화 상태이상 ScriptableObject
   - 양수: 공격력 증가, 음수: 공격력 감소
   - 능동형 아이콘 시스템 지원 (ATKUp/ATKDown)

2. **StatusEffectSpeedBuffData.asset** (021003)
   - 속도 변화 상태이상 ScriptableObject
   - 양수: 속도 증가, 음수: 속도 감소
   - 임시로 DefUp/DefDown 아이콘 사용

3. **StatusEffectHpChangeData.asset** (021004)
   - 체력 변화 상태이상 ScriptableObject
   - 양수: 체력 증가, 음수: 체력 감소
   - 즉시 체력 변화 및 UI 업데이트 지원

4. **관련 .meta 파일들**
   - 각 ScriptableObject에 대한 Unity 메타데이터 파일 생성
   - 고유 GUID 할당으로 Unity 인식 가능

### 기존 파일
- **StatusEffectGuardPowerBuffData.asset** (021001) - 이미 존재
  - 방어력 변화 상태이상 ScriptableObject
  - 양수: 방어력 증가, 음수: 방어력 감소
  - 능동형 아이콘 시스템 지원 (DEFUp/DEFDown)

### 테스트 준비 완료
- 모든 템플릿 상태이상 효과 ScriptableObject 생성 완료
- 테스트용 스킬 999001에 모든 효과 포함 (방어력-1, 공격력+3, 속도-2, 체력+5)
- StatusEffectManager가 자동으로 모든 ScriptableObject 로드
- 실제 게임에서 테스트 가능한 상태

### 다음 단계
- Unity에서 실제 게임 실행하여 템플릿 상태이상 효과 작동 테스트
- 능동형 아이콘 시스템 검증 (값에 따른 아이콘 변경)
- 기존 상태이상과의 호환성 테스트
- 아이콘 파일 준비 (SpeedUp/SpeedDown, HpUp/HpDown)

## 2025-08-01 템플릿 상태이상 효과 아이콘 시스템 완성

### 아이콘 파일 현황
1. **기존 아이콘들**
   - ATKUp.png, ATKDown.png (공격력 변화용) ✅
   - DEFUp.png, DEFDown.png (방어력 변화용) ✅
   - Bleed.png, Poison.png, Burn.png (지속 피해용) ✅

2. **임시 아이콘 할당**
   - **속도 변화 (021003)**: ATKUp/ATKDown 아이콘 임시 사용
   - **체력 변화 (021004)**: DEFUp/DEFDown 아이콘 임시 사용
   - 실제 속도/체력 전용 아이콘 생성 필요

### 아이콘 경로 수정 완료
1. **StatusEffectSpeedBuffData.cs**
   - 기본 아이콘: "StatusEffect/ATKUp"
   - 양수 값: "StatusEffect/ATKUp"
   - 음수 값: "StatusEffect/ATKDown"

2. **StatusEffectHpChangeData.cs**
   - 기본 아이콘: "StatusEffect/DEFUp"
   - 양수 값: "StatusEffect/DEFUp"
   - 음수 값: "StatusEffect/DEFDown"

### 능동형 아이콘 시스템 준비 완료
- 모든 템플릿 상태이상 효과가 능동형 아이콘 시스템 지원
- 값에 따른 아이콘 변경 로직 구현 완료
- 임시 아이콘으로 테스트 가능한 상태

### 테스트 준비 완료
- 모든 ScriptableObject 파일 생성 완료
- 아이콘 시스템 연결 완료
- 테스트용 스킬 999001 준비 완료
- Unity에서 실제 테스트 가능한 상태

### 다음 단계
- Unity에서 실제 게임 실행하여 템플릿 상태이상 효과 작동 테스트
- 능동형 아이콘 시스템 검증 (값에 따른 아이콘 변경)
- 기존 상태이상과의 호환성 테스트
- 전용 아이콘 파일 생성 (SpeedUp/SpeedDown, HpUp/HpDown)

## 2025-08-01 호환성 테스트용 스킬 생성 완료

### 생성된 테스트 스킬
- **스킬 ID**: 999009
- **스킬명**: 호환성 테스트
- **효과**: 기존 상태이상 + 새로운 템플릿 상태이상 혼합 테스트

### 포함된 상태이상 효과
1. **기존 상태이상 (020001)**: 출혈
   - Value: 3, Duration: 2턴
   - 지속 피해형 상태이상

2. **새로운 템플릿 상태이상 (021001)**: 방어력 변화
   - Value: -2 (감소), Duration: 3턴
   - 능동형 아이콘 시스템 테스트 (DEFDown 아이콘)

3. **새로운 템플릿 상태이상 (021002)**: 공격력 변화
   - Value: +4 (증가), Duration: 3턴
   - 능동형 아이콘 시스템 테스트 (ATKUp 아이콘)

### 테스트 목적
- 기존 출혈 상태이상과 새로운 템플릿 상태이상들이 동시에 작동하는지 확인
- 능동형 아이콘 시스템이 올바르게 작동하는지 검증
- 상태이상 시스템의 확장성과 호환성 검증

### GameProgressManager 연동
- SetupTestInventory() 메서드에 999009 스킬 해금 로직 추가
- 테스트용 스킬 목록에 호환성 테스트 스킬 포함
- 실제 게임에서 즉시 테스트 가능한 상태

### 완료된 테스트 준비
1. ✅ 모든 템플릿 상태이상 ScriptableObject 생성
2. ✅ 아이콘 시스템 연결 및 임시 아이콘 할당
3. ✅ 능동형 아이콘 시스템 구현
4. ✅ 호환성 테스트용 스킬 생성
5. ✅ GameProgressManager 연동

### 다음 단계
- Unity에서 실제 게임 실행하여 모든 테스트 진행
- 템플릿 상태이상 효과 작동 확인
- 능동형 아이콘 시스템 검증
- 기존 상태이상과의 호환성 테스트
- 전용 아이콘 파일 생성 (SpeedUp/SpeedDown, HpUp/HpDown)

## 2025-08-01 스킬 정보 팝업 UI 개선 및 동적 아이콘 시스템 구현 완료

### 스킬 정보 팝업 UI 중첩 효과 문제 해결
- **문제 상황**: 스킬 정보 팝업에서 여러 상태이상 효과가 겹쳐 보이는 문제 발생
- **해결 방법**: 이펙트 뷰 전환 버튼 시스템 구현
  - 이전/다음 버튼으로 효과 간 스왑 기능
  - 페이지 표시 (예: "1/3") 및 배경 제어
  - 단일 효과일 때 불필요한 UI 숨김 처리

### 동적 아이콘 시스템 구현
- **능동형 아이콘 시스템**: 수치에 따른 아이콘 자동 변경
  - 양수 값 (0 포함): Up 아이콘 사용
  - 음수 값: Down 아이콘 사용
  - 아이콘 경로: UI/StatusEffect/ 폴더 기반

### 템플릿 상태이상 효과 ScriptableObject 생성
- **StatusEffectAttackPowerBuffData.asset** (021002): 공격력 변화
- **StatusEffectSpeedBuffData.asset** (021003): 속도 변화
- **StatusEffectHpChangeData.asset** (021004): 체력 변화
- **기존 StatusEffectGuardPowerBuffData.asset** (021001): 방어력 변화

### UI 텍스트 개선
- **동적 이름 생성**: "공격도 상승", "방어도 하락" 등 게임 공식 용어 적용
- **수치 표시 개선**: "상승 +3", "하락 -5" 등 직관적 표시
- **지속시간 표시**: "지속 3턴" 등 명확한 표현

### 호환성 테스트 시스템 구축
- **테스트용 스킬 999009**: 기존 출혈 + 새로운 템플릿 상태이상 혼합
- **GameProgressManager 연동**: 테스트용 스킬 자동 해금
- **아이콘 시스템 검증**: 기존/새로운 상태이상 동시 작동 확인

### 기술적 개선사항
- **확장성**: 새로운 상태이상 효과 쉽게 추가 가능
- **안정성**: null 체크, 예외 처리, fallback 시스템
- **성능**: 효율적인 UI 관리 및 메모리 사용
- **사용성**: 직관적인 인터페이스 및 명확한 정보 표시

### 완료된 기능
1. ✅ 스킬 정보 팝업 UI 중첩 효과 문제 완전 해결
2. ✅ 동적 아이콘 시스템 구현 완료
3. ✅ 템플릿 상태이상 효과 ScriptableObject 생성
4. ✅ UI 텍스트 개선 및 게임 공식 용어 적용
5. ✅ 호환성 테스트 시스템 구축
6. ✅ GameProgressManager 연동 완료

### 다음 단계
- Unity에서 실제 게임 실행하여 모든 테스트 진행
- 템플릿 상태이상 효과 작동 확인
- 능동형 아이콘 시스템 검증
- 기존 상태이상과의 호환성 테스트
- 전용 아이콘 파일 생성 (SpeedUp/SpeedDown, HpUp/HpDown)

## 2025-08-02 템플릿 상태이상 효과 시스템 완성 및 테스트 준비

### 아이콘 시스템 개선
- **속도 변화 (021003)**: ATKUp/ATKDown 아이콘 사용 (임시)
- **체력 변화 (021004)**: ATKUp/ATKDown 아이콘 사용 (임시, 회복 효과임을 나타냄)
- **능동형 아이콘 시스템**: 값에 따른 아이콘 변경 로직 완성
- **대체 아이콘 시스템**: 아이콘 로드 실패 시 fallback 처리

### 테스트 준비 완료
- **스킬 999001 "능동형 아이콘 테스트"**: 4개의 템플릿 상태이상 효과 포함
  - 방어력-1, 공격력+3, 속도-2, 체력+5
- **스킬 999009 "호환성 테스트"**: 기존 + 새로운 상태이상 혼합
  - 출혈(기존) + 방어력-2, 공격력+4 (새로운 템플릿)
- **GameProgressManager**: 모든 테스트용 스킬 해금 완료

### 스킬 정보 팝업 UI 시스템 완성
- **이펙트 뷰 전환 버튼**: 여러 효과 간 스왑 기능
- **페이지 표시**: "1/3" 형태의 페이지 표시
- **동적 효과 이름**: "공격도 상승", "방어도 하락" 등 게임 공식 용어
- **능동형 아이콘**: 수치에 따른 아이콘 자동 변경

### 다음 단계
- Unity에서 실제 게임 실행하여 테스트 진행
- 템플릿 상태이상 효과 작동 확인
- 능동형 아이콘 시스템 검증
- 기존 상태이상과의 호환성 테스트
- 전용 아이콘 파일 생성 (SpeedUp/SpeedDown, HpUp/HpDown)

## 2025-08-02 힐 스킬 시스템 문제 해결

### 발견된 문제점
- **힐 스킬 멈춤 현상**: 오드가 병사 캐릭터에게 힐을 주는 상황에서 게임이 멈춤
- **힐량 파싱 문제**: XML에서 HealMin/HealMax를 사용하지만 SkillLoader에서 healAmount를 파싱하여 불일치 발생
- **힐 스킬 연출 문제**: 힐 스킬에서도 타겟이 피격 모션을 하여 부자연스러운 연출

### 해결한 문제점
1. **SkillData Clone 메서드 수정**
   - healAmount, Type, cooldown, currentCooldown 필드 복사 추가
   - 스킬 데이터 복제 시 모든 필드가 올바르게 복사되도록 수정

2. **SkillLoader 힐량 파싱 수정**
   - CalculateHealAmount 메서드 추가: HealMin과 HealMax의 평균값 계산
   - XML 파싱에서 HealMin/HealMax를 올바르게 읽어서 healAmount로 변환
   - 기존 healAmount 필드와의 호환성 유지

3. **힐 스킬 연출 개선**
   - 힐 스킬에서 타겟이 피격 모션을 하지 않도록 수정
   - 힐 스킬 전용 연출 로직 적용

4. **디버그 로그 추가**
   - 힐 스킬 사용 시 상세한 로그 출력
   - CharacterStats.Heal 메서드에 힐량 변화 로그 추가
   - UI 업데이트 로직 추가

### 수정된 파일들
- **SkillData.cs**: Clone 메서드에 누락된 필드 추가
- **SkillLoader.cs**: 힐량 파싱 로직 개선
- **SkillManager.cs**: 힐 스킬 연출 개선 및 디버그 로그 추가
- **CharacterStats.cs**: Heal 메서드에 디버그 로그 및 UI 업데이트 추가

### 예상 결과
- 힐 스킬이 정상적으로 작동하여 게임이 멈추지 않음
- 힐량이 올바르게 계산되어 적용됨
- 힐 스킬 사용 시 자연스러운 연출
- 상세한 디버그 로그로 힐 스킬 동작 추적 가능

## 2025-08-02 힐 스킬 랜덤 힐량 시스템 구현

### 구현된 기능
1. **SkillData에 HealMin/HealMax 필드 추가**
   - 기존 healAmount 필드와 함께 범위 힐량 지원
   - Clone 메서드에서 HealMin/HealMax 복사 추가

2. **SkillLoader 힐량 파싱 개선**
   - GetHealRange 메서드로 HealMin/HealMax 파싱
   - 기존 healAmount 필드와의 호환성 유지
   - 스킬 상속 시 HealMin/HealMax 오버라이드 지원

3. **SkillManager 랜덤 힐량 계산**
   - 단일 타겟 힐 스킬: UnityEngine.Random.Range(HealMin, HealMax + 1)
   - 범위 힐 스킬: 각 타겟마다 개별 랜덤 힐량 계산
   - 상세한 디버그 로그로 힐량 범위와 실제 힐량 표시

4. **SkillInfoPopup 힐량 표시 개선**
   - 범위 힐량 표시 (예: "힐량: 10-15")
   - 단일 힐량 표시 (예: "힐량: 12")

### 수정된 파일들
- **SkillData.cs**: HealMin/HealMax 필드 추가 및 Clone 메서드 수정
- **SkillLoader.cs**: 범위 힐량 파싱 로직 구현
- **SkillManager.cs**: 랜덤 힐량 계산 로직 구현
- **SkillInfoPopup.cs**: 범위 힐량 표시 UI 개선

### 예상 결과
- 힐 스킬 사용 시 HealMin과 HealMax 사이의 랜덤 힐량 적용
- 스킬 정보 팝업에서 힐량 범위 표시
- 범위 힐 스킬에서 각 타겟마다 다른 힐량 적용
- 기존 힐 스킬과의 완전한 호환성 유지

## 2025-08-02 전체 공격 스킬 연출 개선

### 기획 의도
- **단일 공격**: 한 명 한 명의 전투를 집중 = 줌인
- **전체 공격**: 줌인하지 않고 제자리에서 효과 연출만

### 구현된 기능
1. **범위 스킬에서 힐 스킬만 특별 처리**
   - 힐 스킬: 줌인 + 캐릭터 이동 + 위치 복귀
   - 전체 공격: 제자리에서 효과만

2. **카메라 줌인/줌아웃 조건부 적용**
   - 힐 스킬만 카메라 줌인/줌아웃 실행
   - 전체 공격은 줌인/줌아웃 생략

3. **캐릭터 이동 조건부 적용**
   - 힐 스킬만 캐릭터 위치 조정 및 복귀
   - 전체 공격은 제자리에서 효과만

4. **위치 복귀 조건부 적용**
   - 힐 스킬만 위치 복귀 로직 실행
   - 전체 공격은 위치 복귀 생략

### 수정된 파일들
- **SkillManager.cs**: 범위 스킬에서 힐 스킬과 전체 공격 구분 처리

### 예상 결과
- **힐 스킬**: 줌인 + 캐릭터 이동 + 효과 + 위치 복귀
- **전체 공격**: 제자리에서 효과만 (줌인/이동/복귀 없음)
- **단일 공격**: 기존과 동일 (줌인 + 집중 연출)

## 2025-08-02 힐 스킬 초록색 표시 시스템 구현

### 구현된 기능
1. **힐 이벤트 시스템 추가**
   - `OnHealEvent` 액션 추가 (CharacterStats)
   - 힐 시 초록색 표시를 위한 이벤트 발생

2. **BattleEffectManager 힐 이벤트 처리**
   - `OnCharacterHeal` 이벤트 핸들러 추가
   - 기존 `PlayHealEffect` 메서드와 연동
   - 힐 이벤트 구독/해제 로직 추가

3. **전체 공격 제자리 연출**
   - 모든 캐릭터가 제자리에서 작동
   - 움직임 없이 효과만 연출

### 수정된 파일들
- **CharacterStats.cs**: 힐 이벤트 시스템 추가
- **BattleEffectManager.cs**: 힐 이벤트 처리 로직 추가

### 예상 결과
- **힐 스킬**: 초록색으로 힐량 표시 (피해의 빨간색과 반대)
- **전체 힐**: 모든 캐릭터가 제자리에서 효과만 연출 (줌인/이동/복귀 없음)
- **일반 범위 스킬**: 줌인 + 캐릭터 이동 + 효과 + 위치 복귀

## 2025-08-02 힐 스킬 초록색 표시 문제 해결

### 발견된 문제
- 힐 스킬에서 초록색 표시가 안 나오는 문제
- `CreateDamagePopup`에서 `isCritical = false`일 때 `normalDamageColor` 사용
- 힐 스킬은 `healColor`를 사용해야 함

### 해결 방법
1. **힐 전용 팝업 생성 메서드 추가**
   - `CreateHealPopup` 메서드 구현
   - 초록색(`healColor`)으로 텍스트 색상 설정
   - 힐량 앞에 "+" 기호 추가

2. **PlayHealEffect 메서드 수정**
   - `CreateDamagePopup` 대신 `CreateHealPopup` 사용
   - 힐 전용 초록색 팝업 생성

### 수정된 파일들
- **BattleEffectManager.cs**: 힐 전용 팝업 생성 메서드 추가

### 예상 결과
- **힐 스킬**: 초록색 "+힐량" 표시
- **피해 스킬**: 빨간색/흰색 데미지 표시
- **전체 힐**: 제자리에서 초록색 힐 팝업이 애니메이션 후 사라짐

## 2025-08-02 힐 팝업 애니메이션 및 자동 삭제 구현

### 발견된 문제
- 힐 팝업이 생성되지만 사라지지 않는 문제
- 데미지 팝업처럼 애니메이션 후 자동 삭제가 안 됨

### 해결 방법
1. **힐 팝업 애니메이션 메서드 추가**
   - `AnimateHealPopup` 코루틴 구현
   - 포물선 운동으로 올라갔다가 내려오는 애니메이션
   - 페이드 아웃 효과 후 자동 삭제

2. **PlayHealEffect 메서드 수정**
   - 힐 팝업 생성 후 애니메이션 코루틴 시작
   - 데미지 팝업과 동일한 애니메이션 패턴 적용

### 수정된 파일들
- **BattleEffectManager.cs**: 힐 팝업 애니메이션 및 자동 삭제 로직 추가

### 예상 결과
- **힐 팝업**: 포물선 운동 애니메이션 후 페이드 아웃으로 자동 삭제
- **데미지 팝업**: 기존과 동일한 애니메이션 패턴
- **전체 힐**: 제자리에서 초록색 힐 팝업이 애니메이션 후 사라짐

## 2025-08-02 팝업 시스템 통합 및 최적화

### 개선 사항
- **중복 코드 제거**: `CreateHealPopup`과 `AnimateHealPopup` 메서드 삭제
- **통합 팝업 시스템**: `CreateDamagePopup`을 `CreatePopup`으로 변경하여 힐/데미지 통합 처리
- **코드 간소화**: 기존 `AnimateHitAndDamage`를 힐 팝업에도 재사용

### 수정된 파일들
- **BattleEffectManager.cs**: 팝업 시스템 통합 및 최적화

### 최종 결과
- **힐 팝업**: 초록색 "+힐량" → 기존 애니메이션 → 자동 삭제
- **데미지 팝업**: 빨간색/흰색 데미지 → 기존 애니메이션 → 자동 삭제
- **코드 효율성**: 중복 제거로 유지보수성 향상

## 2025-08-02 스킬 정보 팝업 데미지 표시 활성화

### 발견된 문제
- 스킬 정보 팝업에서 데미지/힐 정보가 표시되지 않는 문제
- `UpdateDamageHealDisplay`에서 `damageOrHealText`를 항상 비활성화하고 있음

### 해결 방법
1. **UpdateDamageHealDisplay 메서드 수정**
   - `GetDamageHealInfo()` 결과를 표시하도록 변경
   - 데미지/힐 정보가 있을 때만 활성화
   - 정보가 없을 때는 비활성화

### 수정된 파일들
- **SkillInfoPopup.cs**: 데미지/힐 정보 표시 활성화

### 예상 결과
- **기초적인 검격**: "데미지: 10-15" 표시
- **힐 스킬**: "힐량: 10-15" 표시
- **상태이상 스킬**: 상태이상 효과 정보 표시

## 2025-08-07
### 전체 스킬 연출 분기 시스템 구현
- 파일 위치: Assets/Scrips/SkillSystem/SkillManager.cs
- 작업 내용: 전체 스킬과 단일 스킬의 연출을 분기하여 차별화된 연출 시스템 구현
- 변경 사항:
  - IsAllTargetSkill() 메서드 추가: 전체 타겟 스킬(AllEnemies, AllAllies) 판별
  - UseAllTargetSkill() 메서드 추가: 전체 타겟 스킬 전용 처리
  - PlayAllTargetSkillEffect() 메서드 추가: 전체 스킬 연출 분기 처리
  - PlayAllHealEffect() 메서드 추가: 전체 회복 스킬 연출 (제자리에서)
  - PlayAllAttackEffect() 메서드 추가: 전체 공격 스킬 연출 (앞으로 나가서 타격)
  - UseSkill() 메서드 수정: 전체 스킬 → 범위 스킬 → 단일 스킬 순서로 분기
- 연출 차별화:
  - 전체 회복: 제자리에서만 연출 (카메라 줌인/이동 없음)
  - 전체 공격: 공격자가 앞으로 이동하여 타격 연출 (화면은 가만히)
  - 단일 스킬: 기존 방식 유지 (줌인 + 집중 연출)
- 문제 해결:
  - 전체 공격에서 공격자가 피격모션을 하지 않도록 수정 (target != caster 조건 추가)
  - 피격된 타겟들의 모션 원위치 처리 (ResetMotion, ResetPosition)
  - 배틀UI 관리 개선 (ChangeUINormal() 호출로 배틀UI 끄기)
  - TurnManager null 오류 해결 (불필요한 재시도 로직 제거)
- 참고 사항:
  - 전체 스킬과 범위 스킬의 명확한 구분
  - 연출의 일관성과 차별화 동시 달성
  - 기존 시스템과의 호환성 유지

## 2025-08-12 버프/디버프 팝업 시스템 구축 및 팝업 정리 시스템 구현

### 버프/디버프 팝업 시스템 구축
- 파일 위치: 
  - Assets/Scrips/UI/BuffDebuffPopup.cs (신규 생성)
  - Assets/Scrips/Prefab/BuffandDebuffPopup.prefab (신규 생성)
  - Assets/Scrips/Battle/BattleEffectManager.cs
  - Assets/Scrips/SkillSystem/SkillManager.cs
  - Assets/Scrips/CharacterScrips/CharacterMotionController.cs
- 작업 내용: 버프/디버프 전용 팝업 시스템 구현
- 변경 사항:
  - **BuffDebuffPopup.cs 신규 생성**: 아이콘 + 텍스트 조합의 상세 팝업
  - **BuffandDebuffPopup.prefab 신규 생성**: 버프/디버프 전용 UI 프리팹
  - **BattleEffectManager.CreateNewBuffDebuffPopup() 메서드 추가**: 팝업 생성 및 위치 조정
  - **SkillManager 버프 처리 로직 개선**: StatusEffectData 기반 실제 효과명 표시
  - **CharacterMotionController.PlayBuffMotion() 추가**: 버프 전용 모션 (파란색)
  - **StatusEffectData 아이콘 경로 수정**: UI/ 접두사 자동 추가로 아이콘 로딩 해결
- 팝업 시스템 특징:
  - **아이콘 + 텍스트 조합**: 실제 상태이상 이름과 아이콘 표시
  - **동적 아이콘 로딩**: Resources.Load로 UI/StatusEffect 폴더에서 동적 로드
  - **수직 스택링**: 같은 캐릭터에 여러 팝업 시 -100픽셀 간격으로 배치
  - **색상 구분**: 텍스트는 파란색/빨간색, 아이콘은 원본 색상 유지
  - **애니메이션**: 빠른 페이드인(0.1초) + 부드러운 페이드아웃(0.8초)

### 팝업 정리 시스템 구현 및 정리
- 파일 위치: 
  - Assets/Scrips/CharacterScrips/CharacterMotionController.cs
- 작업 내용: 캐릭터 복귀 시 팝업 정리 시스템 구현 후 정리
- 변경 사항:
  - **ClearAllPopupsExceptExceptions() 메서드 추가**: 예외 오브젝트 제외한 모든 팝업 정리
  - **IsExceptionObject() 메서드 추가**: 예외 오브젝트 판별 (BuffandDebuffPopup, DamageCount)
  - **IsPopupObject() 메서드 추가**: 팝업 오브젝트 판별 (컴포넌트 + 이름 기반)
  - **ResetPosition() 메서드 수정**: 캐릭터 복귀 시 팝업 정리 호출
- 정리된 코드들:
  - BattleEffectManager.ClearAllPopups() 제거
  - BattleCamera 팝업 정리 로직 제거
  - BattleManager.EndBattle() 팝업 정리 제거
  - TurnManager.EndBattle() 팝업 정리 제거
  - BattleUIManager.ChangeUINormal() 팝업 정리 제거
- 팝업 정리 시스템 특징:
  - **직접적 접근**: BattleUI 하위 오브젝트 직접 검사
  - **예외 처리**: 지정된 두 오브젝트는 유지
  - **리플렉션 사용**: 타입 참조 문제 해결
  - **캐릭터 복귀 시점**: 가장 확실한 타이밍에 정리

### 해결된 문제들
- **버프 모션 문제**: 피격 모션 대신 파란색 버프 모션 구현
- **팝업 정보 부족**: 수치만 표시하던 것을 아이콘+이름으로 개선
- **아이콘 로딩 실패**: UI/ 접두사 자동 추가로 해결
- **팝업 겹침**: 수직 스택링으로 해결
- **화면 지저분함**: 캐릭터 복귀 시 팝업 정리로 해결

### 참고 사항
- 버프/디버프 팝업은 실제 StatusEffectData 기반으로 정확한 정보 표시
- 아이콘은 UI/StatusEffect 폴더의 기존 리소스 활용
- 팝업 정리는 캐릭터 복귀 시점에 직접적이고 확실하게 처리
- 예외 오브젝트 설정으로 필요한 팝업은 유지 가능

## 2025-10-14 선택/턴 대상 UI 월드 공간 전환

### 작업 내용
- 선택 대상 UI (Selcetor) 월드 공간 전환
- 턴 대상 UI (TurnMarker) 월드 공간 전환
- 3D 공간에서 UI 표시 최적화

### 변경 사항
- **TargetSelector.cs**: 위치 계산 로직을 월드 공간용으로 수정
  - `WorldToScreenPoint()` 제거
  - 직접 월드 좌표 설정: `CurrentTarget.transform.position + new Vector3(0, 2f, 0)`
  - y 오프셋 조정: 200f → 2f
- **TurnIndicatorHandler.cs**: 위치 계산 로직을 월드 공간용으로 수정
  - `WorldToScreenPoint()` 제거
  - 직접 월드 좌표 설정: `currentTarget.position + new Vector3(0, 2f, 0)`
  - y 오프셋 조정: 200f → 2f
  - 디버그 로그 변수명 수정: `screenPos` → `worldPos`

### 해결된 문제들
- **UI 안정성**: 월드 공간에서 UI가 안정적으로 표시됨
- **타겟 데이터 전달**: 스킬 시스템과의 연동 정상 작동
- **전투 진행**: 전체적인 전투 시스템 안정성 확보
- **성능 개선**: 불필요한 스크린 변환 제거로 성능 향상

### 참고 사항
- Canvas Render Mode는 이미 World Space로 설정되어 있었음
- 위치 계산 로직만 월드 공간에 맞게 수정
- 인스펙터 연결은 기존 설정 그대로 유지

## 2025-10-21 상태이상 시스템 정비 및 턴 관리 개선

### 작업 내용
- 상태이상 시스템 구조 분석 및 문제점 파악
- 슬롯 기반 상태이상 정산 메서드 추가 및 TurnManager 연동
- 턴 진행 안전장치 구현 및 무한 루프 방지 시스템 구축

### 변경 사항
- **SlotHandler.cs**: 상태이상 정산 메서드 추가
  - `SettleStatusEffectsAtTurnStart()` 메서드 구현
  - StatusEffectController에 상태이상 처리 위임
- **TurnManager.cs**: 턴 진행 안전장치 구현
  - `AdvanceTurn()` 메서드 추가로 턴 진행 중앙 관리
  - 무한 루프 방지 안전장치 (2초 내 최대 100회 호출 제한)
  - 상태이상 정산 후 사망 체크 및 턴 진행 로직 개선
- **SkillManager.cs**: 슬롯 기반 인접 타겟팅 시스템 구현
  - 거리 기반 → 슬롯 기반(+1, -1) 타겟팅으로 전환
  - 아군/적군 구분 인접 타겟팅 로직 구현
  - 광역형 인접 스킬 지원 (단일형: 랜덤 선택, 광역형: 모두 선택)

### 해결된 문제들
- **상태이상으로 인한 게임 멈춤**: 상태이상 정산 후 사망 시 턴 진행 중단 문제 해결
- **인접 타겟팅 부정확**: 거리 기반 계산의 부정확성 → 슬롯 기반 정확한 타겟팅
- **무한 루프 방지**: 턴 진행 호출 과다 시 안전장치로 시스템 보호
- **상태이상 관리 중앙화**: 슬롯 기반 상태이상 정산으로 관리 체계 개선

### 참고 사항
- 상태이상 시스템은 ScriptableObject 기반으로 잘 정립되어 있음
- 턴 관리 흐름: 캐릭터 지목 → 상태이상 정산 → 유닛 턴 시작
- 인접 타겟팅은 아군은 아군끼리, 적은 적끼리만 계산
- 안전장치는 턴 수 제한이 아닌 호출 횟수 제한으로 무한 루프 방지

## 2024-12-21 작업 로그

### 0.1버전 상태이상 정산 이펙트 구현 완료
- SlotHandler.cs에 상태이상 연출 시스템 추가
  - 연출용 스프라이트 오브젝트 생성 및 관리
  - 상태이상 아이콘 + 피해 수치 표시
  - 순차적 연출 시스템 (타다다다다다~)
  - 스프라이트 초기화 및 정리 시스템
- TurnManager.cs 수정
  - StartTurn 메서드를 코루틴으로 변경
  - 상태이상 정산 + 연출 통합 처리
  - 연출 완료 후 다음 턴 진행
- 연출 시퀀스 흐름
  1. 상태이상 정산 (데이터 처리)
  2. 연출용 스프라이트 초기화 (null로 설정)
  3. 상태이상별 순차 연출 (0.2초 간격)
  4. 각 연출: 아이콘 설정 → 애니메이션 → 정리
  5. 최종 정리 및 다음 턴 진행

### StartTurn 구조 최적화
- StartTurn을 일반 메서드로 유지
- 정산 부분만 코루틴으로 처리 (ProcessStatusEffectsWithAnimation)
- 연출 완료 후 UI 업데이트 및 다음 턴 진행
- 더 깔끔하고 효율적인 구조로 개선

### StatusEffectSlot으로 상태이상 정산 이펙트 이전
- 기존 StatusEffectSlot.cs 클래스에 연출 기능 추가
- 상태이상 정산 + 연출 시스템을 StatusEffectSlot으로 이동
- SlotHandler는 StatusEffectSlot 호출만 담당
- 프리팹 구조와 코드 구조의 일치성 개선
- 상태이상 관련 모든 기능이 StatusEffectSlot에 집중
- 기존 상태이상 UI 관리 기능과 연출 기능이 통합

### 상태이상 피해량 팝업 추가
- StatusEffectInstance.cs에 직접 DamagePopup 프리팹 사용
- BattleEffectManager를 거치지 않고 직접 프리팹 로드 및 생성
- 상태이상 피해 시 피해량 팝업 표시 (흰색 텍스트)
- 간단하고 효율적인 구조로 시각적 피드백 개선

## 2025-10-29 VirtualMouse 스킬 호버 UI 시스템 개선

### 구현/변경 사항
- VirtualMouse 호버 감지 시스템 개선
  - 태그 기반(`CompareTag`) → 컴포넌트 기반(`GetComponentInParent`)으로 전환
  - `EventSystem.current.RaycastAll` 사용으로 다중 캔버스 환경 지원
  - `SkillInstance`, `SkillSlot`, `SkillBlock` 컴포넌트 감지로 안정성 향상
- 스킬 정보 패널 활성화 문제 해결
  - 비활성 오브젝트가 스스로 활성화하지 못하는 문제 해결
  - `VirtualMouseUIPanel.SetVisible()`에서 `EnsureActiveHierarchy()` 호출로 부모 체인 활성화
  - `VirtualMouse`에서 패널 활성화를 중앙 통제하도록 구조 개선
- 패널 위치 계산 단순화
  - 복잡한 화면 경계 계산 제거 → 고정 오프셋 방식으로 전환
  - `VirtualMouseUIPanel`: `fixedOffsetX`(기본 300), `fixedOffsetY`로 좌우 배치
  - `VirtualMouseSkillPanel`: `statusAnchorOffsetX`(700), `statusAnchorOffsetY`로 상태이상 설명 앵커 배치
  - 좌우 판단(`ShouldShowPanelOnLeft`)에 따라 ±부호만 적용하는 간단한 구조
- 상태이상 설명 패널 활성화 조건 개선
  - `StatusEffectDescriptionPanel`은 효과 유무 판단만 담당
  - `VirtualMouseSkillPanel.SetSkillData()`에서 `statusEffectAnchor.SetActive()` 직접 제어
  - 효과 있을 때만 앵커 활성화 + 설명 업데이트, 없을 때는 비활성화
- 초기화 및 정리 루틴 강화
  - `SkillInfoPopup.ResetUI()`: 패널 초기화 시 모든 UI 요소(텍스트, 아이콘, 버튼) 리셋
  - `VirtualMouseSkillPanel.InitializeSkillPanel()`: 임시 생성물 제거 후 비활성화
  - `PanelSkillInfo`, `StEfDecAnchor` 모두 초기 상태에서 비활성화
- Canvas 및 GraphicRaycaster 자동 생성
  - `VirtualMouse`에 부모 Canvas가 없을 경우 자동으로 Screen Space Overlay Canvas 생성
  - GraphicRaycaster도 자동 추가하여 UI 레이캐스팅 보장
- Cursor 텍스처 null 처리
  - `SetCursor()`에서 텍스처 null 체크 추가
  - 텍스처 없을 시 시스템 기본 커서 사용 및 경고 로그만 출력

### 결과/의도
- 호버 감지 안정성 향상: 태그 의존성 제거로 다양한 UI 구조에서 동작 보장
- 패널 활성화 문제 해결: 비활성 오브젝트의 자체 활성화 문제를 중앙 통제로 해결
- 위치 계산 단순화: 복잡한 경계 계산 제거로 유지보수성 향상 및 버그 감소
- 상태이상 설명 표시 개선: 스킬 패널에서 직접 제어하여 조건 기반 표시 보장
- 초기화 안정성: 모든 패널과 하위 요소가 초기 상태에서 비활성화되어 깨끗한 시작 보장

### 다음 작업 예정
- 상태이상 UI 호버 구현: 전투 중 상태이상 아이콘에 마우스 호버 시 상세 정보 표시

## 2025-11-05 상태이상 시스템 상속 구조 구축 및 아이콘 시스템 개선

### 구현/변경 사항
- StatusEffectInstanceBase 베이스 클래스 생성: 공통 필드(effectData, remainingTurns, value, owner, triggerCount)와 InitializeBasic 메서드 제공
  - StatusEffectInstanceBuff, StatusEffectInstanceReaction이 Base 상속하도록 변경
  - 공통 필드를 public으로 유지하여 기존 코드 호환성 확보
- 상태이상 아이콘 시스템 개선
  - StatusEffectData.GetDynamicIcon: Buff/Debuff 타입만 Up/Down 접미사 사용, 지속피해/토큰은 기본 아이콘 사용
  - StatusEffectGuardPowerBuffData.GetDynamicIcon: 기본 클래스 로직 재사용하도록 수정 (iconPath 기반 접미사 추가)
  - 0값 상태이상은 기본 아이콘 사용 (경고 억제)
- VirtualMouse DontDestroyOnLoad 이슈 해결
  - VirtualMouse에서 DontDestroyOnLoad 제거
  - VirtualMouseCanvas에서 루트 오브젝트 기준으로 DontDestroyOnLoad 처리 (싱글톤 가드 포함)
- StatusEffectInstance DamageCount 프리팹 로드 방식 개선
  - 인스펙터 참조 방식 추가 (damageCountPrefab 필드)
  - Resources 로드는 폴백으로 유지
- CharacterMotionController Buff 모션 처리 개선
  - PlaySkillMotion에서 "Buff" 타입일 때 PlayBuffMotion() 호출하도록 분기
  - Buff 전용 스프라이트 없으면 Stand로 폴백
- TurnIndicatorHandler 카메라 경고 제거
  - 메인 카메라를 기본값으로 명시적 설정, 경고 메시지 제거
- SkillStarter 스크립트 복원: 드래그 앤 드롭으로 슬롯 배치 및 스킬 활성화 제어

### 진행 중인 작업
- 버프 상태이상 생성 위치 추적 및 분석 완료
  - 생성 위치: CreatedUnit/StatusEffectSlot (로컬 pos: 0, -3.8, 0)
  - StatusEffectBuff 프리팹의 SpriteRenderer 초기 sprite가 NULL인 상태
  - StatusEffectInstanceBuff의 iconRenderer 필드가 인스펙터에서 미할당 가능성 확인
  - 다음 작업: iconRenderer 자동 할당 및 아이콘 초기화 보장

### 결과/의도
- 상태이상 인스턴스 클래스들의 공통 로직을 베이스 클래스로 통합하여 코드 중복 제거
- 아이콘 시스템의 일관성 확보 (타입별 적절한 아이콘 표시 규칙 정립)
- DontDestroyOnLoad 경고 해결 및 올바른 루트 오브젝트 관리
- 버프 상태이상이 생성되지만 화면에 표시되지 않는 문제의 원인 파악 진행 중

## 2024-11-10 주인공 보장 시스템 및 게임 컨셉트 정립

### 구현/변경 사항
- 스킬 인벤토리 ScrollRect 스크롤 문제 해결
  - Content Size Fitter의 Vertical Fit을 Preferred Size로 자동 변경
  - GridLayoutGroup과 함께 작동하도록 Content 크기 자동 업데이트
- 주인공 보장 시스템 구축
  - `PlaceMainCharacterBlockToSlot1()`: UI에서 주인공 블록을 1번 슬롯에 자동 배치
  - `UpdateSpawnManagerParty()`: 전투에 주인공이 없으면 인벤토리에서 강제 추가
  - 인벤토리 데이터 우선 사용으로 강화/커스터마이징 등 변경사항 반영

### 게임 컨셉트: 1번 슬롯의 의미
**중요: 이 컨셉트는 게임의 핵심 메커니즘입니다. 반드시 기억하고 구현 시 고려해야 합니다.**

- **1번 슬롯 = 주인공의 힘의 중추**
  - 1번 슬롯에 배치된 캐릭터는 주인공이 힘을 빌려주는 존재
  - 그 존재가 전투 중 죽으면 주인공도 같은 피해를 받음
  - 따라서 1번 슬롯의 캐릭터가 죽으면 게임오버

- **구현 의미**
  - 기본적으로 주인공이 1번 슬롯에 배치됨 (자동 배치)
  - 필요 시 다른 캐릭터를 1번 슬롯에 배치 가능 (뺄 수 있음)
  - 1번 슬롯이 비어있으면 주인공을 강제로 추가 (보장 시스템)
  - 1번 슬롯의 캐릭터가 죽으면 게임오버 로직 필요 (향후 구현)

- **설계 철학**
  - 주인공이 직접 싸우는 것이 아니라, 힘을 빌려주는 존재를 통해 싸움
  - 1번 슬롯의 존재가 죽으면 주인공도 함께 죽는 구조
  - 전략적 선택: 강한 캐릭터를 1번에 배치하면 위험하지만 강력함, 주인공을 배치하면 안전하지만 약함

- **향후 구현 예정: 1번 슬롯 체력 시스템**
  - 1번 슬롯의 캐릭터는 고정 체력 100을 가짐
  - 특성(패시브 등)을 통해 추가 체력을 받을 수 있음
  - 예: 기본 100 + 특성 보너스 = 최종 체력
  - 이는 주인공의 힘을 빌려주는 존재이므로 고정된 기본 체력을 가지는 것이 합리적

- **향후 구현 예정: 1번 슬롯 고정 스탯 시스템**
  - 1번 슬롯의 특수성을 고려한 고정 스탯 추가 시스템 정립 필요
  - 공격력 방어력은 캐릭터의 스탯을 따라갈 예정
  - 주인공 캐릭터는 공방 0인 대신 스킬 커스텀의 장점
  - 비주인공 캐릭터는 자체 스탯과 패시브를 사용 가능
  - 특성/패시브를 통한 추가 스탯 보너스 시스템과 연동
  - 주의: 하나를 수정하면 연쇄적으로 다른 시스템도 수정이 필요하므로 신중하게 설계 필요

### 결과/의도
- 주인공이 항상 전투에 포함되도록 보장
- 인벤토리 데이터 우선 사용으로 게임 진행 중 변경사항 반영
- 게임 컨셉트 명확화로 향후 게임오버 로직 구현 시 참고 가능

## 작업: 상태이상 아이콘 표시 문제 해결 및 시스템 개선

### 최초 목표
- **상태이상 아이콘이 표시되지 않는 문제 해결**
  - 아이콘 이미지가 ScriptableObject에 등록되어 있지만 화면에 표시되지 않음
  - 반응형 아이콘은 일단 제외하고 기본 아이콘 표시부터 해결

### 구현/변경 사항
- **상태이상 아이콘 표시 문제 해결**
  - `StatusEffectInstance`, `StatusEffectInstanceBuff`, `StatusEffectInstanceReaction`에 `Awake()` 메서드 추가
  - `iconRenderer`가 인스펙터에서 할당되지 않았을 때 자동으로 찾아서 할당
  - `Initialize()`에서 기본 아이콘(`GetIcon()`)을 우선 사용하도록 수정
  - 동적 아이콘(`GetDynamicIcon()`)은 기본 아이콘이 없을 때만 사용

- **동적 아이콘 시스템 개선**
  - `StatusEffectData`에 `icon`(기본/양수)과 `negativeIcon`(음수) 필드 추가
  - ScriptableObject에서 직접 아이콘을 할당할 수 있도록 개선
  - 경로 기반 로딩은 폴백으로 유지
  - `GetDynamicIcon()`에서 직접 할당된 아이콘을 우선 사용하도록 수정

- **스킬 인벤토리 ScrollRect 스크롤 문제 해결**
  - Content Size Fitter의 Vertical Fit을 Preferred Size로 자동 변경
  - `CheckAndFixContentSizeFitter()` 메서드 추가
  - GridLayoutGroup과 함께 작동하도록 Content 크기 자동 업데이트

- **주인공 보장 시스템 개선**
  - `PlaceMainCharacterBlockToSlot1()`: UI에서 주인공 블록을 1번 슬롯에 자동 배치 (잠금 해제)
  - `UpdateSpawnManagerParty()`: 주인공이 슬롯에 없으면 경고만 출력 (강제 추가 제거)
  - 1번 슬롯도 다른 슬롯처럼 자유롭게 교체/제거 가능하도록 변경
  - 인벤토리 데이터 우선 사용으로 강화/커스터마이징 등 변경사항 반영

- **1번 슬롯 스프라이트 플립 로직 수정**
  - 1번 슬롯에 주인공이 아닐 때만 스프라이트 플립 적용
  - 주인공(000001): 우향 (flipX = false)
  - 다른 캐릭터: 좌향 (flipX = true)
  - 2~4번 슬롯: 모두 좌향 (flipX = true)

- **인벤토리 재정렬 시스템 구축**
  - `SortCharacterBlocks()` 메서드 추가
  - GameProgressManager의 CharacterInventory 순서를 기준으로 정렬
  - `ReturnCharacterBlock()` 호출 시 자동 재정렬
  - `RefreshInventory()` 초기화 시에도 재정렬 메서드 사용으로 일관성 확보

### 작업 과정
- 미흡한 점 탐색 및 문제 해결에 집중
- 여러 시스템의 연쇄적 문제 발견 및 해결
- 코드 일관성 및 유지보수성 개선

### 결과/의도
- **상태이상 아이콘 표시 문제 해결**: iconRenderer 자동 할당 및 기본 아이콘 우선 사용으로 아이콘 정상 표시
- **동적 아이콘 시스템 개선**: ScriptableObject에서 직접 아이콘 할당 가능, 경로 기반 로딩은 폴백으로 유지
- **스킬 인벤토리 스크롤 정상 작동**: Content Size Fitter 자동 설정으로 스크롤 기능 복구
- **주인공 보장 시스템 안정화 및 유연성 확보**: 1번 슬롯 자유 교체 가능, 인벤토리 데이터 우선 사용
- **1번 슬롯의 특수성 반영**: 주인공 우향, 다른 캐릭터 좌향으로 플립 로직 개선
- **인벤토리 정렬 일관성 확보**: 재정렬 메서드로 초기화와 반환 시 일관된 순서 유지

## 2024-11-11 작업: 음수 상태이상 아이콘 대응형 시스템 테스트 준비

### 최초 목표
- **음수 상태이상 아이콘 테스트**: 음수 상태이상(negativeIcon)이 제대로 표시되는지 테스트
  - `StatusEffectData`에 `negativeIcon` 필드는 이미 추가되어 있음
  - `GetDynamicIcon()`에서 음수값일 때 `negativeIcon` 사용 로직은 구현되어 있음
  - 하지만 실제로 음수값일 때 아이콘이 제대로 표시되지 않는 문제 발견

### 구현/변경 사항
- **아이콘 설정 로직 개선**: 버프/디버프 타입일 때 동적 아이콘 우선 사용
  - `StatusEffectInstanceBuff`: 버프/디버프 타입일 때 `GetDynamicIcon(value)`를 우선 호출하도록 수정
  - `StatusEffectInstance`: 버프/디버프 타입일 때 `GetDynamicIcon(effectValue)`를 우선 호출하도록 수정
  - `StatusEffectInstanceReaction`: 버프/디버프 타입일 때 `GetDynamicIcon(value)`를 우선 호출하도록 수정
  - 이제 음수값일 때 `GetDynamicIcon()`이 먼저 호출되어 `negativeIcon`이 제대로 사용됨

### 작업 과정
- 기존 코드에서 `GetIcon()`을 먼저 호출하여 기본 아이콘을 반환하면 `GetDynamicIcon()`이 호출되지 않는 문제 발견
- 버프/디버프 타입일 때는 동적 아이콘을 우선 사용하도록 로직 변경
- 디버그 로그에 값(value) 정보 추가하여 테스트 시 확인 가능하도록 개선

### 결과/의도
- **음수 상태이상 아이콘 테스트 준비 완료**: 음수값일 때 `negativeIcon`이 제대로 표시되도록 로직 수정
- 버프/디버프 타입 상태이상은 이제 값에 따라 적절한 아이콘(양수: icon, 음수: negativeIcon)을 표시
- 실제 테스트는 Unity 에디터에서 음수값을 가진 상태이상을 적용하여 확인 필요

### 테스트 완료 (2024-11-11)
- **음수 상태이상 아이콘 테스트 성공**: 실제 전투에서 음수값 상태이상의 아이콘이 다운 화살표로 바뀌는 것을 확인
  - 음수값일 때 `negativeIcon`이 제대로 표시됨
  - 동적 아이콘 시스템이 정상 작동함
  - 버프/디버프 타입 상태이상의 값에 따른 아이콘 변경이 정상 작동함

## 작업: 스킬 호버 정보 갱신 문제 해결

### 최초 목표
- **스킬 호버 정보 갱신 문제**: 스킬 인벤토리에서 첫 번째로 호버한 스킬의 정보만 표시되고, 이후 다른 스킬을 호버해도 정보가 갱신되지 않는 문제 해결

### 구현/변경 사항
- **VirtualMouse.cs의 OnHoverEnter 메서드 수정**: 같은 패널이어도 데이터를 항상 갱신하도록 수정
  - 데이터 설정을 패널 활성화 전에 먼저 수행하도록 순서 변경
  - 같은 패널(`skillPanel`)을 재사용할 때도 데이터 갱신 보장
  - 주석 추가: "같은 패널이어도 데이터는 항상 갱신 (중요!)"

- **VirtualMouseSkillPanel.cs의 SetSkillData 메서드 개선**: 항상 UI 갱신 보장
  - 같은 데이터여도 항상 UI 갱신하도록 주석 추가
  - 디버그 로그에 `isSameData` 정보 추가하여 같은 데이터인지 확인 가능

### 작업 과정
- 사용자 보고: 첫 번째 호버한 스킬 정보만 표시되고 이후 갱신되지 않음
- 문제 원인: 같은 패널(`skillPanel`)을 재사용할 때 데이터 갱신이 제대로 이루어지지 않음
- 해결: 데이터 설정을 패널 활성화 전에 먼저 수행하고, 항상 UI 갱신 보장

### 결과/의도
- **스킬 호버 정보 갱신 정상 작동**: 다른 스킬을 호버할 때마다 정보가 제대로 갱신됨
- 같은 패널을 재사용하더라도 데이터가 항상 갱신되어 올바른 스킬 정보 표시

## 2024-11-11 작업: 상태이상 월드 스페이스 호버 기능 구현

### 최초 목표
- **상태이상 호버 기능 재작업**: 월드 스페이스에 있는 상태이상 아이콘에 마우스를 올렸을 때 정보를 표시하는 기능 구현
  - 이전에 계획했던 상태이상 호버 기능을 콜라이더 충돌 기반으로 재구현
  - VirtualMouseWorldObject 스크립트 생성

### 구현/변경 사항
- **VirtualMouseWorldObject.cs 생성**: 월드 스페이스 상태이상 호버 감지 전용 스크립트
  - 싱글톤 패턴 및 DontDestroyOnLoad 적용
  - 마우스 위치를 월드 좌표로 변환하여 추적
  - `Physics2D.OverlapPointAll`을 사용한 직접 충돌 체크 방식
  - 상태이상 컴포넌트 체크 (StatusEffectInstance, StatusEffectInstanceBuff, StatusEffectInstanceReaction)
  - VirtualMouse에 호버 진입/벗어남 알림

- **충돌 감지 방식 결정**: 트리거 이벤트 대신 직접 충돌 체크 방식 채택
  - 초기에는 OnTriggerEnter2D/Exit2D 방식 시도
  - 작동하지 않아 매 프레임 직접 충돌 체크 방식으로 변경
  - 불필요한 Rigidbody2D, Collider2D 요구사항 제거

- **레이어 마스크 필터링 추가**: 상태이상 레이어만 감지하도록 최적화
  - `statusEffectLayerMask` 필드 추가 (기본값: 모든 레이어)
  - Inspector에서 StEf 레이어만 선택 가능
  - 상태이상 프리팹이 StEf 레이어에 있음을 확인

- **코드 간소화**: 불필요한 기능 제거
  - RequireComponent 제거
  - 태그/레이어 체크 로직 간소화
  - 복잡한 HashSet 관리 제거
  - 핵심 기능만 남김 (약 159줄)

### 작업 과정
- 사용자 요청: 상태이상 호버 기능 재작업
- VirtualMouseWorldObject 스크립트 생성 및 초기 구현
- 트리거 이벤트 방식 시도 → 작동하지 않음
- 직접 충돌 체크 방식으로 변경
- 불필요한 코드 제거 및 간소화
- 레이어 마스크 필터링 추가
- 상태이상 프리팹 레이어 확인 (StEf 레이어)

### 결과/의도
- **상태이상 월드 스페이스 호버 기능 기본 구조 완성**: VirtualMouseWorldObject를 통한 충돌 감지 시스템 구축
- 콜라이더 충돌 기반 직접 체크 방식으로 안정적인 감지 가능
- 레이어 마스크를 통한 성능 최적화 가능
- Inspector에서 StEf 레이어 설정으로 상태이상 아이콘만 감지 가능

### 다음 작업 예정
- 실제 호버 테스트 및 디버깅
- VirtualMouse와의 연동 확인
- 상태이상 팝업 표시 테스트

---

## 작업: 상태이상 설명 패널 음수값 아이콘 대응

### 최초 목표
- **상태이상 설명 패널 아이콘 개선**: 스킬 호버 시 표시되는 상태이상 설명 패널에서도 음수값일 때 아이콘이 변경되도록 개선
  - 전투 중에는 음수값 아이콘이 다운 화살표로 표시되는 것을 확인
  - 스킬 정보 패널의 상태이상 설명에서도 동일하게 음수값 아이콘 표시 필요

### 구현/변경 사항
- **StatusEffectDescriptionPanel.cs 수정**: 상태이상 설명 패널 아이콘 설정 로직 개선
  - 버프/디버프 타입일 때 동적 아이콘 우선 사용하도록 수정
  - 음수값일 때 `negativeIcon`이 제대로 표시되도록 보장

- **StatusValueBlockManager.cs 수정**: 상태이상 값 블록 아이콘 설정 로직 개선
  - 버프/디버프 타입일 때 동적 아이콘 우선 사용하도록 수정
  - 음수값일 때 `negativeIcon`이 제대로 표시되도록 보장

- **VirtualMouse.cs 수정**: 상태이상 팝업 아이콘 추출 로직 개선
  - `ExtractStatusEffectBuffData`: 버프/디버프 타입일 때 동적 아이콘 우선 사용
  - `ExtractStatusEffectReactionData`: 버프/디버프 타입일 때 동적 아이콘 우선 사용

- **StatusEffectInstance.cs 수정**: 팝업 데이터 제공 메서드 개선
  - `GetStatusPopupData`: 버프/디버프 타입일 때 동적 아이콘 우선 사용

### 작업 과정
- 사용자 요청: 상태이상 설명 패널에서도 음수값일 때 아이콘이 변경되어야 함
- 모든 상태이상 설명 관련 코드에서 아이콘 설정 로직을 일관되게 수정
- 버프/디버프 타입일 때만 동적 아이콘 우선 사용하도록 통일

## 전술 축복 시스템 설계 및 대상 선택 UI 구축

### 결정된 사항
- **전술 축복 3개 컨셉 결정** (라인 2: 전술의 형태 테마)
  1. **"하나를 위한 모두"**: 파티 3명의 ATK, DEF를 각각 1씩 빼서 대상 하나에게 추가
  2. **"모두를 위한 하나"**: 지정 대상의 ATK, DEF를 3씩 깎고 파티 전체에 나눠주기
  3. **"권한대행"**: 한 개체를 플레이어가 아닌 AI화 시키는 대신 스탯 자체를 강화

### 구현/작성 완료
- **TacticalBlessingTargetSelector.cs 작성** (Assets/Scrips/UI/)
  - 전술 축복의 대상 선택 UI 관리 클래스
  - 파티 멤버 선택 버튼 생성 및 관리
  - 전투 중이 아닐 때만 사용 가능 (월드맵/스테이지 씬)
  - 축복 타입에 따른 제목 텍스트 자동 변경

### 추가 작업 필요 사항
- 선택된 캐릭터 ID 저장 구조 설계 (GameProgressData 또는 BlessingManager)
- BlessingPanel에서 전술 축복 체크 및 대상 선택 UI 호출 로직 추가
- Unity 씬 설정 (패널 오브젝트, 버튼 프리팹 등)
- 전술 축복 효과 클래스 구현 (BlessingEffectOneForAll, BlessingEffectAllForOne, BlessingEffectAuthorityDelegation)
- 스탯 재분배 로직 및 AI 전환 시스템 구현

### 결과/의도
- **상태이상 설명 패널 음수값 아이콘 대응 완료**: 스킬 호버 시 표시되는 상태이상 설명에서도 음수값일 때 다운 화살표 아이콘 표시
- 전투 중과 스킬 정보 패널에서 일관된 아이콘 표시 보장
- 모든 상태이상 설명 관련 UI에서 음수값 아이콘이 제대로 표시됨

## 월드맵 업그레이드 UI/스탯 조작 마감(선택/표기/환급 정합성)
### 작업 일자
- 2026-03-26 (목)

### 구현/수정 사항
- `WorldMapUiHub` 정리
  - `GameManager.ScreenState` 기반으로 메뉴 크롬(메뉴 바/메뉴 버튼) 레이아웃을 동기화하는 `SyncHubLayoutToScreenState()` 추가
  - `CharacterUpgrade` 진입 시 메뉴 크롬을 즉시 숨겨 겹침 방지
  - 업그레이드 패널 루트는 버튼 메서드에서 `SetActive(true/false)`로 제어

- `CharacterUpgradePannel` 정리
  - 캐릭터 선택 대상 표시 로직 유지 + `ClosePanelIfOpen()` 추가
  - 캐릭터 선택 저장/복원(마지막 선택 ID 유지) 흐름 추가
  - 저장된 대상이 사용불가면 표시를 `null`로 비우는 복원 정책 포함

- 캐릭터 스프라이트 로딩 규칙 통일(리소스 폴더 경로)
  - 업그레이드 UI 쪽 로딩을 `character.Sprite`(폴더 경로) 기반으로 `.../Stand` 우선 로드
  - 기존 데이터 호환을 위한 폴백(`Resources.Load(character.Sprite)`) 유지

- `UpgradeStat` 단계/코스트/환급 정합성 수정
  - 회피/명중: 클릭당 `+0.05` 단위로 증가/감소
  - 비용/환급 계산도 “클릭 단계” 기준으로 다시 맞춰 `Spent/Decrease`가 일관되게 작동하도록 수정
  - 표시 포맷을 `Evasion/Accuracy = F2`로 변경
  - 체력(HP/MaxHP): 클릭당 `+10` 적용

- 전투 UI 역할 라우팅 정리(보편 진입점 기반)
  - `CharacterInfo`에 `isTurnTargetInfo`, `isSelectedTargetInfo` 역할 bool 추가
  - `TurnManager`에서 기존 직접 갱신 로직 대신 역할 기반으로 `CharacterInfo`들을 라우팅 갱신
  - `TargetSelector`에서 선택 대상 갱신 시 `isSelectedTargetInfo` UI로 바로 전달하도록 연결
  - 선택 대상 UI가 타이밍 문제로 꺼지던 케이스 방지(선택 UI는 타겟 null이어도 HideInfo 호출 제거)

### 확인/테스트 메모
- 캐릭터 선택 → 닫았다가 재오픈 → 이전 선택 유지 동작 확인
- 회피/명중: 증가/감소 및 `Spent`/환급 정합성 확인(감소 버튼이 역할/단계 기준으로 정상 동작)
- HP: 클릭당 +10/-10 동작 확인

### 검증 결과
- 업그레이드 UX/정합성 검증 **전체 통과(PASS)**.
- 캐릭터 전환/재선택/재오픈/저장 복원 시 값 유지 정상.
- 스프라이트 표시, 증감, 환급, `Spent` 표시 모두 의도대로 동작 확인.

### 다음 작업(우선순위)
1. **사용불가 판정 시스템 구현**
   - 캐릭터가 사용불가로 전환되는 조건/타이밍 정의
   - 판정 발생 시 `MarkCharacterUnavailable(characterId)` 실제 호출 연결
2. **전투 UI 데이터 타입 정리**
   - `CharacterInfo`의 `Text`/`TMP` 혼용 여부 정리 및 참조 일관성 점검
3. **업그레이드 UI 폴리싱**
   - 선택 카드 하이라이트/기본 선택 연출/스크롤 포커스 개선

---

## 스냅샷 시스템 기반 구축(턴 되돌리기/연전 상태 승계 준비)
### 작업 일자
- 2026-03-26 (목)

### 구현/수정 사항
- `BattleSnapshotManager` 신규 작성 (`Assets/Scrips/Battle/BattleSnapshotManager.cs`)
  - 싱글톤 기반(`Instance`)으로 전투 스냅샷 관리
  - **턴 스냅샷 히스토리**(기본 최대 8개) + **전투 종료 스냅샷**(1개) 구조 분리
  - 저장 데이터: `characterId`, `hp`, `isDead`, `kdp`, `collapseChance` (+ turn 메타 일부)

- 전투 종료 스냅샷 저장 연결
  - `BattleManager.EndBattle(bool isVictory)`에서
  - `BattleSnapshotManager.Instance.CaptureBattleEndSnapshot(allCharacters)` 호출

- 턴 스냅샷 저장 연결(정책 반영)
  - `TurnManager.StartTurn(...)`에서 턴 시작 직전에 저장
  - **플레이어 턴(`character.IsPlayer`)에서만 저장**하도록 제한

- 패시브 로더 패턴 통일 작업 병행
  - `PassiveLoader`를 `Instance + Initialize()` 패턴으로 변경
  - `GameManager`에서 `PassiveLoader.Instance.Initialize()` 호출로 전환

### 현재 상태(중요)
- 스냅샷 **저장 기반은 연결 완료**
  - 전투 종료 시점 저장: 연결됨
  - 플레이어 턴 시작 전 저장: 연결됨
- 스냅샷 **복원/되돌리기 실제 적용은 미완**
  - TurnManager/BattleManager에서 `TryRestore...` 호출 경로 미연결
  - 월드맵 도달 시 `ClearAllSnapshots()` 강제 정리 훅 미연결

### 다음 작업 예정
1. `TryRestoreTurnSnapshot(...)` 실제 호출 지점 연결(턴 되돌리기 UI/입력 연동)
2. `TryRestoreBattleEndSnapshot(...)`를 다음 전투 진입 직후에 연결
3. 월드맵 복귀 시 `ClearAllSnapshots()` 확정 호출
4. 보상/자원/랜덤 처리와 충돌 없는 복원 규칙 확정

---

## 패시브 미적용 트러블슈팅(원인 규명 및 해결)
### 작업 일자
- 2026-03-27 (금)

### 사건 발단
- 캐릭터 `Mora(ID=000007)`에 `080001(체력 강화, MaxHp +20)`를 부여했으나 전투 UI/인스펙터에서 HP가 `83`으로 유지됨.
- 기대값은 `103`이었고, 실제 수치가 오르지 않아 패시브 적용 루트 점검 시작.

### 점검 과정
- 1차 점검: 패시브 XML 및 로더 검증
  - `BasePassive.xml`에서 `080001` 스펙(`Type=None`, `TargetStat=MaxHp`, `Value=20`) 확인
  - `PassiveLoader`에서 `5개 로드` 로그 확인
- 2차 점검: 실제 적용 지점 추적
  - 적용 기준은 `CharacterStats.SetData()` -> `ApplyPassives(data.Passives)`로 확인
  - `PassiveTrace` 로그 추가 후, `Passives=080001`이 들어오는데 `PassiveBonus=0`이 찍히는 현상 확인
  - 동시에 `패시브 데이터를 찾을 수 없습니다. ID=080001` 경고 확인
- 3차 점검: 저장소 구조 비교
  - Character/Skill은 `Data` 클래스 static 딕셔너리 기반
  - Passive는 조회 경로가 `PassiveLoader.Instance` 의존이라, 로드 성공 후에도 조회 시점에 참조 불일치가 발생할 수 있는 구조임을 확인

### 원인
- 패시브 데이터 저장 자체보다 **조회 경로의 인스턴스 의존성**이 문제.
- 로더는 정상 로드됐지만 적용 시점 조회에서 `Instance` 경로가 안정적이지 않아 `GetById` 미스가 발생.

### 해결
- `PassiveLoader` 저장소를 전역 static 딕셔너리로 고정
- `CharacterData`/`CharacterStats`의 패시브 조회를 `Instance` 의존 경로에서 static 조회 경로로 통일
  - `PassiveLoader.GetByIdStatic(id)` 추가
  - 상시 스탯 계산(`CharacterData.GetFinalStatValue`) 및 전투 적용(`CharacterStats.ApplyPassives`) 모두 static 조회 사용
- 스탯부스트 패시브(`Type=None`)는 데이터 단계 상시 반영, 클래스형 패시브는 전투 중 동작 분리 정책 유지

### 결과
- 로그에서 `PassiveBonus=20` 확인
- `SetData-After` 기준 `Mora Hp/MaxHp = 103/103` 정상 반영 확인
- `ActivePassives=080001` 확인
- 패시브 적용 경로 정상화 완료

### 후속 정리 예정
- 추적용 `PassiveTrace` / `[Spec]` 상세 로그는 안정화 확인 후 축소 또는 토글화
- `PassiveManager` 역할(레지스트리/중간 캐시) 정리 여부 추후 결정

---

## 전투·패시브·UI·사운드·영체 연출 일괄 정비
### 작업 일자
- 2026-04-01 (수)

### 패시브·상태이상
- `BasePassive.xml` 등: `StartCount` 복구 및 `TurnIntervalGrantStatus`와 코드 연동(`PassiveData`, `PassiveLoader`, `PassiveEffectTurnIntervalGrantStatus`, `CharacterStats.TryTickOwnerTurnPassiveUseAndShouldFire`).
- `StatusEffectImmunity` 타입 및 XML `ImmuneStatusEffectIDs` 파싱·적용(`CharacterStats.AddStatusEffectPrefab` 진입 시 차단).
- 지속피해(ContinuousDamage) **3안**: 동일 EffectID 재적용 시 피해량만 절반(내림) 합산, 지속 턴은 갱신하지 않음 — 주석으로 정책 명시(`CharacterStats`, `StatusEffectInstance.MergeHalfIncomingDamage`, `StatusEffectController.GetStatusEffectInstance`).
- `PassiveSystemExtensionGuide`에 `startUseCount`·면역 패시브 설명 보강.

### 스킬·버프 연출·타겟
- `SkillManager`: Buff 분기에서 `ApplyStatusEffects` 이중 적용 제거, `GetBuffTargets`로 실제 수혜자에만 버프/모션/연출 적용. 버프 시 **시전자만** 전투 위치 이동.
- 주인공 영체(GhostProxy): `GhostSpritePath`·`flipX` 방향, 이후 **고정 거리 스폰 + Lerp 이동 제거**, `CharacterMotionController.GetExpectedSkillMotionDuration`으로 모션 길이만큼 유지 후 페이드.
- `SkillData` / `SkillLoader` / `BossSkills.xml`: **`GhostProxyScale`** XML 파싱·Clone·Override·`PlayGhostProxyEffect`에 `localScale` 반영(미지정·0 이하 시 1).

### 데스 아이콘·슬롯 연동
- `SlotHandler`: `OnDeathEvent` 구독 → 전용 캔버스에 데스 아이콘 스폰. 스프라이트 바운드 기준 높이 비율(`deathIconHeightNormalized`)·앵커·피벗 중앙 고정. 유효 프리팹 검사(Image/SpriteRenderer/TMP/Text).
- `BattleEffectManager`: 데스 아이콘 **레거시 Instantiate 경로 제거**(슬롯 경로 단일화 방향).

### 턴·UI·입력
- `TurnManager`: 상태이상 정산 후 `Hp <= 0`이면 즉시 사망 처리(죽은 유닛 행동 방지). 월드맵 복귀 전 `ScreenState` 정리. 사망 연출 대기·턴 타이밍 로그 등 조정 이력 반영.
- `BattleUIManager` / `StatusEffectSlot`: 상태이상 팝업·연출 대기 시간 단축 및 상한(`maxStatusPopupWaitSeconds`, `maxBlockingStatusEffectAnimations` 등).
- `TurnTransitionSkipManager`: 사망 구간 스킵 불가 유지.
- `GameManager`: `SampleScene` 로드 시 월드맵 스크린 상태 복구(전투 복귀 후 클릭 불가 완화). `SoundManager` 일시정지 연동.
- `WorldMapStageSelection`: 진입 조건(`conditionStageId`)·잠금 표시, `SetUIOpen(false)`로 복귀 시 플래그 리셋.

### 사운드·유저 설정
- `SoundManager`(BGM/SE/Voice, 볼륨·mute, `SetPaused`).
- `UserSettingsManager` + `user_settings.json` — `GameManager.Start`에서 사운드에 적용.

### 기타 데이터·문서
- `Assets/Docs/CharacterXmlChecklist.md` — 캐릭터 XML 체크리스트(Resources/Data/Characters 밖에 두어 XML 파싱 충돌 방지).
- `GameProgressManager`: 테스트용 스킬 해금(영체 등 확인용) 등 기존 작업 맥락 유지.

### 참고
- 승리/패배 핵심 로직(`CheckBattleEnd`, `EndBattle`, `ProcessBattleReward` 등)은 커서룰상 직접 수정하지 않는 방향으로 유지.

---
## 스냅샷 복원 루트 연결 + 패시브/상태이상 회귀 체크
### 작업 일자
- 2026-04-03 (금)

### 스냅샷(턴/전투진입/월드맵) 연결
- `BattleSnapshotManager`
  - 전투 씬 전환에도 유지되도록 `DontDestroyOnLoad` 적용
  - `ClearAllSnapshots()`는 `turnSnapshots`만 정리하고 `battleEndSnapshot`은 “다음 전투 진입 복원” 용도로 보존하도록 변경
- `GameManager`
  - `SampleScene` 로드 시 `BattleSnapshotManager.Instance.ClearAllSnapshots()` 호출(월드맵 복귀 시 턴 히스토리 정리)
- `BattleManager.StartBattle()`
  - `SpawnAllUnits()` 직후 `TryRestoreBattleEndSnapshot(allCharacters)`를 1회 수행하고, 성공 시 `ClearBattleEndSnapshot()`로 재복원 방지
- `TurnManager`
  - `TestBattle` 씬에서 `F8` 입력 시 `TryGetTurnSnapshot(turnsBack)` + `TryRestoreTurnSnapshot`을 실제 호출
  - `snapshot.turnOrderIds` 기반으로 `turnQueue`/`TurnChanse`를 재구성 후 `StartTurn(targetCharacter)`로 복귀
  - 복원 중에는 `StartTurn`의 스냅샷 재캡처를 막기 위해 `isRestoringSnapshot` 가드 추가

### 패시브/상태이상 회귀(최근 변경분) 점검 포인트
- 면역(`StatusEffectImmunity`)
  - `CharacterStats.AddStatusEffectPrefab()` 진입 시 `IsImmuneToStatusEffect(effectId)`면 프리팹 생성/중첩 로직 전체를 차단
- 턴간격 누적/첫 발동(StartCount)
  - `PassiveEffectTurnIntervalGrantStatus`는 `TryTickOwnerTurnPassiveUseAndShouldFire(passiveId, useCount, startUseCount)`로
    - 최초 tick에서 `startUseCount`를 시드(acc 초기값)로 사용
    - 누적이 임계(useCountThreshold) 이상이면 발동 후 누적을 0으로 리셋
- 지속피해(ContinuousDamage) 3안
  - 동일 `EffectID`가 재적용될 때
    - 새 인스턴 생성 없이 `StatusEffectInstance.MergeHalfIncomingDamage()`로 “들어온 값의 절반(내림)”만 기존 value에 합산
    - 지속 턴(duration/remainingTurns)은 갱신하지 않음(기존 remainingTurns 유지)
- 로그 축소 기준(불필요 추적 로그)
  - `CharacterStats.AddStatusEffectPrefab()`에서 `PassiveStatusEffectFlow == false`일 때
    - 면역 차단/지속피해 중첩(3안)/중복 무시에 대한 else 로그를 제거(Trace 플래그가 꺼져 있으면 로그가 쌓이지 않게)

### 확인 시나리오(수동 테스트 체크리스트)
- 면역
  - `StatusEffectImmunity` 패시브가 면역 목록에 포함된 상태이상 ID를 스킬로 거는 상황에서, 해당 상태이상이 UI/인스턴스에 생성되지 않는지 확인
- StartCount
  - `TurnIntervalGrantStatus`가 `UseCount=2`일 때 `StartCount=1`로 지정된 경우,
    - 소유자의 첫 턴에서 1(시드) + 1(tick) → threshold 도달로 “첫 발동”이 되는지 확인
  - StartCount=0/빈 값(=0 시드)일 때 “발동 타이밍이 1턴 늦지” 않는지 확인
- ContinuousDamage 3안
  - 동일 출혈/중독/화상 EffectID를 연속으로 적용했을 때,
    - 기존 인스턴은 유지되고(value만 증가)
    - 증가량이 `incomingValue/2`(내림)인지,
    - 지속 턴이 늘어나지(remainingTurns 갱신 X) 않는지 확인

### 오늘 완료 기준(DoD)
- 스냅샷 복원 경로 3종(턴 복원/전투 진입 시 battleEnd 복원/월드맵 복귀 시 정리)이 실제로 호출되도록 코드 연결 완료
- 패시브/상태이상 최근 변경분(면역, StartCount, 지속피해 3안)에 대한 확인 항목(수동 체크리스트) 정리 완료

### 방향성 확정(우선순위 전환)
- 스냅샷 기반 시간 되돌리기 기능은 **기반 구현 단계로 일단 고정**하고, 본격 플레이어 능력/해금 연동은 후순위로 미룸.
- 당장 다음 작업 우선순위는 **유닛 능력 구현(패시브/스킬/상태이상 연계 확장)**으로 전환.
- 구조 검진 결과 핵심 오동작(롤백 시 행동 주체 판정)을 수정했고, `currentActorId` 기반 복원으로 정렬.
- `BattleSnapshotManager`는 `GameManager`와 함께 존재하는 구조를 유지하며, 별도 `DontDestroyOnLoad` 적용은 보류/비적용 상태로 정리.

---

## 세션 로그: 스킬/상태이상·스턴 설계·데이터 연동 (2026-04-04)

### 흐름 요약
1. **UseSkill(상호작용/명령 스킬)**  
   - XML `<UseSkill>` → 데이터 반영을 위해 `SkillData.UseSkillId` 추가, `SkillLoader` 파싱 및 `Clone`/`OverrideSkill` 연동.  
   - 실제 “명령 시 보유자 전원 즉시 발동·턴 미소모” 런타임 분기는 **미구현**(다음 작업 후보).

2. **스킬 효과 확률**  
   - `SkillEffectInfo.Chance`(0~1, 기본 1), XML `<Chance>` 파싱, `SkillManager`에서 상태이상/버프 적용 루프에 확률 판정 연결.

3. **스턴·행동불가 설계**  
   - 패시브는 **행동 가능 여부와 무관**하게 턴 시작에 동작하는 것으로 합의.  
   - 턴 흐름: 상태이상 정산 → `InvokePassivesOnOwnerTurnStart` → 그 다음 **행동불가면 커맨드 단계 생략**.

4. **턴 강제 종료(스턴 감지)**  
   - `TurnManager`: 위 순서 이후 `StatusEffectController.HasStatusEffectType(Stun)`이면 `EndTurn()` 호출(슬롯 코루틴 경로 + 슬롯 없는 백업 `StartTurn` 경로 둘 다).  
   - `StatusEffectController`: 활성 프리팹에서 `EffectData` 타입 조회용 `HasStatusEffectType` 추가(리플렉션 보조 포함).

5. **상태이상 프리팹 생성**  
   - 기존에는 컨트롤러에 박은 기본 프리팹만 사용.  
   - `StatusEffectData.effectPrefab`이 있으면 **우선 사용**, 없으면 기존 폴백(`ResolveEffectPrefab`).

6. **ScriptableObject 생성 혼선**  
   - 베이스 `StatusEffectData`는 Create 메뉴에 있으나 찾기 어려울 수 있음 → **`StatusEffectCustomData`** 추가: `Create → Scriptable Objects/StatusEffect/Custom (범용)`, `OnEnable` 덮어쓰기 없음.  
   - `StatusEffectNoDamageBuffData` 등은 `OnEnable`로 ID/타입 고정 → 스턴용으로 부적합함을 정리.

7. **모션 ScriptableObject**  
   - `MotionDataObject`는 `CharacterMotionController`에서 `Resources.Load("MotionData/{캐릭터ID}_Motions")`로 로드되는 경로가 있음.  
   - 레포에 `Resources/MotionData` 에셋이 없으면 **고급 모션은 안 타고** 기본 모션으로 폴백.

8. **상태이상 ID 규칙**  
   - 코드 강제는 없음. 문서·데이터 관례 정리.  
   - **스턴·행동불가(제어) 계열은 `023` 시작(`023001`~)으로 확정** → `scriptmap.md` 상태이상 목록에 반영(021005·022001·024001 보갈 포함).

### 내일(다음 세션) 후보
- 스턴용 아이콘·`Effect Prefab` 제작 및 SO 배치.  
- **`CharacterStats.AddStatusEffectPrefab`의 `Stun` 분기**에서 프리팹 생성·초기화 연결(없으면 스턴이 리스트에 안 올라가 턴 스킵도 동작 안 함).  
- 필요 시 `UseSkill` 런타임 적용.

### 후속(기록만 · 미수정)
- **빈 스킬 슬롯 호버**: 장착되지 않은 슬롯에 마우스를 올리면 부적절한 툴팁이 뜨는 경우가 있음(예: 스킬/상태이상 설명이 빈 슬롯에 매핑되어 “Stun”, “효과 설명 없음” 등 표시). → 빈 인덱스는 호버·패널 갱신을 스킵하거나 전용 빈 슬롯 UI로 분기할 것.

---

## 2026-04-04 — 기절(스턴) 토큰·턴 연동·UI·모션·스탯 API 정리

### 데이터
- `EnemyMobSkills.xml` / `PlayerSkills.xml`: 기절 `023001`은 **토큰형**(XML에서 `Value`/`Duration` 생략 가능, `Chance`만 의미).
- 테스트 스킬 `015055`(테스트 기절) 유지.

### 런타임 — 기절 코어
- `StatusEffectInstanceStun.cs`: 토큰 전용 인스턴스(지속턴 도트 루프 비참여).
- `StatusEffectController`: `AddStunEffect`, `ConsumeOneStunEffect`, `HasStatusEffectType`에 스턴 분기.
- `CharacterStats.AddStatusEffectPrefab` → `Stun` 시 위 경로로 적용.
- `TurnManager`: 행동불가 시 토큰 소모 후 `EndTurn`(슬롯 코루틴·백업 `StartTurn` 모두).

### 스탯 중심 턴 판정(리팩터)
- `CharacterStats`: `IsCommandPhaseBlockedByStatus()`, `ConsumeStunTokenAfterForcedTurnSkip()` — 턴 스킵 판정·소모는 **컨트롤러 직접 호출 대신 이 API**만 사용.
- `TurnManager`: `ShouldForceSkipTurnByStatus` 제거, 위 메서드로 통일.

### UI
- `VirtualMouse` / `VirtualMouseStEfPanel`: 스턴 호버 시 **수치 대신 설명 모드**(`useDescriptionInsteadOfNumbers`), SO `description` 없을 때 폴백 문구. 선택 필드 `descriptionBodyText`.
- `VirtualMouse`: `StatusEffectInstanceStun` 추출·호버 분기, `GetComponentInParent` 정리.

### 테스트 해금
- `GameProgressManager.SetupTestInventory()` (`isTestMode`): `UnlockSkill("015055")` 추가.

### 모션(기절 해제 타이밍 정합)
- **스턴 유지 중**: 스킬 종료 `ResetMotion`으로 **스탠드**(피격 고정 제거).
- **토큰 소모 순간**: `ApplyPostStunReleaseHitHold()` — Hit 스프라이트 + 흰색 유지.
- **다음 본인 턴 시작**(`StartTurn` 직후): `ApplyPostStunTurnStartMotionRecover()` — 플래그 있으면 `ResetMotion`(스탠드). 재기절이면 복구 보류.
- `DeathAction`에서 `pendingStandRecoverAfterStunConsume` 클리어.

### 프로그레스 / 설계 메모(대화 정리)
- KDP: `TakeDamage` 누적은 있으나 `MaxKDP` 미설정 XML 다수·`TriggerKnockdown()` 비어 있음 → **스턴 연동 미완**.
- 향후: `CharacterStats` 단일 `Recompute`·KDP→행동불가, `StatusEffectSlot` 기절 레이아웃/정산 연출, 스냅샷에 상태이상, 빈 슬롯 호버·**기본 스킬 깔기** 등은 후속.
- 연출 아이디어: 캐릭터 가운데 **제어계 전용 아이콘**(기절 등) 보조 표시는 선택.

### 확인
- 에디터에서 `023001` SO 또는 `stunEffectPrefab`·`StatusEffectInstanceStun` 프리팹 연결 필수.

---

## 2026-04-05 — 가상마우스 스킬 패널·캔버스 가시성·적 AI 생존 검사

### VirtualMouse / 스킬 인포 패널
- **`VirtualMouseUIPanel.SetVisible`**: `isVisible == visible`만 보고 조기 return 하면, 자식이 `SetActive`만 썼을 때 **실제 비활성인데 true로 간주**해 패널이 다시 안 켜지는 문제 → `gameObject.activeSelf == visible`까지 같이 검사.
- **`VirtualMouseSkillPanel`**: 패널 켜기/끄기를 `SetVisible(true/false)`로 통일(초기화·리셋·호버 진입·종료).
- **`ShowPanel` / `HidePanel`**: 동일한 `isVisible`·`activeSelf` 불일치 방지.
- **배경 색**: `Color * float`가 알파까지 깎던 문제 이후, **런타임에서 배경 Image 덮어쓰기 제거** — 배경은 씬/프리팹 인스펙터만 사용.
- **검진 메모**: 빈 슬롯 호버에도 툴팁이 뜨는 이유 — `SkillSlot.GetSkillData()`는 **`currentSkillID` + `skillDict`** 기준이라 `currentSkillBlock`이 None이어도 ID만 있으면 표시됨(`Initialize`/`SetSkill` 등).

### 스킬 패널이 안 보이던 원인(씬·런타임)
- **`SampleScene` `VirtualMouse` 루트 `RectTransform.localScale` (0,0,0)** → 자식 전체가 사실상 안 그려짐 → **(1,1,1)로 수정**.
- **`VirtualMouseCanvas`**: `HealVirtualMouseVisibility()` — 루트·**자손 중 localScale 정확히 (0,0,0)** 인 RectTransform을 (1,1,1)로 복구, 루트 `CanvasGroup.alpha` 1.
- **`EnsureVisibleAfterWorldMapLoad()`**: `SampleScene` 로드 시 `GameManager.HandleSceneLoaded`에서 `DismissAllHoverUi` 뒤 호출 — DDOL이라 `Start`가 다시 안 돌 때도 초기화 루틴 재실행.
- **스냅샷**: `BattleSnapshotManager`는 유닛 수치만 다루며 **UI/VirtualMouse와 무관** — 월드맵 복귀 시 문제 원인으로는 비해당(주석으로 명시).

### 전투 — 적 AI
- **`CharacterStats.IsCombatCapable()`**: `gameObject` 유효 + `IsActive` + `!IsDead` + `Hp > 0`.
- **`TurnManager.ProcessStatusEffectsWithAnimation`**: 적 턴에서 `EnemyActionRoutine` 시작 **직전** `IsCombatCapable()` 및 `EnemyAIController` 존재 확인, 실패 시 `AdvanceTurn`.
- **`EnemyAIController.EnemyActionRoutine`**: 진입 시·**0.8초 대기 후**·**스킬 사용 직전** 재검사; 생존 불가 시 `EndTurn()` 후 중단 — **죽어가는(빈사) 구간에서 턴이 넘어가기 전 공격** 방지.

### 확인(테스트 중)
- 월드맵 복귀 후 스킬 호버·패널 표시, 전투 중 적 턴 빈사/사망 경계.

---

## 2026-04-06 — KDP/넉다운·가상마우스·광역 버프·OpeningBuff AI·반사(리액션) 설계

### KDP 런타임 → `CharacterStats`
- **`KnockdownBuildup`** (`CharacterStats` 공개 필드, 인스펙터 표시): 전투 누적. 한도만 `data.MaxKDP`(XML).
- **`TakeDamage`**: `data.KDP` 대신 `KnockdownBuildup` 증가·한도 시 `TriggerKnockdown()` 후 0 리셋.
- **`SetData`**: `KnockdownBuildup = 0`, 공유 `CharacterData` 오염 방지용 `data.KDP = 0` 유지.
- **`BattleSnapshotManager`**: 스냅샷 `kdp` ↔ `KnockdownBuildup`(적만).

### KDP 턴 감쇠
- **`DecayKnockdownBuildupAtTurnStart()`**: 적·`MaxKDP>0`·누적>0일 때 **자기 턴 시작(`StartTurn`) 직후** `KnockdownBuildup /= 2`(내림). 로그 `[KDP]`.

### AttackType / 넉다운 배율
- **`TakeDamage`**: `AttackType`이 **none·null·공백**이면 KDP에 **`KnockdownMultiplier` 무관 1배** (`IsKnockdownAttackTypeNone`).
- **`SkillLoader`**: `ParseAttackTypeFromElement` — **태그 없음 → null**(부모 상속), **빈 태그 → `none`**. 최종 `skillDict` 등록 전 `NormalizeAttackType`(null/공백 → `none`).
- **`OverrideSkill`**: `AttackType != null`일 때만 덮어쓰기(자식에서 태그 생략 시 부모 유지).
- **`SkillData`**: `AttackType` 주석 갱신(상속·최종 none 규칙).

### 붕괴·빈사·타겟(이전 세션 연속 반영 요약)
- 붕괴: `CollapseChance`≤50% 시 판정 문턱 절반; 주사위 **난수 2회 max**로 완화.
- **`Deathcheck(allowAllyCollapseDiceRoll)`**: 아군 붕괴 주사위는 **피해 경로만 true**(`TakeDamage`, 도트 `BattleUIManager`, `StatusEffectSlot`). 턴 정산 등은 false.
- **`ApplyNearDeathDamagePressureForCollapse`**: 빈사 유지 중 추가 피해 시 `CollapseChance`만 +0.08(배율 상수).
- **`TurnManager`**: 플레이어 빈사는 정산 직후 턴 강제 종료 제외(`ShouldAbortTurnAfterStatusSettlement`).
- **`TargetSelector`**: `Hp≤0`도 **`!IsDead`면 선택 가능**(빈사 케어).
- **`VirtualMouse` / `VirtualMouseWorldObject`**: 넉다운 슬롯 **호버·팝업 제외**.

### 후속(메모)
- KDP/넉다운 **인게임 UI·게이지**는 여전히 없음(인스펙터·로그 위주).
- `AttackType` 물리/마법 세분 타입별 `KnockdownMultiplier` 테이블화는 미정.

### 넉다운 즉시 피격 모션·`SkillManager` 정합

#### 의도
- **넉다운(023002)만** 프리팹이 붙는 순간 “맞고 쓰러짐”이 보이게. **기절**은 예전과 같이 **행동불가 턴 스킵으로 토큰이 빠질 때** `ApplyPostStunReleaseHitHold`가 돌아가는 흐름 유지(부착 직시에는 모션 안 건드림).

#### 구현 요약
- `StatusEffectController.AddKnockdownEffect` 마지막에 `CharacterStats.ApplyImmediateKnockdownHitFeedback()` → 내부 `ApplyHitHoldPendingStandRecover()` (`pendingStandRecoverAfterStunConsume` + `CharacterMotionController.ApplyPostStunReleaseHitHold`).
- `SkillManager`: 연출 끝 타겟 `ResetMotion()`이 **위 즉시 피격을 Stand로 지우던 것**이 핵심 원인 → `ResetTargetMotionAfterSkillUnlessKnockdown()` 추가. **넉다운 토큰이 있을 때만** `ApplyPostStunReleaseHitHold`, 아니면 기존 `ResetMotion`. 경로: `PlaySkillEffect`, `PlayRangeSkillEffect`, `PlayAllAttackEffect`의 타겟 처리.
- `CharacterMotionController.ApplyPostStunReleaseHitHold` 주석: 넉다운 부착 직시(코드 경유) + 기절/넉다운 토큰 소모 직후 용도.

#### 작업 중 실수·되돌림 (본인 정리)
- **“스턴이랑 적용을 똑같이 하면 문제 없겠지”**라고 판단해, 넉다운 **즉시 피격**과 `SkillManager` 쪽 예외 처리를 **한번 싹 되돌림** → 화면에서는 여전히 **턴이 넘어가며 토큰 소모될 때만** 모션이 바뀌는 것처럼 보였음(즉시 피드백이 사라진 상태).
- 그때 **“피격 모션이 없어서 안 된다”**고 생각했던 부분도 있었는데, 원인은 **코드만이 아니라** (1) 위 `ResetMotion` 덮어쓰기, (2) 캐릭터별 **`Resources` 쪽 `Hit` 스프라이트 미배치**가 겹칠 수 있음 — 리소스 경로 `UnitSprite/{Sprite}/Hit` 존재 여부는 별도 확인.
- 이후 **즉시 피격을 다시 켠 뒤**, **기절에도 동일 적용**했다가 디자인상 **넉다운만 빠른 피드백**이 맞다고 해서 `AddStunEffect` 쪽 즉시 호출은 제거하고, API도 **`ApplyImmediateKnockdownHitFeedback`**(넉다운 전용 명칭)으로 정리.

#### 확인 메모
- KDP로 넉다운이 붙은 직후·스킬 연출이 끝난 뒤에도 피격 자세가 유지되는지, 다음 본인 턴 `ApplyPostStunTurnStartMotionRecover`와 충돌 없는지 플레이로 재확인 권장.

### VirtualMouse — 스킬 슬롯 호버 경로 정리
- 호버는 **`SkillInstance` / `SkillBlock`** 기준으로만 스킬 정보 패널을 띄우기로 정리. **`SkillSlot`을 통한 간접 감지**는 제거(빈 슬롯이 슬롯만 잡히던 문제·설계 의도 불일치 정리).
- 제거된 참고 지점: `DetectHoveredObject`의 `SkillSlot` 분기, `DetermineTargetPanel`의 `SkillSlot` 조건, `GetSkillDataFromObject`의 `SkillSlot.GetSkillData()` 조회.
- **보류 아이디어**: 빈 슬롯에서도 의미 있게 보이게 하려면 추후 **슬롯별 디폴트 스킬 ID fallback** 검토. (복원 시에는 위 지점 + **`skillData == null`이면 패널 미표시** 가드 절충 가능.)

### 광역 버프(`AllAllies` 등) 연출
- `Buff` 타입이 `PlayAllTargetSkillEffect`에서 공격 광역 분기로 들어가 **피격 모션**이 나오던 문제 → `Buff` 전용 **`PlayAllBuffEffect`** 경로 추가(버프 모션 쪽으로 분기).

### 테스트용 AI 패턴 `OpeningBuff`
- **`PatternType.OpeningBuff`** (`CharacterLoader` enum), **`BattleManager.AssignAIComponent`**에서 `OpeningBuffEnemyAIController` 부착.
- **첫 행동 1회**만 스킬 목록에서 `SkillType.Buff`·`IsUsable()`인 스킬을 우선 선택, 없으면 즉시 일반 랜덤과 동일하게 전환. 이후 턴부터는 랜덤.
- **`ChooseTarget`**: 마지막 선택 스킬이 버프/힐이면 `Me`·`AllAllies`·`Ally`일 때 **자기 자신**으로 고정해 테스트 재현성 확보.
- **`NamedCharacterCh1.xml`**: 모라(`001003`) `<Pattern>`을 **`OpeningBuff`**로 변경(테스트).

### 반사·피해감소(리액션) 확장
- **`StatusEffectData`**: `ReactionEffectMode`(무시/고정 반사/받은 피해 % 반사 등), `ReactionDamageReductionMode`(고정·% 감소), `reflectReducedAmountDirect`·`useReducedAmountAsReflectBase` 등 기획 플래그.
- **`StatusEffectInstanceReaction.OnTakeDamage`**: 피해자 기준으로 감쇠 적용 후 반사량 계산, 공격자에게 `TakeDamage(..., owner, isReflectedDamage: true)`. **`isReflectedDamage`면 재진입 차단**으로 무한 루프 방지.
- **`CharacterStats.TakeDamage`**: `attacker`·`isReflectedDamage` 인자 추가. 피해무시는 리액션이 `true` 반환 시 조기 종료.
- **`CharacterStats.AddStatusEffectPrefab`**: 타입이 `Buff`이더라도 **`reactionMode` / `reductionMode`가 설정된 경우**에는 **`StatusEffectInstanceReaction`**으로 붙이도록 분기(반사는 `Buff` 인스턴스만으로는 `OnTakeDamage`가 없음).
- **`StatusEffectInstanceBuff`**: 일부 스탯이 `float`인데 `int`로 캐스트하던 부분에서 **`InvalidCastException`** → 필드 타입에 맞게 반영하도록 수정.
- **`maxTriggerCount`**: `0`은 **횟수 무제한**으로 취급. `triggerCount == 0` 일괄 차단 가드는 **`maxTriggerCount > 0`일 때만** 적용하도록 수정.

### 데이터·테스트 해금(당일 작업 맥락)
- **`EnemyMobSkills.xml`**: `010015`·`010016` 광역 버프 형태로 조정, **`GameProgressManager.SetupTestInventory`**에서 해당 스킬 임시 해금(번호는 세션 중 `011003` 등과 바꿔가며 검증).
- **`BossSkills.xml`**: `011005` 자가 버프(효과·지속·밸류) 조정 시도.
- **XML 수동 편집이 안 먹는 것처럼 보일 때**: 플레이 모드 캐시·상속 스킬·중복 ID·**세이브에 남은 해금 스킬** 등이 겹칠 수 있음 — 당일에도 `UnlockSkill`/세이브와 착시가 여러 번 이슈로 올라옴.

### 미해결 / 다음에 볼 것
- **반사 피해가 실제 전투에서 기대대로 들어가지 않는 현상** 남음(공격 경로에서 `attacker` 미전달·SO 설정·UI 표시 등 추가 확인 필요).

---

## 2026-04-07 — 저장 슬롯 정책 고정·힐 스킬 인포·주인공 보장 복구

### 저장/로드 정책 정리 (재분리 방지)
- `StageManager`의 스테이지 진행 저장 파일을 **슬롯 기준**으로 고정:
  - `stage_progress_slot_{slot}.json`
- `GameProgressManager.SetCurrentSlot()`에서 슬롯 전환 시
  - `StageManager.LoadStageProgress()`를 즉시 호출하도록 연결.
- 코드 주석으로 저장 방침 명시:
  - 진행도는 슬롯 기준으로만 저장/로드
  - `UserSettings`는 별도 저장소 유지
  - 단일 공용 `stage_progress.json` 구조로 회귀 금지

### UI 버그 수정 — 힐 스킬 값 미표시
- 증상: 스킬 인포(`VirtualMouseSkillPanel`)에서 힐 스킬이 `-`로 보이며 값 반영이 안 됨.
- 원인: 패널 수치 표시가 `DamageMin/Max`만 읽고 있었음.
- 조치:
  - 힐 타입(`SkillType.Heal` 또는 `HealMin/Max` 존재)일 때 `HealMin/HealMax`를 우선 표시하도록 수정.
  - 비힐 스킬은 기존처럼 `DamageMin/DamageMax` 표시 유지.

### 진행 불가 이슈 복구 — 주인공 `000001` 누락
- 증상: 임시 캐릭터 지급 제거 후 주인공까지 빠져 진행 불가.
- 조치:
  - `GameProgressManager`에 `MAIN_CHARACTER_ID = "000001"` 상수 추가.
  - `EnsureMainCharacterForSlot(slot, saveImmediately)` 추가:
    - `unlockedCharacters`에 `000001` 강제 보장
    - `characterInventory`에 `000001` 미존재 시 DB(`CharacterData.characterDict`)에서 복제 추가
  - 적용 시점:
    - `LoadGameProgress()` 직후 1차 보정
    - `Initialize()`에서 2차 보정(데이터 로드 순서 이슈 대비)

### 확인 메모
- 린트는 신규 오류 없이, 기존 `BlessingManager` 미해결 참조 오류만 유지.
- 다음 권장 보강:
  - `RemoveCharacterFromInventory()`에서 `000001` 삭제 차단 가드 추가 검토.

---

## 2026-04-08 — 스킬 슬롯/쿨다운 버그 수정 + 도발·지목 우선순위 설계/구현

### 전투/세팅 버그 수정
- **스킬 재배치 중복 버그**
  - 증상: 1번 슬롯 스킬을 3번으로 옮기면 전투 UI에서 동일 스킬이 중복으로 남는 현상.
  - 원인: 이동 시 이전 슬롯 `currentSkillID` 정리가 누락.
  - 조치: `SkillSlot.PlaceSkillBlock()`에서 이전 슬롯 참조/ID/UI/`SetPlayerSkill`을 먼저 비움 처리.

- **빈 슬롯 기본기 보장 복구**
  - 증상: 재배치 후 빈 칸이 전투로 전달되어 검은 원(빈 슬롯) 발생.
  - 조치:
    - `BattleSettingManager`에 기본기 배열 상수화(`010001~010004`) 및 `GetDefaultSkillForSlot()` 추가.
    - 슬롯 제거/이동 시 빈 칸 대신 슬롯별 기본기로 즉시 복구.
    - `SetPlayerSkill()`에서 빈 문자열 입력 시 기본기로 자동 치환.

- **스킬 쿨다운 1턴 복귀 미동작**
  - 증상: `Cooldown=1` 스킬 사용 후 다음 자기 턴에도 계속 사용 불가.
  - 원인: `CurrentCooldown` 증가만 있고 턴 시작 감소 루프 부재.
  - 조치: `TurnManager.StartTurn()`에서 `TickSkillCooldownOnTurnStart()` 호출 추가(중복 ID는 1회만 감소).
  - 보강: `SkillInstance.UseSkill()` 검증을 로컬 변수 대신 `skillData.IsUsable()` 기준으로 통일.

### 상태이상/호버 점검
- 넉다운 호버 차단 점검 결과:
  - 코드상 넉다운(`StatusEffectInstanceKnockdown`)만 제외 처리.
  - 단, 데이터 점검에서 `StatusEffect_023002_Knockdown.asset` 내부 `EffectID`가 `029001`로 불일치 상태를 확인(후속 정합 필요).

### 데이터 수정
- `EnemyMobSkills.xml`
  - `010006(방어)`에 `021007` 효과 연결.
  - 효과 형식을 상세형으로 변경:
    - `EffectID=021007`, `Value=7`, `Duration=1`.

### 도발/지목 시스템 확장 (튜토리얼 데이터 완결 대비)
- **도발 기본 방향**
  - 버프 기반 표식으로 처리.
  - 일반 도발은 랜덤/광역 강제 보호를 하지 않도록 분기.
  - 확장 도발(완전 보호형)은 체크박스로 켜는 구조 준비.

- **`StatusEffectData` 확장**
  - 도발/지목 관련 플래그 추가:
    - `blockOtherAlliesAsTarget`
    - `tauntAffectsRandomTargeting`
    - `tauntProtectsAgainstAoE`
    - `markPriorityTarget`
    - `markDamageTakenMultiplier`(기본 1.1)

- **AI 타겟 우선순위**
  - `EnemyAIController` 공통 후보 필터에 우선순위 반영:
    - `지목(mark) > 도발(taunt) > 기본 후보`
  - `Default/Adelia/OpeningBuff` 타겟 선택 경로에 공통 필터 적용.
  - 도발 다중 대상일 때는 각 AI 고유 로직(랜덤/주인공 보호/패턴 우선)으로 최종 선택.

- **지목 피해 배율**
  - `CharacterStats.TakeDamage()`에 지목 배율 적용 추가.
  - `StatusEffectController.GetHighestMarkDamageTakenMultiplier()`로 대상의 지목 디버프 배율을 조회해 최종 피해에 반영.

- **Debuff 프리팹 경로 정리**
  - `StatusEffectType.Debuff`도 일반적으로 `AddBuffEffect` 경로 재사용하도록 연결.
  - 리액션 설정이 있는 특수 케이스만 `AddReactionEffect`로 분기.

### 결정/합의 메모
- 유저 조작 타겟팅은 자유 유지(강제 차단 없음).
- 도발은 장기적으로 확장 가능하되, 현재는 일반형(랜덤/광역 보호 없음) 기준.
- 지목은 도발 파훼 + 장시간 디버프 컨셉(예: 5턴)으로 운용.