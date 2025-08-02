using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// SkillSet 오브젝트에 부착, 자식 SkillSlot 4개를 자동 인식하여 프리셋 저장/불러오기/적용을 담당
/// </summary>
public class SkillPresetHandler : MonoBehaviour
{
    [Header("스킬 슬롯(순서 보장)")]
    public List<SkillSlot> skillSlots = new List<SkillSlot>();

    // Inspector에서 직접 할당하므로 Awake의 자동 할당 코드는 제거

    /// <summary>
    /// SkillPreset 데이터를 받아 슬롯에 적용
    /// </summary>
    public void ApplyPreset(SkillPreset preset)
    {
        if (preset == null || preset.skillIDs == null || preset.skillIDs.Length != 4) return;
        for (int i = 0; i < skillSlots.Count && i < preset.skillIDs.Length; i++)
        {
            skillSlots[i].SetSkill(preset.skillIDs[i]);
        }
        Debug.Log($"[SkillPresetHandler] 프리셋 적용: {string.Join(", ", preset.skillIDs)}");
    }

    /// <summary>
    /// 현재 슬롯 상태를 SkillPreset으로 반환
    /// </summary>
    public SkillPreset GetCurrentPreset(string presetName = "최근 사용")
    {
        var preset = new SkillPreset();
        preset.presetName = presetName;
        preset.skillIDs = new string[4];
        for (int i = 0; i < skillSlots.Count && i < 4; i++)
        {
            preset.skillIDs[i] = skillSlots[i].GetSkillID();
        }
        return preset;
    }

    /// <summary>
    /// 현재 슬롯에 세팅된 모든 스킬ID를 리스트로 반환 (비어있지 않은 것만)
    /// </summary>
    public List<string> GetUsedSkillIDs()
    {
        return skillSlots
            .Select(slot => slot.GetSkillID())
            .Where(id => !string.IsNullOrEmpty(id))
            .ToList();
    }

    /// <summary>
    /// PresetManager의 디폴트 프리셋을 슬롯에 적용
    /// </summary>
    public void ApplyDefaultPreset()
    {
        if (PresetManager.Instance != null)
        {
            ApplyPreset(PresetManager.Instance.defaultPreset);
        }
    }

    /// <summary>
    /// 현재 상태를 PresetManager의 디폴트 프리셋에 저장
    /// </summary>
    public void SaveToDefaultPreset()
    {
        if (PresetManager.Instance != null)
        {
            var preset = GetCurrentPreset("최근 사용");
            PresetManager.Instance.UpdateDefaultPreset(preset.skillIDs);
        }
    }
} 