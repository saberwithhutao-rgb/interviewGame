using UnityEngine;

/// <summary>
/// 可交互接口，所有能被玩家按 F 交互的对象都实现它
/// </summary>
public interface IInteractable
{
    /// <summary>交互提示文字，如 "对话"、"拾取"</summary>
    string GetLabelText();

    /// <summary>执行交互</summary>
    void Interact(GameObject interactor);

    /// <summary>是否可以交互（比如 NPC 已经对话过了就不能再交互）</summary>
    bool CanInteract();

    /// <summary>获取交互对象的 Transform（用于距离计算）</summary>
    Transform GetTransform();
}