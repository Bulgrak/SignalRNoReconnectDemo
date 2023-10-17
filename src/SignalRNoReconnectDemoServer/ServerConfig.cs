namespace SignalRNoReconnectDemoServer
{
    public class ServerConfig
    {
        public long MaximumReceiveMessageSize { get; set; }
        public string Urls { get; set; }

        public ServerConfig()
        {
            MaximumReceiveMessageSize = long.MaxValue;
        }
    }
}
