using UnityEngine;

/// <summary>
/// 中毒Tick效果类，继承自 StatusEffect，用于在状态上下文中对目标造成持续的毒属性伤害
/// </summary>
[CreateAssetMenu(fileName = "PoisonTickEffect", menuName = "状态/中毒Tick效果")]
public class PoisonTickEffect : StatusEffect
{
    [Header("伤害系数（基于施法者魔法攻击）")]
    [SerializeField] private float damageMultiplier = 0.1f;

    public override void Apply(StatusContext context)
    {
        if (context.target == null) return;

        Stats casterStats = context.source?.GetComponent<Stats>();
        if (casterStats == null) return;

        float damage = casterStats.MagicAttack * damageMultiplier;

        IDamageable damageable = context.target.GetComponent<IDamageable>();
        if (damageable != null)
        {
            damageable.TakeDamage(damage, context.source, DamageType.Poison);
        }
    }
}