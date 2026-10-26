using TMPro;
using UnityEngine;

/// <summary>
/// EndlessKillUI 类用于显示无尽模式下的击杀数 UI
/// </summary>
public class EndlessKillUI : MonoBehaviour
{
    [SerializeField] private GameObject killCountPanel;
    [SerializeField] private TMP_Text KillCountText;

    void OnEnable()
    {
        LevelManager.OnEndlessStarted += Show;
        Enemy.OnAnyEnemyDied += OnKill;
    }

    void OnDisable()
    {
        LevelManager.OnEndlessStarted -= Show;
        Enemy.OnAnyEnemyDied -= OnKill;
    }

    void Show()
    {
        //UIManager.Instance.OpenPanel("KillCountPanel");
        killCountPanel.SetActive(true);
        UpdateDisplay(0);
    }

    void OnKill(Enemy enemy)
    {
        if (LevelManager.Instance == null || !LevelManager.Instance.IsEndless) return;

        UpdateDisplay(LevelManager.Instance.EndlessKillCount);
    }

    void UpdateDisplay(int count)
    {
        KillCountText.text = $"击杀数: {count}";
    }
}