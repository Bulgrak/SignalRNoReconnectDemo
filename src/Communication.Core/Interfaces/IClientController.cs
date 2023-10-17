using System;
using System.Threading.Tasks;

namespace Communication.Core.Interfaces
{
    public delegate Task ConnectionChangedHandler(bool isConnected);

    public interface IClientController
    {
        event TransportMessageReceivedHandler TransportMessageReceived;
        event ConnectionChangedHandler ConnectionChanged;
        
        Task ConnectAsync();
        Task DisconnectAsync();
        /// <summary>
        /// Sends a message to the Server.
        /// Uses the default timeout of the implementation.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns>true if the Server successfully handled the message.</returns>
        Task<bool> SendMessageToServerAsync(TransportMessage message);
        /// <summary>
        /// Sends a message to the Server.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="timeout">The timespan to wait before the request times out.</param>
        /// <returns>true if the Server successfully handled the message.</returns>
        Task<bool> SendMessageToServerAsync(TransportMessage message, TimeSpan timeout);
    }
}
