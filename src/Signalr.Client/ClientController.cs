using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Microsoft.Extensions.Logging;
using Communication.Core.Interfaces;
using Communication.Core;
using NLog.Extensions.Logging;

namespace Signalr.Client
{
    public class ClientController : IClientController
    {
        public event TransportMessageReceivedHandler TransportMessageReceived;
        public event ConnectionChangedHandler ConnectionChanged;

        private const string SendMessageToServerAsyncMethodName = "SendMessageToServerAsync";

        private readonly IConnectionSettings _connectionSettings;
        private readonly IClientSettings _clientSettings;
        private HubConnection _connection;
        private bool _initialized;
        private readonly ILogger _logger;

        public ClientController(IConnectionSettings connectionSettings, IClientSettings clientSettings, ILogger<ClientController> logger, HubConnection hubConnection = null)
        {
            _connectionSettings = connectionSettings;
            _clientSettings = clientSettings;
            _logger = logger;
            if (hubConnection != null)
            {
                _connection = hubConnection;
            }
            InitializeConnection();
        }

        private void InitializeConnection()
        {
            if (_connection == null)
            {
                _connection = new HubConnectionBuilder()
                    .WithUrl($"http://{_connectionSettings.ServerAddress}:{_connectionSettings.ServerPort}/hubs/server?user={_clientSettings.ClientId}")
                    .ConfigureLogging(x =>
                    {
                        x.AddNLog();
                        x.SetMinimumLevel(LogLevel.Trace);
                    })
                    .AddMessagePackProtocol()
                    .WithAutomaticReconnect(new CustomRetryPolicy(_connectionSettings.ReconnectDelaysInSeconds))
                    .Build();
            }

            _initialized = true;

            _connection.On("TransportMessageReceived", (TransportMessage message) =>
            {
                return ClientController_TransportMessageReceivedAsync(message);
            });

            _connection.Closed += _connection_ClosedAsync;
            _connection.Reconnecting += _connection_ReconnectingAsync;
            _connection.Reconnected += _connection_ReconnectedAsync;
        }

        public HubConnectionState GetState()
        {
            return _connection.State;
        }

        private async Task _connection_ReconnectedAsync(string clientId)
        {
            _logger.LogInformation("Connection successfully reconnected with the client ID {ClientId}", clientId);
            await OnConnectionChanged().ConfigureAwait(false);
        }

        private async Task _connection_ReconnectingAsync(Exception ex)
        {
            if (ex == null)
            {
                _logger.LogWarning(ex, "Connection started reconnecting with exception null");
            }
            else
            {
                _logger.LogWarning(ex, "Connection started reconnecting due to an error");
            }

            await OnConnectionChanged().ConfigureAwait(false);
        }

        private async Task _connection_ClosedAsync(Exception ex)
        {
            await OnConnectionChanged().ConfigureAwait(false);
            if (ex == null)
            {
                _logger.LogError("Connection closed with exception null");
            }
            else
            {
                _logger.LogError(ex, "Connection closed due to an error");
            }
        }

        public async Task ConnectAsync()
        {
            await Policy.Handle<Exception>((ex) =>
                {
                    if (ex == null)
                    {
                        _logger.LogError("failed to connect with exception null");
                    }
                    else
                    {
                        _logger.LogError(ex, "Failed to connect");
                    }

                    return _connection.State != HubConnectionState.Connected;
                })
                .WaitAndRetryForeverAsync(retryAttempt => TimeSpan.FromSeconds(_connectionSettings.ReconnectDelaysInSeconds == default ? 3 : _connectionSettings.ReconnectDelaysInSeconds))
                .ExecuteAsync(async () =>
                {
                    _logger.LogTrace($"ExecuteAsync");
                    if (_initialized && _connection.State != HubConnectionState.Connected)
                    {
                        _logger.LogTrace($"_connection.StartAsync");
                        await _connection.StartAsync().ConfigureAwait(false);
                        _logger.LogTrace($"_connection.Connected");
                    }
                }).ConfigureAwait(false);
            await OnConnectionChanged().ConfigureAwait(false);
        }

        public async Task DisconnectAsync()
        {
            try
            {
                await _connection.StopAsync().ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to disconnect");
                throw;
            }
            finally
            {
                await OnConnectionChanged().ConfigureAwait(false);
            }
        }

        public async Task<bool> SendMessageToServerAsync(TransportMessage message)
        {
            return await InternalSendMessageToServerAsync(message, TimeSpan.FromSeconds(15)).ConfigureAwait(false);
        }

        public async Task<bool> SendMessageToServerAsync(TransportMessage message, TimeSpan timeout)
        {
            return await InternalSendMessageToServerAsync(message, timeout).ConfigureAwait(false);
        }

        private async Task<bool> InternalSendMessageToServerAsync(TransportMessage message, TimeSpan timeout)
        {
            try
            {
                using (var cts = new CancellationTokenSource(timeout))
                {
                    if (!IsConnectedToServer())
                    {
                        _logger.LogWarning("Can't send TransportMessage with label {Label} from client ID {ClientId} to server because the client isn't connected. The connection state is {state}", message.Label, message.ClientId, _connection.State);
                        return false;
                    }

                    _logger.LogInformation("Sending TransportMessage with label {Label} from client ID {ClientId} to server. Timeout is set to {TimeoutInSeconds} seconds", message.Label, message.ClientId, timeout.TotalSeconds);
                    var result = await _connection.InvokeAsync<bool>(SendMessageToServerAsyncMethodName, message, cts.Token).ConfigureAwait(false);
                    var isDeliveredAndHandled = result ? "successfully been delivered and handled" : "not been successfully delivered and handled";
                    _logger.LogInformation("TransportMessage with label {Label} to client ID {ClientId} has {IsDeliveredAndHandled}", message.Label, message.ClientId, isDeliveredAndHandled);
                    return result;
                }
            }
            catch (OperationCanceledException oce)
            {
                _logger.LogWarning("Send message timed out after {TimeoutInSeconds} seconds to client id: {ClientId}", timeout.Seconds, message.ClientId);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SendMessageToServerAsync exception");
                return false;
            }
        }

        private async Task<bool> ClientController_TransportMessageReceivedAsync(TransportMessage message)
        {
            var transportMessageReceived = TransportMessageReceived;
            if (transportMessageReceived == null) return false;
            return await transportMessageReceived.Invoke(message).ConfigureAwait(false);
        }

        private async Task OnConnectionChanged()
        {
            var connectionChanged = ConnectionChanged;
            if (connectionChanged == null)
            {
                return;
            }
            await connectionChanged(IsConnectedToServer()).ConfigureAwait(false);
        }

        private bool IsConnectedToServer()
        {
            return GetState() == HubConnectionState.Connected;
        }
    }
}