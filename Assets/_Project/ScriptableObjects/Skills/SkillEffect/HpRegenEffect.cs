using UnityEngine;

/// <summary>
/// 技能效果：HP自动恢复
/// </summary>
[CreateAssetMenu(fileName = "HpRegenEffect", menuName = "技能/HP自动恢复")]
public class HpRegenEffect : SkillEffect, IPassiveUpdatable
{
    [Header("回复量公式")]
    [SerializeField] private float baseHeal = 1f;
    [SerializeField] private float growthPerLevel = 1f;

    [Header("间隔")]
    [SerializeField] private float baseInterval = 60f;
    [SerializeField] private float reductionPerLevel = 5f;
    [SerializeField] private float minInterval = 5f;

    /// <summary>
    /// 获取技能效果的执行间隔，随着技能等级的提升，间隔会逐渐减少，但不会低于最小间隔
    /// </summary>
    /// <param name="level">技能等级</param>
    /// <returns>技能效果的执行间隔</returns>
    public override float GetInterval(int level)
    {
        return Mathf.Max(baseInterval - reductionPerLevel * (level - 1), minInterval);
    }

    /// <summary>
    /// 执行技能效果，恢复施法者的HP
    /// </summary>
    /// <param name="context">技能上下文，包含施法者和技能的基础数值</param>
    public override void Execute(SkillContext context)
    {
        //不做逻辑
    }

    public void OnUpdate(SkillContext context, PassiveEffectRuntime runtime, float deltaTime)
    {
        if (context.caster == null) return;

        PlayerStats stats = context.caster.GetComponent<PlayerStats>();
        if (stats == null) return;

        // ★ HP 满 → 计时归零
        if (stats.CurrentHP >= stats.MaxHP)
        {
            runtime.timer = 0f;
            return;
        }

        runtime.timer += deltaTime;

        if (runtime.timer >= GetInterval(context.skillLevel))
        {
            runtime.timer = 0f;
            float healAmount = baseHeal + growthPerLevel * (context.skillLevel - 1);
            stats.IncreaseHP(healAmount);
        }
    }
}