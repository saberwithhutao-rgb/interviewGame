using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// 自动启动主机的组件，挂在场景中的任意 GameObject 上
/// </summary>
public class AutoHostStarter : MonoBehaviour
{
    void Start()
    {
        if (NetworkManager.Singleton != null &&
            !NetworkManager.Singleton.IsServer &&
            !NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.StartHost();
        }
    }
}