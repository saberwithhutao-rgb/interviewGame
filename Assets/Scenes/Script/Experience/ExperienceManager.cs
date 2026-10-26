using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 经验管理器 - 负责所有经验转移和计算逻辑
/// 只在服务器端执行，防止作弊
/// </summary>
public class ExperienceManager : NetworkBehaviour
{
    public static ExperienceManager Instance { get; private set; }

    [Header("经验公式设置")]
    [SerializeField] private bool enableLevelGapPenalty = true;  // 是否启用等级差衰减
    [SerializeField] private float levelGapMultiplier = 0.8f;    // 每超过1级衰减系数

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable()
    {
        GameEvents.OnEntityKilled += OnEntityKilled;
    }

    void OnDisable()
    {
        GameEvents.OnEntityKilled -= OnEntityKilled;
    }

    private void OnEntityKilled(GameObject victim, GameObject killer)
    {
        if (!IsServer) return;

        try
        {
            if (victim == null || killer == null)
            {
                Debug.LogWarning("[ExperienceManager] OnEntityKilled: victim 或 killer 为 null");
                return;
            }
        }
        catch
        {
            Debug.LogWarning("[ExperienceManager] OnEntityKilled: 对象已销毁");
            return;
        }

        // 执行经验转移
        TransferExperience(victim, killer);
    }

    /// <summary>
    /// 核心经验转移方法（必须由服务器调用）
    /// </summary>
    /// <param name="source">被击杀者（提供经验）</param>
    /// <param name="receiver">击杀者（接收经验）</param>
    public void TransferExperience(GameObject source, GameObject receiver)
    {
        // 安全校验：只有服务器执行
        if (!IsServer)
        {
            Debug.LogWarning("[ExperienceManager] 经验转移只能在服务器执行");
            return;
        }

        // 1. 获取源对象经验提供者
        IExperienceSource expSource = source.GetComponent<IExperienceSource>();
        if (expSource == null)
        {
            Debug.LogWarning($"[ExperienceManager] {source.name} 没有 IExperienceSource 组件");
            return;
        }

        // 2. 获取接收者经验接收组件
        IExperienceReceiver expReceiver = receiver.GetComponent<IExperienceReceiver>();
        if (expReceiver == null)
        {
            Debug.LogWarning($"[ExperienceManager] {receiver.name} 没有 IExperienceReceiver 组件");
            return;
        }

        // 3. 获取基础经验值
        float baseExp = expSource.GetBaseExperience();

        // 4. 【扩展点】应用各种公式计算实际经验
        float actualExp = CalculateActualExperience(baseExp, source, receiver);

        // 5. 转移给接收者
        expReceiver.AddExperience(actualExp);

        // 6. 触发事件（可用于UI飘字、日志等）
        GameEvents.NotifyExperienceTransferred(source, receiver, actualExp);

        Debug.Log($"[ExperienceManager] {receiver.name} 从 {source.name} 获得了 {actualExp} 经验 (基础: {baseExp})");
    }

    /// <summary>
    /// 计算实际经验值（所有公式都在这里扩展）
    /// </summary>
    private float CalculateActualExperience(float baseExp, GameObject source, GameObject receiver)
    {
        float exp = baseExp;

        // ---- 扩展点1: 等级差衰减 ----
        if (enableLevelGapPenalty)
        {
            exp = ApplyLevelGapPenalty(exp, source, receiver);
        }

        // ---- 扩展点2: 双倍经验卡 ----
        // exp *= GetExperienceMultiplier(receiver);

        // ---- 扩展点3: 组队分摊 ----
        // exp = ApplyPartySharing(exp, source, receiver);

        // ---- 扩展点4: 首杀奖励 ----
        // exp *= GetFirstKillBonus(source);

        return exp;
    }

    /// <summary>
    /// 等级差衰减逻辑（可单独提取为方法方便调整）
    /// </summary>
    private float ApplyLevelGapPenalty(float exp, GameObject source, GameObject receiver)
    {
        // 尝试获取等级信息
        Stats sourceStats = source.GetComponent<Stats>();
        Stats receiverStats = receiver.GetComponent<Stats>();

        if (sourceStats == null || receiverStats == null)
            return exp;

        int sourceLevel = sourceStats.Level;
        int receiverLevel = receiverStats.Level;

        int levelDiff = receiverLevel - sourceLevel;

        float clamped = Mathf.Clamp(Mathf.Pow(levelGapMultiplier, levelDiff), 0.05f, 2.0f);

        float multiplier = Mathf.Round(clamped * 100f) / 100f;

        return exp * multiplier;
    }

    /// <summary>
    /// 调试方法：手动给玩家增加经验
    /// </summary>
    [ServerRpc(RequireOwnership = false)]
    public void AddDebugExpServerRpc(ulong targetClientId, float amount)
    {
        if (!IsServer) return;

    }
}