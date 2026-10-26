using UnityEngine;

/// <summary>
/// 技能释放的上下文
/// </summary>
public class SkillContext
{
    // ===== 来源信息 =====
    public GameObject caster;          // 施法者
    public GameObject source;          // 伤害来源（用于被动技能）
    public GameObject target;          // 技能目标

    // ===== 位置信息 =====
    public Vector3 castPosition;       // 施法位置
    public Vector3 aimDirection;       // 瞄准方向（主动技能用）
    public Ray aimRay;                 // 瞄准射线（主动技能用）

    // ===== 数值信息 =====
    public float baseValue;            // 基础数值
    public int skillLevel;             // 技能等级
    public float damageAmount;         // 本次伤害值（用于动态消耗技能）
}