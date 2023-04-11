using Newtonsoft.Json.Linq;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace StacksForce.Utils
{
    public abstract class JsonRpcServiceBase
    {
        private long _requestId = 0;

        private readonly ConcurrentDictionary<long, Request> _pendingRequests = new ConcurrentDictionary<long, Request>();

        public event Action<string, object>? OnNotificationRaw;

        protected string CreateRequest(string method, object parameters, out Request request)
        {
            var id = Interlocked.Increment(ref _requestId);
            request = new Request(id, method, parameters);
            var serializedRequest = JsonService.Serialize(request);
            _pendingRequests.TryAdd(id, request);
            return serializedRequest;
        }

        protected void HandleResponse(string message)
        {
            Response? response = null;
            try
            {
                response = JsonService.Deserialize<Response>(message)!;
            } catch (Exception e)
            {
                HandleParseException(message, e);
                return;
            }

            if (response.id > 0)
            {
                if (_pendingRequests.TryRemove((long) response.id, out var r))
                {
                    if (response.error != null)
                        HandleError(r, response);
                    else
                        HandleSuccess(r, response);
                }
                else
                    HandleMissingRequestId(response);
            }
            else
            {
                OnNotificationRaw?.Invoke(response.method, response.@params);
                try
                {
                    HandleNotification(response);
                } catch (Exception e)
                {
                    Log.Warning($"JsonRpcServiceBase: Failed to handle reponse {response}, e = {e.Message}");
                }
            }
        }

        protected virtual void HandleNotification(Response response)
        {
            
        }

        protected virtual void HandleMissingRequestId(Response response)
        {
            Log.Warning($"JsonRpcServiceBase: missing request id, response = {response}");
        }

        protected virtual void HandleParseException(string response, Exception e)
        {
            Log.Warning($"JsonRpcServiceBase: can't parse {response}, err = {e}");
        }

        protected virtual void HandleSuccess(Request request, Response response)
        {
            
        }

        protected virtual void HandleError(Request request, Response response)
        {
            Log.Warning($"JsonRpcServiceBase: request error, response = {response}");
        }

        protected class Response
        {
            public ulong id;
            public Error? error;
            public JObject? result;
            public string method;
            public JObject? @params;

            public class Error
            {
                public string message;
                public long code;
                public string data;
            }
        }

        protected class Request
        {
            public readonly string jsonrpc = "2.0";

            public ulong id;
            public string method;
            public object @params; // obj or array

            public Request(long id, string method, object parameters)
            {
                this.id = (ulong) id;
                this.method = method;
                this.@params = parameters;
            }
        }
    }

    public abstract class JsonRpcService : JsonRpcServiceBase
    {
        private readonly ConcurrentQueue<PendingRequest> _unsentQueue = new ConcurrentQueue<PendingRequest>();
        private readonly ConcurrentDictionary<Request, PendingRequest> _pendingRequests = new ConcurrentDictionary<Request, PendingRequest>();

        protected override void HandleSuccess(Request request, Response response)
        {
            if (_pendingRequests.TryRemove(request, out var p))
            {
                p.SetComplete(response, null);
            }
        }

        protected override void HandleError(Request request, Response response)
        {
            if (_pendingRequests.TryRemove(request, out var p))
            {
                p.SetComplete(response, new Error(response.error.message, response.error.data));
            }
        }

        protected abstract Task<bool> Send(string message);

        private async Task<PendingRequest> SendRequest(string method, object parameters)
        {
            var requestBody = CreateRequest(method, parameters, out var request);
            var pr = new PendingRequest(request, requestBody);
            _pendingRequests.TryAdd(request, pr);
            var sent = await Send(requestBody).ConfigureAwait(false);
            if (!sent)
                _unsentQueue.Enqueue(pr);
            return pr;
        }

        protected async Task<AsyncCallResult<Response?>> SendAndWait(string method, object parameters)
        {
            var pendingRequest = await SendRequest(method, parameters).ConfigureAwait(false);
            var error = await pendingRequest.Complete.Task.ConfigureAwait(false);
            Log.Trace($"JsonRpcService SendAndWait {method}: {error}");
            if (error != null)
                return new AsyncCallResult<Response?>(error);
            else
                return new AsyncCallResult<Response?>(pendingRequest.Response);
        }

        protected async Task SendUnsentQueue()
        {
            while (_unsentQueue.TryDequeue(out var pr))
                await Send(pr.RequestBody).ConfigureAwait(false);
        }

        private class PendingRequest
        {
            public Request Request { get; }
            public string RequestBody { get; }
            public PendingRequest(Request request, string body)
            {
                Request = request;
                RequestBody = body;
            }

            public void SetComplete(Response? response, Error? error)
            {
                Response = response;
                Complete.SetResult(error);
            }

            public Response? Response { get; set; }
            public TaskCompletionSource<Error?> Complete { get; } = new TaskCompletionSource<Error?>();
        }
    }
}
