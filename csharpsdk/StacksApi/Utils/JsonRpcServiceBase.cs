﻿using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading;

namespace StacksForce.Utils
{
    public abstract class JsonRpcServiceBase
    {
        private static readonly JsonSerializerOptions SERIALIZER_OPTIONS = new JsonSerializerOptions
        {
            IncludeFields = true,
            NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.AllowReadingFromString,
        };

        private long _requestId = 0;

        private readonly ConcurrentDictionary<long, Request> _pendingRequests = new ConcurrentDictionary<long, Request>();

        public event Action<string, object>? OnNotificationRaw;

        protected string CreateRequest(string method, object parameters, out Request request)
        {
            var id = Interlocked.Increment(ref _requestId);
            request = new Request(id, method, parameters);
            var serializedRequest = JsonSerializer.Serialize(request, SERIALIZER_OPTIONS);
            _pendingRequests.TryAdd(id, request);
            return serializedRequest;
        }

        protected void HandleResponse(string message)
        {
            Response? response = null;
            try
            {
                response = JsonSerializer.Deserialize<Response>(message, SERIALIZER_OPTIONS)!;
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
            public JsonElement? result;
            public string method;
            public JsonElement? @params;

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
}
