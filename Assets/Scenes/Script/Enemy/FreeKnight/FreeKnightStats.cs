using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 自由骑士的属性类，继承自EnemyStats，定义了自由骑士的基础属性
/// </summary>
public class FreeKnightStats : EnemyStats
{
    private void Reset()
    {
        enemyName = "自由骑士";
        race = "人族";
        level.Value = 1;

        baseAttack = 7f;        // 攻击力
        baseDefense = 7f;        // 防御力
        baseMagicAttack = 0f;    // 魔法能力
        baseMagicDefense = 0f;   // 抵御能力
        baseSpeed = 6f;          // 速度能力
        baseMaxHP = 150f;         // 最大生命值
        baseMaxMP = 50f;         // 最大魔法值
        baseMaxBurstSP = 30f;       // 最大爆发力
        baseMaxStaminaSP = 30f;     // 最大体力值
    }
}
