using UnityEngine;

public class PassiveEffectManaBoost : PassiveEffectBase
{
    /// <summary>
    /// 마나 시스템을 활성화하고 마나를 부여하는 패시브 효과를 적용합니다.
    /// </summary>
    /// <param name="character">효과를 적용할 캐릭터</param>
    /// <param name="data">패시브 데이터</param>
    public override void Apply(CharacterStats character, PassiveData data)
    {
        if (character == null || data == null) return;

        // 마나 시스템 활성화
        if (data.grantsMana)
        {
            // CharacterStats에 마나 필드가 있다면 여기서 설정
            // 현재는 CharacterData에만 마나 필드가 있으므로 data를 통해 관리
            Debug.Log($"[패시브] {character.Label}에게 마나 시스템이 활성화되었습니다. (최대 마나: {data.maxMana})");
        }

        // 마나 관련 효과 적용
        if (data.maxMana > 0)
        {
            Debug.Log($"[패시브] {character.Label}의 최대 마나가 {data.maxMana}로 설정되었습니다.");
        }

        if (data.manaRegenInterval > 0)
        {
            Debug.Log($"[패시브] {character.Label}는 {data.manaRegenInterval}턴마다 1 마나를 회복합니다.");
        }
    }
} 