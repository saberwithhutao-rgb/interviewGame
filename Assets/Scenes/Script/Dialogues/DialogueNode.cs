using System;

/// <summary>
/// 单个对话节点，对应 CSV 中的一行
/// </summary>
[Serializable]
public class DialogueNode
{
    public DialogueRowType rowType;
    public int id;
    public string speaker;
    public string content;
    public int nextId;

    // 条件与效果（暂时留空）
    public string conditionType;
    public string conditionValue;
    public string effectType;
    public string effectValue;

    public bool HasCondition => !string.IsNullOrEmpty(conditionType);
    public bool HasEffect => !string.IsNullOrEmpty(effectType);

    /// <summary>
    /// 是否是结束节点
    /// </summary>
    public bool IsEnd => rowType == DialogueRowType.End || nextId == -1;
}