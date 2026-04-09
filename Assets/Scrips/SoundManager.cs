using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 전역 사운드 매니저.
/// GameManager 오브젝트에 함께 부착해 씬 전환 간 유지해서 사용한다.
/// </summary>
public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    [Header("Audio Sources")]
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private AudioSource seSource;
    [SerializeField] private AudioSource voiceSource;

    [Header("Volumes")]
    [Range(0f, 1f)] [SerializeField] private float masterVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float bgmVolume = 0.8f;
    [Range(0f, 1f)] [SerializeField] private float seVolume = 1f;
    [Range(0f, 1f)] [SerializeField] private float voiceVolume = 1f;
    [SerializeField] private bool isMuted = false;

    private readonly Dictionary<string, AudioClip> clipCache = new Dictionary<string, AudioClip>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        EnsureAudioSources();
        ApplyVolumes();
    }

    private void EnsureAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.playOnAwake = false;
            bgmSource.loop = true;
        }

        if (seSource == null)
        {
            seSource = gameObject.AddComponent<AudioSource>();
            seSource.playOnAwake = false;
            seSource.loop = false;
        }

        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.playOnAwake = false;
            voiceSource.loop = false;
        }
    }

    private AudioClip LoadClip(string resourcesPath)
    {
        if (string.IsNullOrWhiteSpace(resourcesPath))
            return null;

        if (clipCache.TryGetValue(resourcesPath, out AudioClip cached) && cached != null)
            return cached;

        AudioClip clip = Resources.Load<AudioClip>(resourcesPath);
        if (clip == null)
        {
            Debug.LogWarning($"[SoundManager] AudioClip 로드 실패: {resourcesPath}");
            return null;
        }

        clipCache[resourcesPath] = clip;
        return clip;
    }

    public void PlayBGM(string resourcesPath, bool loop = true)
    {
        AudioClip clip = LoadClip(resourcesPath);
        if (clip == null) return;

        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.loop = loop;
        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource == null) return;
        bgmSource.Stop();
    }

    public void PlaySE(string resourcesPath, float volumeScale = 1f)
    {
        AudioClip clip = LoadClip(resourcesPath);
        if (clip == null) return;

        seSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void PlayVoice(string resourcesPath, bool interrupt = true)
    {
        AudioClip clip = LoadClip(resourcesPath);
        if (clip == null) return;

        if (interrupt)
            voiceSource.Stop();

        voiceSource.clip = clip;
        voiceSource.Play();
    }

    public void StopVoice()
    {
        if (voiceSource == null) return;
        voiceSource.Stop();
    }

    public void SetPaused(bool paused)
    {
        if (paused)
        {
            bgmSource.Pause();
            seSource.Pause();
            voiceSource.Pause();
        }
        else
        {
            bgmSource.UnPause();
            seSource.UnPause();
            voiceSource.UnPause();
        }
    }

    public void SetMasterVolume(float value)
    {
        masterVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetBgmVolume(float value)
    {
        bgmVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetSeVolume(float value)
    {
        seVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetVoiceVolume(float value)
    {
        voiceVolume = Mathf.Clamp01(value);
        ApplyVolumes();
    }

    public void SetMute(bool mute)
    {
        isMuted = mute;
        ApplyVolumes();
    }

    private void ApplyVolumes()
    {
        float muteMul = isMuted ? 0f : 1f;
        if (bgmSource != null) bgmSource.volume = muteMul * masterVolume * bgmVolume;
        if (seSource != null) seSource.volume = muteMul * masterVolume * seVolume;
        if (voiceSource != null) voiceSource.volume = muteMul * masterVolume * voiceVolume;
    }
}
