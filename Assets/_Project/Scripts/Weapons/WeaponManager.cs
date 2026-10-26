using System;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 武器管理器 - 负责玩家武器的切换和状态管理
/// </summary>
public class WeaponManager : NetworkBehaviour
{
    /// <summary>
    /// 武器类型枚举
    /// </summary>
    public enum WeaponType
    {
        Hand,
        Sword,
        Gun
    }

    [Header("武器对象")]
    [SerializeField] private GameObject swordObject;
    [SerializeField] private GameObject gunObject;

    [Header("动画层")]
    [SerializeField] private Animator animator;
    [SerializeField] private int baseLayerIndex = 0;
    [SerializeField] private int swordLayerIndex = 1;
    [SerializeField] private int gunLayerIndex = 2;
    [SerializeField] private int reloadLayerIndex = 3;

    [Header("武器解锁状态")]
    [SaveField] private NetworkVariable<bool> canUseSword = new(false);
    [SaveField] private NetworkVariable<bool> canUseGun = new(false);
    [SaveField] private NetworkVariable<WeaponType> currentWeapon = new(WeaponType.Hand);

    private SaveManager saveManager;

    public bool CanUseSword
    {
        get => canUseSword.Value;
        set { if (IsServer) canUseSword.Value = value; }
    }

    public bool CanUseGun
    {
        get => canUseGun.Value;
        set { if (IsServer) canUseGun.Value = value; }
    }

    public int CurrentWeaponType => (int)currentWeapon.Value;

    private void Awake()
    {
        SetWeaponRenderers(swordObject, false);
        SetWeaponRenderers(gunObject, false);

        saveManager = GetComponent<SaveManager>();
    }

    private void Update()
    {
        if (!IsOwner) return;
        if (Input.GetKeyDown(KeyCode.Alpha0)) SwitchTo(WeaponType.Hand);
        if (Input.GetKeyDown(KeyCode.Alpha1) && canUseSword.Value) SwitchTo(WeaponType.Sword);
        if (Input.GetKeyDown(KeyCode.Alpha2) && canUseGun.Value) SwitchTo(WeaponType.Gun);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        SetWeaponRenderers(swordObject, true);
        SetWeaponRenderers(gunObject, true);
        swordObject.SetActive(false);
        gunObject.SetActive(false);

        currentWeapon.OnValueChanged += OnWeaponChanged;
        canUseSword.OnValueChanged += OnUnlockChanged;
        canUseGun.OnValueChanged += OnUnlockChanged;


        if (IsServer)
        {
            if (SaveManager.IsDataReady) RestoreWeaponFromSave();
            else SaveManager.OnSaveDataReady += OnSaveDataReady;
        }
    }

    private void OnWeaponChanged(WeaponType oldVal, WeaponType newVal)
    {
        swordObject.SetActive(newVal == WeaponType.Sword);
        gunObject.SetActive(newVal == WeaponType.Gun);
        animator.SetLayerWeight(swordLayerIndex, newVal == WeaponType.Sword ? 1f : 0f);
        animator.SetLayerWeight(gunLayerIndex, newVal == WeaponType.Gun ? 1f : 0f);
        if (oldVal == WeaponType.Gun)
            animator.Play("Empty", reloadLayerIndex, 0f);
        GameEvents.NotifyWeaponChanged(GetCurrentWeapon());
    }

    private void OnUnlockChanged(bool oldVal, bool newVal) => HideUnlockedPickups();

    public override void OnNetworkDespawn()
    {
        SaveManager.OnSaveDataReady -= OnSaveDataReady;
        base.OnNetworkDespawn();
    }

    private void OnSaveDataReady()
    {
        SaveManager.OnSaveDataReady -= OnSaveDataReady; // 只响应一次
        RestoreWeaponFromSave();
    }

    private void RestoreWeaponFromSave()
    {
        OnWeaponChanged(WeaponType.Hand, currentWeapon.Value);
        HideUnlockedPickups();
    }

    // ============================================================
    // 核心切换逻辑
    // ============================================================

    public void SwitchTo(WeaponType newWeapon, bool forceRefresh = false)
    {
        if (!IsServer)
        {
            RequestSwitchServerRpc(newWeapon, forceRefresh);
            return;
        }
        if (newWeapon == WeaponType.Sword && !canUseSword.Value) return;
        if (newWeapon == WeaponType.Gun && !canUseGun.Value) return;
        if (currentWeapon.Value == newWeapon && !forceRefresh) return;
        currentWeapon.Value = newWeapon;
    }

