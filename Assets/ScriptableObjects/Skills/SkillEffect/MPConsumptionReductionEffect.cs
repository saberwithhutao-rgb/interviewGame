using UnityEngine;

/// <summary>
/// MP消耗减缓效果的技能效果类，继承自SkillEffect
/// </summary>
[CreateAssetMenu(fileName = "MPConsumptionReductionEffect", menuName = "技能/MP消耗减缓")]
public class MPConsumptionReductionEffect : SkillEffect
{
    [Header("消耗减少比例")]
    [SerializeField] private float reductionPercent = 0.1f;  // 0.1 = 减少10%

    public override void Execute(SkillContext context)
    {
        // ★ 被动技能学习时，调用 Apply
        Apply(context);
    }

    public void Apply(SkillContext context)
    {
        PlayerStats stats = context.caster?.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.MultiplyMPConsumptionMultiplier(1f - reductionPercent);
        Debug.Log($"[MP消耗减缓] 学习，MP消耗变为 {(1f - reductionPercent) * 100}%");
    }

    public void Remove(SkillContext context)
    {
        PlayerStats stats = context.caster?.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.MultiplyMPConsumptionMultiplier(1f / (1f - reductionPercent));
        Debug.Log($"[MP消耗减缓] 遗忘，MP消耗恢复");
    }
}