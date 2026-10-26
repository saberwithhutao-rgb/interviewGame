using UnityEngine;

[CreateAssetMenu(menuName = "关卡流程/条件/Boss死亡", fileName = "BossDeadStep")]
public class BossDeadStepSO : FlowStepSO
{
    public string bossUniqueId;

    private LevelFlowController currentController;
    private bool isListening = false;

    public override void StartStep(LevelFlowController controller)
    {
        // ★ 强制清理上一次的订阅（防止 Domain Reload 导致状态残留）
        StopStep(controller);

        currentController = controller;
        isListening = true;
        GameEvents.OnEntityKilled += OnEntityKilled;

    }

    private void OnEntityKilled(GameObject victim, GameObject killer)
    {
        if (!isListening) return;

        EnemyPersistentId idComp = victim.GetComponent<EnemyPersistentId>();

        if (idComp == null)
        {
            Debug.Log($"[排查] 死掉的 {victim.name} 身上根本没有 EnemyPersistentId 组件！");
            return;
        }

        if (idComp != null && idComp.UniqueId == bossUniqueId)
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

    public override void ExecuteAction(LevelFlowController controller)
    {
        Debug.Log("[BossDeadStepSO] Boss 已击败，准备执行通关动作");
    }

    public override void UpdateTaskUI()
    {
        TaskUIManager.Instance?.UpdateTaskText("击败 Boss");
    }
}