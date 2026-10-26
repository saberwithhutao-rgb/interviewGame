using System.Collections;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 骷髅战士 - 近战敌人，死亡后有概率复活并进入狂暴状态
/// </summary>
public class SkeletonWarrior : Enemy
{
    [Header("===== 复活设置 =====")]
    [SerializeField] private float resurrectChance = 0.2f;        // 复活概率 20%
    [SerializeField] private float resurrectHPRatio = 0.5f;       // 复活后血量比例 50%
    [SerializeField] private float resurrectDelay = 2f;           // 倒地等待时间
    [SerializeField] private float resurrectAnimDuration = 1.5f;  // 复活动画时长

    [Header("===== 复活增益 =====")]
    [SerializeField] private float resurrectAttackMultiplier = 1.3f;  // 攻击 +30%
    [SerializeField] private float resurrectSpeedMultiplier = 1.2f;   // 速度 +20%

    [Header("===== 复活特效 =====")]
    [SerializeField] private GameObject resurrectVFXPrefab;       // 复活特效预制体
    [SerializeField] private GameObject glowVFXPrefab;            // 发光特效预制体
    [SerializeField] private Transform vfxSpawnPoint;             // 特效生成点

    [Header("===== 复活音效 =====")]
    [SerializeField] private AudioClip resurrectStartSound;       // 复活开始音效
    [SerializeField] private AudioClip resurrectCompleteSound;    // 复活完成音效
    [SerializeField] private AudioClip glowLoopSound;             // 持续发光音效


    [Header("===== 材质切换 =====")]
    [SerializeField] private Material normalBoneMaterial;         // 正常骨骼材质
    [SerializeField] private Material normalArmorMaterial;        // 正常盔甲材质
    [SerializeField] private Material glowBoneMaterial;           // 发光骨骼材质
    [SerializeField] private Material glowArmorMaterial;          // 发光盔甲材质
    [SerializeField] private Renderer[] bodyRenderers;            // 身体渲染器数组

    // 复活状态
    private bool hasResurrected = false;
    private bool isResurrecting = false;
    private GameObject glowVFXInstance;

    // 事件
    public event System.Action OnResurrectStarted;
    public event System.Action OnResurrectCompleted;

    void Reset()
    {
        sfxVolume = 0.8f;
    }

    protected override void Awake()
    {
        base.Awake();

        // 确保有默认材质引用
        if (bodyRenderers == null || bodyRenderers.Length == 0)
        {
            bodyRenderers = GetComponentsInChildren<Renderer>();
        }
    }

    protected override void Start()
    {
        base.Start();
        SkeletonWarriorManager.Instance.RegisterEnemy(this);
    }

    // ============================================================
    // 死亡处理 - 核心复活逻辑
    // ============================================================

    protected override void Die(GameObject killer = null)
    {
        if (isDead) return;

        // 判定是否触发复活
        if (!hasResurrected && ShouldResurrect())
        {
            // 触发复活流程
            StartCoroutine(ResurrectSequence());
        }
        else
        {
            // 正常死亡流程
            SkeletonWarriorManager.Instance.UnregisterEnemy(this);
            base.Die(killer);
        }
    }

    /// <summary>
    /// 判定是否应该复活
    /// </summary>
    private bool ShouldResurrect()
    {
        float roll = Random.value;
        Debug.Log($"[{name}] 复活判定: 随机值={roll:F3}, 阈值={resurrectChance}, 结果={roll < resurrectChance}");
        return roll < resurrectChance;
    }

    // ============================================================
    // 复活序列
    // ============================================================

    private IEnumerator ResurrectSequence()
    {
        isDead = true;
        isResurrecting = true;

        Debug.Log($"[{name}] 开始复活序列");
        OnResurrectStarted?.Invoke();

        // === 阶段1: 假装死亡 ===

        // 播放死亡动画
        animator.SetTrigger("Dead");

        // 停止导航
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.velocity = Vector3.zero;
        }

