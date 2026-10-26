using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Stats 类是一个抽象类，继承自 NetworkBehaviour，并实现了 IExperienceSource 和 IExperienceReceiver 接口。
/// </summary>
public abstract class Stats : NetworkBehaviour, IExperienceSource, IExperienceReceiver
{
    // ─── 基础属性 ───
    [Header("基础属性")]
    [SaveField][SerializeField] protected float baseAttack = 10f;
    [SaveField][SerializeField] protected float baseDefense = 10f;
    [SaveField][SerializeField] protected float baseMagicAttack = 10f;
    [SaveField][SerializeField] protected float baseMagicDefense = 10f;
    [SaveField][SerializeField] protected float baseSpeed = 6f;
    [SaveField][SerializeField] protected float baseMaxHP = 10f;
    [SaveField][SerializeField] protected float baseMaxMP = 10f;
    [SaveField][SerializeField] protected float baseMaxBurstSP = 30f;
    [SaveField][SerializeField] protected float baseMaxStaminaSP = 30f;

    // ─── 倍率 ───
    [Header("倍率")]
    [SaveField][SerializeField] protected float attackMultiplier = 1f;
    [SaveField][SerializeField] protected float defenseMultiplier = 1f;
    [SaveField][SerializeField] protected float magicAttackMultiplier = 1f;
    [SaveField][SerializeField] protected float magicDefenseMultiplier = 1f;
    [SaveField][SerializeField] protected float speedMultiplier = 1f;
    [SaveField][SerializeField] protected float sprintMultiplier = 1.5f;
    [SaveField][SerializeField] protected float maxHPMultiplier = 1f;
    [SaveField][SerializeField] protected float maxMPMultiplier = 1f;
    [SaveField][SerializeField] protected float maxBurstSPMultiplier = 1f;
    [SaveField][SerializeField] protected float maxStaminaSPMultiplier = 1f;
    [SaveField][SerializeField] protected float hpMultiplier = 1f;

    [SaveField] private float mpConsumptionMultiplier = 1f;
    [SaveField] private float mpRegenMultiplier = 1f;

    // ─── 恢复 ───
    [Header("基础回复")]
    [SerializeField] private float baseMpRegenInterval = 60f;
    [SerializeField] private float baseMpRegenAmount = 1f;
    private float mpRegenTimer = 0f;

    [SerializeField] private float baseSpRegenInterval = 0.5f;
    [SerializeField] private float baseSpRegenAmount = 1f;
    //private float spRegenTimer = 0f;

    [Header("僵直")]
    [SerializeField] protected float stiffnessThreshold = 0.1f;

    // NetworkVariable字段
    [SaveField][SerializeField] protected NetworkVariable<int> level = new NetworkVariable<int>(1);


    // ─── 当前值 ───
    [Header("当前值")]
    [SaveField]protected NetworkVariable<float> currentHP = new NetworkVariable<float>();
    [SaveField]protected NetworkVariable<float> currentMP = new NetworkVariable<float>();
    [SaveField]protected NetworkVariable<float> currentBurstSP = new NetworkVariable<float>();
    [SaveField]protected NetworkVariable<float> currentStaminaSP = new NetworkVariable<float>();

    // ─── 公共属性 ───
    public virtual int Level => level.Value;
    public virtual float Attack => baseAttack * attackMultiplier;
    public virtual float Defense => baseDefense * defenseMultiplier;
    public virtual float MagicAttack => baseMagicAttack * magicAttackMultiplier;
    public virtual float MagicDefense => baseMagicDefense * magicDefenseMultiplier;
    public virtual float Speed => baseSpeed * speedMultiplier;
    public virtual float SprintSpeed => Speed * sprintMultiplier;
    public virtual float MaxHP => baseMaxHP * hpMultiplier;
    public virtual float MaxMP => baseMaxMP * maxMPMultiplier;
    public virtual float MaxBurstSP => baseMaxBurstSP * maxBurstSPMultiplier;
    public virtual float MaxStaminaSP => baseMaxStaminaSP * maxStaminaSPMultiplier;

    public float MpConsumptionMultiplier => mpConsumptionMultiplier;
    public float MpRegenMultiplier => mpRegenMultiplier;
    public float StiffnessThreshold => stiffnessThreshold;

    // ─── 当前值 ───
    public float CurrentHP => currentHP.Value;
    public float CurrentMP => currentMP.Value;
    public float CurrentBurstSP => currentBurstSP.Value;
    public float CurrentStaminaSP => currentStaminaSP.Value;
    public float CurrentHPPct => currentHP.Value / MaxHP;
    public float CurrentMPPct => currentMP.Value / MaxMP;
    public float CurrentBurstSPPct => currentBurstSP.Value / MaxBurstSP;
    public float CurrentStaminaSPPct => currentStaminaSP.Value / MaxStaminaSP;

