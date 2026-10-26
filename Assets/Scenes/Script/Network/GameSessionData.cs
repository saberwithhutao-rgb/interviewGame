/// <summary>
/// 跨场景临时数据，主菜单设置，游戏场景读取后自动清零。
/// </summary>
public static class GameSessionData
{
    public static string NetworkRole = "";   // "Host", "Server", "Client"
    public static ulong TargetSteamId = 0;  // 要连接的目标玩家 Steam ID

    /// <summary> 进入游戏场景后调用，清空临时数据。 </summary>
    public static void Clear()
    {
        NetworkRole = "";
        TargetSteamId = 0;
    }
}