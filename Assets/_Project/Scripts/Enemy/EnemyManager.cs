using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 泛型怪物管理器基类
/// </summary>
public abstract class EnemyManager<T> : MonoBehaviour where T : Enemy
{
    protected List<T> enemies = new List<T>();
    public List<T> AllEnemies => enemies;

    [SerializeField] protected GameObject enemyPrefab;
    [Header("刷怪点")]
    [SerializeField] protected float spawnRadius = 3f;

    public static EnemyManager<T> Instance { get; protected set; }

    protected virtual void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// 注册怪物到管理器中，并订阅其受伤事件
    /// </summary>
    /// <param name="enemy">要注册的怪物</param>
    public virtual void RegisterEnemy(T enemy)
    {
        if (!enemies.Contains(enemy))
        {
            enemies.Add(enemy);
        }
    }

    /// <summary>
    /// 注销怪物并取消订阅其受伤事件
    /// </summary>
    /// <param name="enemy">要注销的怪物</param>
    public virtual void UnregisterEnemy(T enemy)
    {
        if (enemies.Contains(enemy))
        {
            enemies.Remove(enemy);
        }
    }

    protected virtual void OnDestroy()
    {
        // 子类可以重写
    }
}