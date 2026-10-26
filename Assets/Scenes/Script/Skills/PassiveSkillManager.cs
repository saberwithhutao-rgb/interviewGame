using System.Collections.Generic;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// 被动技能管理器，负责管理和触发角色的被动技能
/// </summary>
public class PassiveSkillManager : NetworkBehaviour
{
    [SerializeField]private SkillManager skillManager;

    // 缓存被动技能列表
    private List<SkillData> cachedPassiveSkills = new List<SkillData>();
    private bool needRefreshCache = true;


    private Dictionary<(int skillId, int effectIndex), PassiveEffectRuntime> passiveRuntimes
        = new Dictionary<(int, int), PassiveEffectRuntime>();

    private float updateTimer = 0f;
    const float tickInterval = 0.25f;

    void Awake()
    {
        if (skillManager == null)
            skillManager = GetComponent<SkillManager>();
    }



    private void Update()
    {
        if (!IsServer) return;

        updateTimer += Time.deltaTime;
        if (updateTimer < tickInterval) return;
        updateTimer = 0f;

        // 获取所有持续生效的被动技能
        var updateSkills = GetPassiveSkills(PassiveTriggerCondition.OnUpdate);

        foreach (var skill in updateSkills)
        {
            for (int i = 0; i < skill.Effects.Length; i++)
            {
                var effect = skill.Effects[i];
                if (effect == null) continue;

                if (effect is not IPassiveUpdatable updatable) continue;

                var key = (skill.SkillId, i);
                if (!passiveRuntimes.TryGetValue(key, out var runtime))
                {
                    runtime = new PassiveEffectRuntime();
                    passiveRuntimes[key] = runtime;
                }

                if (!skillManager.TryGetSkillLevel(skill, out int level))
                {
                    Debug.LogWarning($"数据异常:{skill.SkillName}");
                    continue;
                }
                SkillContext context = new SkillContext
                {
                    caster = gameObject,
                    skillLevel = level
                };

                updatable.OnUpdate(context, runtime, tickInterval);
            }
        }
    }

    void OnEnable()
    {
        GameEvents.OnBeforeDeath += OnBeforeDeath;
        GameEvents.OnDamageDealt += OnDamageDealt;
        GameEvents.OnDamageReceived += OnDamageReceived;
        GameEvents.OnPlayerLevelUp += OnLevelUp;
        GameEvents.OnSkillUsed += OnSkillUsed;
        GameEvents.OnSkillLearned += OnSkillLearned;
    }

    void OnDisable()
    {
        GameEvents.OnBeforeDeath -= OnBeforeDeath;
        GameEvents.OnDamageDealt -= OnDamageDealt;
        GameEvents.OnDamageReceived -= OnDamageReceived;
        GameEvents.OnPlayerLevelUp -= OnLevelUp;
        GameEvents.OnSkillUsed -= OnSkillUsed;
        GameEvents.OnSkillLearned -= OnSkillLearned;
    }

    // ========================================
    // 获取技能的间隔
    // ========================================

    private float GetIntervalForSkill(SkillData skill)
    {
        if (skill == null || skill.Effects == null) return 60f;

        skillManager.TryGetSkillLevel(skill, out int level);

        foreach (var effect in skill.Effects)
        {
            float interval = effect.GetInterval(level);
            if (interval >= 0f)  // 不是 -1，说明有间隔
                return interval;
        }

        return 60f;  // 默认值
    }

    // ========================================
    // 事件处理方法
    // ========================================

    private void OnBeforeDeath(GameObject victim, GameObject killer, float damage)
    {
        if (victim != gameObject) return;
        if (!IsServer) return;
        foreach (var skill in GetPassiveSkills(PassiveTriggerCondition.OnBeforeDeath))
        {
            TryUseSkill(skill, damage);
        }
    }

    private void OnDamageDealt(GameObject target, GameObject attacker, float damage, DamageType damageType)
    {
        if (attacker != gameObject) return;
        if (!IsServer) return;

        foreach (var skill in GetPassiveSkills(PassiveTriggerCondition.OnDamageDealt))
        {
            TryUseSkill(skill);
        }
    }

    private void OnDamageReceived(GameObject target, GameObject attacker, float damage, DamageType damageType)
    {
        if (target != gameObject) return;
        if (!IsServer) return;

        foreach (var skill in GetPassiveSkills(PassiveTriggerCondition.OnDamageReceived))
        {
            TryUseSkill(skill);
        }
    }


    private void OnLevelUp(GameObject player)
    {
        if (player != gameObject) return;
        if (!IsServer) return;

        foreach (var skill in GetPassiveSkills(PassiveTriggerCondition.OnLevelUp))
        {
            TryUseSkill(skill);
        }
    }

    private void OnSkillUsed(GameObject caster, SkillData skillData)
    {
        if (caster != gameObject) return;
        if (!IsServer) return;

        foreach (var skill in GetPassiveSkills(PassiveTriggerCondition.OnSkillUsed))
        {
            TryUseSkill(skill);
        }
    }

    // ========================================
    // 核心方法
    // ========================================

    private List<SkillData> GetPassiveSkills(PassiveTriggerCondition condition)
    {
        if (needRefreshCache)
            RefreshCache();

        List<SkillData> result = new List<SkillData>();
        foreach (var skill in cachedPassiveSkills)
        {
            if (skill.PassiveTrigger == condition)
                result.Add(skill);
        }
        return result;
    }

    private void RefreshCache()
    {
        cachedPassiveSkills.Clear();
        foreach (var skill in skillManager.GetOwnedSkills())
        {
            if (skill.SkillType == SkillType.Passive)
                cachedPassiveSkills.Add(skill);
        }
        needRefreshCache = false;
    }

    /// <summary>
    /// 当玩家学习新技能时，如果是被动技能，则刷新缓存
    /// </summary>
    /// <param name="player">学习技能的玩家对象</param>
    /// <param name="skill">学习的技能数据</param>
    public void OnSkillLearned(GameObject player, SkillData skill)
    {
        if (skill.SkillType == SkillType.Passive)
            needRefreshCache = true;
    }

    /// <summary>
    /// 当玩家遗忘技能时，如果是被动技能，则刷新缓存
    /// </summary>
    /// <param name="skill">遗忘的技能数据</param>
    public void OnSkillForgotten(SkillData skill)
    {
        if (skill.SkillType == SkillType.Passive)
            needRefreshCache = true;
    }

    //中转站
    private void TryUseSkill(SkillData skill, float value = 0f)
    {
        skillManager.ServerTriggerPassiveSkill(skill);
    }

}