using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class WinMenu : MonoBehaviour
{
    [SerializeField] private GameObject winPanel;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text timeText;

    private void Start()
    {
        if (winPanel == null)
            winPanel = UIManager.Instance.GetPanel("WinPanel");
        if (scoreText == null)
            scoreText = winPanel.transform.Find("ScoreText").GetComponent<TMP_Text>();
        if (timeText == null)
            timeText = winPanel.transform.Find("TimeText").GetComponent<TMP_Text>();
    }
    private void OnEnable()
    {
        LevelManager.OnAllLevelsCompleted += Show;
    }

    private void OnDisable()
    {
        LevelManager.OnAllLevelsCompleted -= Show;
    }

    void Show(int score, float time)
    {
        scoreText.text = $"总分: {score}";
        int minutes = Mathf.FloorToInt(time / 60);
        int seconds = Mathf.FloorToInt(time % 60);
        timeText.text = $"总时间: {minutes:D2}:{seconds:D2}";

        UIManager.Instance.OpenModal("WinPanel");
    }

    public static void GoToEndless()
    {
        UIManager.Instance.CloseModal();
        LevelManager.Instance.StartEndlessMode();
    }
}