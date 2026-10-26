using UnityEngine;

/// <summary>
/// 技能效果：鉴定目标的属性信息
/// </summary>
[CreateAssetMenu(fileName = "AppraiseEffect", menuName = "技能/鉴定")]
public class AppraiseEffect : SkillEffect
{
    public float maxDistance = 30f;

    /// <summary>
    /// 执行技能效果，鉴定目标的属性信息，并显示在鉴定面板上
    /// </summary>
    /// <param name="skillContext"></param>
    public override void Execute(SkillContext skillContext)
    {
        GameObject target = skillContext.target;

        if (target == null) return;

        // 获取 Stats
        Stats stats = target.GetComponentInParent<Stats>();
        if (stats == null) return;

        int level = skillContext.skillLevel;
        string info = BuildAppraiseInfo(stats, level);

        // 获取鉴定面板并显示
        GameObject panel = UIManager.Instance.GetPanel($"AppraiseLV{level}Panel");
        if (panel != null)
        {
            panel.GetComponent<AppraisePanel>().Show(info);
        }
    }

    // 不同的技能等级显示不同的属性信息
    string BuildAppraiseInfo(Stats stats, int level)
    {
        string info = "";
        if (level >= 1) info += stats.StatsInfoLV1;
        if (level >= 2) info += stats.StatsInfoLV2;
        if (level >= 3) info += stats.StatsInfoLV3;
        if (level >= 5) info += stats.StatsInfoLV5;
        return info.Trim();
    }
}