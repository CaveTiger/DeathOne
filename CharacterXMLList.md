# 캐릭터 XML 데이터 목록표

## 캐릭터 데이터
| ID | 이름 | 설명 | 타입 | 희귀도 | 스프라이트 경로 | XML 파일 |
|----|------|------|------|--------|----------------|----------|
| 000001 | 오드 | 어둠의 신자 | 플레이어 | One | UnitSprite/Player | PlayerCharacter.xml |
| 000002 | 스텔리 | 설명 영혼의 기억에서 확인 가능 | 플레이어 | Rare | UnitSprite/Stelly | NamedCharacterCh1.xml |
| 000003 | 호송병 | 설명 영혼의 기억에서 확인 가능 | 일반 | Normal | UnitSprite/MobA | MobCharacter.xml |
| 000004 | 귀족 신참병 | 설명 영혼의 기억에서 확인 가능 | 일반 | Normal | UnitSprite/MobB | MobCharacter.xml |
| 000005 | 아델리아 | 설명 영혼의 기억에서 확인 가능 | 일반 | Normal | UnitSprite/MobA/MobA_Stand | NamedCharacterCh1.xml |

## ID 체계
- 000000~009999: 일반 적 아군화가능
- 001000~001999: 일방적 적 아군화 불가능
- 000000~002999: 특수 적

## 공통 구조
- Stats: Hp, Atk, Def, Evasionrate, Accuracy, Speed
- Skills: 4개의 스킬 ID
- Passives: 패시브 스킬 (현재 비어있음)
- Pattern: 행동 패턴 (현재 모두 Normal)
- Sprite: 스프라이트 경로

--- 