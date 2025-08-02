# 데이터 메모

## 스킬 ID 체계
- 010000~010999: 일반 스킬 (플레이어 + 몬스터)
- 011000~011999: 보스 스킬
- 012000~012999: 특수 스킬 (예약)

## 능동형 아이콘 시스템 (Dynamic Icon System)

### 개요
상태이상 효과의 값(양수/음수)에 따라 다른 아이콘을 표시하는 시스템입니다.

### 작동 원리
- **0 이상 값**: 기본/버프 아이콘 (Up 접미사)
- **음수 값**: 디버프 아이콘 (Down 접미사)

### 아이콘 파일 명명 규칙
```
기본 경로: StatusEffect/GuardPower
0 이상 (기본/버프): StatusEffect/GuardPowerUp
음수 (디버프): StatusEffect/GuardPowerDown
```

### 구현된 통합 상태이상
- **021001**: 방어력 변화 (GuardPower)
- **021002**: 공격력 변화 (AttackPower)
- **021003**: 속도 변화 (Speed)
- **021004**: 체력 변화 (Hp)

### 사용 예시
```csharp
// 방어력 +2 버프
StatusEffectData effect = StatusEffectManager.Instance.GetById("021001");
instance.Initialize(effect, 3, 2, target); // 양수 값으로 버프 아이콘 표시

// 방어력 -1 디버프
StatusEffectData effect = StatusEffectManager.Instance.GetById("021001");
instance.Initialize(effect, 2, -1, target); // 음수 값으로 디버프 아이콘 표시
```

### 장점
1. **통합 관리**: 하나의 상태이상으로 버프/디버프 모두 처리
2. **시각적 명확성**: 값에 따라 직관적인 아이콘 표시
3. **확장성**: 새로운 스탯 변화도 쉽게 추가 가능
4. **코드 효율성**: 중복 코드 감소 