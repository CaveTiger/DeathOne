using UnityEngine;

/// <summary>
/// 축복 효과의 기본 추상 클래스. 모든 축복 효과는 이 클래스를 상속받아 구현합니다.
/// </summary>
public abstract class BlessingEffectBase
{
    /// <summary>
    /// 축복 효과를 캐릭터에 적용하는 메서드. 상속받아 구현.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="blessingData">축복 데이터</param>
    public abstract void Apply(CharacterStats character, BlessingData blessingData);
}

