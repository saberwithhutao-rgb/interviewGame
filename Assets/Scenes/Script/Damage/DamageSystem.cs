using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 伤害系统，负责处理伤害计算和事件通知
/// </summary>
public class DamageSystem 
{ 
    /// <summary>
    /// 计算伤害值
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="attacker">攻击者对象</param>
    /// <param name="baseDamage">基础伤害值</param>
    /// <param name="damageType">伤害类型</param>
    /// <returns>最终计算的伤害值</returns>
    public static float CalculateDamage(GameObject target, GameObject attacker, float baseDamage, DamageType damageType)
    {
        float finalDamage = baseDamage;

        Stats targetStats = target?.GetComponent<Stats>();
        Stats attackerStats = attacker?.GetComponent<Stats>();

        if (targetStats == null) return finalDamage;

        if (damageType == DamageType.Physical)
        {
            float defense = targetStats.Defense;
            finalDamage = baseDamage * (100f / (100f + defense));
        }
        else if (damageType == DamageType.Magic || damageType == DamageType.Poison)
        {
            float magicDefense = targetStats.MagicDefense;
            finalDamage = baseDamage * (100f / (100f + magicDefense));
        }

        finalDamage = Mathf.Max(finalDamage, 0.1f);
        return finalDamage;
    }
}