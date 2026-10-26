PlayerStats.cs

简述
玩家属性类,挂载在玩家身上


字段                类型		  描述
playerName         string    玩家名称    
race		       string    玩家种族
playerLevel        int       玩家等级
skillPoints        int       技能点数
exp                float     玩家经验值
expToNextLevelBase float     升级所需经验值

PlayerLevel    对外只读属性，返回玩家等级
CurrentExpToNextLevel 私有属性，返回升级所需经验值
SkillPoints 对外只读属性，返回技能点数
StatsInfoLV1 对外只读属性，返回玩家名称
StatsInfoLV2 对外只读属性，返回玩家种族和玩家等级
StatsInfoLV3 对外只读属性，返回当前生命值/最大生命值(非比例)

StatsInfoLV5 对外只读属性，返回攻击力和防御力

方法
GetCurrentExp()                 公开方法，返回当前经验值
GetExpProgress()                公开方法，返回当前经验值占升级所需经验值的百分比
GetExpToNextLevel()             私有方法，返回升级所需经验值

OnNetworkSpawn()                当网络生成时触发，初始化currentHP和exp
AddExperience(float amount)     公开方法，增加经验值，如果经验值达到升级所需经验值，则触发升级逻辑
LevelUp()                       私有方法，处理升级逻辑
AddSkillPoints(int amount)      公开方法，增加技能点数
SpendSkillPoints(int amount)    公开方法，返回bool，消耗技能点数，如果技能点数不足则返回false
GetSaveData()                   公开方法，返回一个PlayerStatsSaveData对象，用于保存玩家属性数据
LoadSaveData(PlayerSaveData data)  公开方法，加载保存的玩家属性数据


事件
OnExperienceChanged  float, float 当经验值发生变化时触发，参数为当前经验值和升级所需经验值
OnPlayerLevelUp int               当玩家升级时触发，参数为新等级
OnSkillPointsChanged int          当技能点数发生变化时触发，参数为当前技能点数


依赖关系
继承自 Stats类
被以下类依赖：
DamageUpEffect
HealthUpEffect
SpeedUpEffect
BulletPoolManager
LevelManager
PlayerBar
PlayerControl
PlayerHealth
SaveManager
SkillManager
SkillSelectionPanel
Gun
Sword
_skillEffect
EndureEffect
HealEffect
HpRegenEffect
MPConsumptionReductionEffect
MPRegenSpeedEffect

注意事项：
无

该文档更新日期：
2026/08/01 江蓝