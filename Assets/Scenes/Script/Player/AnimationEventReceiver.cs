using UnityEngine;

/// <summary>
/// 挂在 Animator 所在的 GameObject 上，接收动画事件并转发给对应的武器
/// </summary>
public class AnimationEventReceiver : MonoBehaviour
{
    [SerializeField] private Sword sword;

    [SerializeField] private Gun gun;


    private void Start()
    {
        // 初始化时找到剑
        if (sword == null)
            UpdateWeaponReference();
    }

    /// <summary>
    /// 更新武器引用（换武器时调用）
    /// </summary>
    public void UpdateWeaponReference()
    {
        sword = GetComponentInChildren<Sword>();
    }

    // ============================================================
    // 攻击动画事件（由动画事件调用）
    // ============================================================

    /// <summary>
    /// 剑的攻击动画播放到开始攻击时调用
    /// </summary>
    public void OnAttackStart()
    {
        if (sword != null)
            sword.OnAttackStart();
        else
            Debug.LogWarning("AnimationEventReceiver: 没有找到 Sword 组件");
    }

    /// <summary>
    /// 剑的攻击动画播放到攻击结束时调用
    /// </summary>
    public void OnAttackEnd()
    {
        if (sword != null)
        {
            sword.OnAttackEnd();
        }
    }

    // ============================================================
    // 扩展：其他动画事件（预留）
    // ============================================================

    public void OnReloadComplete()
    {
        //留白
    }
}