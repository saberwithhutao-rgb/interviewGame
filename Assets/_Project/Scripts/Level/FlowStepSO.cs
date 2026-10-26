using UnityEngine;

/// <summary>
/// 流程步骤基类（ScriptableObject 版）
/// </summary>
public abstract class FlowStepSO : ScriptableObject
{
    public abstract void StartStep(LevelFlowController controller);
    public abstract void StopStep(LevelFlowController controller);
    public abstract void ExecuteAction(LevelFlowController controller);

    /// <summary>
    /// 刷新任务面板的 UI（由子类重写）
    /// </summary>
    public virtual void UpdateTaskUI() { }

    public virtual int GetProgress() => 0;
    public virtual void SetProgress(int progress) { }
}