using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 玩家追踪器，负责管理所有活着的玩家的 Transform
/// </summary>
public class PlayerTracker : MonoBehaviour
{
    public static PlayerTracker Instance { get; private set; }

    // 所有活着的玩家的 Transform
    public List<Transform> AlivePlayers { get; private set; } = new List<Transform>();

    private void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 注册一个活着的玩家的 Transform，如果已经存在则不重复添加
    /// </summary>
    /// <param name="playerTransform">要注册的玩家的 Transform</param>
    public void Register(Transform playerTransform)
    {
        if (!AlivePlayers.Contains(playerTransform))
            AlivePlayers.Add(playerTransform);
    }

    /// <summary>
    /// 注销一个玩家的 Transform，从活着的玩家列表中移除
    /// </summary>
    /// <param name="playerTransform">要注销的玩家的 Transform</param>
    public void Unregister(Transform playerTransform)
    {
        AlivePlayers.Remove(playerTransform);
    }
}