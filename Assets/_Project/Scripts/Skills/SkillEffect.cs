using UnityEngine;

/// <summary>
/// 技能效果的抽象基类，继承自 ScriptableObject
/// </summary>
public abstract class SkillEffect : ScriptableObject
{
    /// <summary>
    /// 执行技能效果的方法，接受一个 SkillContext 参数
    /// </summary>
    /// <param name="skillContext">技能上下文，包含施法者、目标等信息</param>
    public abstract void Execute(SkillContext skillContext);

    /// <summary>
    /// 获取技能效果的间隔时间的方法，接受一个等级参数
    /// </summary>
    /// <param name="level">技能等级</param>
    /// <returns>技能效果的间隔时间</returns>
    public virtual float GetInterval(int level)
    {
        return -1f;  // 默认没有间隔
    }
}