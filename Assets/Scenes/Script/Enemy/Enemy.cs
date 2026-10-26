using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敌人状态枚举
/// </summary>
public enum EnemyState
{
    Idle,
    Chase,
    Waiting,
    Attack,
    Stiffness,
    Death,
    Skill,
}


/// <summary>
/// 敌人类，继承自 NetworkBehaviour，实现 IDamageable 接口
/// </summary>
public class Enemy : NetworkBehaviour, IDamageable
{
    [SerializeField] protected EnemyStats enemyStats;
    protected EnemyState currentState = EnemyState.Idle;

    protected NavMeshAgent agent;
    protected Transform player;


    [Header("视野")]
    [SerializeField] protected LayerMask sightBlockMask;
    [SerializeField] protected float lostSightTimer;

    protected float lastAttackTime;

    protected bool hasSpottedPlayer;

    [Header("状态")]
    [SerializeField] protected Animator animator;
    protected bool isDead = false;

    [SerializeField] private GameObject damagePopupPrefab;


    protected Vector3 startPosition;                       
    protected Vector3 patrolTarget;                        
    protected float patrolTimer;                           
    protected bool isWaiting = true;
    protected Vector3 spawnPosition;

    //僵直
    private float stiffnessTimer = 0f;
    private float stiffnessDuration = 0.3f;

    [Header("音效")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip hurtSound;
    [SerializeField] private AudioClip deathSound;
    [SerializeField] protected float sfxVolume = 0.8f;

    public event Action<Enemy> OnDamaged;
    public event Action<float> OnHealthChanged;
    public event Action<float> OnAttack;
    public static event Action<Enemy> OnAnyEnemyDied;

    public bool IsDead => isDead;

    private float diagnoseTimer = 0f;

    /// <summary>
    /// 在网络生成时调用，初始化敌人的属性和状态
    /// </summary>
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer)  // 只在服务端初始化 Buff
        {
            enemyStats.SetBuffs(EnemyBuffManager.HealthMult, EnemyBuffManager.SpeedMult, EnemyBuffManager.DamageMult);
        }

