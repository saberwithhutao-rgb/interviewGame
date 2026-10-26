using UnityEngine;

/// <summary>
/// 音频管理器，负责管理游戏中的音效和背景音乐
/// </summary>
public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }
    [SerializeField] private AudioSource sfxSource;

    [Header("背景音乐")]
    [SerializeField] private AudioSource musicSource;

    /// <summary>音乐基础音量倍率，统一所有BGM初始音量为0.5</summary>
    [Header("音乐基础音量")]
    [Range(0f, 1f)] public float musicBaseVolume = 0.9f;

    /// <summary>当前音乐音量实际值（已从线性0-1转换为对数，并乘以base）</summary>
    public float CurrentMusicVolume { get; private set; } = 1f;

    /// <summary>当前音效音量实际值（已从线性0-1转换为对数）</summary>
    public float CurrentSFXVolume { get; private set; } = 1f;

    /// <summary>线性 0~1 → 对数音量（-40dB ~ 0dB → 0~1）</summary>
    public static float LinearToVolume(float linear)
    {
        if (linear <= 0.01f) return 0f;
        float db = Mathf.Lerp(-40f, 0f, linear);
        return Mathf.Pow(10f, db / 20f);
    }

    void Awake()
    {
        Instance = this;
        
    }

    void Start()
    {
        if (musicSource != null && musicSource.clip != null)
            musicSource.Play();
        UpdateVolume();
        UpdateMusicVolume();
    }

    private void OnEnable()
    {
        SettingsManager.Instance.OnSettingsLoaded += UpdateVolume;
    }

    private void OnDisable()
    {
        SettingsManager.Instance.OnSettingsLoaded -= UpdateVolume;
    }

    /// <summary>
    /// 更新音效音量，读取SettingsManager中的MasterVolume并转换为对数音量
    /// </summary>
    public void UpdateVolume()
    {
        if (SettingsManager.Instance == null) return;

        float linear = SettingsManager.Instance.MasterVolume;
        CurrentSFXVolume = LinearToVolume(linear);
        sfxSource.volume = CurrentSFXVolume;
    }

    /// <summary>
    /// 更新音乐音量，读取SettingsManager中的MusicVolume并转换为对数音量
    /// </summary>
    public void UpdateMusicVolume()
    {
        if (SettingsManager.Instance == null) return;

        float linear = SettingsManager.Instance.MusicVolume;
        float rawVolume = LinearToVolume(linear);
        CurrentMusicVolume = rawVolume;
        // BGM初始基础音量统一为musicBaseVolume (0.2)，再乘以滑块倍率
        musicSource.volume = musicBaseVolume * rawVolume;
    }

    /// <summary>
    /// 播放音效，使用PlayOneShot方式播放，避免打断当前音效
    /// </summary>
    /// <param name="clip">要播放的音效片段</param>
    /// <param name="volume">音量倍率（0~1）</param>
    public void PlayOneShot(AudioClip clip, float volume = 1f)
    {
        if (clip != null && sfxSource != null)
            sfxSource.PlayOneShot(clip, volume);
    }

    /// <summary>
    /// 播放背景音乐，如果当前正在播放的音乐与clip相同，则不重新播放
    /// </summary>
    /// <param name="clip">要播放的背景音乐片段</param>
    public void PlayMusic(AudioClip clip)
    {
        if (musicSource.clip == clip) return;
        musicSource.clip = clip;
        UpdateMusicVolume();
        musicSource.Play();
    }

    /// <summary>
    /// 停止播放背景音乐
    /// </summary>
    public void StopMusic()
    {
        musicSource.Stop();
    }
}