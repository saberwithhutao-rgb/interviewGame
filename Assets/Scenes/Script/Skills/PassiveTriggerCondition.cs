/// <summary>
/// 被动技能触发条件枚举
/// </summary>
public enum PassiveTriggerCondition
{
    None, //主动技能
    // ===== 战斗相关 =====
    OnBeforeDeath,          // 死亡前（免死、复活）
    OnDamageDealt,          // 造成伤害时（反伤、吸血、毒伤触发）
    OnDamageReceived,       // 受到伤害时（减伤、护盾、荆棘）
    OnKill,                 // 击杀时（击杀回血、击杀加攻）

    // ===== 成长相关 =====
    OnLevelUp,              // 升级时（升级回复、升级加属性）
    OnExperienceGained,     // 获得经验时（经验加成）

    // ===== 状态相关 =====
    OnHealthLow,            // 血量低于阈值时（狂暴、自动回血）
    OnHealthFull,           // 血量满时（溢出治疗转护盾）

    // ===== 技能相关 =====
    OnSkillUsed,            // 使用技能时（技能连击、减冷却）
    OnSkillCooldown,        // 技能冷却时（冷却缩减）

    // ===== 每时每刻 =====
    OnUpdate,

    // ===== 其他 =====
    OnCombatStart,          // 进入战斗时
    OnCombatEnd,            // 脱离战斗时
    OnStatusApplied,        // 被施加状态时（状态抗性）
    OnStatusTick,           // 状态 Tick 时（毒伤增幅）
}