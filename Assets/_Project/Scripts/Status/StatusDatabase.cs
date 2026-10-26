using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// StatusDatabase 类是一个 ScriptableObject，用于存储伤害类型与状态效果的映射关系
/// </summary>
[CreateAssetMenu(fileName = "StatusDatabase", menuName = "状态/状态数据库")]
public class StatusDatabase : ScriptableObject
{
    [Header("伤害类型 → 状态映射")]
    [SerializeField] private List<StatusMapping> mappings;

    private Dictionary<DamageType, StatusData> statusMap;

    /// <summary>
    /// 根据伤害类型获取对应的状态数据
    /// </summary>
    /// <param name="damageType">伤害类型</param>
    /// <returns>对应的状态数据，如果不存在则返回 null</returns>
    public StatusData GetStatusForDamageType(DamageType damageType)
    {
        if (statusMap == null) BuildMap();
        statusMap.TryGetValue(damageType, out StatusData result);
        return result;
    }

    private void BuildMap()
    {
        statusMap = new Dictionary<DamageType, StatusData>();
        foreach (var mapping in mappings)
        {
            if (!statusMap.ContainsKey(mapping.damageType))
                statusMap[mapping.damageType] = mapping.statusData;
        }
    }

    /// <summary>
    /// StatusMapping 类用于定义伤害类型与状态数据的映射关系
    /// </summary>
    [System.Serializable]
    public class StatusMapping
    {
        public DamageType damageType;
        public StatusData statusData;
    }
}