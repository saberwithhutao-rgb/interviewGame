using UnityEngine;

/// <summary>
/// Buff 数据类，继承自 ScriptableObject，用于定义 Buff 的属性和效果
/// </summary>
[CreateAssetMenu(fileName = "Buff_01", menuName = "Buff 数据")]
public class BuffData : ScriptableObject
{
    public string buffName = "未命名";
    [TextArea] public string description = "";
    public Sprite icon;

    [SerializeField] private BuffEffect[] effects;

    /// <summary>
    /// 应用 Buff 效果到目标对象
    /// </summary>
    /// <param name="target">要应用 Buff 的目标对象</param>
    public void Apply(GameObject target)
    {
        foreach (var effect in effects)
        {
            if (effect != null)
                effect.OnApply(target);
        }
    }

    /// <summary>
    /// 移除 Buff 效果从目标对象
    /// </summary>
    /// <param name="target">要移除 Buff 的目标对象</param>
    public void Remove(GameObject target)
    {
        foreach (var effect in effects)
        {
            if (effect != null)
                effect.OnRemove(target);
        }
    }

    /// <summary>
    /// 获取 Buff 的名称，如果未设置名称，则使用第一个效果的类型名称作为默认值，如果没有效果，则返回 "未命名"
    /// </summary>
    public string Name
    {
        get
        {
            if (!string.IsNullOrEmpty(buffName))
                return buffName;
            if (effects != null && effects.Length > 0 && effects[0] != null)
                return effects[0].ToString().Replace(" (BuffEffect)", "");
            return "未命名";
        }
    }

    /// <summary>
    /// 获取 Buff 的描述，如果未设置描述，则返回空字符串
    /// </summary>
    public string Description
    {
        get
        {
            if (string.IsNullOrEmpty(description)) return "";
            return description;
        }
    }
}