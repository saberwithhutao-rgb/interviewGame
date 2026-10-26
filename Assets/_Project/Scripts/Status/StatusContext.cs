using UnityEngine;

/// <summary>
/// 状态上下文类，用于在状态效果中传递相关信息
/// </summary>
public class StatusContext
{
    public GameObject target;       // 被施加状态的对象
    public GameObject source;       // 施加状态的对象（施法者）
    public StatusData statusData;   // 状态配置
    public float damage;            // 方便传递伤害数值
}