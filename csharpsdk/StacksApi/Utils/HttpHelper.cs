using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace StacksForce.Utils
{
    static public class HttpHelper
    {
        static public readonly IRetryStrategy NoRetryStrategy = new NoRetryStragegy();

        static private readonly HttpClient _httpClient = CreateHttpClient();

        public interface IRetryStrategy
        {
            int GetRetryDelayMs(int tryCount, Error? lastError);
            Error? CheckResponseForError(string contentAsString);
        }

        static public string GetHttpUrlFrom(string url)
        {
            const string prefix = "https://ipfs.io/ipfs/";
            if (url.StartsWith("ipfs://"))
            {
                if (url.StartsWith("ipfs://ipfs/"))
                    return $"{prefix}{url.Substring(12)}";
                return $"{prefix}{url.Substring(7)}";
            }
            return url;
        }

        static public string BuildUrl<T>(string baseUrl, IDictionary<string, T> getFieldList)
        {
            return baseUrl + ((getFieldList != null && getFieldList.Count > 0) ? "?" +
                string.Join("&", getFieldList.Select(keyAndValue => (keyAndValue.Key, ObjectToGetField(keyAndValue.Value))).
                Where(f => !string.IsNullOrEmpty(f.Item2)).Select(x => x.Key + "=" + System.Web.HttpUtility.UrlEncode(x.Item2))) 
                : string.Empty);
        }

        static public async Task<AsyncCallResult<string>> SendRequest(string url, HttpContent? content = null, IRetryStrategy? retryStrategy = null) {

            Error? lastError = null;
            int tryCount = -1;

            if (retryStrategy == null)
                retryStrategy = HttpHelper.NoRetryStrategy;

            while (true)
            {
                tryCount++;
                if (tryCount > 0)
                {
                    var retryDelay = retryStrategy.GetRetryDelayMs(tryCount, lastError);
                    if (retryDelay > 0)
                        await Task.Delay(retryDelay).ConfigureAwait(false);
                    else
                        break;
                }

                var tryStartTime = DateTime.Now;
                HttpResponseMessage? response = null;
                try
                {
                    if (content != null)
                        response = await _httpClient.PostAsync(url, content).ConfigureAwait(false);
                    else
                        response = await _httpClient.GetAsync(url).ConfigureAwait(false);
                }
                catch (Exception httpException)
                {
                    lastError = new NetworkError("HttpRequestException", httpException);
                    Log.Info("HttpHelper.SendRequest network error: " + lastError.ToString());
                }

                if (response != null)
                {
                    var contentAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var duration = DateTime.Now - tryStartTime;
                    Log.Trace("SendRequest: " + url + " response " + response.StatusCode + " " + contentAsString + " duration = " + duration.TotalSeconds);

                    if (response.IsSuccessStatusCode)
                        lastError = retryStrategy.CheckResponseForError(contentAsString);
                    else
                        lastError = new HttpError(response.StatusCode, contentAsString);

                    response.Dispose();

                    if (lastError == null)
                        return new AsyncCallResult<string>(contentAsString);
                }
            }

            return new AsyncCallResult<string>(lastError!);
        }

        static private string? ArrayToGetField(string[]? array) => array != null ? $"{string.Join(",", array)}" : null;

        static private string? ObjectToGetField(object? obj)
        {
            if (obj is string str)
                return str;

            if (obj is IEnumerable enumerable)
            {
                List<string> items = new List<string>();
                foreach (var element in enumerable)
                {
                    var s = element?.ToString();
                    if (!string.IsNullOrEmpty(s))
                        items.Add(s);
                }
                return ArrayToGetField(items.ToArray());
            }

            return obj?.ToString();
        }

        static private HttpClient CreateHttpClient()
        {
            var client = new HttpClient();
            client.DefaultRequestHeaders.Clear();
            return client;
        }

        private class NoRetryStragegy : IRetryStrategy
        {
            public Error? CheckResponseForError(string contentAsString) => null;

            public int GetRetryDelayMs(int tryCount, Error? lastError) => -1;
        }

        public class NRetryStrategy : IRetryStrategy
        {
            private readonly int _tryCount;
            private readonly int _delayMs;

            public NRetryStrategy(int tryCount, int delayMs)
            {
                _tryCount = tryCount;
                _delayMs = delayMs;
            }

            public Error? CheckResponseForError(string contentAsString) => null;

            public int GetRetryDelayMs(int tryCount, Error? lastError) => tryCount <= _tryCount ? _delayMs : -1;
        }
    }
}