        agent.speed = enemyStats.Speed;
    }

    protected virtual void Awake() 
    {
        agent = GetComponent<NavMeshAgent>();
        spawnPosition = transform.position;

        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }


        if (sightBlockMask == 0)
        {
            sightBlockMask = LayerMask.GetMask("Environment");
        }

        if (enemyStats == null)
        {
            enemyStats = GetComponent<EnemyStats>();
        }
    }

    protected virtual void Start()
    {
        if (player == null)
        {
            PlayerControl.OnLocalPlayerSpawned += (pc) =>
            {
                player = pc.transform;
            };
        }
        if (player == null && PlayerControl.LocalInstance != null)
        {
            player = PlayerControl.LocalInstance.transform;
        }
        if (SaveManager.IsDataReady)
        {
            CheckSavedDeathState();
        }
        else
        {
            SaveManager.OnSaveDataReady += OnSaveDataReady;
        }
    }

    /// <summary>
    /// 每帧更新敌人的状态和行为
    /// </summary>
    protected virtual void Update()
    {
        if (!IsServer) return;
        if(isDead || agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) 
            return;

        switch (currentState)
        {
            case EnemyState.Idle:
                UpdateIdle();
                break;
            case EnemyState.Chase:
                UpdateChase();
                break;
            case EnemyState.Waiting: 
                UpdateWaiting(); 
                break;
            case EnemyState.Attack:
                UpdateAttack();
                break;
            case EnemyState.Stiffness:
                UpdateStiffness();  
                break;
            case EnemyState.Skill:
                UpdateSkill();
                break;
        }
        UpdateMoveSpeed();
        //Diagnose();
    }

    void OnEnable()
    {
        GameEvents.OnPlayerDied += OnPlayerDied;
    }

    void OnDisable()
    {
        GameEvents.OnPlayerDied -= OnPlayerDied;
    }

    public override void OnDestroy()
    {
        base.OnDestroy();
        SaveManager.OnSaveDataReady -= OnSaveDataReady;

    }

    protected virtual void NotifyHealthChanged(float healthPercent)
    {
        OnHealthChanged?.Invoke(healthPercent);
    }

    private void OnSaveDataReady()
    {
        SaveManager.OnSaveDataReady -= OnSaveDataReady; 
        CheckSavedDeathState();
    }

    private void CheckSavedDeathState()
    {
        EnemyPersistentId idComp = GetComponent<EnemyPersistentId>();
        if (idComp == null || string.IsNullOrEmpty(idComp.UniqueId)) return;

        bool isSavedDead = false;
        float savedHP = 0f;

        // 1. 查表
        if (SaveManager.TryGetEnemySaveData(idComp.UniqueId, out EnemySaveData data))
        {
            isSavedDead = data.isDead;
            savedHP = data.currentHP;
        }

        // 2. 根据查到的状态执行
        if (isSavedDead)
        {
            // 如果存档里是死的，直接隐藏，不参与游戏
            if (agent != null && agent.isActiveAndEnabled) agent.enabled = false;
            Collider col = GetComponent<Collider>();
            if (col != null) col.enabled = false;
            isDead = true;
            this.enabled = false;
            gameObject.SetActive(false);
        }
        else
        {
            // 如果活着（或者没查到，默认活着），恢复血量
            if (IsServer && savedHP > 0f)
            {
                enemyStats.SetHP(savedHP);
            }
        }
    }

    void OnPlayerDied(GameObject deadPlayer)
    {
        if (player != null && player.gameObject == deadPlayer)
        {
            hasSpottedPlayer = false;
            player = null;
        }

        if (currentState != EnemyState.Death && player == null)
        {
            SwitchState(EnemyState.Idle);
        }
    }

    /// <summary>
    /// 更新 Idle 状态的行为逻辑，包括检测玩家是否在视野范围内
    /// </summary>
    protected virtual void UpdateIdle()
    {
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

    protected virtual void UpdateChase()
    {
        if (player == null)
        {
            // 当前没目标，尝试找一个
            Transform newTarget = FindBestTarget();
            if (newTarget != null)
            {
                player = newTarget;
                hasSpottedPlayer = true;
                lostSightTimer = 0f;
            }
            else
            {
                SwitchState(EnemyState.Idle);
                return;
            }
        }

        if (CanSeeTarget(player))
        {
            lostSightTimer = 0f;
        }
        else
        {
            lostSightTimer += Time.deltaTime;
            if (lostSightTimer >= enemyStats.LoseSightTime)
            {
                // 丢失目标时间到了，尝试换人
                Transform newTarget = FindBestTarget();
                if (newTarget != null && newTarget != player)
                {
                    player = newTarget;
                    lostSightTimer = 0f;
                }
                else if (newTarget == player)
                {
                    // 还是同一个人，但被墙挡了，重置计时器继续追
                    lostSightTimer = 0f;
                }
                else
                {
                    // 没找到任何目标，回 Idle
                    hasSpottedPlayer = false;
                    lostSightTimer = 0f;
                    SwitchState(EnemyState.Idle);
                    return;
                }
            }
        }

        float distance = Vector3.Distance(transform.position, player.position);

        if (distance <= enemyStats.AttackRange)
        {
            SwitchState(EnemyState.Waiting);
        }
        else
        {
            agent.SetDestination(player.position);
        }
    }

    protected virtual void UpdateWaiting()
    {
        if (player == null)
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.velocity = Vector3.zero;
            agent.isStopped = true;
        }

        // 1. 面向玩家
        Vector3 lookDir = (player.position - transform.position).normalized;
        lookDir.y = 0;
        transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(lookDir), Time.deltaTime * 10f);

        // 2. 检查距离，如果玩家跑远了，切回 Chase
        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > enemyStats.AttackRange * 1.5f) // 给一点容错空间，避免边缘反复横跳
        {
            SwitchState(EnemyState.Chase);
            return;
        }

        // 3. 如果冷却好了，切到攻击
        if (Time.time - lastAttackTime >= enemyStats.AttackCooldown)
        {
            SwitchState(EnemyState.Attack);
        }
    }

    void UpdateAttack()
    {
        if (player == null) return;

        PlayerHealth ph = player.GetComponent<PlayerHealth>();
        if (ph == null || ph.IsDead || !player.gameObject.activeSelf) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > enemyStats.AttackRange * 1.5f)
        {
            SwitchState(EnemyState.Chase);
        }
    }

    /// <summary>
    /// 更新僵直状态的行为逻辑，包括计时器和状态切换
    /// </summary>
    protected virtual void UpdateStiffness()
    {
        stiffnessTimer -= Time.deltaTime;
        if (stiffnessTimer <= 0f)
        {
            SwitchState(hasSpottedPlayer ? EnemyState.Chase : EnemyState.Idle);
        }
    }

    protected virtual void UpdateSkill() { }

    protected virtual void UpdateMoveSpeed()
    {
        if (animator == null) return;
        float multiplier = EnemyBuffManager.SpeedMult;

        // ★ 僵直状态：速度为 0
        if (currentState == EnemyState.Stiffness || currentState == EnemyState.Waiting)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            agent.velocity = Vector3.zero;
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }

        // ★ 死亡：速度为 0
        if (isDead)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }

        // ★ Idle 状态（巡逻）
        if (currentState == EnemyState.Idle)
        {
            if (isWaiting)
            {
                agent.isStopped = true;
                agent.speed = 0f;
                animator.SetFloat("MoveSpeed", 0f);
            }
            else
            {
                agent.isStopped = false;
                agent.speed = enemyStats.PatrolSpeed * multiplier;
                animator.SetFloat("MoveSpeed", 0.5f);
            }
            return;
        }

        // ★ Chase 状态（追击）
        if (enemyStats.CurrentHPPct <= enemyStats.LowHPPct)
        {
            agent.speed = enemyStats.FastSpeed * multiplier;
            animator.SetFloat("MoveSpeed", 2f);
        }
        else
        {
            agent.speed = enemyStats.Speed * multiplier;
            animator.SetFloat("MoveSpeed", 1f);
        }

        if (currentState == EnemyState.Skill)
        {
            agent.isStopped = true;
            agent.speed = 0f;
            animator.SetFloat("MoveSpeed", 0f);
            return;
        }
    }

    /// <summary>
    /// 切换敌人的状态，并根据新状态执行相应的行为逻辑
    /// </summary>
    /// <param name="newState">要切换到的新的敌人状态</param>
    protected virtual void SwitchState(EnemyState newState)
    {
        if (agent == null || !agent.isActiveAndEnabled || !agent.isOnNavMesh) return;
        switch (currentState)
        {
            case EnemyState.Chase:
                agent.isStopped = true;
                break;
            case EnemyState.Attack:
                break;
            case EnemyState.Stiffness:
                break;
        } 

        currentState = newState;

        switch (newState)
        {
            case EnemyState.Idle:
                    agent.isStopped = true;
                break;
            case EnemyState.Chase:
                agent.isStopped = false;
                break;
            case EnemyState.Waiting:
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;
                break;
            case EnemyState.Attack:
                agent.isStopped = true;
                PerformAttack();
                break;
            case EnemyState.Stiffness:
                agent.isStopped = true;
                agent.ResetPath();  
                stiffnessTimer = stiffnessDuration;
                animator.ResetTrigger("Stiffness");
                animator.SetTrigger("Stiffness");
                break;
            case EnemyState.Death:
                agent.isStopped = true;
                animator?.SetTrigger("Dead");
                break;
            case EnemyState.Skill:
                agent.isStopped = true;
                ExecuteSkill();
                break;
        }
    }

    protected virtual void ExecuteSkill() { }

    protected bool CanSeeTarget(Transform target)
    {
        if (target == null) return false;
        Vector3 dir = target.position - transform.position;
        float dist = dir.magnitude;
        if (dist > enemyStats.SightRange) return false;
        float angle = Vector3.Angle(transform.forward, dir);
        if (angle > enemyStats.SightAngle / 2f) return false;
        if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dir.normalized, dist, sightBlockMask)) return false;
        return true;
    }

    /// <summary>
    /// 攻击结束时的回调函数，用于切换状态回到追击
    /// </summary>
    protected virtual void OnAttackEnd()
    {
        if (currentState != EnemyState.Attack) return;

        if (player == null)
        {
            SwitchState(EnemyState.Idle);
            return;
        }

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance <= enemyStats.AttackRange * 1.5f)
        {
            // ★ 玩家还在面前，进入等待状态，不要继续追击
            SwitchState(EnemyState.Waiting);
        }
        else
        {
            // 玩家跑远了，重新追
            SwitchState(EnemyState.Chase);
        }
    }

    /// <summary>
    /// 敌人受到伤害的处理逻辑，包括计算最终伤害、减少血量、触发僵直、播放音效和生成伤害弹窗等
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    /// <param name="attacker">攻击者的游戏对象</param>
    /// <param name="damageType">伤害类型</param>
    public virtual void TakeDamage(float damage, GameObject attacker = null, DamageType damageType = DamageType.Physical)
    {
        if (!IsServer)
        {
            // 客户端：通过 RPC 转发到服务器
            //TakeDamageServerRpc(damage, attacker != null ? attacker.GetComponent<NetworkObject>().NetworkObjectId : 0, damageType);
            return;
        }

        ExecuteTakeDamage(damage, attacker, damageType);
    }

    /// <summary>
    /// 服务器端的 RPC 方法，用于处理客户端发送的伤害请求
    /// </summary>
    /// <param name="damage">受到的伤害值</param>
    /// <param name="attackerNetworkObjectId">攻击者的网络对象 ID</param>
    /// <param name="damageType">伤害类型</param>
    [Obsolete]
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, ulong attackerNetworkObjectId, DamageType damageType)
    {
        GameObject attacker = null;
        if (attackerNetworkObjectId != 0)
        {
            NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(attackerNetworkObjectId, out var netObj);
            attacker = netObj?.gameObject;
        }
        ExecuteTakeDamage(damage, attacker, damageType);
    }

    /// <summary>
    /// 执行伤害逻辑（只在服务器调用）
    /// </summary>
    private void ExecuteTakeDamage(float damage, GameObject attacker, DamageType damageType)
    {
        if (isDead) return;


        float finalDamage = DamageSystem.CalculateDamage(gameObject, attacker, damage, damageType);
        enemyStats.ReduceHP(finalDamage);

        // 触发僵直
        if (damage > enemyStats.MaxHP * enemyStats.StiffnessThreshold && CanBeStunned())
        {
            if (currentState != EnemyState.Stiffness)
            {
                SwitchState(EnemyState.Stiffness);
            }
            else
            {
                // 僵直中再次受伤：只重置计时器，刷新动画
                stiffnessTimer = stiffnessDuration;
                animator.ResetTrigger("Stiffness");
                animator.SetTrigger("Stiffness");
            }
        }

        GameEvents.NotifyDamageDealt(gameObject, attacker, finalDamage, damageType);

        if (damagePopupPrefab != null)
        {
            GameObject popup = Instantiate(damagePopupPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            popup.GetComponent<DamagePopup>()?.Setup(damage, damageType);
        }

        if (hurtSound != null)
        {
            AudioManager.Instance.PlayOneShot(hurtSound, sfxVolume);
        }

        OnDamaged?.Invoke(this);

        AlertNearbyAllies();

        if (enemyStats.CurrentHP <= 0)
        {
            enemyStats.SetHP(0);
            OnHealthChanged?.Invoke(0f);
            Die(attacker);
        }
        else
        {
            OnHealthChanged?.Invoke(enemyStats.CurrentHPPct);
            if (!hasSpottedPlayer)
            {
                hasSpottedPlayer = true;
                lostSightTimer = 0f;
            }
        }
    }

    /// <summary>
    /// 警告附近的盟友敌人，通知它们玩家已被发现
    /// </summary>
    void AlertNearbyAllies()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, enemyStats.AlertRadius);

        foreach (Collider col in colliders)
        {
            Enemy enemy = col.GetComponent<Enemy>();
            if (enemy != null && !enemy.isDead && enemy != this)
            {
                enemy.OnAlerted();
            }
        }
    }

    /// <summary>
    /// 当敌人被警告时的处理逻辑，如果当前没有发现玩家，则将其状态切换为追击
    /// </summary>
    void OnAlerted()
    {
        if (isDead) return;
        if (!hasSpottedPlayer)
        {
            hasSpottedPlayer = true;
            lostSightTimer = 0f;
            if (currentState == EnemyState.Idle)
            {
                SwitchState(EnemyState.Chase);
            }
        }
    }

    /// <summary>
    /// 执行攻击动作的方法，包括检查攻击冷却时间、旋转朝向玩家、触发攻击事件和播放攻击动画
    /// </summary>
    public virtual void PerformAttack()
    {
        if (Time.time - lastAttackTime < enemyStats.AttackCooldown) return;

        lastAttackTime = Time.time;

        Vector3 direction  = (player.position - transform.position).normalized;
        direction.y = 0;
        transform.rotation = Quaternion.LookRotation(direction);

        OnAttack?.Invoke(enemyStats.Attack);
        
        animator.SetTrigger("Attack");
    }

    /// <summary>
    /// 处理敌人对玩家造成伤害的逻辑，包括检查距离、播放攻击音效和调用玩家的 TakeDamage 方法
    /// </summary>
    protected virtual void DealDamage()
    {
        if (!IsServer) return;
        if (player == null) return;

        float distance = Vector3.Distance(transform.position, player.position);
        if (distance > enemyStats.AttackRange * 1.5f)
        {
            return;
        } 
        IDamageable target = player.GetComponent<IDamageable>();
        if (target != null)
        {
            if (attackSound != null)
            {
                AudioManager.Instance.PlayOneShot(attackSound, sfxVolume);
            }
            target.TakeDamage(enemyStats.Attack);
        }
    }

    /// <summary>
    /// 处理敌人死亡的逻辑，包括播放死亡音效、触发死亡事件、停止导航、播放死亡动画、禁用碰撞器和延迟销毁敌人对象
    /// </summary>
    /// <param name="killer">击杀敌人的游戏对象</param>
    protected virtual void Die(GameObject killer = null)
    {
        if (isDead) return;
        isDead = true;

        EnemyPersistentId idComp = GetComponent<EnemyPersistentId>();
        if (idComp != null && !string.IsNullOrEmpty(idComp.UniqueId))
        {
            SaveManager.ReportEnemyDeath(idComp.UniqueId);
        }


        if (deathSound != null)
        {
            AudioManager.Instance.PlayOneShot(deathSound, sfxVolume);
        }

        OnAnyEnemyDied?.Invoke(this);
        GameEvents.NotifyEntityKilled(gameObject, killer);

        if (agent != null && agent.isActiveAndEnabled && agent.isOnNavMesh)
        {
            agent.isStopped = true;
            agent.enabled = false;
        }

        animator?.ResetTrigger("Stiffness");
        animator?.ResetTrigger("Dead");
        animator.SetTrigger("Dead");
        animator.SetBool("IsDead", true);

        Collider collider = GetComponent<Collider>();
        if (collider != null)
        {
            collider.enabled = false;
        }
        this.enabled = false;

        StartCoroutine(DelayedDespawn(3f));
    }

    IEnumerator DelayedDespawn(float delay)
    {
        yield return new WaitForSeconds(delay);
        animator.enabled = false;
        if (NetworkObject != null && NetworkObject.IsSpawned)
            NetworkObject.Despawn();
    }

    /// <summary>
    /// 寻找最佳目标的方法，根据距离、视野范围和角度等条件选择最合适的玩家作为目标
    /// </summary>
    /// <returns>返回最合适的玩家目标的 Transform，如果没有找到合适的目标则返回 null</returns>
    Transform FindBestTarget()
    {
        Transform best = null;
        float bestDist = Mathf.Infinity;

        foreach (var t in PlayerTracker.Instance.AlivePlayers)
        {
            if (t == null) continue;  // 防止被意外销毁

            Vector3 dir = t.position - transform.position;
            float dist = dir.magnitude;

            if (dist > enemyStats.SightRange) continue;
            float angle = Vector3.Angle(transform.forward, dir);
            if (angle > enemyStats.SightAngle / 2f) continue;
            if (Physics.Raycast(transform.position + Vector3.up * 1.5f, dir.normalized, dist, sightBlockMask)) continue;

            if (dist < bestDist)
            {
                bestDist = dist;
                best = t;
            }
        }

        return best;
    }

    protected virtual bool CanBeStunned()
    {
        return true;
    }

    /// <summary>
    /// 瞬移回出生点
    /// </summary>
    protected virtual void ReturnToSpawn()
    {
        // 关闭 NavMeshAgent 再瞬移
        if (agent != null && agent.isActiveAndEnabled)
        {
            agent.enabled = false;
            transform.position = spawnPosition;
            agent.enabled = true;
            agent.Warp(spawnPosition);  // 确保 NavMeshAgent 同步位置
        }
        else
        {
            transform.position = spawnPosition;
        }

        // 回到 Idle
        hasSpottedPlayer = false;
        lostSightTimer = 0f;
        SwitchState(EnemyState.Idle);
    }

    void Diagnose()
    {
        diagnoseTimer += Time.deltaTime;
        if (diagnoseTimer < 1f) return;
        diagnoseTimer = 0f;

        if (agent != null && agent.isActiveAndEnabled)
        {
            Debug.Log($"[Diagnose] 状态:{currentState} | 速度:{agent.velocity.magnitude:F2} | 是否停止:{agent.isStopped} | 剩余距离:{agent.remainingDistance:F2}");
        }
    }
}
