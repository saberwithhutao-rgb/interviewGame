using System;

[AttributeUsage(AttributeTargets.Field)]
public class SaveFieldAttribute : Attribute
{
    public string Key;
    public SaveFieldAttribute(string key = null) { Key = key; }
}