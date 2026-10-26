using UnityEngine;

/// <summary>
/// Buff 管理器，负责应用 Buff 效果
/// </summary>
public class BuffManager : MonoBehaviour
{
    /// <summary>
    /// 应用 Buff 效果到目标对象
    /// </summary>
    /// <param name="buff">要应用的 Buff 数据</param>
    public void ApplyBuff(BuffData buff)
    {
        buff.Apply(gameObject);
    }
}