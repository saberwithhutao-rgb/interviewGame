using System.Collections.Generic;

public class SkillQueryService : ISkillQuery
{
    private readonly SkillState state;

    public SkillQueryService(SkillState state)
    {
        this.state = state ?? throw new System.ArgumentNullException(nameof(state));
    }

    public bool Owns(SkillData skill) => state.Owns(skill);

    public bool TryGetLevel(SkillData skill, out int level)
        => state.TryGetLevel(skill, out level);

    public bool TryGetExp(SkillData skill, out int exp)
        => state.TryGetExp(skill, out exp);

    public bool? IsOnCooldown(SkillData skill, float now)
        => state.IsOnCooldown(skill, now);

    public bool TryGetCooldownReadyTime(SkillData skill, out float readyTime)
        => state.TryGetCooldownReadyTime(skill, out readyTime);

    public IReadOnlyList<SkillData> GetOwnedSkills() => state.OwnedSkills;

    public IReadOnlyDictionary<SkillData, int> GetSkillLevels() => state.SkillLevels;
}