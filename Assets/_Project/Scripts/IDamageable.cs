using UnityEngine;

/// <summary>
/// 接口，表示一个可以受到伤害的对象
/// </summary>
public interface IDamageable
{
    /// <summary>
    /// 受到伤害的方法
    /// </summary>
    /// <param name="damage">受到的伤害量</param>
    /// <param name="attacker">攻击者对象</param>
    /// <param name="damageType">伤害类型</param>
    void TakeDamage(float damage, GameObject attacker = null, DamageType damageType = DamageType.Physical);
}