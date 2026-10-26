using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MusicVolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;

    void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;

        slider.value = SettingsManager.Instance.MusicVolume;
        label.text = $"音量  : {slider.value * 100:F0}%";

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        SettingsManager.Instance.SetMusicVolume(value);
        label.text = $"音量  : {value * 100:F0}%";
    }
}