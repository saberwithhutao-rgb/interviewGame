using System.Globalization;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// PlayerComponentEnabler 类用于在网络游戏中启用或禁用玩家的组件。
/// </summary>
public class PlayerComponentEnabler : NetworkBehaviour
{
    [SerializeField] private GameObject playerRoot;

    /// <summary>
    /// 在网络生成时调用的方法。根据玩家是否拥有该对象，启用或禁用 playerRoot 下的所有组件。
    /// </summary>
    public override void OnNetworkSpawn()
    {
        foreach (var comp in playerRoot.GetComponentsInChildren<MonoBehaviour>(true))
        {
            if (comp is NetworkBehaviour) continue;
            comp.enabled = IsOwner;
        }
    }
}