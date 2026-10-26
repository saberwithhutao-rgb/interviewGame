using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class LevelFlowController : NetworkBehaviour
{
    // ★ 把条件和动作绑在一起
    [Serializable]
    public class FlowStepConfig
    {
        [Tooltip("条件：比如杀怪、Boss死亡")]
        public FlowStepSO condition;

        [Tooltip("动作：条件满足后执行，比如开门、通关")]
        public FlowStepSO action;
    }

    [Header("流程配置 (条件和动作按顺序执行)")]
    [SerializeField] private List<FlowStepConfig> flowSteps = new List<FlowStepConfig>();

    [Header("场景引用")]
    public Transform doorToMove;

    private int currentStepIndex = 0;
    private bool isConditionMet = false;

    private bool _isRestoring = false;
    private bool IsRestoringCurrentStep() => _isRestoring;

    public override void OnNetworkSpawn()
    {
        if (!IsServer) return;

        if (flowSteps == null || flowSteps.Count == 0)
        {
            Debug.LogWarning("[LevelFlowController] 没有配置任何流程步骤！");
            return;
        }

        if (SaveManager.IsDataReady)
        {
            RestoreFlowProgress();
        }
        else
        {
            SaveManager.OnSaveDataReady += OnSaveDataReady;
        }
    }

    private void OnSaveDataReady()
    {
        SaveManager.OnSaveDataReady -= OnSaveDataReady;
        RestoreFlowProgress();
    }

    public override void OnNetworkDespawn()
    {
        if (IsServer && flowSteps != null)
        {
            foreach (var step in flowSteps)
            {
                step.condition?.StopStep(this);
                step.action?.StopStep(this);
            }
        }
        SaveManager.OnSaveDataReady -= OnSaveDataReady;
    }

    private void RestoreFlowProgress()
    {
        // 从 SaveManager 获取存档里的流程步骤索引
        int savedIndex = SaveManager.GetSavedFlowStepIndex();
        currentStepIndex = Mathf.Clamp(savedIndex, 0, flowSteps.Count);

        if (currentStepIndex >= flowSteps.Count)
        {
            Debug.Log("[LevelFlowController] 存档显示所有流程已完成，不再执行。");
            return;
        }

        int savedProgress = SaveManager.GetSavedFlowConditionProgress();
        if (flowSteps[currentStepIndex].condition != null)
        {
            flowSteps[currentStepIndex].condition.SetProgress(savedProgress);
            Debug.Log($"[LevelFlowController] 恢复了当前步骤进度: {savedProgress}");
        }

        for (int i = 0; i < currentStepIndex; i++)
        {
            flowSteps[i].action?.ExecuteAction(this);
        }

        StartCoroutine(RunFlow());
    }

    private IEnumerator RunFlow()
    {
        for (; currentStepIndex < flowSteps.Count; currentStepIndex++)
        {
            FlowStepConfig config = flowSteps[currentStepIndex];
            if (config.condition == null && config.action == null) continue;

            isConditionMet = false;

            // 1. 开始监听条件
            if (config.condition != null)
            {
                config.condition.StartStep(this);
                config.condition.UpdateTaskUI();
            }

            // 2. 等待条件达成
            if (config.condition != null)
            {
                yield return new WaitUntil(() => isConditionMet);
            }

            // 3. 执行动作
            if (config.action != null)
            {
                config.action.ExecuteAction(this);
            }
            SaveManager.SaveCurrentFlowStepIndex(currentStepIndex + 1);
            SaveManager.SaveCurrentFlowProgress(0);
            yield return new WaitForSeconds(0.5f);
        }
        yield return new WaitForSeconds(2f);
        TaskUIManager.Instance?.HideTaskPanel();
    }

    /// <summary>
    /// 供 Condition SO 调用的标记方法
    /// </summary>
    public void MarkStepComplete()
    {
        isConditionMet = true;
    }

    public void ResetFlow()
    {
        if (!IsServer) return;

        Debug.Log("[LevelFlowController] 收到删档重置请求，准备重新开始流程");

        // 停止所有协程（中断当前的 WaitUntil 等待）
        StopAllCoroutines();

        // 清理所有条件和动作的监听状态
        if (flowSteps != null)
        {
            foreach (var step in flowSteps)
            {
                step.condition?.StopStep(this);
                step.action?.StopStep(this);
            }
        }

        // 重置变量
        currentStepIndex = 0;
        isConditionMet = false;

        // 重新开始流程
        StartCoroutine(RunFlow());
    }
}