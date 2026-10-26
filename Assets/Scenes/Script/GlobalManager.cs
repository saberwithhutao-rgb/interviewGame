using UnityEngine;

/// <summary>
/// 全局单例根节点。挂载此脚本的 GameObject 会跨场景保留，
/// 所有需要全局唯一的 Manager 都放在其子级。
/// </summary>
public class GlobalManager : MonoSingleton<GlobalManager>
{
    // 不需要任何业务逻辑
    // 只需要继承 MonoSingleton 自动获得 DontDestroyOnLoad
}