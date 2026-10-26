using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// SkillManager 类用于管理玩家的技能系统，包括技能学习、使用、升级和冷却等功能
/// </summary>
public partial class SkillManager : NetworkBehaviour
{
    [SerializeField] private PlayerStats playerStats;

    [Header("技能配置")]
    [SerializeField] private SkillDatabase skillDatabase;
    [Tooltip("初始技能列表（用于新玩家或重置）")]
    [SerializeField] private List<SkillData> initialSkills = new();

    private SkillState state;
    private SkillQueryService query;

    public ISkillQuery Query => query;

    // 内部用，避免到处 state.xxx
    private IReadOnlyList<SkillData> OwnedSkills => state.OwnedSkills;

    /// <summary>
    /// 技能熟练度变化事件，参数为技能数据、当前熟练度、升级所需熟练度
    /// </summary>
    public event Action<SkillData, int, int> OnSkillExpChanged;
    /// <summary>
    /// 技能升级事件，参数为技能数据、旧等级、新等级
    /// </summary>
    public event Action<SkillData, int, int> OnSkillLevelUp;   
    

    private void Awake()
    {
        state = new SkillState();
        query = new SkillQueryService(state);
        InitializeDictionaries();
    }

    public override void OnNetworkSpawn()
    {
        if (!IsOwner) return;
        InitializeDictionaries();
    }

    private void InitializeDictionaries()
    {
        foreach (var skill in OwnedSkills)
        {
            if (skill == null) continue;
            if (!state.TryGetLevel(skill, out _))
                state.SetLevel(skill, 1);
            if (!state.TryGetExp(skill, out _))
                state.SetExp(skill, 0);
        }
    }



    // ===== 命令 =====

    /// <summary>
    /// 学习新技能（添加到技能列表）
    /// </summary>
    /// <param name="newSkill">要学习的技能数据</param>
    /// <returns>是否成功</returns>
    public bool LearnSkill(SkillData newSkill)
    {
        if (newSkill == null) return false;

        if (state.Owns(newSkill)) return false;

        state.Add(newSkill, 1, 0);
        GameEvents.NotifySkillLearned(gameObject, newSkill);

        // 如果是被动技能，立即应用效果
        if (newSkill.SkillType == SkillType.Passive)
            ApplyPassiveOnLearn(newSkill);

        return true;
    }

    /// <summary>
    /// 遗忘技能（从技能列表移除，并清除相关数据）
    /// </summary>
    /// <param name="skill">要遗忘的技能数据</param>
    /// <returns>是否成功</returns>
    public bool UnlearnSkill(SkillData skill)
    {
        if (skill == null) return false;
        if (!state.Owns(skill)) return false;

        // 如果是被动技能，移除效果
        if (skill.SkillType == SkillType.Passive)
            RemovePassiveOnUnlearn(skill);

        state.Remove(skill);
        return true;
    }

    /// <summary>
    /// 增加技能的熟练度，并处理升级逻辑
    /// </summary>
    /// <param name="skill">要增加熟练度的技能数据</param>
    /// <param name="exp">要增加的熟练度值</param>
    public void AddSkillExp(SkillData skill, int exp)
    {
        if (skill == null) return;
        if (!state.TryGetExp(skill, out int current)) return;
        if (!state.TryGetLevel(skill, out int level)) return;
        if (level >= skill.MaxLevel) return;

        int newExp = current + exp;
        state.SetExp(skill, newExp);

        int requiredExp = skill.GetExpToNextLevel(level);
        OnSkillExpChanged?.Invoke(skill, newExp, requiredExp);

        while (level < skill.MaxLevel && newExp >= skill.GetExpToNextLevel(level))
        {
            newExp -= skill.GetExpToNextLevel(level);
            int oldLevel = level;
            level++;
            state.SetLevel(skill, level);
            OnSkillLevelUp?.Invoke(skill, oldLevel, level);
        }
    }

    // ===== 查询转发（保留，供内部和旧调用方用） =====
    public bool HasSkill(SkillData skill) => state.Owns(skill);

