using UnityEngine;
using Unity.Netcode;

public partial class Gun : NetworkBehaviour
{
    private void Update()
    {
        if (ChatManager.Instance != null && ChatManager.Instance.IsChatActive) return;
        if (!IsOwner) return;
        if (Cursor.lockState == CursorLockMode.None) return;
        if (UIManager.Instance.IsInputBlocked) return;

        // ---- 射击 ----
        if (Input.GetMouseButtonDown(0))
        {
            if (currentAmmo.Value <= 0)
            {
                StartCoroutine(Reload());
                return;
            }

            // ★ 客户端：立即播放表现
            PlayShootEffects();

            // ★ 客户端：发送请求给服务器
            RequestShootServerRpc(CalculateDirection());
        }

        // ---- 换弹 ----
        if (Input.GetKeyDown(KeyCode.R) && !isReloading && currentAmmo.Value < maxAmmo)
        {
            StartCoroutine(Reload());
        }
    }

    private void PlayShootEffects()
    {
        thirdPersonCamera?.ApplyRecoil();
        animator.SetTrigger("Shoot");
        OnShoot?.Invoke();

        if (shootSound != null && AudioManager.Instance != null)
            AudioManager.Instance.PlayOneShot(shootSound, shootVolume);
    }

    private Vector3 CalculateDirection()
    {
        Ray ray = mainCamera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
            return (hit.point - firePoint.position).normalized;
        else
            return (ray.GetPoint(100f) - firePoint.position).normalized;
    }

    [ServerRpc]
    private void RequestShootServerRpc(Vector3 direction, ServerRpcParams rpcParams = default)
    {
        // ★ 逻辑在 Gun.Server.cs
        ExecuteShoot(direction, rpcParams.Receive.SenderClientId);
    }

}