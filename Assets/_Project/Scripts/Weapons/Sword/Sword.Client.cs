using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public partial class Sword : NetworkBehaviour
{
    private void Update()
    {
        // 只有本地玩家且是激活状态才检测输入
        if (!IsOwner) return;
        if (!isActiveAndEnabled) return;
        if (UIManager.Instance != null && UIManager.Instance.IsInputBlocked) return;

        if (Input.GetMouseButtonDown(0))
        {
            if (localIsAttacking)
                return;
            if (Time.time < localNextAttackTime)
                return;
            if (playerStats == null || playerStats.CurrentBurstSP < 1f)
                return;

            localIsAttacking = true;
            localNextAttackTime = Time.time + attackCooldown;
            anticipatedIsAttacking.Anticipate(true);

            if (animator != null)
            {
                animator.SetTrigger("Attack");
            }
            
            PlayerControl.LocalInstance?.SetAttackInputLock(true);

            if (swingClip != null)
                AudioManager.Instance.PlayOneShot(swingClip, swingVolume);
            RequestAttackServerRpc();

        } 
    }
}
