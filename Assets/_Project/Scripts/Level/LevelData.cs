using UnityEngine;

/// <summary>
/// 关卡数据类，用于存储关卡的各种配置和参数
/// </summary>
[CreateAssetMenu(fileName = "Level_01", menuName = "关卡数据")]
public class LevelData : ScriptableObject
{
    [Header("关卡信息")]
    public string levelName = "第一关";
    public int killTarget = 5;
    public float timeLimit = 0f;

    [Header("任务提示")]
    [TextArea(2, 4)]
    public string objectiveText = "击杀所有敌人";

    [Header("玩家 Buff")]
    public float playerDamageMult = 1f;
    public float playerSpeedMult = 1f;

    [Header("玩家 Debuff")]
    public float playerMaxHealthMult = 1f;

    [Header("敌人 Buff")]
    public float enemyHealthMult = 1f;
    public float enemySpeedMult = 1f;
    public float enemyDamageMult = 1f;

    [Header("刷怪")]
    public GameObject[] enemyPrefabs;      
    public int[] spawnCounts;

    [Header("通关奖励")]
    public BuffData[] buffPool;
    public int buffChoices = 0;
}