using UnityEngine;
using TMPro;

/// <summary>
/// ScoreUI 类用于在 UI 上显示玩家的总分数，实时更新分数显示
/// </summary>
public class ScoreUI : MonoBehaviour
{
    private TMP_Text scoreText;

    void Awake()
    {
        scoreText = GetComponent<TMP_Text>();
        if (scoreText == null)
        {
            scoreText = gameObject.AddComponent<TextMeshProUGUI>();
        }
    }

    void Update()
    {
        if (ScoreManager.Instance == null) return;
        scoreText.text = $"分数: {ScoreManager.Instance.TotalScore}";
    }
}