using TMPro;
using UnityEngine;

public class TaskUIManager : MonoBehaviour
{
    public static TaskUIManager Instance { get; private set; }

    [SerializeField] private GameObject taskPanel;
    [SerializeField] private TextMeshProUGUI taskText;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    /// <summary>
    /// 更新任务文本
    /// </summary>
    public void UpdateTaskText(string text)
    {
        if (taskPanel != null) taskPanel.SetActive(true);
        if (taskText != null) taskText.text = text;
    }

    /// <summary>
    /// 隐藏任务面板（比如全部通关后）
    /// </summary>
    public void HideTaskPanel()
    {
        if (taskPanel != null) taskPanel.SetActive(false);
    }
}