using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 枪械面板类，负责显示当前弹药数和换弹状态
/// </summary>
public class GunPanel : MonoBehaviour
{
    [SerializeField] private TMP_Text ammoText;
    [SerializeField] private Gun gun;

    private void Start()
    {
        if (gun == null)
        {
            gun = transform.root.GetComponentInChildren<Gun>();
        }

        if (ammoText == null)
            ammoText = GetComponentInChildren<TMP_Text>();
    }

    private void OnEnable()
    {
        if (gun != null)
            gun.OnAmmo += ChangeAmmoText;
    }
    private void OnDisable()
    {
        if (gun != null)
            gun.OnAmmo -= ChangeAmmoText;
    }

    private void ChangeAmmoText()
    {
        if (gun.IsReloading) 
        {
            ammoText.text = $"子弹：{gun.CurrentAmmo}/{gun.MaxAmmo}" + "\n换弹中...";
        }
        else
        {
            ammoText.text = $"子弹：{gun.CurrentAmmo}/{gun.MaxAmmo}";
        }
    }
}
