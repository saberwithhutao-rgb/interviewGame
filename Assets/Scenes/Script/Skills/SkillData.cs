using UnityEngine;

/// <summary>
/// 技能类型枚举
/// </summary>
public enum SkillType
{
    Active,   // 主动技能
    Passive   // 被动技能
}

public enum SkillCastType
{
    Self,       // 自身释放（治疗、Buff、升级回复）
    Direction,  // 朝固定方向释放（鉴定、射线、弹道）
    Target,     // 锁定目标
    Area,       // 范围释放（AOE、爆炸，暂时预留）
}

/// <summary>
/// 技能数据类，使用 ScriptableObject 存储技能信息
/// </summary>
[CreateAssetMenu(fileName = "Skill_01", menuName = "Skill数据")]
public class SkillData : ScriptableObject
{
    [Header("Identity")]
    [SerializeField] private int skillId; //作为技能的唯一ID
    public int SkillId => skillId;

    [Header("技能类型")]
    [SerializeField] private SkillType skillType = SkillType.Active;
    public SkillType SkillType => skillType;

    [Header("施法类型")]
    [SerializeField] private SkillCastType castType = SkillCastType.Self;
    public SkillCastType CastType => castType;

    [Header("技能参数")]
    [Tooltip("技能名称")]
    [SerializeField] private string skillName = "鉴定";
    public string SkillName => skillName;

    [Tooltip("技能描述")]
    [TextArea] [SerializeField] private string description = "";
    public string Description => description;
    
    [Tooltip("技能图标")]
    [SerializeField] private Sprite icon;
    public Sprite Icon => icon;

    [Tooltip("冷却时间（秒）")]
    [SerializeField] private float cooldown = 0f;        
    public float Cooldown => cooldown;

    [Tooltip("施法时间（秒）")]
    [SerializeField] private float castTime = 0f;            
    public float CastTime => castTime;

    [Tooltip("效果数值（如伤害或治疗量）")]
    [SerializeField] private float effectValue = 0f;        
    public float EffectValue => effectValue;

    [Tooltip("技能等级")]
    [SerializeField] private int skillLevel = 1;
    public int SkillLevel => skillLevel;

    [Tooltip("技能经验值")]
    [SerializeField] private int expPerUse = 1;
    public int ExpPerUse => expPerUse;

    [Tooltip("升级所需经验值")]
    [SerializeField] private int[] expToNextLevel = { 10, 50, 140, 300, 550, 910, 1400, 2040, 2850, 0 };
    public int MaxLevel => expToNextLevel?.Length ?? 0;


    [Tooltip("购买所需技能点")]
    [SerializeField] private int skillPointCost = 1;
    public int SkillPointCost => skillPointCost;

    [Tooltip("MP消耗")]
    [SerializeField] private int mpCost = 0;
    public int MpCost => mpCost;

    [Header("技能效果")]
    [SerializeField] private SkillEffect[] effects;
    public SkillEffect[] Effects => effects;

    [Header("被动技能配置")]
    [SerializeField] private PassiveTriggerCondition passiveTrigger = PassiveTriggerCondition.None;
    public PassiveTriggerCondition PassiveTrigger => passiveTrigger;

    //编辑器校验：确保 skillId 不为空且格式规范
    private void OnValidate()
    {
        if (skillId == 0)
        {
            Debug.LogWarning($"技能 [{SkillName}] 的 SkillId 为 0，请设置唯一 ID！");
        }
    }

    public int GetExpToNextLevel(int currentLevel)
    {
        int index = currentLevel - 1;
        if (expToNextLevel == null || index < 0 || index >= expToNextLevel.Length) return 0;
        return expToNextLevel[index];
    }
}