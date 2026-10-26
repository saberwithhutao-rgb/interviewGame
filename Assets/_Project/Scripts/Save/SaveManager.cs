using Steamworks;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 保存管理器，负责处理玩家存档的保存和加载，包括自动保存、手动保存、删除存档等功能
/// </summary>
public class SaveManager : NetworkBehaviour
{
    [Header("自动保存间隔（秒）")]
    [SerializeField] private float autoSaveInterval = 60f;

    private PlayerHealth playerHealth;
    private PlayerStats playerStats;
    private SkillManager skillManager;
    private WeaponManager weaponManager;
    private string saveDirectory;
    private Coroutine autoSaveCoroutine;

    private RunProgress currentRun;
    private static RunProgress cachedRun;   

    private bool isSaveLoaded = false;
    private bool isPlayerDead = false;
    private bool isLoadInProgress = false;

    // 缓存的 SteamID
    private string cachedSteamId = null;
    private bool isSteamReady = false;


    public static event Action OnSaveDataReady;
    public static bool IsDataReady { get; private set; } = false;

    /// <summary>
    /// 获取当前玩家的 SteamID 字符串
    /// </summary>
    private string SteamIdString
    {
        get
        {
            if (!string.IsNullOrEmpty(cachedSteamId))
                return cachedSteamId;

            if (TryGetSteamId(out string steamId))
            {
                cachedSteamId = steamId;
                return cachedSteamId;
            }

            Debug.LogWarning("[SaveManager] Steam 未就绪，使用默认标识");
            return "LocalPlayer";
        }
    }

