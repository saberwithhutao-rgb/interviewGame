using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class VolumeSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;

    void Start()
    {
        slider.minValue = 0f;
        slider.maxValue = 1f;
        slider.value = SettingsManager.Instance.MasterVolume;
        label.text = $"“Ù–ß  : {slider.value * 100:F0}%";

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        SettingsManager.Instance.SetVolume(value);
        label.text = $"“Ù–ß  : {value * 100:F0}%";
        AudioManager.Instance.UpdateVolume();
    }
}