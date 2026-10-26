using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 敌人血条组件，负责显示敌人的血量
/// </summary>
public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private EnemyStats enemyStats;

    void OnValidate()
    {
        if (slider == null)
            slider = GetComponent<Slider>();
    }

    void Start()
    {
        slider.maxValue = 1f;
        if (enemyStats == null)
        {
            enemyStats = GetComponentInParent<EnemyStats>();
            slider.value = enemyStats.CurrentHPPct;
        }
        
    }


    private void OnEnable()
    {
        enemyStats.OnHPChanged += UpdateHealthBar;
    }

    private void OnDisable()
    {
        enemyStats.OnHPChanged -= UpdateHealthBar;
    }

    void UpdateHealthBar(float healthPercent)
    {
        slider.value = enemyStats.CurrentHPPct;
    }
}