    private bool TryGetSteamId(out string steamId)
    {
        steamId = null;

        if (SteamManager.Instance == null)
            return false;

        try
        {
            if (SteamClient.IsValid)
            {
                steamId = SteamClient.SteamId.ToString();
                return true;
            }
        }
        catch
        {
            // Steam 未初始化
        }

        return false;
    }


    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
        skillManager = GetComponent<SkillManager>();
        weaponManager = GetComponent<WeaponManager>();
        saveDirectory = Path.Combine(Application.persistentDataPath, "Saves");
    }

    private void OnEnable()
    {
        // 订阅 Steam 初始化事件
        if (SteamManager.Instance != null)
        {
            SteamManager.Instance.OnSteamInitialized += OnSteamInitialized;
        }

        // 如果 Steam 已经初始化完成，直接处理
        if (TryGetSteamId(out _))
        {
            OnSteamInitialized();
        }

        PlayerControl.OnLocalPlayerSpawned += OnLocalPlayerSpawned;
    }



    private void OnDisable()
    {
        // 取消订阅
        if (SteamManager.Instance != null)
        {
            SteamManager.Instance.OnSteamInitialized -= OnSteamInitialized;
        }

        PlayerControl.OnLocalPlayerSpawned -= OnLocalPlayerSpawned;

        if (playerHealth != null)
            playerHealth.OnPlayerDied -= DeleteSaveWithPlayer;
    }

    private void OnSteamInitialized()
    {
        isSteamReady = true;
        cachedSteamId = null; // 清除缓存，强制重新获取

        Debug.Log($"[SaveManager] Steam 已就绪，SteamID: {SteamIdString}");

        // 如果已经是服务器，执行存档加载
        if (IsServer && isActiveAndEnabled)
        {
            LoadOrInitAll();
            LoadOrInitTransform(currentRun);
        }
    }

    /// <summary>
    /// 当网络对象生成时调用，负责初始化存档管理器，包括加载存档和启动自动保存协程
    /// </summary>
    public override void OnNetworkSpawn()
    {
        // 只有服务器（Host）负责存档读写
        if (!IsServer) return;

        // 确保存档目录存在
        if (!Directory.Exists(saveDirectory))
            Directory.CreateDirectory(saveDirectory);

        // 如果 Steam 已经就绪，直接加载；否则等待事件
        if (isSteamReady || TryGetSteamId(out _))
        {
            LoadOrInitAll();
            LoadOrInitTransform(currentRun);
        }
        else
        {
            Debug.Log("[SaveManager] 等待 Steam 初始化...");
        }

        // 启动自动保存协程
        if (autoSaveCoroutine == null)
            autoSaveCoroutine = StartCoroutine(AutoSaveRoutine());
    }

    /// <summary>
    /// 加载存档，若无存档则创建默认永久进度和空限时进度
    /// </summary>
    private void LoadOrInitAll()
    {
        if (isSaveLoaded || isLoadInProgress) return;
        isLoadInProgress = true;
        try
        {
            if (!Load())
            {
                InitNewGame();
            }
        }
        finally { isLoadInProgress = false; }
    }

    private void InitNewGame()
    {
        skillManager.InitializeDefaults();
        CreateEmptyRun();
        ApplyRunProgress(currentRun);
    }

    private IEnumerator AutoSaveRoutine()
    {
        while (isActiveAndEnabled && IsServer)
        {
            yield return new WaitForSeconds(autoSaveInterval);
            Save();
        }
    }


    public void Save()
    {
        if (!IsServer) return;
        if (isPlayerDead) return;
        if (playerStats == null || skillManager == null) return;

        var dict = new Dictionary<string, object>();
        SaveRegistry.Capture(playerStats, dict, "player.");
        if (weaponManager != null)
            SaveRegistry.Capture(weaponManager, dict, "weapon.");
        dict["skills"] = skillManager.GetSaveData();

        RunProgress run = GetRunProgress();
        dict["run"] = run;
        cachedRun = run;

        string json = SaveSerializer.Serialize(dict);

        try
        {
            File.WriteAllText(GetSaveFilePath(), json);
            Debug.Log($"[SaveManager] 玩家 {SteamIdString} 存档已保存");
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 保存失败: {e.Message}");
        }
    }

    public bool Load()
    {
        if (!IsServer) return false;

        string filePath = GetSaveFilePath();
        if (!File.Exists(filePath)) return false;

        try
        {
            string json = File.ReadAllText(filePath);
            var dict = SaveSerializer.Deserialize<Dictionary<string, object>>(json);

            SaveRegistry.Restore(playerStats, dict, "player.");
            if (weaponManager != null)
                SaveRegistry.Restore(weaponManager, dict, "weapon.");

            if (dict.TryGetValue("skills", out var skillsObj))
                skillManager.LoadSaveData((List<SkillSaveData>)SaveRegistry.ConvertValue(skillsObj, typeof(List<SkillSaveData>)));

            if (dict.TryGetValue("run", out var runObj))
            {
                currentRun = (RunProgress)SaveRegistry.ConvertValue(runObj, typeof(RunProgress));
                cachedRun = currentRun;
                ApplyRunProgress(currentRun);
            }
            else
            {
                CreateEmptyRun();
            }

            isSaveLoaded = true;
            IsDataReady = true;
            OnSaveDataReady?.Invoke();
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveManager] 加载失败: {e.Message}");
            return false;
        }
    }

    /// <summary>
    /// 从当前玩家状态构建限时进度数据
    /// </summary>
    private RunProgress GetRunProgress()
    {
        if (currentRun == null)
        {
            CreateEmptyRun();
        }


        currentRun.playerPosX = transform.position.x;
        currentRun.playerPosY = transform.position.y;
        currentRun.playerPosZ = transform.position.z;
        currentRun.playerRotY = transform.eulerAngles.y;

        //if (LevelManager.Instance != null)
        //{
        //    currentRun.hasActiveRun = true;
        //    currentRun.currentLevelIndex = LevelManager.Instance.CurrentLevelIndex;
        //    currentRun.statistics.killCount = LevelManager.Instance.CurrentKillCount;
        //    currentRun.statistics.playTime = LevelManager.Instance.PlayTime;
        //}

        List<EnemySaveData> cachedDeaths = new List<EnemySaveData>(currentRun.enemies);
        currentRun.enemies.Clear();

        // 获取场景中的敌人列表
        EnemyList listComp = FindObjectOfType<EnemyList>();
        if (listComp != null && listComp.sceneEnemies != null)
        {
            foreach (var enemy in listComp.sceneEnemies)
            {
                // ★ 引用为 null，说明已经销毁了。死讯已经在 cachedDeaths 里了，跳过。
                if (enemy == null) continue;

                EnemyPersistentId idComp = enemy.GetComponent<EnemyPersistentId>();
                EnemyStats stats = enemy.GetComponent<EnemyStats>();

                if (idComp == null || stats == null) continue;

                // 如果敌人还在场景里（没被销毁），读取它的真实状态
                currentRun.enemies.Add(new EnemySaveData
                {
                    uniqueId = idComp.UniqueId,
                    isDead = enemy.IsDead,
                    currentHP = enemy.IsDead ? 0f : stats.CurrentHP
                });
            }
        }

        // ★ 把缓存里的死讯合并进来（这些通常是被销毁的敌人的记录）
        foreach (var deadData in cachedDeaths)
        {
            // 如果列表里没存到这个死人的记录，说明它确实被销毁了，补上
            if (!currentRun.enemies.Exists(e => e.uniqueId == deadData.uniqueId))
            {
                currentRun.enemies.Add(deadData);
            }
        }

        return currentRun;
    }



    /// <summary>
    /// 将限时进度应用到玩家状态（加载时调用）
    /// </summary>
    // SaveManager.cs
    [Obsolete("目前没有使用的需求")]
    private void ApplyRunProgress(RunProgress run)
    {
        if (run == null) return;

        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.RestoreRunProgress(run);
        }

    }


    /// <summary>
    /// 加载或初始化玩家位置
    /// </summary>
    private void LoadOrInitTransform(RunProgress run)
    {
        PlayerControl pc = GetComponent<PlayerControl>();
        if (pc == null) return;

        if (run != null && (run.playerPosX != 0f || run.playerPosY != 0f || run.playerPosZ != 0f))
        {
            Vector3 pos = new Vector3(run.playerPosX, run.playerPosY, run.playerPosZ);
            pc.TeleportTo(pos, run.playerRotY);
        }
        else
        {
            pc.MoveToSpawnPoint();
        }
    }

    public void ResetRun()
    {
        if (!IsServer) return;

        // 重置限时进度到初始状态
        currentRun = new RunProgress
        {
            currentLevelIndex = 0,
            temporaryBuffs = new List<RunBuffData>(),
            statistics = new RunStatistics(),
        };

        // 应用到玩家身上
        ApplyRunProgress(currentRun);

        // 保存到文件
        Save();
        UpdateCachedRun();

    }

    /// <summary>
    /// 删除当前玩家存档文件
    /// </summary>
    public void DeleteSave()
    {
        if (!IsServer) return;
        string filePath = GetSaveFilePath();
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            Debug.Log($"[SaveManager] 玩家 {SteamIdString} 存档已删除: {filePath}");
        }

        ResetAllData();
    }

    private void ResetAllData()
    {
        // 清空存档缓存
        cachedRun = null;
        isSaveLoaded = false;
        IsDataReady = false;
        currentRun = null;

        if (skillManager != null) skillManager.InitializeDefaults();

        if (weaponManager != null)
        {
            weaponManager.CanUseSword = false;
            weaponManager.CanUseGun = false;
            weaponManager.SwitchTo(WeaponManager.WeaponType.Hand, true);
        }

        PlayerControl pc = GetComponent<PlayerControl>();
        if (pc != null)
        {
            pc.MoveToSpawnPoint();
        }

        LevelFlowController flowController = FindObjectOfType<LevelFlowController>();
        if (flowController != null)
        {
            flowController.ResetFlow();
        }

        Debug.Log("[SaveManager] 内存存档数据已重置");
    }

    /// <summary>
    /// 删除玩家的存档
    /// </summary>
    /// <param name="player">玩家对象</param>
    public void DeleteSaveWithPlayer(GameObject player)
    {
        if (!IsServer) return;
        isPlayerDead = true;
        DeleteSave();
    }

    void OnLocalPlayerSpawned(PlayerControl pc)
    {
        playerHealth = pc.GetComponent<PlayerHealth>();
        if (playerHealth != null)
        {
            playerHealth.OnPlayerDied += DeleteSaveWithPlayer;
        }
    }

    private void ResetRunOnDeath(GameObject player)
    {
        if (!IsServer) return;
        ResetRun();
    }

    private string GetSaveFilePath()
    {
        return Path.Combine(saveDirectory, $"save_{SteamIdString}.json");
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer)
        {
            if (!isPlayerDead) 
                Save();

            if (autoSaveCoroutine != null)
                StopCoroutine(autoSaveCoroutine);
        }
    }

    private void OnApplicationQuit()
    {
        if (IsServer && !isPlayerDead) // ★ 游戏退出时正常保存
        {
            Save();
        }
    }

    private void CreateEmptyRun()
    {
        currentRun = new RunProgress
        {
            currentLevelIndex = 0,
            temporaryBuffs = new List<RunBuffData>(),
            statistics = new RunStatistics(),
        };
        UpdateCachedRun();
        IsDataReady = true;
        isPlayerDead = false;
        OnSaveDataReady?.Invoke();
    }

 

    private void UpdateCachedRun()
    {
        cachedRun = currentRun;
        if (cachedRun != null && cachedRun.enemies == null)
            cachedRun.enemies = new List<EnemySaveData>();
    }


    //静态方法
    public static void ReportEnemyDeath(string uniqueId)
    {
        if (cachedRun == null || cachedRun.enemies == null) return;
        var list = cachedRun.enemies;
        var existing = list.Find(e => e.uniqueId == uniqueId);
        if (existing != null) { existing.isDead = true; existing.currentHP = 0f; }
        else list.Add(new EnemySaveData { uniqueId = uniqueId, isDead = true, currentHP = 0f });
    }

    public static bool TryGetEnemySaveData(string uniqueId, out EnemySaveData data)
    {
        data = null;
        if (cachedRun?.enemies == null) return false;
        foreach (var e in cachedRun.enemies)
            if (e.uniqueId == uniqueId) { data = e; return true; }
        return false;
    }

    public bool IsWeaponUnlocked(WeaponManager.WeaponType weaponType)
    {
        if (weaponManager == null) return false;
        return weaponType switch
        {
            WeaponManager.WeaponType.Sword => weaponManager.CanUseSword,
            WeaponManager.WeaponType.Gun => weaponManager.CanUseGun,
            _ => false
        };
    }

    public int GetSavedWeaponType()
    {
        return weaponManager != null ? (int)weaponManager.CurrentWeaponType : 0;
    }

    public static int GetSavedFlowStepIndex()
        => cachedRun != null ? cachedRun.currentFlowStepIndex : 0;

    public static void SaveCurrentFlowStepIndex(int stepIndex)
    {
        if (cachedRun == null) cachedRun = new RunProgress();
        cachedRun.currentFlowStepIndex = stepIndex;
    }

    public static int GetSavedFlowConditionProgress()
        => cachedRun != null ? cachedRun.currentFlowConditionProgress : 0;

    public static void SaveCurrentFlowProgress(int progress)
    {
        if (cachedRun == null) cachedRun = new RunProgress();
        cachedRun.currentFlowConditionProgress = progress;
    }

}