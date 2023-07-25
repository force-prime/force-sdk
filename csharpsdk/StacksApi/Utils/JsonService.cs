using Newtonsoft.Json;
using System;

namespace StacksForce.Utils
{
    public static class JsonService
    {
        static private JsonSerializerSettings SETTINGS = new JsonSerializerSettings();

        static JsonService()
        {
            SETTINGS.NullValueHandling = NullValueHandling.Ignore;
        }

        static public string Serialize(object obj, Type type, bool pretty = false)
        {
            return JsonConvert.SerializeObject(obj, type, pretty ? Formatting.Indented : Formatting.None, SETTINGS);
        }

        static public string Serialize<T>(T obj, bool pretty = false)
        {
            return Serialize(obj, typeof(T), pretty);
        }

        static public T Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, SETTINGS);
        }
    }
}
