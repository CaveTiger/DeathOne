using UnityEngine;

public abstract class PassiveEffectBase
{
    /// <summary>
    /// 패시브 효과를 캐릭터에 적용하는 메서드. 상속받아 구현.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="data">패시브 데이터</param>
    public abstract void Apply(CharacterStats character, PassiveData data);

    /// <summary>
    /// 해당 유닛에게 턴이 돌아와 상태이상 정산 등이 끝난 뒤, 행동 UI/AI 진입 직전에 호출됩니다.
    /// (<see cref="CharacterStats.InvokePassivesOnOwnerTurnStart"/> ← <see cref="TurnManager"/>) 새 턴 연동 패시브는 여기서 처리.
    /// </summary>
    public virtual void OnOwnerTurnStart(CharacterStats character, PassiveData data) { }
} 