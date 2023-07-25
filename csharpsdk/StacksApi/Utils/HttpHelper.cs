using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
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

        static public string BuildUrl(string baseUrl, List<(string name, string value)> getFieldList)
        {
            return baseUrl + ((getFieldList != null && getFieldList.Count > 0) ? "?" +
                string.Join("&", getFieldList.Where(f => !string.IsNullOrEmpty(f.value)).Select(
                            x => x.name + "=" + System.Web.HttpUtility.UrlEncode(x.value)))
                : string.Empty);
        }

        static public HttpContent GetJsonContent(string json)
        {
            var content = new StringContent(json, System.Text.Encoding.UTF8);
            content.Headers.Clear();
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return content;
        }

        static public string BuildUrl<T>(string baseUrl, IDictionary<string, T> getFieldList) {
            List<(string name, string value)> urlParams = new List<(string name, string value)>();
            foreach (var kv in getFieldList)
            {
                if (kv.Value is IEnumerable en && (!(kv.Value is string)))
                {
                    foreach (var v in en)
                    {
                        urlParams.Add((kv.Key, v?.ToString()));
                    }
                } else
                {
                    urlParams.Add((kv.Key, kv.Value?.ToString()));
                }
            }
            return BuildUrl(baseUrl, urlParams);
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
                    Log.Info($"HttpHelper.SendRequest url={url} network error: " + lastError.ToString());
                }

                if (response != null)
                {
                    var contentAsString = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    var duration = DateTime.Now - tryStartTime;
                    Log.Trace("SendRequest: " + url + " response " + response.StatusCode + " " + 
                        (contentAsString.Length > 1000 ? contentAsString.Substring(0, 1000) + "..." : contentAsString) + " duration = " + duration.TotalSeconds);

                    if (response.IsSuccessStatusCode)
                        lastError = retryStrategy.CheckResponseForError(contentAsString);
                    else
                        lastError = new HttpError(response.StatusCode, contentAsString);

                    response.Dispose();

                    if (lastError == null)
                        return contentAsString;
                }
            }

            return lastError!;
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

        public class InfiniteRetryStrategy: IRetryStrategy
        {
            private readonly int _delayMs;

            public InfiniteRetryStrategy(int delayMs)
            {
                _delayMs = delayMs;
            }

            public Error? CheckResponseForError(string contentAsString) => null;

            public int GetRetryDelayMs(int tryCount, Error? lastError) => _delayMs;
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

            public int GetRetryDelayMs(int tryCount, Error? lastError)
            {
                if (!AllowRetryForError(lastError))
                    return -1;

                return tryCount <= _tryCount ? _delayMs : -1;
            }

            protected virtual bool AllowRetryForError(Error? error) => true;
        }
    }
}