    // ─── 事件 ───
    public event Action<float> OnHPChanged;
    public event Action<float> OnMPChanged;
    public event Action<float> OnBurstSPChanged;
    public event Action<float> OnStaminaSPChanged;
    public event Action<float> OnMaxBurstSPChanged;
    public event Action<float> OnMaxStaminaSPChanged;

    // ─── Network 初始化 ───
    public override void OnNetworkSpawn()
    {
        //先订阅事件，再更新数值
        currentHP.OnValueChanged += (old, val) => OnHPChanged?.Invoke(val);
        currentMP.OnValueChanged += (old, val) => OnMPChanged?.Invoke(val);
        currentBurstSP.OnValueChanged += (old, val) => OnBurstSPChanged?.Invoke(val);
        currentStaminaSP.OnValueChanged += (old, val) => OnStaminaSPChanged?.Invoke(val);

        if (IsServer)
        {
            currentHP.Value = MaxHP;
            currentMP.Value = MaxMP;
            currentBurstSP.Value = MaxBurstSP;
            currentStaminaSP.Value = MaxStaminaSP;
        }

    }

    private void Update()
    {
        if (!IsServer) return;

        // 只有实体存活时才回复（不死亡、没销毁）
        if (CurrentMP < MaxMP)
        {
            mpRegenTimer += Time.deltaTime * mpRegenMultiplier; 
            if (mpRegenTimer >= baseMpRegenInterval)
            {
                mpRegenTimer = 0f;
                IncreaseMP(baseMpRegenAmount);
            }
        }

        if (CurrentBurstSP < MaxBurstSP)
        {
            // 每帧增加，速度 = 总量 / 时间
            float regenSpeed = baseSpRegenAmount / baseSpRegenInterval; 
            float increase = regenSpeed * Time.deltaTime;
            IncreaseBurstSP(increase);
        }
    }

    public virtual void AddLevel(int amount)
    {
        if (!IsServer) return;
        level.Value += amount;
    }

    // ─── HP 修改 ───
    public virtual void ReduceHP(float amount)
    {
        if (!IsServer) return;
        currentHP.Value = Mathf.Max(currentHP.Value - amount, 0);
    }

    public virtual void IncreaseHP(float amount)
    {
        if (!IsServer) return;
        currentHP.Value = Mathf.Min(currentHP.Value + amount, MaxHP);
    }

    public void SetHP(float hp)
    {
        if (!IsServer) return;
        currentHP.Value = Mathf.Clamp(hp, 0, MaxHP);
    }

    // ─── MP 修改 ───
    public virtual void ReduceMP(float amount)
    {
        if (!IsServer) return;

        float finalAmount = amount * mpConsumptionMultiplier;
        currentMP.Value = Mathf.Max(currentMP.Value - finalAmount, 0);
    }

    public virtual void IncreaseMP(float amount)
    {
        if (!IsServer) return;
        currentMP.Value = Mathf.Min(currentMP.Value + amount, MaxMP);
    }

    public void SetMP(float mp)
    {
        if (!IsServer) return;
        currentMP.Value = Mathf.Clamp(mp, 0, MaxMP);
    }

    // ─── SP 修改 ───
    public virtual void ReduceBurstSP(float amount)
    {
        if (!IsServer) return;
        currentBurstSP.Value = Mathf.Max(currentBurstSP.Value - amount, 0);
    }

    public virtual void ReduceStaminaSP(float amount)
    {
        if (!IsServer) return;
        currentStaminaSP.Value = Mathf.Max(currentStaminaSP.Value - amount, 0);
    }
    public virtual void IncreaseBurstSP(float amount)
    {
        if (!IsServer) return;
        currentBurstSP.Value = Mathf.Min(currentBurstSP.Value + amount, MaxBurstSP);
    }
    public virtual void IncreaseStaminaSP(float amount)
    {
        if (!IsServer) return;
        currentStaminaSP.Value = Mathf.Min(currentStaminaSP.Value + amount, MaxStaminaSP);
    }

    public virtual void SetBurstSP(float sp)
    {
        if (!IsServer) return;
        currentBurstSP.Value = Mathf.Clamp(sp, 0, MaxBurstSP);
    }

    public virtual void SetStaminaSP(float sp)
    {
        if (!IsServer) return;
        currentStaminaSP.Value = Mathf.Clamp(sp, 0, MaxStaminaSP);
    }

    /// <summary>
    /// 检查是否有足够的 SP 可消耗（黄色 SP 1:1，红色 SP 1:3 倍率）
    /// </summary>
    /// <param name="amount">需要消耗的 SP 数量</param>
    /// <returns>是否足够</returns>
    public virtual bool HasEnoughSP(float amount)
    {
        if (currentBurstSP.Value >= amount)
            return true;

        float remaining = amount - currentBurstSP.Value;
        return 3 * remaining <= currentStaminaSP.Value;
    }

