using UnityEngine;

/// <summary>
/// 技能按键绑定数据结构
/// </summary>
[System.Serializable]
public class SkillKeyBinding
{
    public KeyCode primaryKey;       // 主键
    public KeyCode modifier1;        // 第一个修饰键
    public KeyCode modifier2;        // 第二个修饰键
    public bool isMouseKey;          // 是否是鼠标按键

    public SkillKeyBinding()
    {
        primaryKey = KeyCode.None;
        modifier1 = KeyCode.None;
        modifier2 = KeyCode.None;
        isMouseKey = false;
    }

    public SkillKeyBinding(KeyCode primary, KeyCode mod1 = KeyCode.None, KeyCode mod2 = KeyCode.None)
    {
        primaryKey = primary;
        modifier1 = mod1;
        modifier2 = mod2;
        isMouseKey = IsMouseKeyCode(primary);
    }

    public bool IsEmpty()
    {
        return primaryKey == KeyCode.None;
    }

    public string GetDisplayName()
    {
        if (IsEmpty()) return "未绑定";

        string result = "";

        if (modifier1 != KeyCode.None)
            result += GetKeyDisplayName(modifier1) + "+";
        if (modifier2 != KeyCode.None)
            result += GetKeyDisplayName(modifier2) + "+";

        result += GetKeyDisplayName(primaryKey);
        return result;
    }

    private string GetKeyDisplayName(KeyCode key)
    {
        // 鼠标按键
        switch (key)
        {
            case KeyCode.Mouse0: return "鼠标左键";
            case KeyCode.Mouse1: return "鼠标右键";
            case KeyCode.Mouse2: return "鼠标中键";
            case KeyCode.Mouse3: return "鼠标侧键1";
            case KeyCode.Mouse4: return "鼠标侧键2";
        }

        // 修饰键
        switch (key)
        {
            case KeyCode.LeftControl:
            case KeyCode.RightControl: return "Ctrl";
            case KeyCode.LeftShift:
            case KeyCode.RightShift: return "Shift";
            case KeyCode.LeftAlt:
            case KeyCode.RightAlt: return "Alt";
        }

        // 字母/数字直接返回
        return key.ToString();
    }

    private bool IsMouseKeyCode(KeyCode key)
    {
        return key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6;
    }

    /// <summary>
    /// 检查两个绑定是否冲突
    /// </summary>
    public bool ConflictsWith(SkillKeyBinding other)
    {
        if (other == null || other.IsEmpty()) return false;
        if (IsEmpty()) return false;

        // 主键必须相同
        if (primaryKey != other.primaryKey) return false;

        // 鼠标按键：只有主键相同就算冲突（鼠标不参与组合）
        if (isMouseKey || other.isMouseKey)
            return primaryKey == other.primaryKey;

        // 键盘按键：主键 + 修饰键必须完全相同
        bool modsMatch = modifier1 == other.modifier1 && modifier2 == other.modifier2;
        return modsMatch;
    }
}