        // 禁用碰撞（防止继续受击）
        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }

        // 等待倒地时间
        yield return new WaitForSeconds(resurrectDelay);

        // === 阶段2: 复活过程 ===

        Debug.Log($"[{name}] 开始复活");

        // 播放复活开始音效
        if (resurrectStartSound != null)
        {
            AudioManager.Instance.PlayOneShot(resurrectStartSound, sfxVolume);
        }

        // 生成复活特效
        Vector3 vfxPosition = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
        if (resurrectVFXPrefab != null)
        {
            Instantiate(resurrectVFXPrefab, vfxPosition, Quaternion.identity);
        }

        // 播放复活动画
        animator.SetTrigger("Resurrect");

        // 等待复活动画
        yield return new WaitForSeconds(resurrectAnimDuration);

        // === 阶段3: 复活完成 ===

        Debug.Log($"[{name}] 复活完成，进入狂暴状态");

        // 标记已复活
        hasResurrected = true;
        isResurrecting = false;
        isDead = false;

        // 恢复碰撞
        if (collider != null)
        {
            collider.enabled = true;
        }

        // 恢复血量
        float resurrectHP = enemyStats.MaxHP * resurrectHPRatio;
        enemyStats.SetHP(resurrectHP);
        NotifyHealthChanged(enemyStats.CurrentHPPct);

        // 应用狂暴增益
        ApplyResurrectBuffs();

        // 切换外观
        SwitchToGlowAppearance();

        // 生成持续发光特效
        SpawnGlowVFX();

        // 播放复活完成音效
        if (resurrectCompleteSound != null)
        {
            AudioManager.Instance.PlayOneShot(resurrectCompleteSound, sfxVolume);
        }

        // 恢复导航
        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        // 重新启用脚本
        this.enabled = true;

        // 切换到追击状态
        SwitchState(EnemyState.Chase);

        OnResurrectCompleted?.Invoke();

        Debug.Log($"[{name}] 狂暴状态 - 攻击:{enemyStats.Attack}, 速度:{enemyStats.Speed}");
    }

    // ============================================================
    // 增益应用
    // ============================================================

    private void ApplyResurrectBuffs()
    {
        // 应用攻击增益
        enemyStats.AddAttackMultiplier(resurrectAttackMultiplier);

        // 应用速度增益
        enemyStats.AddSpeedMultiplier(resurrectSpeedMultiplier);

        // 更新导航速度
        if (agent != null)
        {
            agent.speed = enemyStats.Speed;
        }
    }

    // ============================================================
    // 外观切换
    // ============================================================

    private void SwitchToGlowAppearance()
    {
        if (bodyRenderers == null) return;

        for (int i = 0; i < bodyRenderers.Length; i++)
        {
            if (bodyRenderers[i] == null) continue;

            // 根据渲染器名称或索引切换材质
            // 这里假设第一个是骨骼，第二个是盔甲
            if (i == 0 && glowBoneMaterial != null)
            {
                bodyRenderers[i].material = glowBoneMaterial;
            }
            else if (i == 1 && glowArmorMaterial != null)
            {
                bodyRenderers[i].material = glowArmorMaterial;
            }
        }
    }

    private void SpawnGlowVFX()
    {
        if (glowVFXPrefab == null) return;

        Vector3 spawnPos = vfxSpawnPoint != null ? vfxSpawnPoint.position : transform.position;
        glowVFXInstance = Instantiate(glowVFXPrefab, spawnPos, Quaternion.identity, transform);

        // 播放发光循环音效
        if (glowLoopSound != null)
        {
            // 这里需要一个循环音效播放方法
            // AudioManager.Instance.PlayLoop(glowLoopSound, sfxVolume * 0.3f);
        }
    }

    // ============================================================
    // 重写受击处理
    // ============================================================

    public override void TakeDamage(float damage, GameObject attacker = null, DamageType damageType = DamageType.Physical)
    {
        // 复活过程中无敌
        if (isResurrecting)
        {
            Debug.Log($"[{name}] 复活中，免疫伤害");
            return;
        }

        base.TakeDamage(damage, attacker, damageType);
    }

    // ============================================================
    // 重写状态更新（可选：添加狂暴状态特殊行为）
    // ============================================================

    protected override void UpdateIdle()
    {
        base.UpdateIdle();

        // 复活后的骷髅视野更敏锐（可选）
        if (hasResurrected)
        {
            // 可以在这里添加狂暴状态的特殊行为
            // 例如：更积极地追击玩家
        }
    }

    // ============================================================
    // 调试
    // ============================================================

    private void OnDrawGizmosSelected()
    {
        // 绘制复活判定范围（可视化调试）
        if (hasResurrected)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, 2f);
        }
    }
}
