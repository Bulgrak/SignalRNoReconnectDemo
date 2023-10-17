using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Signalr.Server.Interfaces;
using Communication.Core;

namespace Signalr.Server
{
    public class ServerHub: Hub
    {
        private ISignalrServerController _serverController;
        private ILogger<ServerHub> _logger;

        public ServerHub(ISignalrServerController serverController, ILogger<ServerHub> logger)
        {
            _logger = logger;
            _serverController = serverController;
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;
            _logger.LogInformation("OnConnectedAsync client ID: {ClientId}", userId);
            await _serverController.AddConnectedUserWithConnectionIdAsync(userId, connectionId).ConfigureAwait(false);
            await base.OnConnectedAsync().ConfigureAwait(false);
        }

        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var connectionId = Context.ConnectionId;
            var userId = Context.UserIdentifier;
            _logger.LogInformation("OnDisconnectedAsync client ID: {ClientId}", userId);
            await _serverController.RemoveDisconnectedUserWithConnectionIdAsync(userId, connectionId).ConfigureAwait(false);
            await base.OnDisconnectedAsync(exception).ConfigureAwait(false);
        }

        public async Task<bool> SendMessageToServerAsync(TransportMessage message)
        {
            _logger.LogInformation("Received message from client ID: {ClientId}", message.ClientId);
            return await _serverController.RaiseTransportMessageReceivedAndSendResponseAsync(message).ConfigureAwait(false);
        }

        public Task PingAsync()
        {
            _logger.LogTrace("Received ping");
            return Task.CompletedTask; 
        }
    }
}