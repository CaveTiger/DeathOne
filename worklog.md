# 작업 로그

## 2024-05-23
### 작업 로그 시스템 구축
- 파일 위치: worklog.md
- 작업 내용: 작업 로그 시스템 파일 생성
- 변경 사항:
  - 작업 로그 파일 생성
  - 마크다운 형식의 로그 템플릿 작성
  - cosurrules에서 참고할때 이곳을 체크 작업내용은 큰 카테고리이며 변경사항이 자잘한 변경사항에 해당함
- 참고 사항: .cursorrules 파일과 동일 위치에 생성

### CharacterInfoPlayer.cs
- 파일 위치: Assets/Scripts/UI/CharacterInfoPlayer.cs
- 작업 내용: UI 갱신 기능 구현
- 변경 사항:
  - UpdateInfo() 메서드 구현
  - ShowInfo() 메서드 구현
  - CharacterInfo 클래스 상속 구조 활용
- 참고 사항: UI 갱신 시스템의 기본 구조 설계

### 상태이상 시스템 리팩토링
- 파일 위치: 
  - Assets/Scripts/StatusEffect/StatusEffectInstanceReaction.cs
  - Assets/Scripts/StatusEffect/StatusEffectNoDamageBuffData.cs
- 작업 내용: 상태이상 시스템 구조 개선
- 변경 사항:
  - 리액션 타입 상태이상 분리
  - 무적(피해무시) 효과 구현
  - 상태이상 데이터 구조화
- 참고 사항: 
  - EffectID 체계 정립 (021002: 피해무시)
  - 리액션 시스템 확장성 확보
  - 상태이상 시스템의 기본 구조 설계

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

## 2025-05-29
### UI 투명화 및 카메라 연출 개선
- BattleUIManager에 CanvasGroup을 활용한 UI 투명화/복구 기능 추가
- UI 활성/비활성 대신 알파값으로 자연스럽게 연출
- Start에서 battleUI를 미리 투명하게 설정
- BattleCamera에 CameraZoom 코루틴 구현 및 적용
- SkillManager의 UseSkillCoroutine에서 카메라 줌인/줌아웃 연출 삽입
- 카메라 줌 코루틴 중첩 문제 발견, 내일 개선 예정
- 전투 연출 및 UI/카메라 타이밍 개선, 연출 순서 점검 및 구조 정비

## 2025-05-29 타임라인 시스템 구현
- TimeLineBlock 클래스 구현
  - 행동 기록(TimelineActionLog)과 상태 스냅샷(BattleSnapshot) 저장
  - 마우스 호버 시 상세 정보 표시 기능
  - 시간 되돌리기 관련 필드 추가 (MyBlock, MyTurn, ReturnOnline, ReturnCount)
- TimeLinePopupUI 클래스 구현
  - 타임라인 블록의 상세 정보를 팝업으로 표시
  - 싱글톤 패턴으로 구현하여 전역 접근 가능
- 향후 구현 예정
  - 시간 되돌리기(롤백) 기능 구현
  - 타임라인 UI 연출 개선
  - 상태 스냅샷 저장 및 복원 시스템 구현

## 2025-05-30 전투 연출 및 피격 로직 개선
- SkillManager에서 공격 모션과 데미지 처리 타이밍 분리 논의
- CharacterStats의 HitEffect() 코루틴에서 색상 변화만 담당하도록 단순화
- 피격 연출(빨간색 변화)을 CharacterMotionController의 PlayHitMotion()에서 처리하도록 이동
- PlayHitMotion()에서 스프라이트를 Hit으로 변경하고 동시에 빨간색으로 변경하도록 수정
- ResetMotion()에서 스탠드 스프라이트 복귀와 함께 색상도 Color.white로 원상복구하도록 개선
- 코루틴 구조를 활용해 연출 타이밍을 자연스럽게 제어할 수 있도록 설계 방향 확정
- 전체적으로 코루틴 중첩 대신 모션 컨트롤러에서 연출을 통합 관리하는 구조로 리팩터링 방향 설정

## 2025-05-31 월드맵 스테이지 선택 UI 및 입력 충돌 개선
- 월드맵에서 스테이지 아이콘 클릭 시 카메라 줌인/아웃 및 UI 연동 구조 점검
- UI가 열린 상태에서 월드맵 아이콘이 중복 클릭되는 문제 발견 및 원인 분석
- Raycast Target이 켜진 보라색 배경 패널을 활용해 UI 위 클릭 차단 적용
- 각 스테이지 아이콘에 isSelected, currentSelected(static) 필드 도입으로 정확히 선택된 버튼만 동작하도록 개선
- UI가 열려 있을 때는 다른 아이콘 클릭 무시, 선택된 아이콘만 취소 가능하게 로직 분기
- UI 열기/닫기, 카메라 연출, 선택 해제 등 전체 흐름을 자연스럽게 리팩터링
- 입력 충돌, UI/버튼 중첩 문제 등 실전 플레이에서 발생할 수 있는 UX 이슈를 집중 점검 및 개선

### 내일 작업 예정
- 월드맵 스테이지 버튼 중첩 문제 추가 개선(완전한 중복 방지 및 UX 보완)
- 스테이지 진입 시 컷신(연출) 시스템 설계 및 구현(스테이지 부분에서 다뤄질것)

레이어 잠금기능을 알아볼것

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
- 코드/구조 단순화 및 유지보수성 향상 목적의 구조 개선 진행

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

## 2025-06-22 파티 세팅 UI 캐릭터 블록 생성 오류 해결 및 초기화 로직 정립

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

--- 