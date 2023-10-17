using System;
using System.Threading.Tasks;

namespace Communication.Core
{
    public delegate Task<bool> TransportMessageReceivedHandler(TransportMessage message);

    [Serializable]
    public class TransportMessage
    {
        public string Label { get; private set; }
        public string ClientId { get; private set; }
        public string Content { get; set; }

        public TransportMessage(string label, string content, string clientId)
        {
            Label = label;
            Content = content;
            ClientId = clientId;
        }
    }
}
