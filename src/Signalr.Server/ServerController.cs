using Communication.Core;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using Signalr.Server.Interfaces;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Signalr.Server
{
    public class ServerController : ISignalrServerController
    {
        private readonly IHubContext<ServerHub> _hubContext;
        private readonly ConnectionMapping _connections;
        public event TransportMessageReceivedHandler TransportMessageReceived;
        public event ConnectionChangedEventHandler ClientListChanged;
        private ILogger<ServerController> _logger;

        public ServerController(IHubContext<ServerHub> hubContext, ILogger<ServerController> logger)
        {
            _hubContext = hubContext;
            _connections = new ConnectionMapping();
            _logger = logger;
        }

        public async Task AddConnectedUserWithConnectionIdAsync(string userId, string connectionId)
        {
            _logger.LogInformation("Client ID {ClientId} with connection ID {ConnectionId} has connected", userId, connectionId);
            _connections.Add(userId, connectionId);
            if (ClientListChanged != null)
            {
                await ClientListChanged.Invoke(new ConnectionEventArgs(userId, null, true, true)).ConfigureAwait(false);
            }
        }

        public async Task RemoveDisconnectedUserWithConnectionIdAsync(string userId, string connectionId)
        {
            _logger.LogInformation("Client ID {ClientId} with connection ID {ConnectionId} has disconnected", userId, connectionId);
            if (!_connections.Remove(userId, connectionId))
            {
                _logger.LogInformation("Client ID {ClientId} already has a new connection ID. Disconnection not performed. Client tried to disconnect with old connection ID {ConnectionId}", userId, connectionId);
                return;
            }
            if (ClientListChanged != null)
            {
                await ClientListChanged.Invoke(new ConnectionEventArgs(userId, null, false, false)).ConfigureAwait(false);
            }
        }

        public Task<int> GetCountOfConnectedUsersAsync()
        {
            return Task.FromResult(_connections.GetCount());
        }

        public async Task<bool> RaiseTransportMessageReceivedAndSendResponseAsync(TransportMessage message)
        {
            var transportMessageReceived = TransportMessageReceived;
            if (transportMessageReceived == null) return false;
            return await transportMessageReceived.Invoke(message).ConfigureAwait(false);
        }

        public async Task<bool> SendMessageToClientAsync(TransportMessage message)
        {
            return await InternalSendMessageToClientAsync(message, TimeSpan.FromSeconds(15)).ConfigureAwait(false);
        }
        public async Task<bool> SendMessageToClientAsync(TransportMessage message, TimeSpan timeout)
        {
            return await InternalSendMessageToClientAsync(message, timeout).ConfigureAwait(false);
        }

        private async Task<bool> InternalSendMessageToClientAsync(TransportMessage message, TimeSpan timeout)
        {
            try
            {
                using var cts = new CancellationTokenSource(timeout);
                var connectionId = _connections.GetConnectionIdByUserId(message.ClientId);
                if (connectionId == null)
                {
                    _logger.LogInformation("Cannot send TransportMessage with label {Label} to client ID {ClientId}. Because connectionId is null", message.Label, message.ClientId);
                    return false;
                }
                _logger.LogInformation("Sending TransportMessage with label {Label} to client ID {ClientId}. Timeout is set to {TimeoutInSeconds} seconds", message.Label, message.ClientId, timeout.TotalSeconds);
                var result = await _hubContext.Clients.Client(connectionId).InvokeAsync<bool>("TransportMessageReceived", message, cts.Token).ConfigureAwait(false);
                var isDeliveredAndHandled = result ? "successfully been delivered and handled" : "not been successfully delivered and handled";
                _logger.LogInformation("TransportMessage with label {Label} to client ID {ClientId} has {IsDeliveredAndHandled}", message.Label, message.ClientId, isDeliveredAndHandled);
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send TransportMessage with label {Label} to client ID {ClientId}", message.Label, message.ClientId);
                return false;
            }
        }
    }
}