using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff 效果：攻击力提升
/// </summary>
[CreateAssetMenu(fileName = "DamageUp", menuName = "Buff效果/攻击力提升")]
public class DamageUpEffect : BuffEffect
{
    public float multiplier = 1.5f;

    /// <summary>
    /// 应用 Buff 效果到目标对象
    /// </summary>
    /// <param name="target">目标对象</param>
    public override void OnApply(GameObject target)
    {
        PlayerStats stats = target.GetComponent<PlayerStats>();
        if (stats != null) stats.AddAttackMultiplier(multiplier);
    }

    /// <summary>
    /// 移除 Buff 效果从目标对象
    /// </summary>
    /// <param name="target">目标对象</param>
    public override void OnRemove(GameObject target)
    {
        PlayerStats stats = target.GetComponent<PlayerStats>();
        if (stats != null) stats.RemoveAttackMultiplier(multiplier);
    }
}
