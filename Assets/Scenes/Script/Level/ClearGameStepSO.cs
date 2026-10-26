using UnityEngine;

/// <summary>
/// 通关动作：目前留空，待后续接入通关 UI 或结算逻辑
/// </summary>
[CreateAssetMenu(menuName = "关卡流程/动作/通关", fileName = "ClearGameStep")]
public class ClearGameStepSO : FlowStepSO
{
    public override void StartStep(LevelFlowController controller)
    {
        // 动作类不需要监听条件，留空
    }

    public override void StopStep(LevelFlowController controller)
    {
        // 不需要取消监听，留空
    }

    public override void ExecuteAction(LevelFlowController controller)
    {
        UIManager.Instance?.OpenPausePanel("WinPanel Plot");
    }
}