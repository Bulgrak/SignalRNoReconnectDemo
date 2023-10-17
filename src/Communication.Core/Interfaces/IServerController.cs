using System;
using System.Threading.Tasks;

namespace Communication.Core.Interfaces
{
    public interface IServerController
    {
        event ConnectionChangedEventHandler ClientListChanged;
        event TransportMessageReceivedHandler TransportMessageReceived;
        /// <summary>
        /// Sends a message to the Client with Client ID of TransportMessage.ClientId.
        /// Uses the default timeout of the implementation.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <returns>true if the Client successfully handled the message.</returns>
        Task<bool> SendMessageToClientAsync(TransportMessage message);
        /// <summary>
        /// Sends a message to the Client with Client ID of TransportMessage.ClientId.
        /// </summary>
        /// <param name="message">Message to send.</param>
        /// <param name="timeout">The timespan to wait before the request times out.</param>
        /// <returns>true if the Client successfully handled the message.</returns>
        Task<bool> SendMessageToClientAsync(TransportMessage message, TimeSpan timeout);
    }
}