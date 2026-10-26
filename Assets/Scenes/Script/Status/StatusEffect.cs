using UnityEngine;

/// <summary>
/// 状态效果类，用于定义每次 Tick 时的具体效果
/// </summary>
public abstract class StatusEffect : ScriptableObject
{
    /// <summary>
    /// 每次 Tick 时调用
    /// </summary>
    public abstract void Apply(StatusContext context);
}