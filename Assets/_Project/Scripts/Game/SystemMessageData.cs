using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 
/// </summary>
[System.Serializable]
public class SystemMessageData
{
    public string content;
    public float duration;
    public MessageType messageType;

    public SystemMessageData(string content, MessageType messageType = MessageType.Normal, float duration = -1f)
    {
        this.content = content;
        this.messageType = messageType;
        this.duration = duration;
    }
}

/// <summary>
/// 消息类型枚举
/// </summary>
public enum MessageType
{
    Normal,         // 普通信息（白色）
    Success,        // 成功（绿色）
    Warning,        // 警告（黄色）
    Error,          // 错误（红色）
    Achievement,    // 成就/里程碑（金色）
    Combat          // 战斗（橙色）
}