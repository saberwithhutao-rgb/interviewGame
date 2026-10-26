using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SpeedUpEffect 类继承自 BuffEffect，用于实现速度提升的 Buff 效果
/// </summary>
[CreateAssetMenu(fileName = "SpeedUp", menuName = "Buff效果/速度提升")]
public class SpeedUpEffect : BuffEffect
{
    public float multiplier = 1.5f;

    public override void OnApply(GameObject target)
    {
        PlayerStats stats = target.GetComponent<PlayerStats>();
        if (stats != null) stats.AddSpeedMultiplier(multiplier);
    }

    public override void OnRemove(GameObject target)
    {
        PlayerStats stats = target.GetComponent<PlayerStats>();
        if (stats != null) stats.RemoveSpeedMultiplier(multiplier);
    }
}
