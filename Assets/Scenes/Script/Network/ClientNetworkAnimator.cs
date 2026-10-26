using System.Collections;
using System.Collections.Generic;
using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// 客户端网络动画器，继承自 NetworkAnimator，用于在客户端上处理动画同步
/// </summary>
public class ClientNetworkAnimator : NetworkAnimator
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
