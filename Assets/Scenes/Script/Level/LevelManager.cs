using System;
using System.Collections;
using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;
using UnityEngine.UI;

/// <summary>
/// 关卡管理器，负责管理关卡数据、敌人生成、玩家状态和无尽模式逻辑
/// </summary>
public class LevelManager : MonoBehaviour
{
    public static LevelManager Instance { get; private set; }

    [SerializeField] private LevelData[] levels;
    [SerializeField] private BuffSelector buffSelector;
    [SerializeField] private BuffManager buffManager;
    [SerializeField] private WinMenu winMenu;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private PlayerHealth playerHealth;
    [SerializeField] private PlayerStats playerStats;
    [SerializeField] private SaveManager saveManager;

    [Header("玩家生成点")]
    [SerializeField] private Transform PlayerSpawnPoint;

    [Header("无尽模式")]
    [SerializeField] private GameObject[] endlessEnemyPrefabs;  
    [SerializeField] private float endlessSpawnInterval = 10f;
    [SerializeField] private int endlessSpawnPerWave = 2;
    [Header("无尽模式难度曲线")]
    [SerializeField] private float endlessHealthMultPerWave = 0.05f;   
    [SerializeField] private float endlessDamageMultPerWave = 0.05f;   
    [SerializeField] private float endlessSpeedMultPerWave = 0.02f;
    [SerializeField] private int endlessSpawnIncreasePerWave = 3;
    [SerializeField] private int maxEnemiesOnField = 10;
    private int currentWave;

    private bool isEndless;
    private int currentLevelIndex = 0;
    public int CurrentLevelIndex => currentLevelIndex;
    private int currentKillCount;
    public int CurrentKillCount => currentKillCount;
    private bool levelComplete;
    private float gameStartTime;
    public float PlayTime => Time.time - gameStartTime;

