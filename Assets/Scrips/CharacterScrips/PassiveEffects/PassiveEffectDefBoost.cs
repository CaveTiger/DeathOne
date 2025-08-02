using UnityEngine;

public class PassiveEffectDefBoost : PassiveEffectBase
{
    /// <summary>
    /// 방어력을 증가시키는 패시브 효과를 적용합니다.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="data">패시브 데이터</param>
    public override void Apply(CharacterStats character, PassiveData data)
    {
        if (character == null || data == null) return;

        // 방어력 증가
        int defIncrease = data.value;
        character.Def += defIncrease;

        Debug.Log($"[패시브] {character.Label}의 방어력이 {defIncrease} 증가했습니다. (현재: {character.Def})");
    }
} 