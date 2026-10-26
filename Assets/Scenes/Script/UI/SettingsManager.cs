using System;
using UnityEngine;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager Instance { get; private set; }

    private const string SENSITIVITY_KEY = "MouseSensitivity";
    private const float DEFAULT_SENSITIVITY = 300f;

    private const string VOLUME_KEY = "MasterVolume";
    private const float DEFAULT_VOLUME = 0.7f;

    private const string MUSIC_VOLUME_KEY = "MusicVolume";
    private const float DEFAULT_MUSIC_VOLUME = 0.9f;

    public float MouseSensitivity { get; private set; }
    public float MasterVolume { get; private set; }
    public float MusicVolume { get; private set; } // 新增BGM音量

    public event Action OnSettingsLoaded;
    public event Action OnVolumeChanged;
    public event Action OnMusicVolumeChanged;
    public event Action OnSensitivityChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        LoadSettings();
        OnSettingsLoaded?.Invoke();
    }

    void LoadSettings()
    {
        MouseSensitivity = PlayerPrefs.GetFloat(SENSITIVITY_KEY, DEFAULT_SENSITIVITY);
        MasterVolume = PlayerPrefs.GetFloat(VOLUME_KEY, DEFAULT_VOLUME);
        MusicVolume = PlayerPrefs.GetFloat(MUSIC_VOLUME_KEY, DEFAULT_MUSIC_VOLUME);
    }

    public void SetSensitivity(float value)
    {
        MouseSensitivity = value;
        PlayerPrefs.SetFloat(SENSITIVITY_KEY, value);
        PlayerPrefs.Save();
        OnSensitivityChanged?.Invoke();
    }


    public void SetVolume(float linear)
    {
        MasterVolume = linear;
        PlayerPrefs.SetFloat(VOLUME_KEY, linear);
        PlayerPrefs.Save();
        OnVolumeChanged?.Invoke();
        if (AudioManager.Instance != null)
            AudioManager.Instance.UpdateVolume();
    }

    // ==========新增：背景音乐音量设置方法==========
    public void SetMusicVolume(float linear)
    {
        MusicVolume = linear;
        PlayerPrefs.SetFloat(MUSIC_VOLUME_KEY, linear);
        PlayerPrefs.Save();
        OnMusicVolumeChanged?.Invoke();
        if (AudioManager.Instance != null)
            AudioManager.Instance.UpdateMusicVolume();
    }
}