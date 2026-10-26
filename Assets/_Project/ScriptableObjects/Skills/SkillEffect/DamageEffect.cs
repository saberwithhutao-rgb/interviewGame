using UnityEngine;

/// <summary>
/// 伤害效果类，继承自 SkillEffect，用于处理技能造成的伤害
/// </summary>
[CreateAssetMenu(fileName = "DamageEffect", menuName = "技能/伤害")]
public class DamageEffect : SkillEffect
{
    [SerializeField] private float mult = 1f;
    [SerializeField] private DamageType damageType = DamageType.Physical;

    /// <summary>
    /// 执行伤害效果的方法，根据技能上下文计算伤害并应用到目标
    /// </summary>
    /// <param name="context">技能上下文，包含施法者和目标等信息</param>
    public override void Execute(SkillContext context)
    {
        if (context.caster == null)
        {
            Debug.LogWarning("[PoisonDamageEffect] caster 为空");
            return;
        }
        if (context.target == null)
        {
            Debug.LogWarning("[PoisonDamageEffect] target 为空");
            return;
        }

        IDamageable damageable = context.target?.GetComponent<IDamageable>();
        if (damageable == null) return;

        Stats stats = context.caster.GetComponent<Stats>();

        if (stats == null) return;

        float damage = stats.MagicAttack * mult;
        damageable.TakeDamage(damage, context.caster, damageType);
    }
}