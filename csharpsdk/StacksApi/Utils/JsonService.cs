using Newtonsoft.Json;
using System;

namespace StacksForce.Utils
{
    public static class JsonService
    {
        static JsonService()
        {
        }

        static public string Serialize(object obj, Type type, bool pretty = false)
        {
            var s = new JsonSerializerSettings();
            s.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.SerializeObject(obj, type, s);
        }

        static public string Serialize<T>(T obj, bool pretty = false)
        {
            var s = new JsonSerializerSettings();
            s.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.SerializeObject(obj, typeof(T), s);
        }

        static public T Deserialize<T>(string json)
        {
            var s = new JsonSerializerSettings();
            s.NullValueHandling = NullValueHandling.Ignore;
            return JsonConvert.DeserializeObject<T>(json, s);
        }
    }
}
