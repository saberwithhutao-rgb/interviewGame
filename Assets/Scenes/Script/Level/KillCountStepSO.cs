using UnityEngine;

[CreateAssetMenu(menuName = "关卡流程/条件/击杀数", fileName = "KillCountStep")]
public class KillCountStepSO : FlowStepSO
{
    public int targetKillCount = 10;

    private int currentKillCount;
    private LevelFlowController currentController;
    private bool isListening = false;

    public override void StartStep(LevelFlowController controller)
    {
        StopStep(controller);

        currentController = controller;
        isListening = true;
        GameEvents.OnEntityKilled += OnEntityKilled;
    }

    private void OnEntityKilled(GameObject victim, GameObject killer)
    {
        if (!isListening) return;

        currentKillCount++;

        // ★ 每次杀怪后刷新 UI
        UpdateTaskUI();

        SaveManager.SaveCurrentFlowProgress(currentKillCount);

        if (currentKillCount >= targetKillCount)
        {
            StopStep(currentController);
            currentController.MarkStepComplete();
        }
    }

    public override void StopStep(LevelFlowController controller)
    {
        if (isListening)
        {
            GameEvents.OnEntityKilled -= OnEntityKilled;
            isListening = false;
        }
    }

    public override void ExecuteAction(LevelFlowController controller) { }

    // ★ 重写 UI 刷新方法
    public override void UpdateTaskUI()
    {
        string text = $"击杀 ({currentKillCount}/{targetKillCount}) 个敌人";
        TaskUIManager.Instance?.UpdateTaskText(text);
    }

    public override int GetProgress() => currentKillCount;

    public override void SetProgress(int progress)
    {
        currentKillCount = progress;
        Debug.Log($"[KillCountStepSO] 从存档恢复杀怪进度: {currentKillCount}");
    }
}