using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// SensitivitySlider 类用于在 UI 上显示和调整鼠标灵敏度，实时更新灵敏度值
/// </summary>
public class SensitivitySlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TMP_Text label;

    void Start()
    {
        slider.minValue = 50f;
        slider.maxValue = 1000f;
        slider.value = SettingsManager.Instance.MouseSensitivity;
        label.text = $"灵敏度: {slider.value:F0}";

        slider.onValueChanged.AddListener(OnValueChanged);
    }

    void OnValueChanged(float value)
    {
        SettingsManager.Instance.SetSensitivity(value);
        label.text = $"灵敏度: {value:F0}";

    }
}