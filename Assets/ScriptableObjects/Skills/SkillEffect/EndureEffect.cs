using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 忍耐技能效果类，继承自 SkillEffect
/// </summary>
[CreateAssetMenu(fileName = "EndureEffect", menuName = "技能/忍耐")]
public class EnduranceEffect : SkillEffect
{
    [SerializeField] private float mpCostPct = 0.1f; // 忍耐技能的 MP 消耗占受到伤害的百分比（0-100）

    /// <summary>
    /// 执行忍耐技能效果，将施法者的 HP 设置为 1，并消耗相应的 MP
    /// </summary>
    /// <param name="context">技能上下文，包含施法者等信息</param>
    public override void Execute(SkillContext context)
    {
        PlayerStats stats = context.caster?.GetComponent<PlayerStats>();
        if (stats == null) return;

        float mpCost = stats.MaxMP * mpCostPct;
        if (stats.CurrentMP < mpCost)
        {
            Debug.Log($"[忍耐] MP 不足，无法使用忍耐技能。当前 MP={stats.CurrentMP}, 需要 MP={mpCost}");
            return;
        }
        stats.ReduceMP(mpCost);

        stats.SetHP(1);  // 直接改 HP
        
        Debug.Log($"[忍耐]以 HP=1 存活，消耗 MP={mpCost}");
    }
}