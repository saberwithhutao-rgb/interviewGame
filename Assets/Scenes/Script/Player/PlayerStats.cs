using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 玩家属性类，继承自 Stats，增加了经验值、技能点和玩家特有的属性
/// </summary>
public class PlayerStats : Stats
{
    [Header("鉴定属性")]
    [SaveField][SerializeField] protected string playerName = "玩家";
    [SaveField][SerializeField] protected string race = "人族";

    
    [SaveField][SerializeField] protected NetworkVariable<int> skillPoints = new NetworkVariable<int>(100);

    [Header("经验值")]
    [SaveField][SerializeField] private NetworkVariable<float> exp = new NetworkVariable<float>(0f);
    [SerializeField] private float expToNextLevelBase = 10f;

    // ─── 经验值属性 ───
    /// <summary>
    /// 获取当前累计的经验值
    /// </summary>
    /// <returns></returns>
    public float GetCurrentExp() => exp.Value;

    /// <summary>
    /// 获取当前经验值进度比例
    /// </summary>
    /// <returns></returns>
    public float GetExpProgress() => exp.Value / GetExpToNextLevel();

    /// <summary>
    /// 计算升到下一级所需的经验值(不包含当前经验值进度)
    /// </summary>
    /// <returns></returns>
    private float GetExpToNextLevel()
    {
        // 根据你的设计计算
        return expToNextLevelBase + Level * 20f; 
                                             
    }

    // ─── 事件 ───
    /// <summary>
    /// 当经验值发生变化时触发，参数为当前经验值和升级所需经验值
    /// </summary>
    public event Action<float, float> OnExperienceChanged;
    /// <summary>
    /// 当玩家升级时触发，参数为新的等级
    /// </summary>
    public event Action<int> OnPlayerLevelUp;
    /// <summary>
    /// 当技能点发生变化时触发，参数为当前技能点数
    /// </summary>
    public event Action<int> OnSkillPointsChanged;

    /// <summary>
    /// 当网络对象生成时调用，初始化玩家的经验值和技能点
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        if (IsServer)
        {
            currentHP.Value = MaxHP;
            currentMP.Value = MaxMP;
            currentStaminaSP.Value = MaxStaminaSP;
            currentBurstSP.Value = MaxBurstSP;
            skillPoints.Value = 100;
            exp.Value = 0f;
            level.Value = 1;
        }
    }

    /// <summary>
    /// 增加经验值的方法，如果经验值达到升级所需的阈值，则触发升级逻辑
    /// </summary>
    /// <param name="amount">增加的经验值数量</param>
    public override void AddExperience(float amount)
    {
        if (!IsServer) return;

        base.AddExperience(amount);

        float newExp = exp.Value + amount;

        while (newExp >= GetExpToNextLevel())
        {
            newExp -= GetExpToNextLevel();
            LevelUp();
        }

        exp.Value = newExp;
        SystemMessageManager.Instance.Achievement(
            "经验值增加 {0} 点，当前经验值为 {1} 点。",
            amount, exp.Value
        );
        OnExperienceChanged?.Invoke(exp.Value, GetExpToNextLevel());
    }

    private void LevelUp()
    {
        AddLevel(1);

        // ─── 全属性提升 ───
        baseAttack += 2f;
        baseDefense += 2f;
        baseMagicAttack += 1f;
        baseMagicDefense += 1f;
        baseSpeed += 0.2f;
        baseMaxHP += 2f;
        baseMaxMP += 2f;
        baseMaxBurstSP += 2f;
        baseMaxStaminaSP += 2f;

        NotifyMaxBurstSPChanged();
        NotifyMaxStaminaSPChanged();

        // ─── 技能点奖励 ───
        SkillPoints += 300;

        // ─── 触发事件 ───
        GameEvents.NotifyPlayerLevelUp(gameObject);

        //    SystemMessageManager.Instance.Achievement(
        //    "《经验值达到一定程度。个体——从 LV{0} 升级为 LV{1}。》",
        //    Level - 1, Level
        //);
        SystemMessageManager.Instance.Achievement(
            "经验值达到一定程度。从 LV{0} 升级为 LV{1}。",
            Level - 1, Level
        );

        OnPlayerLevelUp?.Invoke(Level);
    }

    /// <summary>
    /// 技能点属性，私有设置器确保只能通过方法修改技能点
    /// </summary>
    public int SkillPoints
    {
        get => skillPoints.Value;
        private set
        {
            if (skillPoints.Value != value)
            {
                skillPoints.Value = value;
                OnSkillPointsChanged?.Invoke(skillPoints.Value);
            }
        }
    }

    /// <summary>
    /// 增加技能点的方法，确保技能点不会为负数
    /// </summary>
    /// <param name="amount">增加的技能点数量</param>
    public void AddSkillPoints(int amount)
    {
        if (!IsServer) return;
        SkillPoints += amount;
    }

    /// <summary>
    /// 尝试花费技能点的方法，如果技能点足够则扣除并返回 true，否则返回 false
    /// </summary>
    /// <param name="amount">要花费的技能点数量</param>
    /// <returns>如果技能点足够则返回 true，否则返回 false</returns>
    public bool SpendSkillPoints(int amount)
    {
        if (!IsServer) return false;

        if (SkillPoints >= amount)
        {
            SkillPoints -= amount;
            return true;
        }
        return false;
    }

    // ─── 鉴定信息 ───
    public override string StatsInfoLV1 => $"{playerName}";
    public override string StatsInfoLV2 => $"种族: {race}\n等级: {Level}";
    public override string StatsInfoLV3 => $"HP: {CurrentHP:F0}/{MaxHP}";
    public override string StatsInfoLV5 => $"攻击力: {Attack}\n防御力: {Defense}";

}