    /// <summary>
    /// 尝试消耗 SP（优先消耗黄色 SP，若不足则消耗红色 SP，红色 SP 消耗倍率为3倍）
    /// </summary>
    /// <param name="amount">要消耗的 SP 数量</param>
    /// <returns>是否成功消耗 SP</returns>
    public virtual bool ConsumeSP(float amount)
    {
        if (!HasEnoughSP(amount))
            return false;
        // 优先扣黄色
        if (currentBurstSP.Value >= amount)
        {
            currentBurstSP.Value -= amount;
            return true;
        }

        // 黄色不够，扣光黄色，剩余扣红色
        float remaining = amount - currentBurstSP.Value;
        currentBurstSP.Value = 0;
        currentStaminaSP.Value = Mathf.Max(currentStaminaSP.Value - 3 * remaining, 0);

        if (currentStaminaSP.Value <= 0.001f && IsServer)
        {
            currentStaminaSP.Value = 0;
            GameEvents.NotifyStaminaExhausted(gameObject);
        }

        return true;
    }

    // ─── 最大生命值修改 ───
    public virtual void IncreaseMaxHealth(float multiplier)
    {
        if (!IsServer) return;
        float percent = currentHP.Value / MaxHP;
        baseMaxHP *= multiplier;
        currentHP.Value = MaxHP * percent;
        OnHPChanged?.Invoke(percent);
    }

    public virtual void DecreaseMaxHealth(float multiplier)
    {
        if (!IsServer) return;
        baseMaxHP /= multiplier;
        currentHP.Value = Mathf.Min(currentHP.Value, MaxHP);
        OnHPChanged?.Invoke(currentHP.Value / MaxHP);
    }

    // ─── Buff 修改接口 ───
    public virtual void AddAttackMultiplier(float mult){
        if (!IsServer) return;
        attackMultiplier *= mult;
    }

    public virtual void RemoveAttackMultiplier(float mult)
    {
        if (!IsServer) return;
        attackMultiplier /= mult;
    }

    public virtual void AddSpeedMultiplier(float mult)
    {
        if (!IsServer) return;
        speedMultiplier *= mult;
    }

    public virtual void RemoveSpeedMultiplier(float mult)
    {
        if (!IsServer) return;
        speedMultiplier /= mult;
    }

    public virtual void AddHPMultiplier(float mult)
    {
        if (!IsServer) return;
        hpMultiplier *= mult;
    }

    public virtual void RemoveHPMultiplier(float mult)
    {
        if (!IsServer) return;
        hpMultiplier /= mult;
    }


    /// <summary>
    /// 设置增益效果的倍率，包括最大生命值、速度和攻击力的倍率。
    /// </summary>
    /// <param name="maxHPMult">最大生命值的倍率</param>
    /// <param name="speedMult">速度的倍率</param>
    /// <param name="attackMult">攻击力的倍率</param>
    public virtual void SetBuffs(float maxHPMult, float speedMult, float attackMult)
    {
        if (!IsServer) return;

        hpMultiplier *= maxHPMult;
        speedMultiplier *= speedMult;
        attackMultiplier *= attackMult;

        if (IsSpawned)
        {
            float percent = currentHP.Value / MaxHP;
            currentHP.Value = MaxHP * percent;
            OnHPChanged?.Invoke(percent);
        }
    }

    // ─── MP 消耗倍率修改 ───
    public void SetMPConsumptionMultiplier(float multiplier)
    {
        mpConsumptionMultiplier = multiplier;
    }

    public void AddMPConsumptionMultiplier(float additive)
    {
        mpConsumptionMultiplier += additive;
    }

    public void MultiplyMPConsumptionMultiplier(float multiplier)
    {
        mpConsumptionMultiplier *= multiplier;
    }

    // ─── MP 回复倍率修改 ───
    public void MultiplyMPRegenMultiplier(float multiplier)
    {
        mpRegenMultiplier *= multiplier;
    }

    // ─── IExperienceSource / IExperienceReceiver ───
    public virtual float GetBaseExperience() => 0f;
    public virtual void AddExperience(float amount) { }

    // ─── 抽象鉴定信息 ───
    public abstract string StatsInfoLV1 { get; }
    public abstract string StatsInfoLV2 { get; }
    public abstract string StatsInfoLV3 { get; }
    public abstract string StatsInfoLV5 { get; }

    // ─── 事件方法 ───
    // 这些方法用于在属性变化时通知外部订阅者
    protected void NotifyMaxBurstSPChanged()
    {
        OnMaxBurstSPChanged?.Invoke(MaxBurstSP);
    }

    protected void NotifyMaxStaminaSPChanged()
    {
        OnMaxStaminaSPChanged?.Invoke(MaxStaminaSP);
    }
}