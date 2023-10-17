using Communication.Core.Interfaces;

namespace SignalRNoReconnectDemoClient
{
    public class ClientSettings : IClientSettings
    {
        public string ClientId { get; set; }
    }
}