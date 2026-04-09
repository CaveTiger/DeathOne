using System;
using System.IO;
using UnityEngine;

/// <summary>
/// 유저 공용 설정(사운드 등) 저장/로드 매니저.
/// GameManager 오브젝트에 함께 부착해서 사용한다.
/// </summary>
public class UserSettingsManager : MonoBehaviour
{
    public static UserSettingsManager Instance { get; private set; }

    [Serializable]
    public class UserSettingsData
    {
        public string settingsVersion = "1.0.0";
        public float masterVolume = 1f;
        public float bgmVolume = 0.8f;
        public float seVolume = 1f;
        public float voiceVolume = 1f;
        public bool mute = false;
    }

    private const string SETTINGS_FILE_NAME = "user_settings.json";
    private UserSettingsData currentSettings = new UserSettingsData();

    private string GetSettingsPath()
    {
        return Path.Combine(Application.persistentDataPath, SETTINGS_FILE_NAME);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadSettings();
    }

    private void Start()
    {
        ApplySettingsToSoundManager();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveSettings();
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
    }

    public UserSettingsData GetCurrentSettings()
    {
        return currentSettings;
    }

    public void LoadSettings()
    {
        string path = GetSettingsPath();
        if (!File.Exists(path))
        {
            currentSettings = new UserSettingsData();
            SaveSettings();
            return;
        }

        try
        {
            string json = File.ReadAllText(path);
            currentSettings = JsonUtility.FromJson<UserSettingsData>(json);
            if (currentSettings == null)
                currentSettings = new UserSettingsData();
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[UserSettingsManager] 설정 로드 실패. 기본값 사용: {e.Message}");
            currentSettings = new UserSettingsData();
        }
    }

    public void SaveSettings()
    {
        try
        {
            string json = JsonUtility.ToJson(currentSettings, true);
            File.WriteAllText(GetSettingsPath(), json);
        }
        catch (Exception e)
        {
            Debug.LogWarning($"[UserSettingsManager] 설정 저장 실패: {e.Message}");
        }
    }

    public void ApplySettingsToSoundManager()
    {
        if (SoundManager.Instance == null) return;

        SoundManager.Instance.SetMasterVolume(currentSettings.masterVolume);
        SoundManager.Instance.SetBgmVolume(currentSettings.bgmVolume);
        SoundManager.Instance.SetSeVolume(currentSettings.seVolume);
        SoundManager.Instance.SetVoiceVolume(currentSettings.voiceVolume);
        SoundManager.Instance.SetMute(currentSettings.mute);
    }

    // UI OnValueChanged 연결용 API
    public void SetMasterVolume(float value)
    {
        currentSettings.masterVolume = Mathf.Clamp01(value);
        if (SoundManager.Instance != null) SoundManager.Instance.SetMasterVolume(currentSettings.masterVolume);
        SaveSettings();
    }

    public void SetBgmVolume(float value)
    {
        currentSettings.bgmVolume = Mathf.Clamp01(value);
        if (SoundManager.Instance != null) SoundManager.Instance.SetBgmVolume(currentSettings.bgmVolume);
        SaveSettings();
    }

    public void SetSeVolume(float value)
    {
        currentSettings.seVolume = Mathf.Clamp01(value);
        if (SoundManager.Instance != null) SoundManager.Instance.SetSeVolume(currentSettings.seVolume);
        SaveSettings();
    }

    public void SetVoiceVolume(float value)
    {
        currentSettings.voiceVolume = Mathf.Clamp01(value);
        if (SoundManager.Instance != null) SoundManager.Instance.SetVoiceVolume(currentSettings.voiceVolume);
        SaveSettings();
    }

    public void SetMute(bool value)
    {
        currentSettings.mute = value;
        if (SoundManager.Instance != null) SoundManager.Instance.SetMute(value);
        SaveSettings();
    }
}
