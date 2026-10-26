using Unity.Netcode;

/// <summary>
/// 用于标记子弹的拥有者（发射者）的组件
/// </summary>
public class BulletOwner : NetworkBehaviour
{
    /// <summary>
    /// 拥有者的客户端 ID
    /// </summary>
    public ulong ShooterClientId;
}