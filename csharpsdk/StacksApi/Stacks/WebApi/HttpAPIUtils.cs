using StacksForce.Dependencies;
using StacksForce.Utils;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    public static class HttpAPIUtils
    {
        private static IHttpClient HTTP => DependencyProvider.HttpClient;

        public static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new JsonSerializerOptions { IncludeFields = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString };

        static public Task<AsyncCallResult<T>> PostJson<T>(string url, Dictionary<string, object?>? getFields, object obj2JsonContent) where T : class
        {
            url = getFields == null ? url : HttpHelper.BuildUrl(url, getFields);
            return HandleHttpRequest<T>(HTTP.PostJson(url, obj2JsonContent));
        }

        static public Task<AsyncCallResult<T>> PostBinary<T>(string url, Dictionary<string, object?>? getFields, byte[] content) where T : class
        {
            url = getFields == null ? url : HttpHelper.BuildUrl(url, getFields);
            return HandleHttpRequest<T>(HTTP.PostBinary(url, content));
        }

        static public Task<AsyncCallResult<T>> GetRequest<T>(string url, Dictionary<string, object?>? getFields = null)
        {
            url = getFields == null ? url : HttpHelper.BuildUrl(url, getFields);
            return HandleHttpRequest<T>(HTTP.Get(url));
        }

        static private async Task<AsyncCallResult<T>> HandleHttpRequest<T>(Task<AsyncCallResult<string>> request)
        {
            var result = await request.ConfigureAwait();
            if (result.IsError)
            {
                if (result.Error is HttpError httpError && httpError.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    try
                    {
                        var error = JsonSerializer.Deserialize<ErrorMessage>(httpError.Content, SERIALIZER_OPTIONS);
                        return new AsyncCallResult<T>(new BadRequestError(error.error + "::" + error.reason, error.reason_data?.ToString()));
                    }
                    catch (Exception e) { }
                }
                return new AsyncCallResult<T>(result.Error!);
            }

            if (typeof(T) == typeof(string))
            {
                object o = result.Data.Trim('"');
                return new AsyncCallResult<T>((T)o);
            }

            if (typeof(T) == typeof(ulong))
            {
                object o = Convert.ToUInt64(result.Data);
                return new AsyncCallResult<T>((T)o);
            }

            try
            {
                var data = JsonSerializer.Deserialize<T>(result.Data!, SERIALIZER_OPTIONS);
                return new AsyncCallResult<T>(data!);
            }
            catch (Exception e)
            {
                return new AsyncCallResult<T>(e);
            }
        }

        private class ErrorMessage
        {
            public string error;
            public string reason;
            public object reason_data;
        }

        public class JsonDataBase
        {
            public override string ToString()
            {
                var r = JsonSerializer.Serialize(this, GetType(), SERIALIZER_OPTIONS);
                return r;
            }
        }

        public class BadRequestError : Error
        {
            public BadRequestError(string id, string? info = null) : base(id, info)
            {
            }
        }
    }
}
