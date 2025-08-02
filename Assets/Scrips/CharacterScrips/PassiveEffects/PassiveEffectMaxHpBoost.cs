using UnityEngine;

public class PassiveEffectMaxHpBoost : PassiveEffectBase
{
    /// <summary>
    /// 최대 체력을 증가시키는 패시브 효과를 적용합니다.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="data">패시브 데이터</param>
    public override void Apply(CharacterStats character, PassiveData data)
    {
        if (character == null || data == null) return;

        // 최대 체력 증가
        int hpIncrease = data.value;
        character.MaxHp += hpIncrease;
        
        // 현재 체력도 같은 비율로 증가 (체력 비율 유지)
        float healthRatio = (float)character.Hp / (character.MaxHp - hpIncrease);
        character.Hp = Mathf.RoundToInt(character.MaxHp * healthRatio);
        
        // UI 업데이트
        if (character.HpUI != null)
        {
            character.HpUI.UpdateHpBar(character.Hp, character.MaxHp);
        }

        Debug.Log($"[패시브] {character.Label}의 최대 체력이 {hpIncrease} 증가했습니다. (현재: {character.MaxHp})");
    }
} 