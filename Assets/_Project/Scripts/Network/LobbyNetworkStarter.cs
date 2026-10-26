using Netcode.Transports.Facepunch;
using Steamworks;
using Steamworks.Data;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

/// <summary>
/// 在 MultiplayerScene 加载后，等待大厅就绪，然后根据 GameSessionData 启动 Host 或 Client。
/// </summary>
public class LobbyNetworkStarter : MonoBehaviour
{
    [Header("调试")]
    [SerializeField] private float waitTimeout = 15f;  // 等待大厅就绪的超时时间

    private IEnumerator Start()
    {
        // 等待大厅就绪
        yield return WaitForLobbyReady(waitTimeout);

        if (!SteamManager.Instance.IsLobbyReady)
        {
            Debug.LogError("[LobbyNetworkStarter] 大厅未在超时时间内就绪，无法启动网络。");
            yield break;
        }

        Lobby lobby = SteamManager.Instance.mLobby;
        if (lobby.Id == 0)
        {
            Debug.LogError("[LobbyNetworkStarter] mLobby 无效 (Id=0)。");
            yield break;
        }

        // 根据主菜单选择的角色启动网络
        string role = GameSessionData.NetworkRole;
        if (string.IsNullOrEmpty(role))
        {
            Debug.LogError("[LobbyNetworkStarter] NetworkRole 未设置，请在主菜单选择 Host 或 Client。");
            yield break;
        }

        var transport = NetworkManager.Singleton.GetComponent<FacepunchTransport>();
        if (transport == null)
        {
            Debug.LogError("[LobbyNetworkStarter] 未找到 FacepunchTransport 组件！");
            yield break;
        }

        if (role == "Host")
        {
            transport.targetSteamId = SteamClient.SteamId;
            bool success = NetworkManager.Singleton.StartHost();

            if (success)
            {
                Debug.Log("[LobbyNetworkStarter] Host 启动成功。");
            }
            else
            {
                Debug.LogError("[LobbyNetworkStarter] Host 启动失败。");
            }
        }
        else if (role == "Client")
        {
            transport.targetSteamId = lobby.Owner.Id;
            NetworkManager.Singleton.StartClient();
            Debug.Log($"[LobbyNetworkStarter] 已启动 Client，连接目标: {lobby.Owner.Id}");
        }
        else
        {
            Debug.LogError($"[LobbyNetworkStarter] 未知的 NetworkRole: {role}");
        }
    }

    /// <summary>
    /// 等待 SteamManager 的大厅标志变为 true，或超时。
    /// </summary>
    private IEnumerator WaitForLobbyReady(float timeout)
    {
        float elapsed = 0f;
        while (!SteamManager.Instance.IsLobbyReady && elapsed < timeout)
        {
            yield return null;
            elapsed += Time.deltaTime;
        }

        if (!SteamManager.Instance.IsLobbyReady)
        {
            Debug.LogWarning($"[LobbyNetworkStarter] 等待大厅就绪超时 ({timeout}秒)。");
        }
    }
}