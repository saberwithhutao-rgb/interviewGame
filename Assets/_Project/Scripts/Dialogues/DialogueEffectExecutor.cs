using UnityEngine;

/// <summary>
/// 对话效果执行器
/// </summary>
public static class DialogueEffectExecutor
{
    public static void Execute(DialogueNode node, GameObject player)
    {
        if (node == null || !node.HasEffect) return;
        if (player == null)
        {
            Debug.LogWarning("[DialogueEffectExecutor] 玩家对象为空，无法执行效果");
            return;
        }

        switch (node.effectType)
        {
            case "GiveSkill":
                ExecuteGiveSkill(node, player);
                break;

            case "GiveSkillPoints":
                ExecuteGiveSkillPoints(node, player);
                break;

            case "GiveExp":
                ExecuteGiveExp(node, player);
                break;

            case "GiveItem":
                // 背包系统实现后补充
                Debug.LogWarning("[DialogueEffectExecutor] GiveItem 尚未实现");
                break;

            default:
                Debug.LogWarning($"[DialogueEffectExecutor] 未知效果: {node.effectType}");
                break;
        }
    }

    private static void ExecuteGiveSkill(DialogueNode node, GameObject player)
    {
        if (!int.TryParse(node.effectValue, out int skillId)) return;

        SkillManager skillManager = player.GetComponent<SkillManager>();
        if (skillManager == null) return;

        SkillData skill = skillManager.FindSkillById(skillId);
        if (skill != null)
        {
            skillManager.LearnSkill(skill);
            Debug.Log($"[DialogueEffectExecutor] 学习技能: {skill.SkillName}");
        }
    }

    private static void ExecuteGiveSkillPoints(DialogueNode node, GameObject player)
    {
        if (!int.TryParse(node.effectValue, out int amount)) return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.AddSkillPoints(amount);
        Debug.Log($"[DialogueEffectExecutor] 增加技能点: {amount}");
    }

    private static void ExecuteGiveExp(DialogueNode node, GameObject player)
    {
        if (!float.TryParse(node.effectValue, out float amount)) return;

        PlayerStats stats = player.GetComponent<PlayerStats>();
        if (stats == null) return;

        stats.AddExperience(amount);
        Debug.Log($"[DialogueEffectExecutor] 增加经验: {amount}");
    }
}