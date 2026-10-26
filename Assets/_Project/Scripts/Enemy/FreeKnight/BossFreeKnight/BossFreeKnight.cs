using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

public enum BossSkillType
{
    None,
    Roar,
}

/// <summary>
/// Boss 版 FreeKnight，不巡逻，只在原地检测玩家
/// </summary>
public class BossFreeKnight : FreeKnight
{
    [Header("Boss 专属")]
    //[SerializeField] private float jumpMoveSpeed = 15f;
    [SerializeField] private float jumpDamageRadius = 3f;
    [SerializeField] private float jumpHeight = 3f;
    [SerializeField] private float jumpDuration = 1f;

    [Header("攻击预警")]
    [SerializeField] private GameObject telegraphPrefab;
    private TelegraphIndicator currentTelegraph;
    private Vector3 jumpTargetPosition;

    [Header("咆哮技能")]
    [SerializeField] private float roarInterval = 4f;          // 每隔 4 秒尝试一次
    [SerializeField] private float roarChance = 0.5f;          // 50% 概率释放
    [SerializeField] private int roarMPCost = 20;              // MP 消耗
    [SerializeField] private float roarAttackMultiplier = 2f;  // 下次攻击伤害翻倍

    private BossSkillType currentSkill = BossSkillType.None;
    private float roarTimer = 0f;
    private bool isRoarActive = false;

    [Header("调试")]
    [SerializeField] private bool enableDebug = false;
    private float debugTimer = 0f;

    private bool isJumping = false;

    protected override void Update()
    {
        base.Update();

        if (currentState == EnemyState.Skill) return;  // 感觉这些代码都挺有问题

        if (player == null) return;

        // 咆哮触发计时
        roarTimer += Time.deltaTime;
        if (roarTimer >= roarInterval)
        {
            roarTimer = 0f;
            TryUseSkill(BossSkillType.Roar);
        }

    }

    private void TryUseSkill(BossSkillType skillType)
    {
        if (currentState == EnemyState.Skill) return;

        switch (skillType)
        {
            case BossSkillType.Roar:
                if (CanUseRoar())
                {
                    currentSkill = BossSkillType.Roar;
                    SwitchState(EnemyState.Skill);
                }
                break;
        }
    }

    private bool CanUseRoar()
    {
        if (isRoarActive) return false;             // 已经在翻倍状态，不重复
        if (enemyStats.CurrentMP < roarMPCost) return false;  // MP 不足
        if (Random.value > roarChance) return false;          // 概率未命中
        return true;
    }

    protected override void ExecuteSkill()
    {
        switch (currentSkill)
        {
            case BossSkillType.Roar:
                ExecuteRoar();
                break;
        }
    }

    private void ExecuteRoar()
    {
        // 扣 MP
        enemyStats.ReduceMP(roarMPCost);

        // 播放动画
        animator.SetTrigger("Skill_Roar");

        // 攻击力翻倍
        enemyStats.AddAttackMultiplier(roarAttackMultiplier);
        isRoarActive = true;

        // 通知客户端播特效
        RoarEffectClientRpc();

        SystemMessageManager.Instance.Warning("Boss 使用了咆哮技能！");
    }

    [ClientRpc]
    private void RoarEffectClientRpc()
    {
        // 客户端播放咆哮特效、屏幕震动等
    }

    public void OnRoarEnd()
    {
        if (!IsServer)
        {
            OnRoarEndServerRpc();
            return;
        }

        currentSkill = BossSkillType.None;
        SwitchState(hasSpottedPlayer ? EnemyState.Chase : EnemyState.Idle);
    }

    [ServerRpc]
    private void OnRoarEndServerRpc()
    {
        currentSkill = BossSkillType.None;
        SwitchState(hasSpottedPlayer ? EnemyState.Chase : EnemyState.Idle);
    }


    protected override void UpdateIdle()
    {
        DebugBoss("Idle");

        if (CanSeeTarget(player))
        {
            lostSightTimer = 0f;
            hasSpottedPlayer = true;
            SwitchState(EnemyState.Chase);
        }
        if (hasSpottedPlayer)
        {
            SwitchState(EnemyState.Chase);
        }
    }

