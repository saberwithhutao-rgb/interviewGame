using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// SkillUnlockManager 类用于管理玩家技能的解锁和购买逻辑
/// </summary>
public class SkillUnlockManager : MonoBehaviour
{
    [Header("全局技能库")]
    [SerializeField] private SkillDatabase skillDatabase;

    [Header("玩家技能状态")]
    [SerializeField] private SkillManager skillManager;

    /// <summary>
    /// 获取当前可购买/可学习的技能列表（尚未拥有的技能）
    /// </summary>
    public List<SkillDisplayInfo> GetBuyableSkills()
    {
        List<SkillDisplayInfo> buyable = new List<SkillDisplayInfo>();

        if (skillDatabase == null || skillDatabase.Skills == null)
        {
            Debug.LogWarning("技能库未配置");
            return buyable;
        }

        if (skillManager == null)
        {
            Debug.LogWarning("SkillManager 未引用");
            return buyable;
        }

        // 遍历所有技能，筛选出玩家尚未拥有的
        foreach (SkillData skill in skillDatabase.Skills)
        {
            if (!IsSkillOwned(skill))
            {
                buyable.Add(new SkillDisplayInfo { skillName = skill.SkillName });
            }
        }

        return buyable;
    }

    /// <summary>
    /// 判断玩家是否已拥有该技能
    /// </summary>
    private bool IsSkillOwned(SkillData skill)
    {
        return skillManager.HasSkill(skill);
    }

    /// <summary>
    /// 根据技能名称获取技能数据
    /// </summary>
    /// <param name="name">技能名称</param>
    /// <returns>技能数据，如果未找到则返回 null</returns>
    public SkillData GetSkillDataByName(string name)
    {
        foreach (var s in skillDatabase.Skills)
            if (s.SkillName == name)
                return s;
        return null;
    }
}