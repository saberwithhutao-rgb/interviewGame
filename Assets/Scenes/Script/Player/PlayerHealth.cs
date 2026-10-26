using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using Unity.Services.Qos.V2.Models;
using Unity.VisualScripting.Antlr3.Runtime.Misc;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 玩家血量类，负责处理玩家的生命值、死亡和重生逻辑
/// </summary>
public class PlayerHealth : NetworkBehaviour, IDamageable
{
    [Header("动画")]
    [SerializeField] private Animator animator;
    
    [SerializeField] private PlayerStats playerStats;

    private bool isDead = false;
    public event Action<GameObject> OnPlayerDied;

    [SerializeField] private float respawnTime = 5f;
    private CharacterController characterController;

    public bool IsDead => isDead;


    void Awake()
    {
        if (animator == null)
            animator = GetComponent<Animator>();
        if (playerStats == null)
        {
            playerStats = GetComponent<PlayerStats>();
        }
        characterController = GetComponent<CharacterController>();
    }

    void Start()
    {
        PlayerTracker.Instance?.Register(transform);
    }

    void OnEnable()
    {
        GameEvents.OnStaminaExhausted += OnStaminaExhausted;
    }

    void OnDisable()
    {
        GameEvents.OnStaminaExhausted -= OnStaminaExhausted;
    }

    public override void OnDestroy()
    {
        PlayerTracker.Instance.Unregister(transform);
        base.OnDestroy();  
    }

    [Obsolete]
    [ServerRpc(RequireOwnership = false)]
    public void TakeDamageServerRpc(float damage, NetworkBehaviourReference attackerRef, DamageType damageType)
    {
        GameObject attacker = attackerRef.TryGet(out NetworkBehaviour behaviour) ? behaviour.gameObject : null;
        ExecuteTakeDamage(damage, attacker, damageType);
    }

    
    public void TakeDamage(float damage, GameObject attacker = null, DamageType damageType = DamageType.Physical)
    {
        if (!IsServer)
        {
            //NetworkBehaviourReference attackerRef = new NetworkBehaviourReference(attacker?.GetComponent<NetworkBehaviour>());
            //TakeDamageServerRpc(damage, attackerRef, damageType);
            return;
        }

        ExecuteTakeDamage(damage, attacker, damageType);
    }

    /// <summary>
    /// 执行伤害逻辑（只在服务器调用）
    /// </summary>
    private void ExecuteTakeDamage(float damage, GameObject attacker, DamageType damageType)
    {
        float finalDamage = DamageSystem.CalculateDamage(gameObject, attacker, damage, damageType);

        // 伤害一定会发生
        playerStats.ReduceHP(finalDamage);
        // 如果 HP ≤ 0，触发被动技能
        if (playerStats.CurrentHP <= 0)
        {
            GameEvents.NotifyBeforeDeath(gameObject, attacker, finalDamage);
        }
        // 触发伤害事件
        GameEvents.NotifyDamageDealt(gameObject, attacker, finalDamage, damageType);
        GameEvents.NotifyDamageReceived(gameObject, attacker, finalDamage, damageType);


        if (playerStats.CurrentHP <= 0)
            Die(attacker);
    }

    [ClientRpc]
    private void DieClientRpc()
    {
        animator?.SetTrigger("Dead");
        // 禁用移动、开枪等
    }

    private void OnStaminaExhausted(GameObject entity)
    {
        if (entity != gameObject) return;
        if (!IsServer) return;
        Die();
    }

    void Die(GameObject killer = null)
    {
        if (!IsServer) return;
        if (isDead) return;
        isDead = true;


        PlayerTracker.Instance?.Unregister(transform);

        DieClientRpc();  // 广播给所有客户端
        OnPlayerDied?.Invoke(gameObject);

        GameEvents.NotifyPlayerDied(gameObject);
        GameEvents.NotifyEntityKilled(gameObject, killer);

        if (SceneManager.GetActiveScene().name == "MultiplayerScene")
        {
            StartCoroutine(RespawnAfterDelay());
        }
    }

    IEnumerator RespawnAfterDelay()
    {
        yield return new WaitForSeconds(respawnTime);

        // 选择重生点
        Vector3 spawnPos = UnityEngine.Random.insideUnitSphere * 5f;
        spawnPos.y = 0;

        // 关闭 CharacterController 再重置位置
        if (characterController != null)
            characterController.enabled = false;
        transform.position = spawnPos;
        if (characterController != null)
            characterController.enabled = true;
        Gun gun = GetComponentInChildren<Gun>();
        if (gun != null)
            gun.enabled = true;


        // 重置血量
        playerStats.SetHP(playerStats.MaxHP);
        playerStats.SetStaminaSP(playerStats.MaxStaminaSP);
        playerStats.SetMP(playerStats.MaxMP);
        isDead = false;
        PlayerTracker.Instance?.Register(transform);

        // 通知客户端重生
        RespawnClientRpc(spawnPos);
    }

    [ClientRpc]
    void RespawnClientRpc(Vector3 spawnPos)
    {
        // 关闭 CharacterController 再重置位置
        if (characterController != null)
            characterController.enabled = false;
        transform.position = spawnPos;
        if (characterController != null)
            characterController.enabled = true;

        // 重置动画状态
        if (animator != null)
        {
            animator.Rebind();
            animator.Update(0f);
        }

        // 重新启用控制
        PlayerControl playerControl = GetComponent<PlayerControl>();
        if (playerControl != null)
            playerControl.enabled = true;

        Gun gun = GetComponentInChildren<Gun>();
        if (gun != null)
            gun.enabled = true;
        isDead = false;
    }
}