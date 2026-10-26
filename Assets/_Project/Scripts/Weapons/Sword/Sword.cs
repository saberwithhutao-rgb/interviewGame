using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// Sword 类继承自 NetworkBehaviour，用于实现剑类武器的攻击逻辑，包括攻击动画、碰撞检测、伤害计算和位移效果。
/// </summary>
public partial class Sword : NetworkBehaviour
{
    [Header("武器属性")]
    [SerializeField] private float baseDamage = 15f;
    [SerializeField] private float attackCooldown = 0.5f;
    [SerializeField] private DamageType damageType = DamageType.Physical;

    [Header("碰撞体")]
    [SerializeField] private Collider attackCollider;

    [Header("命中卡肉")]
    [SerializeField] private float hitStopDuration = 0.06f;  // 仅暂停动画表现，不暂停整个游戏

    [Header("攻击位移")]
    [SerializeField] private float attackDashDistance = 0.8f;   // 突进距离
    [SerializeField] private float attackDashDuration = 0.45f;  // 突进持续时间

    [Header("攻击索敌")]
    [SerializeField] private float attackDashLockRange = 2f;
    [SerializeField] private LayerMask enemyLayerMask = ~0;

    [Header("��Ч")]
    [SerializeField] private AudioClip swingClip;
    [SerializeField] private float swingVolume = 0.8f;

    [Header("位移速度曲线（X轴：进度0~1；Y轴：速度倍率）")]
    [SerializeField] private AnimationCurve dashSpeedCurve = AnimationCurve.EaseInOut(0f, 0.3f, 1f, 1.5f);

    //[Header("特效")]
    //[SerializeField] private GameObject hitEffect;
    //[SerializeField] private GameObject slashEffect;      // 剑气/挥砍特效

    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private Animator animator;

    private double nextAttackTime;
    private bool isAttacking = false;
    private bool localIsAttacking = false;
    private float localNextAttackTime = 0f;

    private AnticipatedNetworkVariable<bool> anticipatedIsAttacking = new AnticipatedNetworkVariable<bool>(false);

    private List<GameObject> hitTargets = new List<GameObject>();

    private void Start()
    {
        if (playerStats == null)
            playerStats = GetComponentInParent<PlayerStats>();
        if (animator == null)
            animator = GetComponentInParent<Animator>();
        if (attackCollider == null)
            attackCollider = GetComponent<Collider>();

        if (attackCollider != null)
        {
            attackCollider.isTrigger = true;
            attackCollider.enabled = false;
        }

    }


    private void OnDisable()
    {
        isAttacking = false;
        if (attackCollider != null)
            attackCollider.enabled = false;
        StopAllCoroutines();
    }


    //[ClientRpc]
    //private void SpawnSlashEffectClientRpc(Vector3 position, Quaternion rotation)
    //{
    //    if (slashEffect != null)
    //    {
    //        GameObject effect = Instantiate(slashEffect, position, rotation);
    //        Destroy(effect, 1f);
    //    }
    //}

    //[ClientRpc]
    //private void SpawnHitEffectClientRpc(Vector3 position)
    //{
    //    if (hitEffect != null)
    //    {
    //        GameObject effect = Instantiate(hitEffect, position, Quaternion.identity);
    //        Destroy(effect, 1f);
    //    }
    //}


    // ============================================================
    // 编辑器辅助
    // ============================================================

    private void OnValidate()
    {
        if (attackCollider == null)
            attackCollider = GetComponent<Collider>();

        hitStopDuration = Mathf.Max(0f, hitStopDuration);
    }

    private IEnumerator AttackDash()
    {
        float elapsed = 0f;
        CharacterController characterController = GetComponentInParent<CharacterController>();
        if (characterController == null) yield break;

        Vector3 center = characterController.bounds.center;

        Transform target = FindNearestTarget();

        Vector3 dashDirection;
        if (target != null)
        {
            Vector3 toTarget = target.position - center;
            toTarget.y = 0f;

            if (toTarget.sqrMagnitude > 0.0001f)
            {
                dashDirection = toTarget.normalized;
            }
            else
            {
                dashDirection = transform.root.forward;
            }
        }
        else
        {
            dashDirection = transform.root.forward;
        }
        if (target != null)
        {
            transform.root.rotation = Quaternion.LookRotation(dashDirection);
        }

        // 计算总移动量，使得全程按曲线累计正好等于 attackDashDistance
        float totalCurveIntegral = 0f;
        int sampleSteps = 100;
        for (int i = 0; i <= sampleSteps; i++)
        {
            float t = i / (float)sampleSteps;
            totalCurveIntegral += dashSpeedCurve.Evaluate(t);
        }
        totalCurveIntegral /= (sampleSteps + 1);  // 平均曲线值

        float baseSpeed = attackDashDistance / attackDashDuration / totalCurveIntegral;


        while (elapsed < attackDashDuration)
        {
            float progress = elapsed / attackDashDuration;
            float speedMultiplier = dashSpeedCurve.Evaluate(progress);

            float moveAmount = baseSpeed * speedMultiplier * Time.deltaTime;
            characterController.Move(dashDirection * moveAmount);
            elapsed += Time.deltaTime;
            yield return null;
        }
    }

    public override void OnReanticipate(double lastRoundTripTime)
    {
        // 检查预测变量是否真的需要重新预测
        if (anticipatedIsAttacking.ShouldReanticipate)
        {
            // ShouldReanticipate 为 true，意味着权威值覆盖了预期值
            // 如果权威值是 false（服务端拒绝），而本地刚才 Anticipate 的是 true
            if (anticipatedIsAttacking.AuthoritativeValue == false)
            {
                // 执行回滚逻辑
                if (animator != null) animator.ResetTrigger("Attack");
                PlayerControl.LocalInstance?.SetAttackInputLock(false);
            }

            // 可选：把本地预期值也拉回权威值，避免状态混乱
            anticipatedIsAttacking.Anticipate(anticipatedIsAttacking.AuthoritativeValue);
        }
    }
}