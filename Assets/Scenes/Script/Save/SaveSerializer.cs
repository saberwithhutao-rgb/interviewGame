using Newtonsoft.Json;

public static class SaveSerializer
{
    private static readonly JsonSerializerSettings settings = new()
    {
        TypeNameHandling = TypeNameHandling.Auto,
        Formatting = Formatting.Indented,
        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
        NullValueHandling = NullValueHandling.Ignore,
    };

    public static string Serialize(object data) => JsonConvert.SerializeObject(data, settings);
    public static T Deserialize<T>(string json) => JsonConvert.DeserializeObject<T>(json, settings);
}