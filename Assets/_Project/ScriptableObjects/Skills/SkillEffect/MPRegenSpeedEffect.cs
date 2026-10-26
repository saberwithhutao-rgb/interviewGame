using UnityEngine;

/// <summary>
/// MP恢复速度效果，继承自SkillEffect，实现了IPassiveEffect接口
/// </summary>
[CreateAssetMenu(fileName = "MPRegenSpeedEffect", menuName = "技能/MP恢复速度")]
public class MPRegenSpeedEffect : SkillEffect, IPassiveEffect
{
    [Header("恢复速度倍率")]
    [SerializeField] private float multiplier = 1.1f;  // 1.1倍恢复速度

    public override void Execute(SkillContext context)
    {
        Apply(context);
    }

    public void Apply(SkillContext context)
    {
        PlayerStats stats = context.caster?.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.MultiplyMPRegenMultiplier(multiplier);
        Debug.Log($"[MP恢复速度] 学习，MP恢复速度变为 {multiplier}x");
    }

    public void Remove(SkillContext context)
    {
        PlayerStats stats = context.caster?.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.MultiplyMPRegenMultiplier(1f / multiplier);
        Debug.Log($"[MP恢复速度] 遗忘，MP恢复速度恢复");
    }
}