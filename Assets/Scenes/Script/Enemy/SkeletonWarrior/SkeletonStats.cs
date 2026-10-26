using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkeletonStats 类继承自 EnemyStats，用于定义骷髅战士的属性和统计数据
/// </summary>
public class SkeletonStats : EnemyStats
{
    private void Reset()
    {
        enemyName = "骷髅战士";
        race = "亡灵";
        level.Value = 2;

        baseAttack = 15f;        // 攻击力
        baseDefense = 8f;        // 防御力
        baseMagicAttack = 0f;    // 魔法攻击
        baseMagicDefense = 0f;   // 魔法防御
        baseSpeed = 5.5f;        // 速度
        baseMaxHP = 80f;         // 最大生命值
        baseMaxMP = 50f;         // 最大魔法值
        baseMaxBurstSP = 30f;       // 最大爆发力
        baseMaxStaminaSP = 30f;     // 最大体力值

        attackRange = 2.5f;      // 攻击范围
        attackCooldown = 1.2f;   // 攻击冷却
        sightRange = 18f;        // 视野范围
        sightAngle = 150f;       // 视野角度
    }
}
