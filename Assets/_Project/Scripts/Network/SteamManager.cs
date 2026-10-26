using UnityEngine;
using System.Collections;
using Steamworks;
using Steamworks.Data;
using System.Threading.Tasks;
using System.Collections.Generic;
using Netcode.Transports.Facepunch;
using System;
using Unity.Netcode;

/// <summary>
/// SteamManager 类用于管理 Steam 大厅的创建、加入和事件处理
/// </summary>
[DisallowMultipleComponent]
public class SteamManager : MonoSingleton<SteamManager>
{
    //大厅
    [HideInInspector]
    public Lobby mLobby;
    [HideInInspector]
    public ELobbyType mLobbyType;//玩家设置的大厅类型

    //Steam初始化事件
    public event Action OnSteamInitialized;
    //更新大厅事件
    public event Action OnLobbyUpdate;
    //客户端成功加入大厅事件
    public event Action OnJoinLobbySuccess;

    public bool IsLobbyReady { get; private set; }

    /// <summary>
    /// 异步创建大厅
    /// </summary>
    /// <param name="lobbyType">创建大厅的类型</param>
    /// <param name="MaxNumber">创建大厅的人数</param>
    public async Task CreatLobbyAsync(ELobbyType lobbyType, int MaxNumber)
    {
        Debug.Log("开始创建大厅");

        await SteamMatchmaking.CreateLobbyAsync(MaxNumber);
        mLobbyType = lobbyType;
    }

    /// <summary>
    /// 加入游戏大厅
    /// </summary>
    /// <param name="lobbyID"></param>
    /// <returns></returns>
    public async Task<bool> JoinLobbyAsync(string lobbyID)
    {
        //将得到的string转换为ulong
        ulong ID;
        if (!ulong.TryParse(lobbyID, out ID))
        {
            Debug.LogWarning("加入大厅失败，ID错误" + lobbyID);
            return false;
        }

        //将ulong转换为SteamID并加入大厅
        SteamId steamId = ID;
        Lobby? lobby = await SteamMatchmaking.JoinLobbyAsync(steamId);
        if (lobby == null)
        {
            Debug.Log("加入大厅失败");
            return false;
        }
        else
        {
            JoinLobbySuccess(lobby ?? new Lobby());
            Debug.Log("加入大厅成功");
            return true;
        }
    }

    /// <summary>
    /// 离开大厅
    /// </summary>
    public void LeaveLobby()
    {
        Debug.Log("离开大厅");
        //关闭网络连接
        NetworkManager.Singleton.Shutdown();
        mLobby.Leave();
    }

    /*-----------------------回调-----------------------*/
    /// <summary>
    /// 有人加入大厅
    /// </summary>
    /// <param name="lobby"></param>
    /// <param name="friend"></param>
    private void MemberJoined(Lobby lobby, Friend friend)
    {
        Debug.LogWarning(friend.Name + "加入大厅");
        OnLobbyUpdate?.Invoke();
    }

    /// <summary>
    /// 有人离开大厅
    /// </summary>
    private void MemberLeave(Lobby lobby, Friend friend)
    {
        Debug.LogWarning(friend.Name + "离开大厅");
        OnLobbyUpdate?.Invoke();
    }

    /// <summary>
    /// 大厅已创建
    /// </summary>
    private void LobbyCreated(Result result, Lobby lobby)
    {
        mLobby = lobby;
        //设置大厅类型，并允许加入
        if (result == Result.OK)
        {
            switch (mLobbyType)
            {
                //公开的大厅
                case ELobbyType.Public:
                    lobby.SetPublic();
                    lobby.SetJoinable(true);
                    break;
                //好友大厅
                case ELobbyType.FriendsOnly:
                    lobby.SetFriendsOnly();
                    lobby.SetJoinable(true);
                    break;
                //私有大厅
                case ELobbyType.Private:
                    lobby.SetPrivate();
                    lobby.SetJoinable(true);
                    break;
                default:
                    break;
            }

            IsLobbyReady = true;

            Debug.Log("大厅创建成功");
            Debug.Log($"大厅 ID: {lobby.Id}");
            //刷新大厅
            OnLobbyUpdate?.Invoke();
            //启动Host 交给 LobbyNetworkStarter
            //NetworkManager.Singleton.StartHost();
        }
        else
        {
            Debug.LogWarning("大厅创建失败" + result.ToString());
        }
    }

    /// <summary>
    /// 从好友列表申请加入大厅时调用
    /// </summary>
    /// <param name="lobby"></param>
    /// <param name="id"></param>
    private async void LobbyJoinRequested(Lobby lobby, SteamId id)
    {
        //正在加入大厅
        Debug.Log("正在加入大厅");
        RoomEnter roomEnter = await lobby.Join();
        if (roomEnter == RoomEnter.Success)
        {
            //加入大厅成功
            JoinLobbySuccess(lobby);
        }
        else
        {
            Debug.Log("加入大厅失败" + roomEnter);
        }
    }

    /// <summary>
    /// 有人成功加入大厅(只有主机调用? 可能没用)
    /// </summary>
    /// <param name="lobby"></param>
    private void LobbyEntered(Lobby lobby)
    {
        if (NetworkManager.Singleton.IsHost)
        {
            Debug.Log(lobby.Owner.Name + "已成功加入大厅");
            return;
        }

        //刷新加入大厅面板
        OnLobbyUpdate?.Invoke();
        Debug.Log("客人已成功加入大厅");
    }

    /*-----------------------其它方法-----------------------*/
    /// <summary>
    /// 客户端加入大厅成功
    /// </summary>
    private void JoinLobbySuccess(Lobby lobby)
    {
        //加入大厅成功
        Debug.Log("成功加入" + lobby.Owner.Name + "的大厅");
        //指定大厅
        mLobby = lobby;
        IsLobbyReady = true;
        OnJoinLobbySuccess?.Invoke();
    }

    private void Start()
    {
        try
        {
            if (!SteamClient.IsValid)
            {
                SteamClient.Init(480);  // 你的 App ID
            }
            OnSteamInitialized?.Invoke();
        }
        catch (System.Exception e)
        {
            Debug.LogError($"Steam 初始化失败: {e.Message}");
            return;
        }

        SteamMatchmaking.OnLobbyCreated += LobbyCreated;
        SteamMatchmaking.OnLobbyEntered += LobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined += MemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave += MemberLeave;
        SteamFriends.OnGameLobbyJoinRequested += LobbyJoinRequested;
    }

    void Update()
    {
        SteamClient.RunCallbacks();
    }

    //private void OnDestroy()  改前代码
    //{
    //    SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
    //    SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
    //    SteamMatchmaking.OnLobbyMemberJoined -= MemberJoined;
    //    SteamMatchmaking.OnLobbyMemberLeave -= MemberLeave;
    //    SteamFriends.OnGameLobbyJoinRequested -= LobbyJoinRequested;
    //}
    protected override void OnDestroy()
    {
        SteamMatchmaking.OnLobbyCreated -= LobbyCreated;
        SteamMatchmaking.OnLobbyEntered -= LobbyEntered;
        SteamMatchmaking.OnLobbyMemberJoined -= MemberJoined;
        SteamMatchmaking.OnLobbyMemberLeave -= MemberLeave;
        SteamFriends.OnGameLobbyJoinRequested -= LobbyJoinRequested;

        base.OnDestroy();
    }
}

/// <summary>
/// 大厅的类型
/// </summary>
public enum ELobbyType
{
    None = 0,
    Public = 1,
    FriendsOnly = 2,
    Private = 3
}