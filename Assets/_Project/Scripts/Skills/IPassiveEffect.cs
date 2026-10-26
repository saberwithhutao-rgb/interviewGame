using UnityEngine;

/// <summary>
/// 被动技能效果接口，支持学习（Apply）和遗忘（Remove）
/// </summary>
public interface IPassiveEffect
{
    /// <summary>
    /// 学习技能时调用（应用效果）
    /// </summary>
    void Apply(SkillContext context);

    /// <summary>
    /// 遗忘技能时调用（移除效果）
    /// </summary>
    void Remove(SkillContext context);
}