using Communication.Core;
using Communication.Core.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace SignalRNoReconnectDemoClient
{
    public class ServerCommunicator
    {
        private readonly ILogger<ServerCommunicator> _logger;
        private readonly IClientController _clientController;
        public event Action<TestMessage> MessageReceived;

        public ServerCommunicator(ILogger<ServerCommunicator> logger, IClientController clientController)
        {
            _logger = logger;
            _clientController = clientController;
            _clientController.TransportMessageReceived += _clientController_TransportMessageReceived;
        }

        public async Task ConnectToServer()
        {
            _logger.LogInformation($"{nameof(ServerCommunicator)}.{nameof(ConnectToServer)}: Connecting to server...");
            await _clientController.ConnectAsync();
            _logger.LogInformation($"{nameof(ServerCommunicator)}.{nameof(ConnectToServer)}: Connected to server");
        }

        public void SendMessage(TestMessage testMessage)
        {
            while (true)
            {
                var data = System.Text.Json.JsonSerializer.Serialize(testMessage);
                _logger.LogInformation("Sending the message with {Id} to server...", testMessage.Id);
                var result = _clientController.SendMessageToServerAsync(new TransportMessage($"client_{testMessage.Id}", data, "1")).GetAwaiter().GetResult();
                if (!result)
                {
                    _logger.LogInformation("Failed to send message with {Id} to server", testMessage.Id);
                }
                else
                {
                    _logger.LogInformation("Sent message with {Id} to server", testMessage.Id);
                    break;
                }
            }
        }

        private async Task<bool> _clientController_TransportMessageReceived(TransportMessage transportMessage)
        {
            await Task.Delay(1000).ConfigureAwait(false); 
            try
            {
                var message = System.Text.Json.JsonSerializer.Deserialize<TestMessage>(transportMessage.Content);
                _logger.LogDebug(
                    "Received TransportMessage with the label {TransportMessageLabel} from server",
                    transportMessage.Label);
                MessageReceived?.Invoke(message);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to handle incoming TransportMessage");
            }
            return false;
        }

        private void OnMessageReceived(TestMessage message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}