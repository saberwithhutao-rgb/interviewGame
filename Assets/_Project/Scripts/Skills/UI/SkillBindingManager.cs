using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 技能按键绑定管理器 - 管理所有技能的按键绑定
/// </summary>
public class SkillBindingManager : MonoSingleton<SkillBindingManager>
{

    private Dictionary<int, SkillKeyBinding> bindings = new Dictionary<int, SkillKeyBinding>();
    protected override void Awake()
    {
        base.Awake();

        LoadAllBindings();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    /// <summary>
    /// 获取技能的绑定
    /// </summary>
    public SkillKeyBinding GetBinding(int skillId)
    {
        if (bindings.TryGetValue(skillId, out var binding))
            return binding;
        return LoadBinding(skillId);
    }

    /// <summary>
    /// 保存绑定
    /// </summary>
    public void SaveBinding(int skillId, SkillKeyBinding binding)
    {
        bindings[skillId] = binding;
        string key = $"SkillBinding_{skillId}";
        string json = JsonUtility.ToJson(binding);
        PlayerPrefs.SetString(key, json);
        PlayerPrefs.Save();
    }

    /// <summary>
    /// 检查按键是否与其他技能冲突
    /// </summary>
    public bool IsKeyConflicting(int skillId, SkillKeyBinding binding)
    {
        if (binding == null || binding.IsEmpty()) return false;

        foreach (var kvp in bindings)
        {
            if (kvp.Key == skillId) continue;
            if (kvp.Value.ConflictsWith(binding))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 获取冲突的技能ID
    /// </summary>
    public int? GetConflictSkillId(int skillId, SkillKeyBinding binding)
    {
        if (binding == null || binding.IsEmpty()) return null;

        foreach (var kvp in bindings)
        {
            if (kvp.Key == skillId) continue;
            if (kvp.Value.ConflictsWith(binding))
                return kvp.Key;
        }
        return null;
    }

    /// <summary>
    /// 加载所有绑定
    /// </summary>
    private void LoadAllBindings()
    {
        // 清空现有绑定
        bindings.Clear();

        // 从 PlayerPrefs 加载（由 SkillBindingManager 统一管理）
        // 具体技能名称由外部调用时加载
    }

    /// <summary>
    /// 加载指定技能的绑定
    /// </summary>
    public SkillKeyBinding LoadBinding(int skillId)
    {
        string key = $"SkillBinding_{skillId}";
        if (PlayerPrefs.HasKey(key))
        {
            string json = PlayerPrefs.GetString(key);
            SkillKeyBinding binding = JsonUtility.FromJson<SkillKeyBinding>(json);
            if (binding != null && !binding.IsEmpty())
            {
                bindings[skillId] = binding;
                return binding;
            }
        }
        return new SkillKeyBinding();
    }

    /// <summary>
    /// 清除所有绑定
    /// </summary>
    public void ClearAllBindings()
    {
        bindings.Clear();

        // 清除 PlayerPrefs 中所有 SkillBinding_ 开头的键
        // 注意：PlayerPrefs 无法枚举所有键，需要配合存档系统
        // 目前只清空内存
    }
}