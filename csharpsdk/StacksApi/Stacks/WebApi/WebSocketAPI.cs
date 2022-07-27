using StacksForce.Utils;
using System.Threading.Tasks;

namespace StacksForce.Stacks.WebApi
{
    public class WebSocketAPI : JsonRpcServiceBase
    {
        private readonly Blockchain _chain;

        private WebSock? _webSock;

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

        public void SubsribeToAddressTransactions(string address)
        {
            var p = new { @event = "address_tx_update", address };
            SendRequest("subscribe", p);
        }

        public void SubsribeToBalanceUpdate(string address)
        {
            var p = new { @event = "address_balance_update", address };
            SendRequest("subscribe", p);
        }

        private Task<bool> SendRequest(string method, object parameters)
        {
            var requestBody = CreateRequest(method, parameters);
            return Send(requestBody);
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

        }

        private void WebSock_OnDisconnected(WebSock webSocket)
        {
            Disconnect();
        }

        private void Disconnect()
        {
            lock (this)
            {
                _webSock?.Destroy();
                _webSock = null;
            }
        }

    }
}
