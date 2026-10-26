using UnityEngine;

/// <summary>
/// 状态数据类，用于定义状态的基本属性和效果
/// </summary>
[CreateAssetMenu(fileName = "Status_01", menuName = "状态/状态数据")]
public class StatusData : ScriptableObject
{
    public string statusName = "新状态";
    public float duration = 5f;           // 总持续时间
    public float tickInterval = 1f;        // Tick 间隔
    public StatusEffect[] tickEffects;     // 每次 Tick 执行的效果
}