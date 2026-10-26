using UnityEngine;
using Unity.Netcode;

/// <summary>
/// 在加载新场景时销毁 NetworkManager 的组件，挂在场景中的任意 GameObject 上
/// </summary>
public class DestroyNetworkManagerOnLoad : MonoBehaviour
{
    void Awake()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.Shutdown();
            Destroy(NetworkManager.Singleton.gameObject);
        }
    }
}