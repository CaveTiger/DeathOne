using UnityEngine;

public abstract class PassiveEffectBase
{
    /// <summary>
    /// 패시브 효과를 캐릭터에 적용하는 메서드. 상속받아 구현.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="data">패시브 데이터</param>
    public abstract void Apply(CharacterStats character, PassiveData data);
} 