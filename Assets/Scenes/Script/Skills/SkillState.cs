using System.Collections.Generic;

public class SkillState
{
    private readonly List<SkillData> ownedSkills = new();
    private readonly Dictionary<SkillData, int> skillLevel = new();
    private readonly Dictionary<SkillData, int> skillExp = new();
    private readonly Dictionary<SkillData, float> cooldowns = new();

    public IReadOnlyList<SkillData> OwnedSkills => ownedSkills;
    public IReadOnlyDictionary<SkillData, int> SkillLevels => skillLevel;

    public bool Owns(SkillData skill) => skill != null && ownedSkills.Contains(skill);

    public bool TryGetLevel(SkillData skill, out int level)
    {
        if (skill == null) { level = 0; return false; }
        return skillLevel.TryGetValue(skill, out level);
    }

    public bool TryGetExp(SkillData skill, out int exp)
    {
        if (skill == null) { exp = 0; return false; }
        return skillExp.TryGetValue(skill, out exp);
    }
    /// <summary>
    /// null = 没拥有；false = 拥有但不在冷却；true = 拥有且在冷却
    /// </summary>
    /// <param name="skill"></param>
    /// <param name="now"></param>
    /// <returns></returns>
    public bool? IsOnCooldown(SkillData skill, float now)
    {
        if (skill == null || !ownedSkills.Contains(skill)) return null;
        return cooldowns.TryGetValue(skill, out var ready) && now < ready;
    }

    public bool TryGetCooldownReadyTime(SkillData skill, out float readyTime)
    {
        if (skill == null) { readyTime = 0f; return false; }
        return cooldowns.TryGetValue(skill, out readyTime);
    }

    public void Add(SkillData skill, int level = 1, int exp = 0)
    {
        if (!ownedSkills.Contains(skill)) ownedSkills.Add(skill);
        skillLevel[skill] = level;
        skillExp[skill] = exp;
    }

    public void SetLevel(SkillData skill, int level)
        => skillLevel[skill] = level;

    public void SetExp(SkillData skill, int exp)
        => skillExp[skill] = exp;

    public void SetCooldown(SkillData skill, float readyTime)
        => cooldowns[skill] = readyTime;

    public void Remove(SkillData skill)
    {
        ownedSkills.Remove(skill);
        skillLevel.Remove(skill);
        skillExp.Remove(skill);
        cooldowns.Remove(skill);
    }

    public void Clear()
    {
        ownedSkills.Clear();
        skillLevel.Clear();
        skillExp.Clear();
        cooldowns.Clear();
    }
}