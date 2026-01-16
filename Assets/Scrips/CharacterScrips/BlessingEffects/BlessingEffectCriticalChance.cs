using UnityEngine;

/// <summary>
/// 크리티컬 확률을 증가시키는 축복 효과 예시
/// </summary>
public class BlessingEffectCriticalChance : BlessingEffectBase
{
    /// <summary>
    /// 크리티컬 확률을 증가시키는 축복 효과를 적용합니다.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="blessingData">축복 데이터</param>
    public override void Apply(CharacterStats character, BlessingData blessingData)
    {
        if (character == null || blessingData == null) return;

        // 축복 데이터의 value를 사용하여 크리티컬 확률 증가
        // 예시: CharacterStats에 CriticalChance 필드가 있다고 가정
        // character.CriticalChance += blessingData.value;
        
        Debug.Log($"[축복] {character.Label}에 크리티컬 확률 증가 효과가 적용되었습니다. (축복: {blessingData.blessingName})");
    }
}










