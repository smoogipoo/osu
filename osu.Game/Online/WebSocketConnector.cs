// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Diagnostics;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using osu.Framework.Bindables;
using osu.Framework.Extensions.TypeExtensions;
using osu.Framework.Logging;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.WebSockets;

namespace osu.Game.Online
{
    public abstract class WebSocketConnector : IDisposable
    {
        /// <summary>
        /// The current connection opened by this connector.
        /// </summary>
        public ClientWebSocket? CurrentConnection { get; private set; }

        /// <summary>
        /// Whether this is connected to the hub, use <see cref="CurrentConnection"/> to access the connection, if this is <c>true</c>.
        /// </summary>
        public IBindable<bool> IsConnected => isConnected;

        protected readonly IAPIProvider API;

        private readonly Bindable<bool> isConnected = new Bindable<bool>();
        private readonly SemaphoreSlim connectionLock = new SemaphoreSlim(1);
        private CancellationTokenSource connectCancelSource = new CancellationTokenSource();

        private readonly IBindable<APIState> apiState = new Bindable<APIState>();

        /// <summary>
        /// Constructs a new <see cref="HubClientConnector"/>.
        /// </summary>
        /// <param name="api"> An API provider used to react to connection state changes.</param>
        protected WebSocketConnector(IAPIProvider api)
        {
            API = api;

            apiState.BindTo(api.State);
            apiState.BindValueChanged(_ => Task.Run(connectIfPossible), true);
        }

        public Task Reconnect()
        {
            Logger.Log($"{GetType().ReadableName()} reconnecting...", LoggingTarget.Network);
            return Task.Run(connectIfPossible);
        }

        private async Task connectIfPossible()
        {
            switch (apiState.Value)
            {
                case APIState.Failing:
                case APIState.Offline:
                    await disconnect(true);
                    break;

                case APIState.Online:
                    await connect();
                    break;
            }
        }

        private async Task connect()
        {
            cancelExistingConnect();

            if (!await connectionLock.WaitAsync(10000).ConfigureAwait(false))
                throw new TimeoutException("Could not obtain a lock to connect. A previous attempt is likely stuck.");

            try
            {
                while (apiState.Value == APIState.Online)
                {
                    // ensure any previous connection was disposed.
                    // this will also create a new cancellation token source.
                    await disconnect(false).ConfigureAwait(false);

                    // this token will be valid for the scope of this connection.
                    // if cancelled, we can be sure that a disconnect or reconnect is handled elsewhere.
                    var cancellationToken = connectCancelSource.Token;

                    cancellationToken.ThrowIfCancellationRequested();

                    try
                    {
                        var tcs = new TaskCompletionSource<string>();
                        var req = new GetNotificationsRequest();
                        req.Success += bundle => tcs.SetResult(bundle.Endpoint);
                        req.Failure += ex => tcs.SetException(ex);
                        API.Queue(req);

                        string endpoint = await tcs.Task;
                        cancellationToken.ThrowIfCancellationRequested();

                        Logger.Log($"{GetType().ReadableName()} connecting to {endpoint}...", LoggingTarget.Network);

                        // importantly, rebuild the connection each attempt to get an updated access token.
                        CurrentConnection = buildConnection();

                        await startAsync(endpoint, CurrentConnection, cancellationToken).ConfigureAwait(false);

                        Logger.Log($"{GetType().ReadableName()} connected!", LoggingTarget.Network);
                        isConnected.Value = true;
                        return;
                    }
                    catch (OperationCanceledException)
                    {
                        //connection process was cancelled.
                        throw;
                    }
                    catch (Exception e)
                    {
                        await handleErrorAndDelay(e, cancellationToken).ConfigureAwait(false);
                    }
                }
            }
            finally
            {
                connectionLock.Release();
            }
        }

        /// <summary>
        /// Handles an exception and delays an async flow.
        /// </summary>
        private async Task handleErrorAndDelay(Exception exception, CancellationToken cancellationToken)
        {
            Logger.Log($"{GetType().ReadableName()} connect attempt failed: {exception.Message}", LoggingTarget.Network);
            await Task.Delay(5000, cancellationToken).ConfigureAwait(false);
        }

