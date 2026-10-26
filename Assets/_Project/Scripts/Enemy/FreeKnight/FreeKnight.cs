using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// ������ʿ�࣬�̳���Enemy������Ѳ����Ϊ��ע�ᵽFreeKnightManager�Ĺ���
/// </summary>
public class FreeKnight : Enemy
{
    protected override void Awake()
    {
        base.Awake();
        startPosition = transform.position;
        SetNewPatrolTarget();
    }

    protected override void Start()
    {
        base.Start();
        FreeKnightManager.Instance.RegisterEnemy(this);
    }

    protected override void UpdateIdle()
    {
        base.UpdateIdle();
        Patrol();

    }

    //Ѳ��
    protected virtual void Patrol()
    {
        if (isWaiting)
        {
            patrolTimer -= Time.deltaTime;
            if (patrolTimer <= 0)
            {
                isWaiting = false;
                SetNewPatrolTarget();
            }
            return;
        }
        agent.SetDestination(patrolTarget);
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.5f)
        {
            isWaiting = true;
            patrolTimer = Random.Range(enemyStats.PatrolWaitMinTime, enemyStats.PatrolWaitMaxTime);
        }
    }

    /// <summary>
    /// �����µ�Ѳ��Ŀ��㣬���ѡ��һ����Ѳ�߰뾶�ڵĿ���λ��
    /// </summary>
    protected void SetNewPatrolTarget()
    {
        for (int i = 0; i < 10; i++) 
        {
            Vector3 randomDir = Random.onUnitSphere;
            randomDir.y = 0;
            randomDir.Normalize();
            Vector3 candidate = startPosition + randomDir * enemyStats.PatrolRadius;

            if (NavMesh.SamplePosition(candidate, out NavMeshHit hit, enemyStats.PatrolRadius, NavMesh.AllAreas))
            {
                NavMeshPath path = new NavMeshPath();
                if (agent.CalculatePath(hit.position, path) && path.status == NavMeshPathStatus.PathComplete)
                {
                    patrolTarget = hit.position;
                    return;
                }
            }
        }
        patrolTarget = startPosition;
    }


    protected override void Die(GameObject killer = null)
    {
        FreeKnightManager.Instance.UnregisterEnemy(this);
        base.Die(killer);
    }
}
