using System;
using System.Collections.Generic;
using System.Reflection;
using Newtonsoft.Json.Linq;
using Unity.Netcode;

public static class SaveRegistry
{
    private static readonly Dictionary<Type, FieldInfo[]> cache = new();
    private static readonly Type nvOpenType = typeof(NetworkVariable<>);


    /// <summary>
    /// 遍历这个类的所有字段（包括 public 和 private）
    /// 只要上面打了 [SaveField] 标签的，统统挑出来放进列表
    /// </summary>
    /// <param name="type"></param>
    /// <returns>包含需要保存的字段（FieldInfo）的数组</returns>
    public static FieldInfo[] GetSaveFields(Type type)
    {
        if (cache.TryGetValue(type, out var fields)) return fields;

        var list = new List<FieldInfo>();
        foreach (var f in type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic))
        {
            if (f.GetCustomAttribute<SaveFieldAttribute>() != null)
                list.Add(f);
        }
        fields = list.ToArray();
        cache[type] = fields;
        return fields;
    }

    /// <summary>
    /// 把对象里的数据拿出来，放进一个 Dictionary<string, object> 字典里。
    /// </summary>
    /// <param name="obj">要保存的对象</param>
    /// <param name="dict">要保存的数据字典</param>
    /// <param name="prefix">字段名前缀</param>
    public static void Capture(object obj, Dictionary<string, object> dict, string prefix)
    {
        foreach (var f in GetSaveFields(obj.GetType()))
        {
            string key = prefix + f.Name;
            object raw = f.GetValue(obj);

            if (TryGetNetworkAccessor(raw, out var accessor))
                dict[key] = accessor.GetValueBoxed();
            else
                dict[key] = raw;
        }
    }

    /// <summary>
    /// 拿着存好的字典，根据字段名去匹配，把值塞回给对象。
    /// </summary>
    /// <param name="obj">要恢复的对象</param>
    /// <param name="dict">要保存的数据字典</param>
    /// <param name="prefix">字段名前缀</param>
    public static void Restore(object obj, Dictionary<string, object> dict, string prefix)
    {
        foreach (var f in GetSaveFields(obj.GetType()))
        {
            string key = prefix + f.Name;
            if (!dict.TryGetValue(key, out var value)) continue;

            object raw = f.GetValue(obj);

            if (TryGetNetworkAccessor(raw, out var accessor))
            {
                accessor.SetValueBoxed(ConvertValue(value, accessor.ValueType));
            }
            else
            {
                f.SetValue(obj, ConvertValue(value, f.FieldType));
            }
        }
    }

    /// <summary>
    /// 直接把 NetworkVariable 转成 JSON，会存下一堆没用的网络同步信息。
    /// 通过泛型反射，判断这个字段是不是 NetworkVariable<>
    /// 如果是，就利用一个叫 NetworkVariableAccessor 的工具
    /// 把里面的真实数据单独拿出来存
    /// </summary>
    /// <param name="raw"></param>
    /// <param name="accessor"></param>
    /// <returns></returns>
    private static bool TryGetNetworkAccessor(object raw, out INetworkVariableAccessor accessor)
    {
        accessor = null;
        if (raw == null) return false;

        Type t = raw.GetType();
        if (!t.IsGenericType || t.GetGenericTypeDefinition() != nvOpenType) return false;

        Type valueType = t.GetGenericArguments()[0];
        Type accessorType = typeof(NetworkVariableAccessor<>).MakeGenericType(valueType);
        accessor = (INetworkVariableAccessor)Activator.CreateInstance(accessorType, raw);
        return true;
    }

    /// <summary>
    /// JSON 读出来的数据往往类型对不上
    /// 需要数据转换
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <returns></returns>
    public static object ConvertValue(object value, Type targetType)
    {
        if (value == null) return null;
        if (targetType.IsInstanceOfType(value)) return value;
        if (targetType.IsEnum)
            return Enum.ToObject(targetType, value);

        if (value is JValue jv)
            return Convert.ChangeType(jv.Value, targetType);

        if (value is JToken token)
            return token.ToObject(targetType);

        return Convert.ChangeType(value, targetType);
    }
}