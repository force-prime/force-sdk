using StacksForce.Utils;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    public static class HttpAPIUtils
    {
        private static readonly HttpHelper.IRetryStrategy DEFAULT_RETRY_STRATEGY = new HttpHelper.NRetryStrategy(0, 3000);

        private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new JsonSerializerOptions { IncludeFields = true, NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString };

        static public Task<AsyncCallResult<T>> PerformHttpRequestJsonContent<T>(string url, Dictionary<string, object?>? getFields, object? obj2JsonContent) where T : class
        {
            // we need to remove UTF-8 encoding part otherwise API doesn't accept it
            var content = obj2JsonContent != null ?
                JsonContent.Create(obj2JsonContent, new MediaTypeHeaderValue("application/json"), SERIALIZER_OPTIONS) : null;

            return PerformHttpRequest<T>(url, getFields, content);
        }

        static public Task<AsyncCallResult<T>> PerformHttpRequestBinaryContent<T>(string url, Dictionary<string, object?>? getFields, byte[] content) where T : class
        {
            var httpContent = new ByteArrayContent(content);
            httpContent.Headers.Clear();
            httpContent.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            return PerformHttpRequest<T>(url, getFields, httpContent);
        }

        static public async Task<AsyncCallResult<T>> PerformHttpRequest<T>(string url, Dictionary<string, object?>? getFields = null, HttpContent? content = null) 
        {
            url = getFields == null ? url : HttpHelper.BuildUrl(url, getFields);

            var result = await HttpHelper.SendRequest(url, content, DEFAULT_RETRY_STRATEGY).ConfigureAwait(false);
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
                return new AsyncCallResult<T>((T) o);
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
