using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// 敌人生成器，负责在游戏中生成敌人
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    public static EnemySpawner Instance { get; private set; }

    [Header("刷怪点")]
    [SerializeField] private List<SpawnPoint> allSpawnPoints = new List<SpawnPoint>();
    [SerializeField] private float spawnRadius = 3f;

    void Awake()
    {
        Instance = this;
        CollectSpawnPoints();
    }

    void CollectSpawnPoints()
    {
        allSpawnPoints.Clear();
        allSpawnPoints.AddRange(FindObjectsOfType<SpawnPoint>());
    }

    /// <summary>
    /// 生成敌人
    /// </summary>
    /// <param name="enemyPrefab">要生成的敌人预制体</param>
    /// <param name="preferredType">首选的生成点类型</param>
    public void SpawnEnemy(GameObject enemyPrefab, SpawnPoint.SpawnType preferredType = SpawnPoint.SpawnType.Any)
    {
        List<SpawnPoint> validPoints = allSpawnPoints
            .FindAll(p => preferredType == SpawnPoint.SpawnType.Any || p.type == preferredType);

        if (validPoints.Count == 0)
            validPoints = allSpawnPoints;

        SpawnPoint chosenPoint = validPoints[Random.Range(0, validPoints.Count)];

        Vector3 randomOffset = Random.insideUnitSphere * spawnRadius;
        randomOffset.y = 0;
        Vector3 spawnPos = chosenPoint.transform.position + randomOffset;

        if (NavMesh.SamplePosition(spawnPos, out NavMeshHit hit, 2f, NavMesh.AllAreas))
            spawnPos = hit.position;

        Quaternion randomRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
        GameObject enemyObj = Instantiate(enemyPrefab, spawnPos, randomRotation);
        NetworkObject netObj = enemyObj.GetComponent<NetworkObject>();
        if (netObj != null)
        {
            netObj.Spawn();
        }
    }
}