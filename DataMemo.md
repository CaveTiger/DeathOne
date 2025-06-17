# 데이터 관리 메모장

## 타임라인 시스템 데이터 구조

### TimelineActionLog
- TurnNumber: int (턴 번호)
- ActorName: string (행동자 이름)
- ActionType: string (행동 타입 - "Attack", "Skill", "Heal" 등)
- TargetName: string (대상 이름)
- Value: int (데미지/회복량 등)
- StatusEffect: string (상태이상 이름)

### BattleSnapshot
- TurnNumber: int (턴 번호)
- AllCharacterStats: List<CharacterStatsSnapshot> (모든 캐릭터 상태)
  - HP, MaxHP, Atk, Def 등 기본 스탯
  - 상태이상 목록
  - 버프/디버프 목록

### TimeLineBlock
- actionLog: TimelineActionLog (행동 기록)
- snapshot: BattleSnapshot (상태 스냅샷)
- MyBlock: bool (플레이어가 만든 블록인지)
- MyTurn: bool (플레이어 턴인지)
- ReturnOnline: bool (시간 되돌리기 가능 여부)
- ReturnCount: int (남은 시간 되돌리기 횟수)

## 상태이상 ID 체계
- 020000: 일반 공격성 상태이상
- 021000: 버프성 상태이상
- 022000: 디버프성 상태이상
- 023000: 행동불가 상태이상
- 024000: 특수기술 상태이상

## 폴더 구조
- Assets/Scripts/UI: UI 관련 스크립트
- Assets/Scripts/Battle: 전투 시스템 스크립트
- Assets/Scripts/StatusEffect: 상태이상 관련 스크립트
- Assets/Resources/Data: XML 데이터 파일
- Assets/Prefabs: 프리팹 파일
- Assets/Resources: 리소스 파일

## 빌드 시 주의사항
- DataMemo.md는 빌드 시 프로젝트 외부로 이동
- XML 데이터 파일은 Resources 폴더 내 유지
- 프리팹과 리소스는 빌드에 포함

--- 