using System;
using UnityEngine;

/// <summary>
/// 全局事件中心，所有模块间通信通过这里解耦
/// </summary>
public static class GameEvents
{
    /// <summary>任何实体死亡时触发 (victim, killer)</summary>
    public static event Action<GameObject, GameObject> OnEntityKilled;
    /// <summary>
    /// 玩家升级时触发 (player)
    /// </summary>
    public static event Action<GameObject> OnPlayerLevelUp;
    /// <summary>
    /// 造成伤害时触发 (target, attacker, damage, damageType)
    /// </summary>
    public static event Action<GameObject, GameObject, float, DamageType> OnDamageDealt;
    /// <summary>
    /// 受到伤害时触发 (target, attacker, damage, damageType)
    /// </summary>
    public static event Action<GameObject, GameObject, float, DamageType> OnDamageReceived;
    /// <summary>
    /// 在实体死亡前触发 (victim, killer, damage)
    /// </summary>
    public static event Action<GameObject, GameObject, float> OnBeforeDeath;
    /// <summary>
    /// 当实体血量低于一定百分比时触发 (entity, hpPercent)
    /// </summary>
    public static event Action<GameObject, float> OnHealthLow;
    /// <summary>
    /// 当技能被使用时触发 (caster, skill)
    /// </summary>
    public static event Action<GameObject, SkillData> OnSkillUsed;
    /// <summary>
    /// 当技能被学习时触发 (player, skill)
    /// </summary>
    public static event Action<GameObject, SkillData> OnSkillLearned;
    /// <summary>
    /// 当玩家死亡时触发 (player)
    /// </summary>
    public static event Action<GameObject> OnPlayerDied;
    /// <summary>
    /// 当玩家攻击时触发 (player)
    /// </summary>
    public static event Action<GameObject> OnPlayerAttacked;
    /// <summary>
    /// 玩家攻击前触发(player)
    /// </summary>
    public static event Action<GameObject> OnBeforePlayerAttack;
    /// <summary>
    /// 当玩家更换武器时触发 (weapon)
    /// </summary>
    public static event Action<GameObject> OnWeaponChanged;
    /// <summary>
    /// 当经验值被转移时触发 (被获取的, 获取的, 经验值)
    /// </summary>
    public static event Action<GameObject, GameObject, float> OnExperienceTransferred;
    /// <summary>
    /// 当耐力耗尽时触发 (entity)
    /// </summary>
    public static event Action<GameObject> OnStaminaExhausted;


    // 通知方法
    public static void NotifyEntityKilled(GameObject victim, GameObject killer)
        => OnEntityKilled?.Invoke(victim, killer);

    public static void NotifyPlayerLevelUp(GameObject player)
        => OnPlayerLevelUp?.Invoke(player);

    public static void NotifyDamageDealt(GameObject target, GameObject attacker, float damage, DamageType damageType)
        => OnDamageDealt?.Invoke(target, attacker, damage, damageType);

    public static void NotifyDamageReceived(GameObject target, GameObject attacker, float damage, DamageType damageType)
        => OnDamageReceived?.Invoke(target, attacker, damage, damageType);

    public static void NotifyBeforeDeath(GameObject victim, GameObject killer, float damage)
    {
        OnBeforeDeath?.Invoke(victim, killer, damage);
    }

    public static void NotifyHealthLow(GameObject entity, float hpPercent)
        => OnHealthLow?.Invoke(entity, hpPercent);

    public static void NotifySkillUsed(GameObject caster, SkillData skill)
        => OnSkillUsed?.Invoke(caster, skill);

    public static void NotifySkillLearned(GameObject player, SkillData skill)
        => OnSkillLearned?.Invoke(player, skill);

    public static void NotifyPlayerDied(GameObject player)
        => OnPlayerDied?.Invoke(player);

    public static void NotifyPlayerAttacked(GameObject player)
        => OnPlayerAttacked?.Invoke(player);

    public static void NotifyBeforePlayerAttack(GameObject player)
        => OnBeforePlayerAttack?.Invoke(player);
    public static void NotifyWeaponChanged(GameObject weapon)
    {
        OnWeaponChanged?.Invoke(weapon);
    }

    public static void NotifyExperienceTransferred(GameObject from, GameObject to, float exp)
        => OnExperienceTransferred?.Invoke(from, to, exp);

    public static void NotifyStaminaExhausted(GameObject entity)
    {
        OnStaminaExhausted?.Invoke(entity);
    }
}