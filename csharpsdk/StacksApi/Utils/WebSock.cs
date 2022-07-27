using System;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace StacksForce.Utils
{
    public class WebSock
    {
        private const int RECEIVE_CHUNK_SIZE = 1024;
        private const int SEND_CHUNK_SIZE = 1024;

        private readonly ClientWebSocket _ws;
        private readonly Uri _uri;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationToken _cancellationToken;

        public event Action<WebSock>? OnConnected;
        public event Action<string, WebSock>? OnMessage;
        public event Action<WebSock>? OnDisconnected;

        public WebSock(string uri)
        {
            _ws = new ClientWebSocket();
            try
            {
                _ws.Options.KeepAliveInterval = TimeSpan.FromSeconds(20);
            }
            catch
            {
                Log.Debug("WebSock: KeepAliveInterval not supported on this platform");
            }

            _uri = new Uri(uri);
            _cancellationToken = _cancellationTokenSource.Token;
        }

        public bool IsConnected()
        {
            return _ws.State == WebSocketState.Open;
        }

        public void Destroy()
        {
            _cancellationTokenSource.Cancel();

            OnConnected = null;
            OnMessage = null;
            OnDisconnected = null;
        }

        public async Task<bool> SendMessage(string message)
        {
            Log.Trace("WebSock::SendMessage: " + message);
            try
            {
                return await SendMessageAsync(message).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
                Log.Info("WebSock.SendMessage: OperationCanceledException");
            }
            catch (Exception e)
            {
                Log.Warning("WebSock.SendMessage exception " + e.ToString());
            }
            return false;
        }

        private async Task<bool> SendMessageAsync(string message)
        {
            if (_ws.State != WebSocketState.Open)
                return false;

            var messageBuffer = Encoding.UTF8.GetBytes(message);
            var messagesCount = (int)Math.Ceiling((double)messageBuffer.Length / SEND_CHUNK_SIZE);

            for (var i = 0; i < messagesCount; i++)
            {
                var offset = (SEND_CHUNK_SIZE * i);
                var count = SEND_CHUNK_SIZE;
                var lastMessage = ((i + 1) == messagesCount);

                if ((count * (i + 1)) > messageBuffer.Length)
                {
                    count = messageBuffer.Length - offset;
                }

                await _ws.SendAsync(new ArraySegment<byte>(messageBuffer, offset, count), WebSocketMessageType.Text, lastMessage, _cancellationToken).ConfigureAwait(false);
            }
            return true;
        }

        public async void ConnectAsync()
        {
            Log.Trace("Web Sock connecting to: " + _uri);
            try
            {
                await _ws.ConnectAsync(_uri, _cancellationToken).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                Log.Debug("Can't connect to websocket: " + e.Message);
                OnDisconnected?.Invoke(this);
                return;
            }

            Log.Debug("Connected to websocket: " + _uri);
            OnConnected?.Invoke(this);
            StartListen();
        }

        private async void StartListen()
        {
            var buffer = new byte[RECEIVE_CHUNK_SIZE];

            try
            {
                Log.Trace("WebSock: started listening");
                while (_ws.State == WebSocketState.Open || _ws.State == WebSocketState.CloseSent)
                {
                    var stringResult = new StringBuilder();

                    WebSocketReceiveResult result;
                    do
                    {
                        result = await _ws.ReceiveAsync(new ArraySegment<byte>(buffer), _cancellationToken).ConfigureAwait(false);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            try
                            {
                                await _ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None).ConfigureAwait(false);
                            }
                            catch { }
                            OnDisconnected?.Invoke(this);
                        }
                        else
                        {
                            var str = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            stringResult.Append(str);
                        }

                    } while (!result.EndOfMessage);

                    OnMessage?.Invoke(stringResult.ToString(), this);
                }
                Log.Debug("WebSock closed");
            }
            catch (OperationCanceledException)
            {
                Log.Info("WebSock canceled");
            }
            catch (Exception e)
            {
                Log.Info("WebSock exception " + e.ToString());
            }
            finally
            {
                _ws.Dispose();

                OnDisconnected?.Invoke(this);

                OnConnected = null;
                OnDisconnected = null;
                OnMessage = null;
            }
        }
    }
}