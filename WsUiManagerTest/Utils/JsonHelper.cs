using System.Text.Json;

namespace WsUiManagerTest.Utils;
public static class JsonHelper
{
    public static T? DeserializeOrDefault<T>(string json, JsonSerializerOptions? options = null) where T : class
    {
        try
        {
            return JsonSerializer.Deserialize<T>(json, options);
        }
        catch (Exception)
        {
            return default;
        }
    }
}
