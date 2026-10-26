using UnityEngine;

/// <summary>
/// 恢复生命效果类，继承自SkillEffect，用于实现技能的生命恢复效果
/// </summary>
[CreateAssetMenu(fileName = "HealEffect", menuName = "技能/恢复生命")]
public class HealEffect : SkillEffect
{
    /// <summary>
    /// 执行技能效果的方法，增加施法者的生命值
    /// </summary>
    /// <param name="skillContext">技能上下文，包含施法者和技能的基础数值</param>
    public override void Execute(SkillContext skillContext)
    {
        PlayerStats stats = skillContext.caster.GetComponent<PlayerStats>();
        if (stats != null)
        {
            stats.IncreaseHP(skillContext.baseValue);
        }
    }
}