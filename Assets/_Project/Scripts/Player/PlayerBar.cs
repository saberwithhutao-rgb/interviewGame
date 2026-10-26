using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 玩家状态栏 UI 组件，用于显示玩家的生命值、魔法值、爆发能量和耐力能量
/// </summary>
public class PlayerBar : MonoBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [Header("HP")]
    [SerializeField] private Slider HPSlider;
    [SerializeField] private TMP_Text HPText;

    [Header("MP")]
    [SerializeField] private Slider MPSlider;
    [SerializeField] private TMP_Text MPText;

    [Header("Burst SP")]
    [SerializeField] private Slider BurstSPSlider;
    [SerializeField] private TMP_Text BurstSPText;

    [Header("Stamina SP")]
    [SerializeField] private Slider StaminaSPSlider;
    [SerializeField] private TMP_Text StaminaSPText;

    void Start()
    {
        if (playerStats == null)
        {
            playerStats = transform.root.GetComponent<PlayerStats>();
        }
        HPSlider.value = playerStats.CurrentHPPct;
        HPText.text = $"{playerStats.CurrentHP:F0}/{playerStats.MaxHP:F0}";
        HPSlider.maxValue = 1f;

        MPSlider.value = playerStats.CurrentMPPct;
        MPText.text = $"{playerStats.CurrentMP:F0}/{playerStats.MaxMP:F0}";
        MPSlider.maxValue = 1f;

        BurstSPSlider.value = playerStats.CurrentBurstSPPct;
        BurstSPText.text = $"{playerStats.CurrentBurstSP:F0}/{playerStats.MaxBurstSP:F0}";
        BurstSPSlider.maxValue = 1f;

        StaminaSPSlider.value = playerStats.CurrentStaminaSPPct;
        StaminaSPText.text = $"{playerStats.CurrentStaminaSP:F0}/{playerStats.MaxStaminaSP:F0}";
        StaminaSPSlider.maxValue = 1f;
    }

    void OnEnable()
    {
        if (playerStats != null)
        {
            playerStats.OnHPChanged += UpdateHealthBar;
            playerStats.OnMPChanged += UpdateManaBar;
            playerStats.OnBurstSPChanged += UpdateBurstBar;
            playerStats.OnStaminaSPChanged += UpdateStaminaBar;
            playerStats.OnMaxBurstSPChanged += UpdateBurstBar;
            playerStats.OnMaxStaminaSPChanged += UpdateStaminaBar;
        }
    }

    void OnDisable()
    {
        if (playerStats != null)
        {
            playerStats.OnHPChanged -= UpdateHealthBar;
            playerStats.OnMPChanged -= UpdateManaBar;
            playerStats.OnBurstSPChanged -= UpdateBurstBar;
            playerStats.OnStaminaSPChanged -= UpdateStaminaBar;
            playerStats.OnMaxBurstSPChanged -= UpdateBurstBar;
            playerStats.OnMaxStaminaSPChanged -= UpdateStaminaBar;
        }

    }
    void UpdateHealthBar(float _)
    {
        HPSlider.value = playerStats.CurrentHPPct;
        HPText.text = $"{playerStats.CurrentHP:F0}/{playerStats.MaxHP:F0}";
    }

    void UpdateManaBar(float _)
    {
        MPSlider.value = playerStats.CurrentMPPct;
        MPText.text = $"{playerStats.CurrentMP:F0}/{playerStats.MaxMP:F0}";
    }

    void UpdateBurstBar(float _)
    {
        BurstSPSlider.value = playerStats.CurrentBurstSPPct;
        BurstSPText.text = $"{playerStats.CurrentBurstSP:F0}/{playerStats.MaxBurstSP:F0}";
    }

    void UpdateStaminaBar(float _)
    {
        StaminaSPSlider.value = playerStats.CurrentStaminaSPPct;
        StaminaSPText.text = $"{playerStats.CurrentStaminaSP:F0}/{playerStats.MaxStaminaSP:F0}";
    }
}