using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 经验接收者接口（击杀者）
/// </summary>
public interface IExperienceReceiver
{
    /// <summary>
    /// 增加经验值
    /// </summary>
    /// <param name="amount">增加的经验值数量</param>
    void AddExperience(float amount);
}