using StacksForce.Dependencies;
using StacksForce.Utils;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;

namespace StacksForce
{
    static public class ForceSDK
    {
        static public void Init()
        {
            Log.LogMethod += Log_LogMethod;

#if UNITY_WEBGL
            DependencyProvider.HttpClient = new UnityHttpClient();
            AwaitConfiguration.AllowConfigureAwaitFalse = false;
#endif
        }

        static private void Log_LogMethod(string message, Log.Severity severity)
        {
            switch (severity)
            {
                case Log.Severity.Warning: Debug.LogWarning(message); break;
                case Log.Severity.Fatal: Debug.LogError(message); break;
                default:
                    Debug.Log(message); break;
            }
        }

        private class UnityHttpClient : IHttpClient
        {
            public UnityHttpClient()
            {
            }

            public async Task<AsyncCallResult<string>> Get(string uri)
            {
                var request = UnityWebRequest.Get(uri);
                await request.SendWebRequest();
                return FromRequest(request);
            }

            public async Task<AsyncCallResult<string>> PostBinary(string uri, byte[] bytes)
            {
                var request = new UnityWebRequest(uri, "POST");
                request.uploadHandler = new UploadHandlerRaw(bytes);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/octet-stream");
                await request.SendWebRequest();
                return FromRequest(request);
            }

            public async Task<AsyncCallResult<string>> PostJson(string uri, object json)
            {
                var jsonText = JsonService.Serialize(json, json.GetType());
                byte[] data = new System.Text.UTF8Encoding().GetBytes(jsonText);
                var request = new UnityWebRequest(uri, "POST");
                request.uploadHandler = new UploadHandlerRaw(data);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                await request.SendWebRequest();
                return FromRequest(request);
            }

            private AsyncCallResult<string> FromRequest(UnityWebRequest request)
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    return new AsyncCallResult<string>(request.downloadHandler.text);
                }
                else
                {
                    return new AsyncCallResult<string>(new Error(request.error));
                }
            }

        }
    }
}