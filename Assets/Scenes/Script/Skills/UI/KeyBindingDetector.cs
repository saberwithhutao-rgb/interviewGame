using UnityEngine;

/// <summary>
/// °´¼ü¼ì²â¹¤¾ß
/// </summary>
public static class KeyBindingDetector
{
    private static KeyCode[] modifierKeys = new KeyCode[]
    {
        KeyCode.LeftControl, KeyCode.RightControl,
        KeyCode.LeftShift, KeyCode.RightShift,
        KeyCode.LeftAlt, KeyCode.RightAlt
    };

    public static SkillKeyBinding DetectKeyPress()
    {
        if (!Input.anyKeyDown) return null;

        // ¡ï ÏÈ¼ì²âÊó±ê°´¼ü£¨Êó±ê°´¼ü²»²ÎÓë×éºÏ£©
        if (Input.GetKeyDown(KeyCode.Mouse0)) return new SkillKeyBinding(KeyCode.Mouse0);
        if (Input.GetKeyDown(KeyCode.Mouse1)) return new SkillKeyBinding(KeyCode.Mouse1);
        if (Input.GetKeyDown(KeyCode.Mouse2)) return new SkillKeyBinding(KeyCode.Mouse2);
        if (Input.GetKeyDown(KeyCode.Mouse3)) return new SkillKeyBinding(KeyCode.Mouse3);
        if (Input.GetKeyDown(KeyCode.Mouse4)) return new SkillKeyBinding(KeyCode.Mouse4);

        // ¡ï ¼ì²â¼üÅÌ°´¼ü
        KeyCode primary = KeyCode.None;
        KeyCode mod1 = KeyCode.None;
        KeyCode mod2 = KeyCode.None;

        // ¼ì²âÐÞÊÎ¼ü
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
            mod1 = KeyCode.LeftControl;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            if (mod1 == KeyCode.None)
                mod1 = KeyCode.LeftShift;
            else
                mod2 = KeyCode.LeftShift;
        }
        if (Input.GetKey(KeyCode.LeftAlt) || Input.GetKey(KeyCode.RightAlt))
        {
            if (mod1 == KeyCode.None)
                mod1 = KeyCode.LeftAlt;
            else if (mod2 == KeyCode.None)
                mod2 = KeyCode.LeftAlt;
        }

        // ¡ï ¼ì²âÖ÷¼ü£¨ÅÅ³ýÐÞÊÎ¼ü£©
        foreach (KeyCode code in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (code == KeyCode.None) continue;
            if (IsModifierKey(code)) continue;
            if (IsMouseKeyCode(code)) continue;
            if (!Input.GetKeyDown(code)) continue;

            // ÕÒµ½Ö÷¼ü
            primary = code;
            break;
        }

        if (primary != KeyCode.None)
        {
            return new SkillKeyBinding(primary, mod1, mod2);
        }

        return null;
    }

    private static bool IsModifierKey(KeyCode key)
    {
        foreach (var mod in modifierKeys)
            if (mod == key) return true;
        return false;
    }

    private static bool IsMouseKeyCode(KeyCode key)
    {
        return key >= KeyCode.Mouse0 && key <= KeyCode.Mouse6;
    }
}