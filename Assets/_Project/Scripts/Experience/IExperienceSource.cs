using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 经验提供者接口（被击杀者）
/// </summary>
public interface IExperienceSource
{
    /// <summary>
    /// 获取基础经验值
    /// </summary>
    /// <returns>基础经验值</returns>
    float GetBaseExperience();
}