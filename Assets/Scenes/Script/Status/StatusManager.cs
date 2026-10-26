using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// StatusManager 类用于管理游戏对象的状态效果，包括施加状态、更新状态
/// </summary>
public class StatusManager : NetworkBehaviour
{
    // 当前活跃的状态实例
    private List<ActiveStatus> activeStatuses = new List<ActiveStatus>();

    [Header("全局状态数据库")]
    [SerializeField] private StatusDatabase statusDatabase;  // 所有生物拖同一个资源

    void OnEnable()
    {
        GameEvents.OnDamageDealt += OnDamageDealt;
    }

    void OnDisable()
    {
        GameEvents.OnDamageDealt -= OnDamageDealt;
    }

    private void OnDamageDealt(GameObject target, GameObject attacker, float damage, DamageType damageType)
    {
        if (target != gameObject) return;
        if (!IsServer) return;


        StatusData status = statusDatabase?.GetStatusForDamageType(damageType);
        if (status != null && !HasStatus(status))
        {
            ApplyStatus(status, attacker);
        }
    }

    /// <summary>
    /// 施加状态
    /// </summary>
    public void ApplyStatus(StatusData statusData, GameObject source)
    {
        if (!IsServer) return;  // 只有服务器能施加状态

        // 简单实现：直接添加，不处理叠加
        ActiveStatus newStatus = new ActiveStatus
        {
            statusData = statusData,
            source = source,
            timeRemaining = statusData.duration,
            tickTimer = 0f
        };
        activeStatuses.Add(newStatus);

    }

    private void Update()
    {
        if (!IsServer) return;

        float delta = Time.deltaTime;

        // 从后往前遍历（便于删除）
        for (int i = activeStatuses.Count - 1; i >= 0; i--)
        {
            ActiveStatus status = activeStatuses[i];

            // 更新剩余时间
            status.timeRemaining -= delta;

            // 更新 Tick 计时器
            status.tickTimer += delta;

            // 检查是否到达 Tick 间隔
            if (status.tickTimer >= status.statusData.tickInterval)
            {
                status.tickTimer = 0f;
                ExecuteTick(status);
            }

            // 如果时间到，移除状态
            if (status.timeRemaining <= 0f)
            {
                activeStatuses.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 执行一次 Tick
    /// </summary>
    private void ExecuteTick(ActiveStatus status)
    {
        StatusContext context = new StatusContext
        {
            target = gameObject,
            source = status.source,
            statusData = status.statusData
        };

        foreach (var effect in status.statusData.tickEffects)
        {
            if (effect != null)
                effect.Apply(context);
        }
    }

    /// <summary>
    /// 清除所有状态
    /// </summary>
    public void ClearAllStatuses()
    {
        activeStatuses.Clear();
    }

    /// <summary>
    /// 获取是否拥有某个状态
    /// </summary>
    public bool HasStatus(StatusData statusData)
    {
        foreach (var s in activeStatuses)
        {
            if (s.statusData == statusData)
                return true;
        }
        return false;
    }

    // 内部类：记录正在运行的状态实例
    private class ActiveStatus
    {
        public StatusData statusData;
        public GameObject source;
        public float timeRemaining;
        public float tickTimer;
    }
}