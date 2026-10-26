using System.Collections.Generic;

public interface ISkillQuery
{
    bool Owns(SkillData skill);

    bool TryGetLevel(SkillData skill, out int level);
    bool TryGetExp(SkillData skill, out int exp);

    bool? IsOnCooldown(SkillData skill, float now);
    bool TryGetCooldownReadyTime(SkillData skill, out float readyTime);

    IReadOnlyList<SkillData> GetOwnedSkills();
    IReadOnlyDictionary<SkillData, int> GetSkillLevels();
}