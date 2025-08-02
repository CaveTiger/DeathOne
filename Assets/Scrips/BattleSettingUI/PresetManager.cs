using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 스킬 프리셋 데이터 구조
/// </summary>
[System.Serializable]
public class SkillPreset
{
    public string presetName;
    public string[] skillIDs = new string[4];
    // 필요시 파티, 장비 등 추가 가능
}

/// <summary>
/// 프리셋 전체를 관리하는 매니저 (싱글톤)
/// </summary>
public class PresetManager : MonoBehaviour
{
    public static PresetManager Instance { get; private set; }

    [Header("항상 존재하는 디폴트 프리셋 (최근 사용)")]
    public SkillPreset defaultPreset = new SkillPreset { presetName = "최근 사용" };

    [Header("유저가 직접 저장한 프리셋 리스트")]
    public List<SkillPreset> userPresets = new List<SkillPreset>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    /// <summary>
    /// 스킬 세팅이 바뀔 때마다 디폴트 프리셋에 자동 저장
    /// </summary>
    public void UpdateDefaultPreset(string[] currentSkillIDs)
    {
        for (int i = 0; i < 4; i++)
            defaultPreset.skillIDs[i] = currentSkillIDs[i];
        Debug.Log("[PresetManager] 디폴트 프리셋(최근 사용) 자동 저장: " + string.Join(", ", defaultPreset.skillIDs));
    }

    /// <summary>
    /// 세팅 UI 오픈 시 디폴트 프리셋의 스킬ID 배열 반환
    /// </summary>
    public string[] GetDefaultPresetSkills()
    {
        return defaultPreset.skillIDs;
    }

    // 추후: 프리셋 추가/삭제/불러오기/이름변경 등 확장 가능
} 