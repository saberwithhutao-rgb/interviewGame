using UnityEngine;
using Unity.Netcode;

public partial class Gun : NetworkBehaviour
{
    private void ExecuteShoot(Vector3 direction, ulong shooterId)
    {
        if (!IsServer) return;

        // ★ 1. 验证请求者
        if (shooterId != OwnerClientId) return;

        // ★ 2. 验证弹药
        if (currentAmmo.Value <= 0) return;

        // ★ 3. 验证是否在换弹
        if (isReloading) return;

        // ★ 4. 验证射速
        if (Time.time < nextFireTime) return;

        // ★ 5. 验证 Direction 是否有效(不验证)
        //if (direction.sqrMagnitude < 0.01f) return;

        // ★ 6. 服务器扣弹药
        CurrentAmmo--;

        // ★ 7. 更新下次射击时间
        nextFireTime = Time.time + fireRate;

        GameEvents.NotifyBeforePlayerAttack(transform.root.gameObject);

        // ★ 8. 通过验证，服务器执行射击
        Vector3 position = firePoint.position;  // 服务器自己算位置
        direction.Normalize();

        // ★ 9. 生成子弹
        bulletPoolManager.GetBullet(position, direction, shooterId);

        // ★ 10. 攻击事件由服务器广播
        GameEvents.NotifyPlayerAttacked(transform.root.gameObject);
    }

}