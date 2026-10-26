using UnityEngine;

/// <summary>
/// 技能效果：玩家升级回复hp和mp
/// </summary>

[CreateAssetMenu(fileName = "_SkillEffect", menuName = "技能/升级回复")]
public class _SkillEffect : SkillEffect
{
    [Header("回复量")]
    [SerializeField] private float healHPPercent = 1f;
    [SerializeField] private float healMPPercent = 1f;

    /// <summary>
    /// 执行技能效果，回复玩家的HP和MP
    /// </summary>
    /// <param name="context"></param>
    public override void Execute(SkillContext context)
    {
        PlayerStats stats = context.caster?.GetComponent<PlayerStats>();
        if (stats == null) return;

        float healHP = stats.MaxHP * healHPPercent;
        float healMP = stats.MaxMP * healMPPercent;

        stats.IncreaseHP(healHP);
        stats.IncreaseMP(healMP);
    }
}