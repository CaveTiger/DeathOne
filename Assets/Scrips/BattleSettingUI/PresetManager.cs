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
    private static readonly string[] FallbackSkillIDs = { "010001", "010002", "010003", "010004" };

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
        var normalized = NormalizeSkillIDs(currentSkillIDs);
        for (int i = 0; i < 4; i++)
            defaultPreset.skillIDs[i] = normalized[i];
        Debug.Log("[PresetManager] 디폴트 프리셋(최근 사용) 자동 저장: " + string.Join(", ", defaultPreset.skillIDs));
    }

    /// <summary>
    /// 세팅 UI 오픈 시 디폴트 프리셋의 스킬ID 배열 반환
    /// </summary>
    public string[] GetDefaultPresetSkills()
    {
        return (string[])NormalizeSkillIDs(defaultPreset.skillIDs).Clone();
    }

    /// <summary>
    /// 현재 세이브 데이터(savedSkillPreset)에 최근 프리셋을 기록한다.
    /// 저장 파일 플러시는 호출자가 담당한다.
    /// </summary>
    public void WriteRecentPresetToCurrentSave(string[] currentSkillIDs)
    {
        var normalized = NormalizeSkillIDs(currentSkillIDs);
        UpdateDefaultPreset(normalized);

        if (GameProgressManager.Instance == null || GameProgressManager.Instance.CurrentSaveData == null)
            return;

        GameProgressManager.Instance.CurrentSaveData.savedSkillPreset = (string[])normalized.Clone();
    }

    /// <summary>
    /// 현재 세이브의 최근 프리셋을 읽는다. 없으면 디폴트 프리셋을 반환한다.
    /// </summary>
    public string[] ReadRecentPresetFromCurrentSaveOrDefault()
    {
        if (GameProgressManager.Instance != null && GameProgressManager.Instance.CurrentSaveData != null)
        {
            var saved = GameProgressManager.Instance.CurrentSaveData.savedSkillPreset;
            if (saved != null && saved.Length == 4)
            {
                var normalizedSaved = NormalizeSkillIDs(saved);
                UpdateDefaultPreset(normalizedSaved);
                return normalizedSaved;
            }
        }

        return NormalizeSkillIDs(defaultPreset.skillIDs);
    }

    public string[] NormalizeSkillIDs(string[] source)
    {
        string[] normalized = new string[4];
        for (int i = 0; i < 4; i++)
        {
            string value = (source != null && i < source.Length) ? source[i] : "";
            normalized[i] = string.IsNullOrEmpty(value) ? FallbackSkillIDs[i] : value;
        }
        return normalized;
    }

    // 추후: 프리셋 추가/삭제/불러오기/이름변경 등 확장 가능
} 