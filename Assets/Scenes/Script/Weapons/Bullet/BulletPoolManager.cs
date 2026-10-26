using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 子弹对象池管理器，负责管理子弹的生成、回收和生命周期
/// </summary>
public class BulletPoolManager : NetworkBehaviour
{
    [SerializeField] private GameObject bulletPrefab;   
    [SerializeField] private Bullet bulletData;         
    [SerializeField] private int poolSize = 20;
    [SerializeField] private Gun gun;
    [SerializeField] private PlayerStats playerStats;

    private class ActiveBullet
    {
        public Transform transform;
        public float spawnTime;
        public int damage;
    }

    private Queue<Transform> inactiveBullets = new Queue<Transform>();
    private List<ActiveBullet> activeBullets = new List<ActiveBullet>();

    /// <summary>
    /// 在网络生成时初始化子弹对象池
    /// </summary>
    public override void OnNetworkSpawn()
    {
        if (gun == null)
        {
            gun = GetComponentInChildren<Gun>();
        }

        for (int i = 0; i < poolSize; i++)
        {
            CreateBullet();
        }
    }

    // 处理子弹的移动和生命周期
    void Update()
    {
        if (!IsServer) return;

        float speed = bulletData.speed;
        float lifetime = bulletData.lifetime;
        float currentTime = Time.time;

        for (int i = activeBullets.Count - 1; i >= 0; i--)
        {
            ActiveBullet active = activeBullets[i];

            Vector3 oldPos = active.transform.position;
            active.transform.Translate(Vector3.forward * speed * Time.deltaTime);
            Vector3 newPos = active.transform.position;

            Vector3 moveDir = (newPos - oldPos).normalized;
            float moveDistance = Vector3.Distance(newPos, oldPos);
            if (Physics.Raycast(oldPos, moveDir, out RaycastHit hit, moveDistance))
            {
                IDamageable damageable = hit.collider.GetComponent<IDamageable>();
                if (damageable != null)
                {
                    BulletOwner owner = active.transform.GetComponent<BulletOwner>();
                    GameObject attacker = null;
                    if (owner != null)
                    {
                        // 通过 ClientId 找到对应的玩家对象
                        NetworkManager.Singleton.ConnectedClients.TryGetValue(owner.ShooterClientId, out var client);
                        if (client != null)
                            attacker = client.PlayerObject.gameObject;
                    }
                    damageable.TakeDamage(active.damage, attacker, DamageType.Physical);
                }
                ReturnBullet(active.transform);
                continue;
            }

            if (currentTime - active.spawnTime > lifetime)
            {
                ReturnBullet(active.transform);
            }
        }
    }

    private void CreateBullet()
    {
        GameObject bulletObj = Instantiate(bulletPrefab);
        bulletObj.SetActive(false);
        inactiveBullets.Enqueue(bulletObj.transform);

        NetworkObject netObj = bulletObj.GetComponent<NetworkObject>();
        if (netObj != null && !netObj.IsSpawned)
        {
            netObj.Spawn();
        }

    }

    /// <summary>
    /// 获取一个子弹实例，如果对象池中有可用的子弹则复用，否则创建新的子弹实例
    /// </summary>
    /// <param name="position">子弹的生成位置</param>
    /// <param name="direction">子弹的生成方向</param>
    /// <param name="shooterId">发射子弹的玩家的客户端 ID</param>
    /// <returns>子弹的 Transform</returns>
    public Transform GetBullet(Vector3 position, Vector3 direction, ulong shooterId)
    {
        Transform bulletTransform;

        if (inactiveBullets.Count > 0)
        {
            bulletTransform = inactiveBullets.Dequeue();
        }
        else
        {
            GameObject obj = Instantiate(bulletPrefab);
            bulletTransform = obj.transform;

            NetworkObject netObj = obj.GetComponent<NetworkObject>();
            if (netObj != null && !netObj.IsSpawned)
            {
                netObj.Spawn();
            }

        }

        bulletTransform.position = position;
        bulletTransform.rotation = Quaternion.LookRotation(direction);
        bulletTransform.gameObject.SetActive(true);

        int finalDamage = Mathf.RoundToInt(gun.GetDamage());
        Bullet bullet = bulletTransform.GetComponent<Bullet>();
        if (bullet != null)
            bullet.damage = finalDamage;

        BulletOwner owner = bulletTransform.GetComponent<BulletOwner>();
        if (owner != null) owner.ShooterClientId = shooterId;

        activeBullets.Add(new ActiveBullet
        {
            transform = bulletTransform,
            spawnTime = Time.time,
            damage = finalDamage  
        });



        return bulletTransform;
    }

    /// <summary>
    /// 将子弹返回到对象池中，并将其从活动子弹列表中移除
    /// </summary>
    /// <param name="bulletTransform">要返回的子弹的 Transform</param>
    public void ReturnBullet(Transform bulletTransform)
    {
        bulletTransform.gameObject.SetActive(false);
        inactiveBullets.Enqueue(bulletTransform);
        activeBullets.RemoveAll(a => a.transform == bulletTransform);
    }
}