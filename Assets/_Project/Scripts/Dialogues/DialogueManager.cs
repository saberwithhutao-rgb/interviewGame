using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// 对话管理器，负责对话的播放、跳转、效果执行
/// </summary>
public class DialogueManager : MonoSingleton<DialogueManager>
{
    // 当前对话的所有节点
    private Dictionary<int, DialogueNode> currentNodes;

    // 当前正在播放的节点 id
    private int currentNodeId = -1;

    // 当前是否正在对话
    private bool isPlaying = false;
    public bool IsPlaying => isPlaying;

    // ============================================================
    // 事件（供 DialogueUI 订阅）
    // ============================================================

    /// <summary>显示一句对话</summary>
    public event Action<DialogueNode> OnLineReceived;

    /// <summary>显示一组选项</summary>
    public event Action<List<DialogueNode>> OnOptionsReceived;

    /// <summary>对话开始</summary>
    public event Action OnDialogueStarted;

    /// <summary>对话结束</summary>
    public event Action OnDialogueEnded;



    // ============================================================
    // 开始 / 结束
    // ============================================================

    /// <summary>
    /// 开始一段对话
    /// </summary>
    /// <param name="fileName">Resources/Dialogues/ 下的文件名（不含扩展名）</param>
    public void StartDialogue(string fileName)
    {
        if (isPlaying)
        {
            Debug.LogWarning("[DialogueManager] 已有对话进行中");
            return;
        }

        currentNodes = DialogueCSVParser.LoadFromResources(fileName);

        if (currentNodes == null || currentNodes.Count == 0)
        {
            Debug.LogError($"[DialogueManager] 对话文件为空: {fileName}");
            return;
        }

        isPlaying = true;

        // 从 id=1 开始（如果 1 不存在，找最小的 id）
        int startId = FindFirstNodeId();
        ShowNode(startId);

        OnDialogueStarted?.Invoke();
    }

    /// <summary>
    /// 结束对话
    /// </summary>
    public void EndDialogue()
    {
        isPlaying = false;
        currentNodeId = -1;
        currentNodes = null;

        OnDialogueEnded?.Invoke();
    }

    // ============================================================
    // 播放逻辑
    // ============================================================

    /// <summary>
    /// 显示指定 id 的节点
    /// </summary>
    private void ShowNode(int id)
    {
        if (!currentNodes.TryGetValue(id, out DialogueNode node))
        {
            Debug.LogWarning($"[DialogueManager] 找不到节点 id={id}，结束对话");
            EndDialogue();
            return;
        }

        currentNodeId = id;

        // 结束节点
        if (node.IsEnd)
        {
            EndDialogue();
            return;
        }

        // 根据类型分派
        if (node.rowType == DialogueRowType.Line)
        {
            OnLineReceived?.Invoke(node);
        }
        else if (node.rowType == DialogueRowType.Option)
        {
            ShowOptionsFrom(id);
        }
    }

    /// <summary>
    /// 从指定的起始 id 收集连续的 option，并通知 UI
    /// </summary>
    private void ShowOptionsFrom(int startId)
    {
        List<DialogueNode> options = new List<DialogueNode>();
        int currentId = startId;

        // 连续收集 option，直到遇到非 option
        while (currentNodes.TryGetValue(currentId, out DialogueNode node)
               && node.rowType == DialogueRowType.Option)
        {
            options.Add(node);
            currentId++;
        }

        if (options.Count == 0)
        {
            Debug.LogWarning($"[DialogueManager] id={startId} 没有找到有效选项");
            EndDialogue();
            return;
        }

        OnOptionsReceived?.Invoke(options);
    }

    // ============================================================
    // 外部输入（由 DialogueUI 调用）
    // ============================================================

    /// <summary>
    /// 玩家点击继续（显示下一句）
    /// </summary>
    public void OnNextClicked()
    {
        if (!isPlaying) return;

        if (!currentNodes.TryGetValue(currentNodeId, out DialogueNode node))
        {
            EndDialogue();
            return;
        }

        // ★ line 的效果：点击跳转时执行
        if (node.rowType == DialogueRowType.Line && node.HasEffect)
        {
            DialogueEffectExecutor.Execute(node, GetLocalPlayer());
        }

        int nextId = node.nextId;

        // 跳转目标如果是 option，就进入选项流程
        if (currentNodes.TryGetValue(nextId, out DialogueNode nextNode)
            && nextNode.rowType == DialogueRowType.Option)
        {
            ShowOptionsFrom(nextId);
        }
        else
        {
            ShowNode(nextId);
        }
    }

    /// <summary>
    /// 玩家选择了一个选项
    /// </summary>
    /// <param name="option">被选中的选项节点</param>
    public void OnOptionSelected(DialogueNode option)
    {
        if (!isPlaying) return;
        if (option == null) return;


        // ★ option 的效果：选完立即执行
        if (option.HasEffect)
        {
            DialogueEffectExecutor.Execute(option, GetLocalPlayer());
        }

        int nextId = option.nextId;

        if (currentNodes.TryGetValue(nextId, out DialogueNode nextNode)
            && nextNode.rowType == DialogueRowType.Option)
        {
            ShowOptionsFrom(nextId);
        }
        else
        {
            ShowNode(nextId);
        }
    }

    // ============================================================
    // 辅助方法
    // ============================================================

    /// <summary>
    /// 找到对话的第一个节点（优先 id=1，找不到就取最小 id）
    /// </summary>
    private int FindFirstNodeId()
    {
        if (currentNodes.ContainsKey(1))
            return 1;

        int minId = int.MaxValue;
        foreach (var id in currentNodes.Keys)
        {
            if (id < minId) minId = id;
        }
        return minId;
    }

    /// <summary>
    /// 获取本地玩家对象（用于执行效果）
    /// </summary>
    private GameObject GetLocalPlayer()
    {
        if (PlayerControl.LocalInstance != null)
            return PlayerControl.LocalInstance.gameObject;
        return null;
    }
}