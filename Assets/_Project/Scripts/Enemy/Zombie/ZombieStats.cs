using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class ZombieStats : EnemyStats
{
    private void Reset()
    {
        enemyName = "僵尸";
        race = "不死族";
        level.Value = 1;

        baseAttack = 15f;        // 攻击力
        baseDefense = 2f;        // 防御力
        baseMagicAttack = 0f;    // 魔法能力
        baseMagicDefense = 0f;   // 抵御能力
        baseSpeed = 6f;          // 速度能力
        baseMaxHP = 70f;         // 最大生命值
        baseMaxMP = 50f;         // 最大魔法值
        baseMaxBurstSP = 30f;       // 最大爆发力
        baseMaxStaminaSP = 30f;     // 最大体力值
    }
    
}