    [ServerRpc]
    private void RequestSwitchServerRpc(WeaponType newWeapon, bool forceRefresh)
    {
        SwitchTo(newWeapon, forceRefresh);
    }

    // ============================================================
    // 退出武器
    // ============================================================
    [Obsolete("此方法不建议使用，请关注OnWeaponChanged方法")]
    private void ExitWeapon(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Sword:
                animator.SetLayerWeight(swordLayerIndex, 0f);
                animator.ResetTrigger("Attack");
                swordObject.SetActive(false);
                break;

            case WeaponType.Gun:
                animator.SetLayerWeight(gunLayerIndex, 0f);
                animator.ResetTrigger("Shoot");
                animator.ResetTrigger("Reload");
                // ★ 强制退出换弹动画
                animator.Play("Empty", reloadLayerIndex, 0f);
                gunObject.SetActive(false);
                break;

            case WeaponType.Hand:
                // 空手不需要特殊处理
                break;
        }
    }

    // ============================================================
    // 进入武器
    // ============================================================
    [Obsolete("此方法不建议使用，请关注OnWeaponChanged方法")]
    private void EnterWeapon(WeaponType weapon)
    {
        switch (weapon)
        {
            case WeaponType.Sword:
                swordObject.SetActive(true);
                animator.SetLayerWeight(swordLayerIndex, 1f);
                animator.SetLayerWeight(gunLayerIndex, 0f);
                break;

            case WeaponType.Gun:
                gunObject.SetActive(true);
                animator.SetLayerWeight(gunLayerIndex, 1f);
                animator.SetLayerWeight(swordLayerIndex, 0f);
                break;

            case WeaponType.Hand:
                animator.SetLayerWeight(swordLayerIndex, 0f);
                animator.SetLayerWeight(gunLayerIndex, 0f);
                break;
        }
    }

    // ============================================================
    // 外部接口
    // ============================================================

    /// <summary>
    /// 获取当前武器类型
    /// </summary>
    /// <returns>当前武器类型</returns>
    public WeaponType GetCurrentWeaponType() => currentWeapon.Value;
    /// <summary>
    /// 获取当前武器对象
    /// </summary>
    /// <returns>当前武器对象</returns>
    public GameObject GetCurrentWeapon() => currentWeapon.Value == WeaponType.Sword ? swordObject : (currentWeapon.Value == WeaponType.Gun ? gunObject : null);

    // ============================================================
    // 辅助方法
    // ============================================================

    // 隐藏或显示武器的渲染器，而不是直接 SetActive(false)，以避免网络同步问题
    private void SetWeaponRenderers(GameObject weaponObj, bool enabled)
    {
        if (weaponObj == null) return;
        Renderer[] renderers = weaponObj.GetComponentsInChildren<Renderer>(true);
        foreach (var renderer in renderers)
        {
            renderer.enabled = enabled;
        }
    }

    public void UnlockWeapon(WeaponType weapon, bool unlock)
    {
        if (!IsServer) return;
        switch (weapon)
        {
            case WeaponType.Sword: canUseSword.Value = unlock; break;
            case WeaponType.Gun: canUseGun.Value = unlock; break;
        }
        if (unlock) SwitchTo(weapon, true);
        saveManager?.Save();
    }

    [ServerRpc(RequireOwnership = true)]
    public void UnlockWeaponServerRpc(WeaponType weapon, bool unlock)
    {
        UnlockWeapon(weapon, unlock);
    }

    private void HideUnlockedPickups()
    {
        foreach (var pickup in FindObjectsOfType<WeaponPickup>(true))
        {
            if (pickup == null) continue;
            if (IsWeaponUnlockedLocally(pickup.WeaponType))
            {
                InteractionPrompt.Instance?.Unregister(pickup);
                pickup.gameObject.SetActive(false);
            }
        }
    }

    private bool IsWeaponUnlockedLocally(WeaponManager.WeaponType type)
    {
        return type switch
        {
            WeaponManager.WeaponType.Sword => canUseSword.Value,
            WeaponManager.WeaponType.Gun => canUseGun.Value,
            _ => false
        };
    }
}