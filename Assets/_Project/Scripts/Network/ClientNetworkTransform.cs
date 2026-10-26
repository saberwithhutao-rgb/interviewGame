using Unity.Netcode.Components;
using UnityEngine;

/// <summary>
/// 客户端网络变换组件，继承自 NetworkTransform，用于在客户端上处理变换同步
/// </summary>
[DisallowMultipleComponent]
public class ClientNetworkTransform : NetworkTransform
{
    protected override bool OnIsServerAuthoritative()
    {
        return false;
    }
}