    public bool TryGetSkillLevel(SkillData skill, out int level)
        => state.TryGetLevel(skill, out level);

    public bool TryGetSkillExp(SkillData skill, out int exp)
        => state.TryGetExp(skill, out exp);

    public IReadOnlyList<SkillData> GetOwnedSkills() => state.OwnedSkills;

    public SkillData FindSkillById(int id)
    {
        foreach (var skill in OwnedSkills)
        {
            if (skill != null && skill.SkillId == id)
                return skill;
        }
        return skillDatabase?.GetById(id);
    }

    public SkillData FindOwnedSkillById(int id)
    {
        foreach (var skill in OwnedSkills)
        {
            if (skill != null && skill.SkillId == id)
                return skill;
        }
        return null;
    }



    // ===== 存档 =====

    public List<SkillSaveData> GetSaveData()
    {
        var saveList = new List<SkillSaveData>();
        float now = Time.time;

        foreach (var skill in OwnedSkills)
        {
            if (skill == null) continue;

            float remainingCooldown = 0f;
            if (state.TryGetCooldownReadyTime(skill, out float ready))
                remainingCooldown = Mathf.Max(0f, ready - now);

            state.TryGetLevel(skill, out int level);
            state.TryGetExp(skill, out int exp);

            saveList.Add(new SkillSaveData
            {
                skillId = skill.SkillId,
                level = level,
                exp = exp,
                remainingCooldown = remainingCooldown
            });
        }
        return saveList;
    }

    public void LoadSaveData(List<SkillSaveData> saveData)
    {
        if (saveData == null) return;

        state.Clear();

        foreach (var data in saveData)
        {
            SkillData skill = FindSkillById(data.skillId);
            if (skill == null)
            {
                Debug.LogWarning($"技能 {data.skillId} 未找到，跳过加载");
                continue;
            }

            int newLevel = Mathf.Clamp(data.level, 1, skill.MaxLevel);
            int newExp = Mathf.Max(0, data.exp);

            state.Add(skill, newLevel, newExp);

            if (data.remainingCooldown > 0f)
                state.SetCooldown(skill, Time.time + data.remainingCooldown);
        }

    }

    public void InitializeDefaults()
    {
        state.Clear();

        foreach (var skill in initialSkills)
        {
            if (skill == null) continue;
            state.Add(skill, 1, 0);
        }
    }

    // ===== 被动辅助（占位，你按现有实现填） =====

    private void ApplyPassiveOnLearn(SkillData skill)
    {
        if (skill.SkillType == SkillType.Passive)
        {
            if (!state.TryGetLevel(skill, out int level))
            {
                Debug.LogWarning($"技能 {skill.SkillName} 未找到等级信息，无法应用被动效果");
                return;
            }
            SkillContext context = new SkillContext
            {
                caster = gameObject,
                skillLevel = level
            };

            foreach (var effect in skill.Effects)
            {
                if (effect is IPassiveEffect passive)
                {
                    passive.Apply(context);
                }
            }
        }
        else
        {
            Debug.LogWarning($"技能 {skill.SkillName} 不是被动技能，无法应用被动效果");
        }
    }

    private void RemovePassiveOnUnlearn(SkillData skill)
    {
        if (skill.SkillType == SkillType.Passive)
        {
            if (!state.TryGetLevel(skill, out int level))
            {
                Debug.LogWarning($"技能 {skill.SkillName} 未找到等级信息，无法移除被动效果");
                return;
            }
            SkillContext context = new SkillContext
            {
                caster = gameObject,
                skillLevel = level
            };
            foreach (var effect in skill.Effects)
            {
                if (effect is IPassiveEffect passive)
                {
                    passive.Remove(context);
                }
            }
        }
        else
        {
            Debug.LogWarning($"技能 {skill.SkillName} 不是被动技能，无法移除被动效果");
        }
    }


    private void ReapplyPassivesAfterLoad() { /* 遍历 Passive 调 Apply，注意防重入 */ }
}