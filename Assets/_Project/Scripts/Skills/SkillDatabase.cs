using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 全局技能库，存放所有可学习的技能配置。
/// 使用 ScriptableObject 存储，所有玩家共用同一份数据。
/// </summary>
[CreateAssetMenu(fileName = "SkillDatabase", menuName = "技能/技能库")]
public class SkillDatabase : ScriptableObject
{
    [SerializeField] private List<SkillData> skills = new List<SkillData>();

    public IReadOnlyList<SkillData> Skills => skills;

    // 缓存字典：SkillId → SkillData，用于 O(1) 查找
    private Dictionary<int, SkillData> idLookup;

    public bool IsHaveSkill => skills?.Count > 0;

    /// <summary>
    /// 构建 SkillId → SkillData 的字典缓存。
    /// 在首次调用 GetById 时自动执行。
    /// </summary>
    private void BuildLookup()
    {
        if (!IsHaveSkill)
        {
            Debug.LogError("技能库为空，请检查 SkillDatabase 资源是否正确配置！");
            return;
        }
        idLookup = new Dictionary<int, SkillData>();

        foreach (var skill in skills)
        {
            if (skill == null) continue;
            if (!idLookup.ContainsKey(skill.SkillId))
                idLookup[skill.SkillId] = skill;
        }
    }

    /// <summary>
    /// 根据 SkillId 查找技能数据。
    /// 这是网络请求、存档加载、技能验证的核心查询方法。
    /// </summary>
    /// <param name="skillId">技能的稳定唯一标识符（如 "SK_HEAL"）</param>
    /// <returns>找到的 SkillData，未找到则返回 null</returns>
    public SkillData GetById(int skillId)
    {
        if (!IsHaveSkill)
        {
            Debug.LogError("技能库为空，请检查 SkillDatabase 资源是否正确配置！");
            return null;
        }
        if (idLookup == null || idLookup.Count == 0)
            BuildLookup();

        idLookup.TryGetValue(skillId, out SkillData result);
        return result;
    }

    public string GetNameById(int skillId)
    {
        SkillData data = GetById(skillId);
        return data ? data.SkillName : null;
    }


    /// <summary>
    /// 编辑器验证方法，在 Inspector 中修改资源时自动调用。
    /// 检查：
    /// 1. 每个技能是否有 SkillId
    /// 2. SkillId 是否重复
    /// </summary>
    private void OnValidate()
    {
        var seen = new HashSet<int>();
        foreach (var skill in skills)
        {
            if (skill == null) continue;

            if (skill.SkillId == 0)
            {
                Debug.LogError($"技能 [{skill.SkillName}] 的 SkillId 为 0！");
                continue;
            }

            if (seen.Contains(skill.SkillId))
            {
                Debug.LogError($"重复的 SkillId: {skill.SkillId} 出现在技能 [{skill.SkillName}]");
            }
            else
            {
                seen.Add(skill.SkillId);
            }
        }
        idLookup = null; // 配置变了，缓存失效
    }
}