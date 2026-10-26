using UnityEngine;

/// <summary>
/// 独立BGM音量控制器：挂在任意GameObject上即可，脚本内部自动创建AudioSource。
/// 实时从SettingsManager读取MusicVolume并同步音量。
/// 用于AudioManager管不到的独立BGM（如MainMenu场景中的mainmenu/managers/music）。
/// 使用方式：挂载此脚本 → 拖入bgmClip → 运行，无需手动挂AudioSource。
/// </summary>
public class BGMController : MonoBehaviour
{
    [Header("BGM音频剪辑")]
    public AudioClip bgmClip;

    [Header("音量设置")]
    [Range(0f, 1f)] public float baseVolume = 0.5f;

    private AudioSource _bgmSource;

    void Awake()
    {
        // 自动创建AudioSource，不需要在Inspector里手动添加
        _bgmSource = gameObject.AddComponent<AudioSource>();
        _bgmSource.clip = bgmClip;
        _bgmSource.loop = true;
        _bgmSource.playOnAwake = true;
        _bgmSource.spatialBlend = 0f; // 2D音效，不受距离影响
        _bgmSource.volume = 0f;       // 先静音，Start里同步正确音量后再播放
    }

    void Start()
    {
        SyncVolume();
        // 音量同步后开始播放
        if (_bgmSource.clip != null && !_bgmSource.isPlaying)
            _bgmSource.Play();
    }

    void OnEnable()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged += SyncVolume;
        }
    }

    void OnDisable()
    {
        if (SettingsManager.Instance != null)
        {
            SettingsManager.Instance.OnMusicVolumeChanged -= SyncVolume;
        }
    }

    void SyncVolume()
    {
        if (_bgmSource == null || SettingsManager.Instance == null) return;

        float rawVolume = AudioManager.LinearToVolume(SettingsManager.Instance.MusicVolume);
        _bgmSource.volume = baseVolume * rawVolume;
    }
}
