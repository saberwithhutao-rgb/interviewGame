using UnityEngine;

/// <summary>
/// ScoreManager 类用于管理游戏中的分数系统，负责记录和更新玩家的总分数
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private int scorePerKill = 100;
    private int totalScore;

    public int TotalScore => totalScore;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void OnEnable() => Enemy.OnAnyEnemyDied += OnEnemyKilled;
    void OnDisable() => Enemy.OnAnyEnemyDied -= OnEnemyKilled;

    void OnEnemyKilled(Enemy enemy)
    {
        AddScore(scorePerKill);
    }

    /// <summary>
    /// 增加分数的方法，根据传入的分数值增加总分数
    /// </summary>
    /// <param name="amount">要增加的分数值</param>
    public void AddScore(int amount)
    {
        totalScore += amount;
    }

    /// <summary>
    /// 重置分数的方法，将总分数重置为零
    /// </summary>
    public void ResetScore()
    {
        totalScore = 0;
    }
}