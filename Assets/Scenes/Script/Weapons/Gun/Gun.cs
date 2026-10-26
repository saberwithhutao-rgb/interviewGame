using System;
using System.Collections;
using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 枪械类，处理射击、换弹、伤害计算等逻辑
/// </summary>
public partial class Gun : NetworkBehaviour
{
    [SerializeField] private Animator animator;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private BulletPoolManager bulletPoolManager;
    [SerializeField] private PlayerStats playerStats;

    [Header("射击")]
    [SerializeField] private Transform firePoint;
    [SerializeField] private float fireRate = 0.1f;

    [Header("音效")]
    [SerializeField] private AudioClip shootSound;
    [SerializeField] private float shootVolume = 0.8f;
    
    [Header("伤害")]
    [SerializeField] private float baseDamage = 10f;

    [Header("弹匣")]
    [SerializeField] private int maxAmmo = 9;          
    [SerializeField] private float reloadTime = 2.2f;

    private float reloadSpeedMultiplier = 1f;
    private NetworkVariable<int> currentAmmo = new NetworkVariable<int>();                          
    private bool isReloading;
    private float nextFireTime;
    private Camera mainCamera;
    private ThirdPersonCamera thirdPersonCamera;

    /// <summary>
    /// 射击事件，当玩家开火时触发
    /// </summary>
    public event Action OnShoot;
    /// <summary>
    /// 弹药变化事件，当当前弹药数或最大弹药数发生变化时触发
    /// </summary>
    public event Action OnAmmo;


    public int CurrentAmmo
    {
        get => currentAmmo.Value;
        private set
        {
            if (currentAmmo.Value != value)
            {
                currentAmmo.Value = Mathf.Max(value, 0);
                OnAmmo?.Invoke();
            }
        }
    }

    public int MaxAmmo
    {
        get => maxAmmo;
        private set
        {
            if (maxAmmo != value)
            {
                maxAmmo = Mathf.Max(value, 0);
                OnAmmo?.Invoke();
            }
        }
    }

    public bool IsReloading
    {
        get => isReloading;
        private set
        {
            if (isReloading != value)
            {
                isReloading = value;
                OnAmmo?.Invoke();
            }
        }
    }

    private void Start()
    {
        CurrentAmmo = maxAmmo;

        mainCamera = Camera.main;
        thirdPersonCamera = mainCamera.GetComponent<ThirdPersonCamera>();

        if (animator == null)
            animator = GetComponentInParent<Animator>();
        if (playerHealth == null)
        {
            playerHealth = GetComponent<PlayerHealth>();
        }

        if (bulletPoolManager == null)
        {
            bulletPoolManager = transform.root.GetComponent<BulletPoolManager>();
        }
        if (playerStats == null)
        {
            playerStats = transform.root.GetComponent<PlayerStats>();
        }
    }

    public void OnEnable()
    {
        if (playerHealth != null)
            playerHealth.OnPlayerDied += OnDied;
    }

    public void OnDisable()
    {
        if (isReloading)
        {
            IsReloading = false;
        }

        if (playerHealth != null)
            playerHealth.OnPlayerDied -= OnDied;
        if (animator != null)
        {
            animator.SetFloat("ReloadSpeed", 1f);
            animator.ResetTrigger("Reload");
        }
        StopAllCoroutines();
    }

    void OnDied(GameObject player)
    {
        this.enabled = false;
    }

    public IEnumerator Reload()
    {
        if (isReloading || currentAmmo.Value >= maxAmmo) yield break;
        IsReloading = true;

        animator.SetLayerWeight(1, 1f); //只有上半身播放换弹动画
        animator.SetFloat("ReloadSpeed", reloadSpeedMultiplier);
        animator?.SetTrigger("Reload");

        float finalReloadTime = reloadTime / Mathf.Max(reloadSpeedMultiplier, 0.1f);
        yield return new WaitForSeconds(finalReloadTime);

        CurrentAmmo = maxAmmo;
        IsReloading = false;
        animator.SetFloat("ReloadSpeed", 1f);
    }

    /// <summary>
    /// 计算并返回枪械的总伤害值，包括基础伤害和玩家攻击力加成
    /// </summary>
    /// <returns>枪械的总伤害值</returns>
    public float GetDamage()
    {
        return baseDamage + playerStats.Attack;
    }

    /// <summary>
    /// 获取当前弹药数
    /// </summary>
    /// <returns>当前弹药数</returns>
    public int GetCurrentAmmo()
    {
        return CurrentAmmo;
    }

    /// <summary>
    /// 提高换弹速度的倍率，乘法叠加
    /// </summary>
    /// <param name="mult">倍率</param>
    public void AddReloadSpeedMultiplier(float mult)
    {
        reloadSpeedMultiplier *= mult;
    }

    /// <summary>
    /// 降低换弹速度的倍率，除法叠加
    /// </summary>
    /// <param name="mult">倍率</param>
    public void RemoveReloadSpeedMultiplier(float mult)
    {
        reloadSpeedMultiplier /= mult;
    }
}