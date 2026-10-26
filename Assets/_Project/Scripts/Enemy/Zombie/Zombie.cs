using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : Enemy
{
    [Header("刀剑音效")]
    [SerializeField] private AudioClip swordHitSound;

    protected override void Start()
    {
        base.Start();
        ZombieManager.Instance.RegisterEnemy(this);
    }

    protected override void Die(GameObject killer = null)
    {
        if (ZombieManager.Instance != null)
            ZombieManager.Instance.UnregisterEnemy(this);
        base.Die(killer);
    }
}