        private ClientWebSocket buildConnection()
        {
            ClientWebSocket socket = new ClientWebSocket();
            socket.Options.SetRequestHeader("Authorization", $"Bearer {API.AccessToken}");
            socket.Options.Proxy = WebRequest.DefaultWebProxy;
            if (socket.Options.Proxy != null)
                socket.Options.Proxy.Credentials = CredentialCache.DefaultCredentials;

            return socket;
        }

        private async Task startAsync(string endpoint, ClientWebSocket socket, CancellationToken cancellationToken)
        {
            await socket.ConnectAsync(new Uri(endpoint), cancellationToken).ConfigureAwait(false);

            await OnConnectedAsync(socket);

            // Start the read thread.
            run();

            void run() => Task.Run(async () =>
            {
                byte[] buffer = new byte[1024];
                StringBuilder messageResult = new StringBuilder();

                while (!cancellationToken.IsCancellationRequested)
                {
                    try
                    {
                        WebSocketReceiveResult result = await socket.ReceiveAsync(buffer, cancellationToken);

                        switch (result.MessageType)
                        {
                            case WebSocketMessageType.Text:
                                messageResult.Append(Encoding.UTF8.GetString(buffer[..result.Count]));

                                if (result.EndOfMessage)
                                {
                                    SocketMessage? message = JsonConvert.DeserializeObject<SocketMessage>(messageResult.ToString());
                                    messageResult.Clear();

                                    Debug.Assert(message != null);

                                    if (message.Error != null)
                                    {
                                        Logger.Log($"{GetType().ReadableName()} error: {message.Error}", LoggingTarget.Network);
                                        break;
                                    }

                                    await ProcessMessage(message);
                                }

                                break;

                            case WebSocketMessageType.Binary:
                                throw new NotImplementedException();

                            case WebSocketMessageType.Close:
                                throw new Exception("Connection closed by remote host.");
                        }
                    }
                    catch (Exception ex)
                    {
                        if (!cancellationToken.IsCancellationRequested)
                            await onConnectionClosed(ex, cancellationToken);
                        break;
                    }
                }
            }, cancellationToken);
        }

        protected virtual Task OnConnectedAsync(ClientWebSocket connection) => Task.CompletedTask;

        protected abstract Task ProcessMessage(SocketMessage message);

        protected async Task SendMessage(SocketMessage message)
        {
            if (CurrentConnection == null)
                return;

            await CurrentConnection.SendAsync(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message)), WebSocketMessageType.Text, true, CancellationToken.None);
        }

        private async Task onConnectionClosed(Exception? ex, CancellationToken cancellationToken)
        {
            isConnected.Value = false;

            if (ex != null)
                await handleErrorAndDelay(ex, cancellationToken).ConfigureAwait(false);
            else
                Logger.Log($"{GetType().ReadableName()} disconnected", LoggingTarget.Network);

            // make sure a disconnect wasn't triggered (and this is still the active connection).
            if (!cancellationToken.IsCancellationRequested)
                await Reconnect().ConfigureAwait(false);
        }

        private async Task disconnect(bool takeLock)
        {
            cancelExistingConnect();

            if (takeLock)
            {
                if (!await connectionLock.WaitAsync(10000).ConfigureAwait(false))
                    throw new TimeoutException("Could not obtain a lock to disconnect. A previous attempt is likely stuck.");
            }

            try
            {
                if (CurrentConnection != null)
                {
                    try
                    {
                        await CurrentConnection.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disconnecting", CancellationToken.None).ConfigureAwait(false);
                    }
                    catch
                    {
                        // Closure can fail if the connection is aborted. Don't really care since it's disposed anyway.
                    }

                    CurrentConnection.Dispose();
                }
            }
            finally
            {
                CurrentConnection = null;
                if (takeLock)
                    connectionLock.Release();
            }
        }

        private void cancelExistingConnect()
        {
            connectCancelSource.Cancel();
            connectCancelSource = new CancellationTokenSource();
        }

        public override string ToString() => $"{GetType().ReadableName()} ({(IsConnected.Value ? "connected" : "not connected")}";

        public void Dispose()
        {
            apiState.UnbindAll();
            cancelExistingConnect();
        }
    }
}
