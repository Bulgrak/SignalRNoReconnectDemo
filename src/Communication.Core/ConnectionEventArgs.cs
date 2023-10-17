using System;
using System.Threading.Tasks;

namespace Communication.Core
{
    public delegate Task ConnectionChangedEventHandler(ConnectionEventArgs connectionEventArgs);

    public class ConnectionEventArgs : EventArgs
    {
        public string ClientId { get; private set; }
        public string IpAddress { get; private set; }
        public bool Connected { get; private set; }
        public bool FreshStart { get; private set; }

        public ConnectionEventArgs(string clientId, string ipAddress, bool connected, bool freshStart)
        {
            ClientId = clientId;
            IpAddress = ipAddress;
            Connected = connected;
            FreshStart = freshStart;
        }
    }
}
