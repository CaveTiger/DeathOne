# 스킬 XML 데이터 목록표

## 스킬 데이터
| ID | 이름 | 타입 | 데미지 | 효과 | 대상 | 모션 | XML 파일 |
|----|------|------|--------|------|------|------|----------|
| 010000 | 스킬 | Damage | 0 | - | All | Stand | PlayerSkills.xml |
| 010001 | 단검베기 | Damage | 9-16 | - | Enemy | Attack | PlayerSkills.xml |
| 010002 | 발목 노리기 | Damage | 4-9 | 독(020001) 2턴 | Enemy | Attack | PlayerSkills.xml |
| 010003 | 여신의 축복 | Buff | - | 힘강화(021001) 2턴<br>무적(021002) 2턴 | Me | Buff | PlayerSkills.xml |
| 010004 | 주변 살피기 | - | - | 020005 | Me | Buff | PlayerSkills.xml |
| 010005 | 기초적인 검격 | Damage | 5-8 | - | Enemy | Attack | EnemyMobSkills.xml |
| 010006 | 방어 | - | 0 | - | Me | Buff | EnemyMobSkills.xml |
| 010007 | 참수 | Damage | 1-18 | 020004 | Enemy | Attack | EnemyMobSkills.xml |
| 010008 | 지목 | - | - | 020005 | Enemy | Buff | EnemyMobSkills.xml |
| 010009 | 성실히 갈아낸 날 | Damage | 3-7 | 020005 | Enemy | Attack | EnemyMobSkills.xml |
| 010010 | 녹슨 날 | Damage | 3-7 | 020005 | Enemy | Attack | EnemyMobSkills.xml |
| 010011 | 시험해보기 | Damage | 2-4 | - | Enemy | Attack | EnemyMobSkills.xml |
| 010012 | 상처 찢기 | Damage | 3-7 | 020005 | Enemy | Attack | EnemyMobSkills.xml |
| 010013 | 찌부러트리기 | Damage | 5-15 | 020005 | Enemy | Thump | EnemyMobSkills.xml |
| 010014 | 길잡이 나침반 | Buff | - | 020005 | Me | Buff | EnemyMobSkills.xml |

## 공통 구조
- ParentID: 상속받은 기본 스킬 ID
- Icon: 스킬 아이콘 경로
- AttackPoint: 공격 부위 (Body, Head, Foot)
- AttackEffect: 공격 이펙트 (Cut, Buff, None)
- ActiveCount: 사용 횟수 제한
- ManaCost: 마나 소모량
- CoolTime: 쿨다운 턴
- priority: 우선순위

## 연관 상태이상 목록 (ScriptableObject)
| ID | 이름 | 타입 | 설명 |
|----|------|------|------|
| 020001 | 출혈 | 피해디버프 | 턴당 데미지 |
| 020002 | 중독 | 피해디버프 | 2턴당 데미지 |
| 020003 | 화상 | 피해디버프 | 턴당 주변 대상 0.5배 피해 |
| 021001 | Def 강화 | 버프 | 방어도 증가 |
| 021002 | 무적 | 버프 | 피해 무시 |

## 상태이상 ID 체계 (ScriptableObject)
- 020000~020999: 피해 상태이상
- 021000~021999: 버프 상태이상

--- 