    public static event Action<int, float> OnAllLevelsCompleted;
    public static event Action OnEndlessStarted;
    public bool IsEndless => isEndless;
    public int EndlessKillCount => currentKillCount;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        gameStartTime = Time.time;
        GameObject objectPanel = UIManager.Instance.GetPanel("ObjectivePanel");
        if (objectPanel != null)
            objectiveText = objectPanel.transform.Find("DescText").GetComponent<TMP_Text>();
        GameObject buffPanel = UIManager.Instance.GetPanel("BuffPanel");
        if (buffPanel != null)
            buffSelector = buffPanel.GetComponent<BuffSelector>();
    }
    void OnLocalPlayerSpawned(PlayerControl pc)
    {
        playerStats = pc.GetComponent<PlayerStats>();
        playerHealth = pc.GetComponent<PlayerHealth>();
        buffManager = pc.GetComponent<BuffManager>();
        saveManager = pc.GetComponent<SaveManager>();
        if (levels.Length > 0)
            ApplyLevel(levels[currentLevelIndex]);
        pc.transform.position = PlayerSpawnPoint.position;
    }

    private void OnEnable()
    {
        Enemy.OnAnyEnemyDied += OnEnemyDied;
        if (playerStats == null || playerHealth == null || buffManager == null)
            PlayerControl.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
    }

    private void OnDisable()
    {
        Enemy.OnAnyEnemyDied -= OnEnemyDied;
        PlayerControl.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;
    }

    void OnEnemyDied(Enemy enemy)
    {
        if (isEndless)
        {
            currentKillCount++;
            return;
        }
        if (levelComplete) return;
        currentKillCount++;

        if (levels == null || levels.Length == 0) return;
        if (currentLevelIndex >= levels.Length) return;

        if (currentKillCount >= levels[currentLevelIndex].killTarget)
        {
            levelComplete = true;
            StartCoroutine(OnLevelComplete(3f));
        }
    }

    void ApplyLevel(LevelData data, bool resetKillCount = true)
    {
        if (resetKillCount)
        {
            currentKillCount = 0;
        }

        levelComplete = false;                                                                                                                                                                                     

        playerStats.SetBuffs(
            data.playerDamageMult,
            data.playerSpeedMult,
            data.playerMaxHealthMult
        );

        EnemyBuffManager.SetBuffs(
            data.enemyHealthMult,
            data.enemySpeedMult,
            data.enemyDamageMult
        );

        if (NetworkManager.Singleton.IsServer)
        {
            foreach (var enemy in FindObjectsOfType<Enemy>())
            {
                if (enemy != null && enemy.NetworkObject != null && enemy.NetworkObject.IsSpawned)
                    enemy.NetworkObject.Despawn();
                else if (enemy != null)
                    Destroy(enemy.gameObject);
            }
        }

        objectiveText.text = data.objectiveText;
        UIManager.Instance.OpenModal("ObjectivePanel");
        SpawnEnemies(data);
    }

    void SpawnEnemies(LevelData data)
    {
        if (!NetworkManager.Singleton.IsServer) return;

        for (int typeIndex = 0; typeIndex < data.enemyPrefabs.Length; typeIndex++)
        {
            if (data.enemyPrefabs[typeIndex] == null) continue;

            for (int i = 0; i < data.spawnCounts[typeIndex]; i++)
            {
                EnemySpawner.Instance.SpawnEnemy(data.enemyPrefabs[typeIndex]);
            }
        }
    }

    IEnumerator OnLevelComplete(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (currentLevelIndex >= levels.Length - 1)
        {
            int score = ScoreManager.Instance != null ? ScoreManager.Instance.TotalScore : 0;
            float time = Time.time - gameStartTime;
            OnAllLevelsCompleted?.Invoke(score, time);
            yield break;
        }

        if (levels[currentLevelIndex].buffPool.Length > 0 || levels[currentLevelIndex].buffChoices > 0)
        {
            buffSelector.Show(
                levels[currentLevelIndex].buffPool,
                levels[currentLevelIndex].buffChoices,
                OnBuffSelected
            );
        }
        else
        {
            GoToNextLevel();
        }
    }

    void OnBuffSelected(BuffData chosenBuff)
    {
        if (buffManager != null)
            buffManager.ApplyBuff(chosenBuff);

        GoToNextLevel();
    }

    void GoToNextLevel()
    {
        currentLevelIndex++;

        ApplyLevel(levels[currentLevelIndex]);
    }

    /// <summary>
    /// 开始无尽模式
    /// </summary>
    public void StartEndlessMode()
    {
        isEndless = true;
        currentKillCount = 0;


        foreach (var enemy in FindObjectsOfType<Enemy>())
            Destroy(enemy.gameObject);

        OnEndlessStarted?.Invoke();
        StartCoroutine(EndlessWaveRoutine());
    }

    IEnumerator EndlessWaveRoutine()
    {
        currentWave = 0;
        while (isEndless)
        {
            currentWave++;

            while (ActiveEnemyCount() >= maxEnemiesOnField)
            {
                yield return new WaitForSeconds(2f);
            }


            int spawnCount = endlessSpawnPerWave + currentWave / endlessSpawnIncreasePerWave;

            for (int i = 0; i < spawnCount; i++)
            {
                GameObject prefab = endlessEnemyPrefabs[UnityEngine.Random.Range(0, endlessEnemyPrefabs.Length)];
                EnemySpawner.Instance.SpawnEnemy(prefab);
            }

            float healthMult = 1f + currentWave * endlessHealthMultPerWave;
            float damageMult = 1f + currentWave * endlessDamageMultPerWave;
            float speedMult = 1f + currentWave * endlessSpeedMultPerWave;

            EnemyBuffManager.SetBuffs(healthMult, speedMult, damageMult);

            endlessSpawnPerWave = Mathf.Min(20, endlessSpawnPerWave + 1);
            endlessSpawnInterval = Mathf.Max(0.5f, endlessSpawnInterval - 0.05f);

            yield return new WaitForSeconds(endlessSpawnInterval);
        }
    }

    /// <summary>
    /// 获取当前场景中活跃的敌人数量
    /// </summary>
    /// <returns>活跃的敌人数量</returns>
    int ActiveEnemyCount()
    {
        return FindObjectsOfType<Enemy>().Length;
    }

    /// <summary>
    /// 恢复关卡进度
    /// </summary>
    /// <param name="run"></param>
    [Obsolete("没有使用需求")]
    public void RestoreRunProgress(RunProgress run)
    {
        if (run == null)
            return;

        if (run.currentLevelIndex < 0 ||
            run.currentLevelIndex >= levels.Length)
            return;

        currentLevelIndex = run.currentLevelIndex;
        currentKillCount = run.statistics.killCount;

        ApplyLevel(levels[currentLevelIndex], resetKillCount: false);
    }
}