using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Buff效果的基类，所有Buff效果都应该继承自此类
/// </summary>
public class BuffEffect : ScriptableObject
{
    /// <summary>
    /// 当Buff被应用到目标时调用的方法，子类可以重写此方法来实现具体的Buff效果
    /// </summary>
    /// <param name="target">目标对象</param>
    public virtual void OnApply(GameObject target) { }

    /// <summary>
    /// 当Buff从目标上移除时调用的方法，子类可以重写此方法来实现具体的Buff效果
    /// </summary>
    /// <param name="target">目标对象</param>
    public virtual void OnRemove(GameObject target) { }
}
