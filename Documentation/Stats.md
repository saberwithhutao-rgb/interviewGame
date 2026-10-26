文件名
Stats.md

简述文件功能
作为游戏中所有属性类的基类，提供基础属性和方法

字段
baseAttack        float	  平均攻击能力
baseDefense       float	  平均防御能力
baseMagicAttack   float	  平均魔法能力
baseMagicDefense  float	  平均抵抗能力
baseSpeed         float	  平均速度能力
baseMaxHP         float	  最大生命值
baseMaxMP         float	  最大魔法值
baseMaxBurstSP    float	  最大爆发力
baseMaxStaminaSP  float	  最大耐力

attackMultiplier  float   攻击力倍率
defenseMultiplier float   防御力倍率
magicAttackMultiplier  float 魔法倍率
magicDefenseMultiplier  float 抵抗倍率
speedMultiplier  float 速度倍率
sprintMultiplier  float 冲刺倍率
maxHPMultiplier  float 最大生命值倍率
maxMPMultiplier  float 最大魔法值倍率
maxBurstSPMultiplier  float 最大爆发力倍率
maxStaminaSPMultiplier  float 最大耐力倍率
hpMultiplier  float 生命值倍率

mpConsumptionMultiplier float mp消耗倍率

baseMpRegenInterval float  mp回复间隔
baseMpRegenAmount float  mp回复量
mpRegenMultiplier float  mp回复倍率
mpRegenTimer float  mp回复计时器

baseSpRegenInterval float  sp回复间隔
baseSpRegenAmount float  sp回复量
spRegenTimer float  sp回复计时器(暂时未使用)

stiffnessThreshold float  硬直阈值(受到伤害大于该值时会进入硬直状态)

currentHP float 当前生命值
currentMP float 当前魔法值
currentBurstSP float 当前爆发力
currentStaminaSP float 当前耐力

Attack 对外只读属性，返回平均攻击能力
Defense 对外只读属性，返回平均防御能力
MagicAttack 对外只读属性，返回平均魔法能力
MagicDefense 对外只读属性，返回平均抵抗能力
Speed 对外只读属性，返回平均速度能力
SprintSpeed 对外只读属性，返回冲刺速度能力
MaxHP 对外只读属性，返回最大生命值
MaxMP 对外只读属性，返回最大魔法值
MaxBurstSP 对外只读属性，返回最大爆发力
MaxStaminaSP 对外只读属性，返回最大耐力

MpConsumptionMultiplier 对外只读属性，返回mp消耗倍率
MpRegenMultiplier 对外只读属性，返回mp回复倍率
StiffnessThreshold 对外只读属性，返回硬直阈值

CurrentHP 对外只读属性，返回当前生命值
CurrentMP 对外只读属性，返回当前魔法值
CurrentBurstSP 对外只读属性，返回当前爆发力
CurrentStaminaSP 对外只读属性，返回当前耐力
CurrentHPPct 对外只读属性，返回当前生命值占最大生命值的百分比
CurrentMPPct 对外只读属性，返回当前魔法值占最大魔法值的百分比
CurrentBurstSPPct 对外只读属性，返回当前爆发力占最大爆发力的百分比
CurrentStaminaSPPct 对外只读属性，返回当前耐力占最大耐力的百分比

方法
OnNetworkSpawn() 当网络生成时触发，订阅及初始化currentHP、currentMP、currentBurstSP、currentStaminaSP
Update() 每帧调用，处理mp回复逻辑，burstSP回复逻辑
ReduceHP(float amount) 公开方法，减少生命值，参数为减少的生命值量
IncreaseHP(float amount) 公开方法，增加生命值，参数为增加的生命值量
SetHP(float hp) 公开方法，设置生命值，参数为设置的生命值量
ReduceMP(float amount) 公开方法，减少魔法值，参数为减少的魔法值量
IncreaseMP(float amount) 公开方法，增加魔法值，参数为增加的魔法值量
SetMP(float mp) 公开方法，设置魔法值，参数为设置的魔法值量
ReduceBurstSP(float amount) 公开方法，减少爆发力，参数为减少的爆发力量
IncreaseBurstSP(float amount) 公开方法，增加爆发力，参数为增加的爆发力量
ReduceStaminaSP(float amount) 公开方法，减少耐力，参数为减少的耐力量
IncreaseStaminaSP(float amount) 公开方法，增加耐力，参数为增加的耐力量
SetBurstSP(float sp) 公开方法，设置爆发力，参数为设置的爆发力量
SetStaminaSP(float sp) 公开方法，设置耐力，参数为设置的耐力量
ConsumeSP(float amount) 公开方法，返回bool，优先消耗爆发力，其次耐力，参数为消耗的SP量，如果SP不足则返回false
IncreaseMaxHealth(float multiplier) 公开方法，增加最大生命值，参数为增加的倍率
DecreaseMaxHealth(float multiplier) 公开方法，减少最大生命值，参数为减少的倍率

AddAttackMultiplier(float mult) 公开方法，增加攻击力倍率，参数为增加的倍率
RemoveAttackMultiplier(float mult) 公开方法，减少攻击力倍率，参数为减少的倍率
AddSpeedMultiplier(float mult) 公开方法，增加速度倍率，参数为增加的倍率
RemoveSpeedMultiplier(float mult) 公开方法，减少速度倍率，参数为减少的倍率
AddHPMultiplier(float mult) 公开方法，增加生命值倍率，参数为增加的倍率
RemoveHPMultiplier(float mult) 公开方法，减少生命值倍率，参数为减少的倍率

事件
OnHPChanged float 当生命值发生变化时触发，参数为更新后的生命值
OnMPChanged float 当魔法值发生变化时触发，参数为更新后的魔法值
OnBurstSPChanged float 当爆发力发生变化时触发，参数为更新后的爆发力
OnStaminaSPChanged float 当耐力发生变化时触发，参数为更新后的耐力
OnMaxBurstSPChanged float 当最大爆发力发生变化时触发，参数为更新后的最大爆发力
OnMaxStaminaSPChanged float 当最大耐力发生变化时触发，参数为更新后的最大耐力

依赖关系

注意事项

更新文档日期