    protected override void UpdateChase()
    {
        DebugBoss("Chase");

        if (player == null)
        {
            ReturnToSpawn();
            return;
        }

        // 玩家跑太远，回原位
        float distanceToSpawn = Vector3.Distance(transform.position, spawnPosition);
        if (distanceToSpawn > enemyStats.ReturnDistance)
        {
            ReturnToSpawn();
            return;
        }

        // 检测玩家是否还在视野内
        if (CanSeeTarget(player))
        {
            lostSightTimer = 0f;
        }
        else
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= enemyStats.LoseSightTime)
            {
                ReturnToSpawn();
                return;
            }
        }

        float distance = Vector3.Distance(transform.position, player.position);

        // 进入攻击范围
        if (distance <= enemyStats.RangedAttackRange && Time.time - GetLastAttackTime() >= enemyStats.AttackCooldown)
        {
            SwitchState(EnemyState.Attack);
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }


    protected override void UpdateMoveSpeed()
    {
        if (animator == null) return;
        float multiplier = EnemyBuffManager.SpeedMult;

        // ★ 僵直状态：速度为 0
        if (currentState == EnemyState.Stiffness || isDead)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            agent.velocity = Vector3.zero;
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }


        // ★ Idle 状态（巡逻）
        if (currentState == EnemyState.Idle)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }

        // ★ Chase 状态（追击）
        if (enemyStats.CurrentHPPct <= enemyStats.LowHPPct)
        {
            agent.speed = enemyStats.FastSpeed * multiplier;
            animator.SetFloat("MoveSpeed", 1f);
        }
        else
        {
            agent.speed = enemyStats.Speed * multiplier;
            animator.SetFloat("MoveSpeed", 0.5f);
        }
    }

    /// <summary>
    /// 消耗咆哮效果（攻击命中后调用）
    /// </summary>
    private void ConsumeRoarEffect()
    {
        if (!isRoarActive) return;
        isRoarActive = false;
        enemyStats.RemoveAttackMultiplier(roarAttackMultiplier);
    }

    protected override void DealDamage()
    {
        base.DealDamage();
        ConsumeRoarEffect();
    }

    /// <summary>
    /// 攻击决策：远距离用 Jump，近距离随机用两个近战
    /// </summary>
    public override void PerformAttack()
    {
        //DebugBoss("Attack");

        if (Time.time - GetLastAttackTime() < enemyStats.AttackCooldown) return;
        if (player == null) return;

        // 面向玩家
        Vector3 direction = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= enemyStats.AttackRange)
        {
            // 近距离：随机两个近战
            int index = Random.Range(0, 2);
            if (index == 0)
                animator.SetTrigger("Attack_Left_Swipe");
            else
                animator.SetTrigger("Attack_Right_Punch");
        }
        else if (distance <= enemyStats.RangedAttackRange)
        {
            // 中距离：跳跃攻击
            PerformJumpAttack();
        }
        else
        {
            // 超过远程范围：只追击，不攻击
            // 不触发任何攻击动画
            return;
        }

        SetLastAttackTime(Time.time);
    }

    /// <summary>
    /// Attack_Jump：禁用 NavMeshAgent，用代码移动，落地后造成伤害
    /// </summary>
    private void PerformJumpAttack()
    {
        isJumping = true;

        if (player != null)
        {
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0;
            transform.rotation = Quaternion.LookRotation(direction);
        }

        animator.SetTrigger("Attack_Jump");

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        Vector3 targetPos = player != null ? player.position : transform.position;

        // 采样半径为 5f，够覆盖玩家周围的可行走区域
        if (NavMesh.SamplePosition(targetPos, out NavMeshHit hit, 5f, NavMesh.AllAreas))
        {
            targetPos = hit.position;
        }
        else
        {
            // 如果采样失败（比如玩家跳到了非 NavMesh 区域），保底用玩家原本位置
            Debug.LogWarning($"[BossFreeKnight] NavMesh 采样失败，使用默认位置: {targetPos}");
        }

        jumpTargetPosition = targetPos;

        // 通知所有客户端显示红圈
        SpawnTelegraphClientRpc(targetPos, jumpDamageRadius, jumpDuration);

        StartCoroutine(JumpRoutine());
    }

    [ClientRpc]
    private void SpawnTelegraphClientRpc(Vector3 position, float radius, float duration)
    {
        if (telegraphPrefab == null) return;

        GameObject obj = Instantiate(telegraphPrefab, position, Quaternion.identity);
        currentTelegraph = obj.GetComponent<TelegraphIndicator>();
        currentTelegraph?.Show(position, radius, duration);
    }

    private System.Collections.IEnumerator JumpRoutine()
    {
        Vector3 startPos = transform.position;
        Vector3 targetPos = jumpTargetPosition;

        if (agent != null) agent.enabled = false;

        float elapsed = 0f;

        while (elapsed < jumpDuration)
        {
            float t = elapsed / jumpDuration;

            // 水平插值
            Vector3 horizontal = Vector3.Lerp(startPos, targetPos, t);

            // 抛物线高度：从 startPos.y 起跳，落到 targetPos.y
            float y = Mathf.Lerp(startPos.y, targetPos.y, t) + Mathf.Sin(t * Mathf.PI) * jumpHeight;

            transform.position = new Vector3(horizontal.x, y, horizontal.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        // 落地：精确设置为目标位置
        transform.position = targetPos;

        if (agent != null)
        {
            agent.enabled = true;
            agent.Warp(targetPos);
        }
    }

    /// <summary>
    /// 动画事件：落地时调用，造成范围伤害
    /// </summary>
    public void OnJumpLand()
    {
        isJumping = false;

        // 隐藏红圈
        HideTelegraphClientRpc();

        if (!IsServer) return;

        Collider[] hits = Physics.OverlapSphere(transform.position, jumpDamageRadius);
        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            IDamageable target = hit.GetComponentInParent<IDamageable>();
            if (target != null)
            {
                target.TakeDamage(enemyStats.Attack, gameObject);
            }
        }

        ConsumeRoarEffect();
    }

    [ClientRpc]
    private void HideTelegraphClientRpc()
    {
        if (currentTelegraph != null)
        {
            currentTelegraph.Hide();
            Destroy(currentTelegraph.gameObject, 0.5f);
            currentTelegraph = null;
        }
    }

    protected override void ReturnToSpawn()
    {
        Debug.Log($"[Boss][ReturnToSpawn] 从 {transform.position} 回到 {spawnPosition}");
        base.ReturnToSpawn();
    }

    // ============================================================
    // 僵直控制
    // ============================================================

    /// <summary>
    /// Attack_Jump 期间不可被打断
    /// </summary>
    protected override bool CanBeStunned()
    {
        if (isJumping) return false;
        if (currentState == EnemyState.Skill) return false;
        return base.CanBeStunned();
    }

    // ============================================================
    // 辅助方法（访问父类私有字段）
    // ============================================================

    // Enemy.cs 里的 lastAttackTime 是 private，子类访问不了
    // 需要你在 Enemy.cs 里把它改成 protected，或者加 getter/setter
    private float GetLastAttackTime()
    {
        return lastAttackTime;
    }

    private void SetLastAttackTime(float time)
    {
        lastAttackTime = time;
    }

    private void DebugBoss(string context)
    {
        if (!enableDebug) return;
        if (player == null) return;

        debugTimer += Time.deltaTime;
        if (debugTimer < 0.5f) return;
        debugTimer = 0f;

        float distance = Vector3.Distance(transform.position, player.position);
        float distanceToSpawn = Vector3.Distance(transform.position, spawnPosition);

        string info = $"[Boss][{context}] " +
            $"State={currentState} | " +
            $"Distance={distance:F1} | " +
            $"AttackRange={enemyStats.AttackRange} | " +
            $"RangedRange={enemyStats.RangedAttackRange} | " +
            $"SightRange={enemyStats.SightRange} | " +
            $"Cooldown={Time.time - lastAttackTime:F1}/{enemyStats.AttackCooldown} | " +
            $"DistToSpawn={distanceToSpawn:F1}/{enemyStats.ReturnDistance} | " +
            $"IsOnNavMesh={agent.isOnNavMesh} | " +
            $"AgentRadius={agent.radius} | " +
            $"HasSpotted={hasSpottedPlayer} | " +
            $"LostSightTimer={lostSightTimer:F1}/{enemyStats.LoseSightTime}";

        Debug.Log(info);
    }
}