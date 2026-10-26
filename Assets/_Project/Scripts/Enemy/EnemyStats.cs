using System;
using Unity.Netcode;
using UnityEngine;

public class EnemyStats : Stats
{
    [Header("鉴定属性")]
    [SerializeField] protected string enemyName = "敌人";
    [SerializeField] protected string race = "未知";

    [Header("经验值")]
    [SerializeField] private float baseExp = 10f;

    [Header("敌人特有属性")]
    [SerializeField] protected float lowHPPct = 0.3f;
    [SerializeField] protected float attackRange = 2.5f;
    [SerializeField] protected float attackCooldown = 1.5f;
    [SerializeField] protected float fastSpeed = 5.5f;
    [SerializeField] protected float sightAngle = 120f;
    [SerializeField] protected float sightRange = 15f;
    [SerializeField] protected float loseSightTime = 5f;
    [SerializeField] protected float rangedAttackRange = 8f;
    [SerializeField] protected float returnDistance = 20f;

    [Header("巡逻")]
    [SerializeField] protected float patrolSpeed = 1.5f;
    [SerializeField] protected float patrolRadius = 5f;
    [SerializeField] protected float patrolWaitMinTime = 2f;
    [SerializeField] protected float patrolWaitMaxTime = 5f;
    [SerializeField] protected float alertRadius = 5f;

    // ─── 敌人特有属性（只读） ───
    public float AttackRange => attackRange;
    public float AttackCooldown => attackCooldown;
    public float FastSpeed => fastSpeed;
    public float SightAngle => sightAngle;
    public float SightRange => sightRange;
    public float AlertRadius => alertRadius;
    public float LoseSightTime => loseSightTime;
    public float RangedAttackRange => rangedAttackRange;
    public float ReturnDistance => returnDistance;
    public float LowHPPct => lowHPPct;
    public float PatrolSpeed => patrolSpeed;
    public float PatrolRadius => patrolRadius;
    public float PatrolWaitMinTime => patrolWaitMinTime;
    public float PatrolWaitMaxTime => patrolWaitMaxTime;

    // ─── 经验值 ───
    public override float GetBaseExperience() => baseExp;

    // ─── 鉴定信息 ───
    public override string StatsInfoLV1 => $"{enemyName}";
    public override string StatsInfoLV2 => $"种族: {race}\n等级: {level}";
    public override string StatsInfoLV3 => $"HP: {CurrentHP:F0}/{MaxHP}";
    public override string StatsInfoLV5 => $"攻击力: {Attack}\n防御力: {Defense}";
}