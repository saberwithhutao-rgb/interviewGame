using System;
using System.Collections.Generic;



/// <summary>
/// 技能存档数据类，包含技能的名称、等级、经验和剩余冷却时间，用于保存和加载技能状态
/// </summary>
[Serializable]
public class SkillSaveData
{
    public int skillId;
    public int level;
    public int exp;
    public float remainingCooldown;  // 剩余冷却秒数（>=0）
}



/// <summary>
/// 限时进度数据：当前一局内的临时状态，死亡或重置时清空
/// </summary>
[Serializable]
public class RunProgress
{
    public int currentLevelIndex;
    public int currentFlowStepIndex;
    public int currentFlowConditionProgress;
    public List<RunBuffData> temporaryBuffs = new List<RunBuffData>();
    public RunStatistics statistics;


    public List<EnemySaveData> enemies = new List<EnemySaveData>();

    public float playerPosX;
    public float playerPosY;
    public float playerPosZ;
    public float playerRotY;

}

/// <summary>
/// 限时 Buff 数据，记录 Buff 来源和剩余时间
/// </summary>
[Serializable]
public class RunBuffData
{
    public string buffId;
    public float remainingTime;
}

/// <summary>
/// 本局统计信息
/// </summary>
[Serializable]
public class RunStatistics
{
    public int killCount;
    public float playTime;
}

[Serializable]
public class EnemySaveData
{
    public string uniqueId;
    public bool isDead;
    public float currentHP;
}
