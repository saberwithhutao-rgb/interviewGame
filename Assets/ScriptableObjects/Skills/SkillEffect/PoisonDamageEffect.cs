using UnityEngine;

/// <summary>
/// 毒伤害效果类，继承自 SkillEffect，用于在技能上下文中对目标造成毒属性伤害
/// </summary>
[CreateAssetMenu(fileName = "PoisonDamageEffect", menuName = "技能/毒伤害")]
public class PoisonDamageEffect : SkillEffect
{
    [SerializeField] private float mult = 1f;

    public override void Execute(SkillContext context)
    {

        // 检查 caster 是否有效
        if (context.caster == null)
        {
            Debug.LogWarning("[PoisonDamageEffect] caster 为空");
            return;
        }

        // 检查 target 是否有效
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
        // 传入 DamageType.Poison
        damageable.TakeDamage(damage, context.caster, DamageType.Poison);
    }
}