﻿using StacksForce.Stacks.ChainTransactions;
using StacksForce.Utils;
using System;
using System.Collections.Concurrent;
using System.Text.Json;
using System.Threading.Tasks;
using static StacksForce.Stacks.WebApi.Transactions;

namespace StacksForce.Stacks.WebApi
{
    public class WebSocketAPI : JsonRpcServiceBase
    {
        private const string SUBSCRIBE = "subscribe";
        private const string UNSUBSCRIBE = "unsubscribe";
        
        private const string TX_UPDATE = "tx_update";
        private const string BLOCK = "block";
        private const string MICROBLOCK = "microblock";
        private const string MEMPOOL = "mempool";
        private const string ADDRESS_TX_UPDATE = "address_tx_update";
        private const string ADDRESS_BALANCE_UPDATE = "address_balance_update";

        private readonly Blockchain _chain;

        private WebSock? _webSock;

        public event Action<TransactionInfo>? OnTxUpdated;
        public event Action? OnNewBlock;
        public event Action? OnNewMicroblock;

        private readonly ConcurrentQueue<PendingRequest> _unsentQueue = new ConcurrentQueue<PendingRequest>();
        private readonly ConcurrentDictionary<Request, PendingRequest> _pendingRequests = new ConcurrentDictionary<Request, PendingRequest>();

        public bool IsConnected => _webSock != null && _webSock.IsConnected();

        public WebSocketAPI(Blockchain chain)
        {
            _chain = chain;
        }

        public void Connect(bool forceReconnect = true)
        {
            lock (this)
            {
                if (!forceReconnect)
                {
                    if (_webSock != null && _webSock.IsConnected())
                        return;
                }

                _webSock?.Destroy();

                _webSock = new WebSock(_chain.Websocket);

                _webSock.OnConnected += WebSock_OnConnected;
                _webSock.OnDisconnected += WebSock_OnDisconnected;
                _webSock.OnMessage += WebSock_OnMessage;

                _webSock.ConnectAsync();
            }
        }

        public Task<Error?> SubsribeToTransactionUpdate(string txId) => SendAndWaitSubscription(new { @event = TX_UPDATE, tx_id = txId });
        public Task<Error?> UnsubsribeToTransactionUpdate(string txId) => SendAndWaitUnsubscription(new { @event = TX_UPDATE, tx_id = txId });

        public Task<Error?> SubsribeToBlockEvent() => SendAndWaitSubscription(new { @event = BLOCK });
        public Task<Error?> UnsubsribeToBlockEvent() => SendAndWaitUnsubscription(new { @event = BLOCK });

        public Task<Error?> SubsribeToMicroblockEvent() => SendAndWaitSubscription(new { @event = MICROBLOCK });
        public Task<Error?> UnsubsribeToMicroblockEvent() => SendAndWaitUnsubscription(new { @event = MICROBLOCK });

        public Task<Error?> SubsribeToMempoolEvent() => SendAndWaitSubscription(new { @event = MEMPOOL });
        public Task<Error?> UnsubsribeToMempoolEvent() => SendAndWaitUnsubscription(new { @event = MEMPOOL });

        public Task<Error?> SubsribeToAddressTransactions(string address) => SendAndWaitSubscription(new { @event = ADDRESS_TX_UPDATE, address });
        public Task<Error?> UnsubsribeToAddressTransactions(string address) => SendAndWaitUnsubscription(new { @event = ADDRESS_TX_UPDATE, address });

        public Task<Error?> SubsribeToBalanceUpdate(string address) => SendAndWaitSubscription(new { @event = ADDRESS_BALANCE_UPDATE, address });
        public Task<Error?> UnsubsribeToBalanceUpdate(string address) => SendAndWaitUnsubscription(new { @event = ADDRESS_BALANCE_UPDATE, address });

        protected override void HandleNotification(Response response)
        {
            if (response.method == ADDRESS_TX_UPDATE ||
                response.method == TX_UPDATE)
            {
                var transactionJsonElement = response.@params.Value;
                if (response.method == ADDRESS_TX_UPDATE)
                    transactionJsonElement = transactionJsonElement.GetProperty("tx");
                var rawJson = transactionJsonElement.GetRawText();
                var transactionData = JsonSerializer.Deserialize<TransactionData>(rawJson, HttpAPIUtils.SERIALIZER_OPTIONS);
                var info = TransactionInfo.FromData(_chain, transactionData);

                if (info != null)
                { 
                    OnTxUpdated?.Invoke(info);
                }
            } else if (response.method == BLOCK)
            {
                OnNewBlock?.Invoke();
            } else if (response.method == MICROBLOCK)
            {
                OnNewMicroblock?.Invoke();
            }
        }

        protected override void HandleSuccess(Request request, Response response)
        {
            if (_pendingRequests.TryRemove(request, out var p))
            {
                p.Complete.SetResult(null);
            }
        }

        protected override void HandleError(Request request, Response response)
        {
            if (_pendingRequests.TryRemove(request, out var p))
            {
                p.Complete.SetResult(new Error(response.error.message, response.error.data));
            }
        }

        private async Task<Error?> SendAndWait(string method, object data)
        {
            var pr = await SendRequest(method, data).ConfigureAwait(false);
            var result = await pr.Complete.Task.ConfigureAwait(false);
            Log.Trace($"WebSocketAPI SendAndWait {method}: {result}");
            return result;
        }

        private Task<Error?> SendAndWaitSubscription(object data) => SendAndWait(SUBSCRIBE, data);
        private Task<Error?> SendAndWaitUnsubscription(object data) => SendAndWait(UNSUBSCRIBE, data);

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

        private Task<bool> Send(string message)
        {
            Task<bool>? task = null;
            lock(this)
            {
                if (_webSock != null)
                    task = _webSock.SendMessage(message);
            }
            return task ?? Task.FromResult(false);
        }

        private void WebSock_OnMessage(string message, WebSock webSocket)
        {
            Log.Trace("WebSock incomming message: " + message);
            HandleResponse(message);
        }

        private void WebSock_OnConnected(WebSock webSocket)
        {
            SendUnsentQueue();
        }

        private void WebSock_OnDisconnected(WebSock webSocket)
        {
            Disconnect();
        }

        private async void SendUnsentQueue()
        {
            while (_unsentQueue.TryDequeue(out var pr))
                await Send(pr.RequestBody).ConfigureAwait(false);
        }

        private void Disconnect()
        {
            lock (this)
            {
                _webSock?.Destroy();
                _webSock = null;
            }
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

            public TaskCompletionSource<Error?> Complete { get; } = new TaskCompletionSource<Error?>();
        }
    }
}
