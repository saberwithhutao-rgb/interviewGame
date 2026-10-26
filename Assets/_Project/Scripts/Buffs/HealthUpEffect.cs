using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff效果：生命值提升
/// </summary>
[CreateAssetMenu(fileName = "HealthUp", menuName = "Buff效果/生命值提升")]
public class HealthUpEffect : BuffEffect
{
    public float multiplier = 1.5f;
    public override void OnApply(GameObject target)
    {
        PlayerStats stats = target.GetComponent<PlayerStats>();
        if (stats != null) stats.AddHPMultiplier(multiplier);
    }

    public override void OnRemove(GameObject target)
    {
        PlayerStats stats = target.GetComponent<PlayerStats>();
        if (stats != null) stats.RemoveHPMultiplier(multiplier);
    }
}
