using StacksForce.Dependencies;
using StacksForce.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    public static class HttpAPIUtils
    {
        private static IHttpClient HTTP => DependencyProvider.HttpClient;

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
                        var error = JsonService.Deserialize<ErrorMessage>(httpError.Content);
                        return ErrorFactory.GetError(error.error, error.reason, error.reason_data);
                    }
                    catch (Exception e) { }
                }
                return result.Error!;
            }

            if (typeof(T) == typeof(string))
            {
                object o = result.Data.Trim('"');
                return (T)o;
            }

            if (typeof(T) == typeof(ulong))
            {
                object o = Convert.ToUInt64(result.Data);
                return (T)o;
            }

            try
            {
                var data = JsonService.Deserialize<T>(result.Data!);
                return data!;
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
                var r = JsonService.Serialize(this, GetType());
                return r;
            }
        }

        public class BadRequestError : Error
        {
            public string Reason { get; }
            public BadRequestError(string id, string reason, string? info = null) : base(id, info)
            {
                Reason = reason;
            }

            public override string ToString()
            {
                return $"{Id}({Reason}): {Info}";
            }

            public override object ToJsonObject() => new { Id, Reason, Info };
        }


        public class NotEnoughFundsError : BadRequestError
        {
            public NotEnoughFundsError(string id, string reason) : base(id, reason)
            {
            }

        }

        public class ConflictingNonceInMempoolError : BadRequestError
        {
            public ConflictingNonceInMempoolError(string id, string reason) : base(id, reason)
            {
            }
        }

        static private class ErrorFactory
        {
            static public Error GetError(string error, string reason, object reason_data)
            {
                if (reason == "ConflictingNonceInMempool")
                    return new ConflictingNonceInMempoolError(error, reason);
                else if (reason == "NotEnoughFunds")
                    return new NotEnoughFundsError(error, reason);

                return new BadRequestError(error, reason, reason_data?.ToString());
            }
        }
    }
}
