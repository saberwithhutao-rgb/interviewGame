using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class Sword : NetworkBehaviour
{
    [ServerRpc]
    private void RequestAttackServerRpc()
    {
        // 攻击合法性由服务端决定；客户端只发送攻击请求。
        if (!IsServer || isAttacking)
            return;

        double serverNow = NetworkManager.ServerTime.Time;
        if (serverNow < nextAttackTime)
            return;

        if (playerStats == null || !playerStats.ConsumeSP(1f))
            return;

        nextAttackTime = serverNow + attackCooldown;
        isAttacking = true;
        hitTargets.Clear();
        anticipatedIsAttacking.AuthoritativeValue = true;

        GameEvents.NotifyBeforePlayerAttack(playerStats.gameObject);

        if (animator != null)
            animator.SetTrigger("Attack");

        GameEvents.NotifyPlayerAttacked(playerStats.gameObject);

        // 碰撞盒时间独立于 Animator：卡肉暂停动画时，服务端攻击窗口仍按时开关。
        StartCoroutine(AttackDash());
    }

    public void OnAttackStart()
    {
        if (!IsServer) return;

        isAttacking = true;
        hitTargets.Clear();

        if (attackCollider != null)
            attackCollider.enabled = true;

        // 突进由服务端驱动，保证位移权威
        StartCoroutine(AttackDash());
    }

    /// <summary>
    /// 由动画事件调用：剑挥完时关闭碰撞体。
    /// </summary>
    public void OnAttackEnd()
    {
        if (IsServer)
        {
            EndAttack();
        }
        if (IsOwner)
        {
            localIsAttacking = false;
            PlayerControl.LocalInstance?.SetAttackInputLock(false);
        }
    }

    private void EndAttack()
    {
        if (!IsServer)
            return;

        isAttacking = false;
        if (attackCollider != null)
            attackCollider.enabled = false;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsServer || !isAttacking || other == null)
            return;

        PlayerHealth attacker = GetComponentInParent<PlayerHealth>();
        PlayerHealth target = other.GetComponentInParent<PlayerHealth>();
        if (target != null && target == attacker)
            return;

        IDamageable damageable = other.GetComponentInParent<IDamageable>();
        if (damageable == null)
            return;

        // 按承伤组件所在对象去重，避免目标身上多个 Collider 导致一刀多次命中。
        Component damageableComponent = damageable as Component;
        GameObject hitKey = damageableComponent != null ? damageableComponent.gameObject : other.gameObject;
        if (hitTargets.Contains(hitKey))
            return;

        float damage = baseDamage;
        if (playerStats != null)
            damage += playerStats.Attack;

        damageable.TakeDamage(damage, transform.root.gameObject, damageType);
        hitTargets.Add(hitKey);

        // 卡肉发生在服务端已确认命中、已结算伤害之后；只影响动画表现。
        NetworkObject targetNetworkObject = other.GetComponentInParent<NetworkObject>();
        if (targetNetworkObject == null && damageableComponent != null)
            targetNetworkObject = damageableComponent.GetComponentInParent<NetworkObject>();

        ulong targetNetworkObjectId =
            targetNetworkObject != null && targetNetworkObject.IsSpawned
                ? targetNetworkObject.NetworkObjectId
                : ulong.MaxValue;

        HitStopClientRpc(targetNetworkObjectId);
    }

    [ClientRpc]
    private void HitStopClientRpc(ulong targetNetworkObjectId)
    {
        ApplyHitStop(animator);

        // 没有 NetworkObject 的目标（例如纯本地对象）无法通过网络 ID 定位，只暂停攻击者动画。
        if (targetNetworkObjectId == ulong.MaxValue || NetworkManager == null)
            return;

        if (!NetworkManager.SpawnManager.SpawnedObjects.TryGetValue(
                targetNetworkObjectId, out NetworkObject targetNetworkObject))
            return;

        Animator targetAnimator = targetNetworkObject.GetComponentInChildren<Animator>(true);
        if (targetAnimator == null)
            targetAnimator = targetNetworkObject.GetComponentInParent<Animator>();

        if (targetAnimator != null && targetAnimator != animator)
            ApplyHitStop(targetAnimator);
    }

    private void ApplyHitStop(Animator targetAnimator)
    {
        if (targetAnimator == null || hitStopDuration <= 0f)
            return;

        // 每个 Animator 只需要一个计时器；连续命中会延长暂停时间，不会提前恢复。
        AnimatorHitStop hitStop = targetAnimator.GetComponent<AnimatorHitStop>();
        if (hitStop == null)
            hitStop = targetAnimator.gameObject.AddComponent<AnimatorHitStop>();

        hitStop.Apply(hitStopDuration);
    }

    /// <summary>
    /// 在索敌范围内找最近的、实现了 IDamageable 的目标。
    /// 找不到返回 null。
    /// </summary>
    private Transform FindNearestTarget()
    {
        CharacterController cc = GetComponentInParent<CharacterController>();
        if (cc == null) return null;

        Vector3 center = cc.bounds.center;

        Collider[] hits = Physics.OverlapSphere(
            center,
            attackDashLockRange,
            enemyLayerMask,
            QueryTriggerInteraction.Ignore
        );

        Transform nearest = null;
        float nearestSqrDist = float.MaxValue;
        PlayerHealth selfHealth = GetComponentInParent<PlayerHealth>();

        foreach (Collider hit in hits)
        {
            if (hit == null) continue;

            // 排除自己
            PlayerHealth targetHealth = hit.GetComponentInParent<PlayerHealth>();
            if (targetHealth != null && targetHealth == selfHealth) continue;

            // 必须有 IDamageable
            IDamageable damageable = hit.GetComponentInParent<IDamageable>();
            if (damageable == null) continue;

            Component damageableComponent = damageable as Component;
            if (damageableComponent == null) continue;

            // 用 IDamageable 所在物体的位置算距离
            float sqrDist = (damageableComponent.transform.position - center).sqrMagnitude;
            if (sqrDist < nearestSqrDist)
            {
                nearestSqrDist = sqrDist;
                nearest = damageableComponent.transform;
            }
        }

        return nearest;
    }
}
