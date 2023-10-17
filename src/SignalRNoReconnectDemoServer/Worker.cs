using Signalr.Server;
using Signalr.Server.Interfaces;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using Communication.Core;

namespace SignalRNoReconnectDemoServer
{
    public class Worker : BackgroundService
    {
        private readonly IHubContext<ServerHub> _hubContext;
        private readonly ISignalrServerController _serverController;
        private readonly IOptions<ServerConfig> _config;
        private readonly ILogger<Worker> _logger;
        private int _messageCounter;

        public Worker(IHubContext<ServerHub> hubContext, ISignalrServerController serverController, IOptions<ServerConfig> config, ILogger<Worker> logger)
        {
            _logger = logger;
            _hubContext = hubContext;
            _serverController = serverController;
            _logger = logger;
            _config = config;

            _serverController.ClientListChanged += _serverController_ClientListChanged;
            _serverController.TransportMessageReceived += _serverController_TransportMessageReceivedAsync;
        }

        private SemaphoreSlim _semaphoreSlim = new SemaphoreSlim(1);

        /// <summary>
        /// This method causes SignalR server to hang on client reconnect because of SemaphoreSlim; somehow.
        /// If SemaphoreSlim is removed, the issue is gone.
        /// </summary>
        /// <param name="connectionEventArgs"></param>
        /// <returns></returns>
        //private Task _serverController_ClientListChanged(ConnectionEventArgs connectionEventArgs)
        //{
        //    _logger.LogInformation("Client list changed");
        //    try
        //    {
        //        _semaphoreSlim.Wait();
        //        return Task.CompletedTask;
        //    }
        //    finally
        //    {
        //        _semaphoreSlim.Release();
        //    }
        //}

        /// <summary>
        /// This method causes SignalR server to hang on client reconnect because of SemaphoreSlim; somehow.
        /// If SemaphoreSlim is removed, the issue is gone.
        /// </summary>
        /// <param name="connectionEventArgs"></param>
        /// <returns></returns>
        private async Task _serverController_ClientListChanged(ConnectionEventArgs connectionEventArgs)
        {
            _logger.LogInformation("Client list changed");
            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        private async Task<bool> _serverController_TransportMessageReceivedAsync(TransportMessage message)
        {
            _logger.LogTrace("_serverController__TransportMessageReceived");
            bool isMessageHandledSuccussfully = false;
            try
            {
                _logger.LogInformation("Received message from client Id {ClientId} with label {Label}", message.ClientId, message.Label);
                Task.Run(async () =>
                {
                    await SendMessageToClient(message.ClientId).ConfigureAwait(false);
                });
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occured when trying to handle the {msgType} with label {label} received from client with ID {clientId}.", nameof(TransportMessage), message.Label, message.ClientId);
            }
            return isMessageHandledSuccussfully;
        }

        private async Task<bool> SendMessageToClient(string clientId)
        {
            try
            {
                await _semaphoreSlim.WaitAsync().ConfigureAwait(false);
                var testMessage = new TestMessage { Id = (++_messageCounter).ToString(), Text = "Server message for client" };
                while (true)
                {
                    _logger.LogInformation("Sending message to client with id {ClientId}...", clientId);
                    var data = System.Text.Json.JsonSerializer.Serialize(testMessage);
                    var result = await _serverController.SendMessageToClientAsync(new TransportMessage($"server_{testMessage.Id}", data, clientId)).ConfigureAwait(false);
                    if (result)
                    {
                        _logger.LogInformation("Sent message to client with id {ClientId}", clientId);
                        return true;
                    }
                    else
                    {
                        _logger.LogInformation("Failed to send message to client with id {ClientId}. Retrying...", clientId);
                    }
                }
            }
            finally
            {
                _semaphoreSlim.Release();
            }
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}