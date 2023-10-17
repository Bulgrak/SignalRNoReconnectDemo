using Communication.Core.Interfaces;

namespace SignalRNoReconnectDemoClient
{
    public class ConnectionSettings : IConnectionSettings
    {
        public string ServerAddress { get; set; }
        public ushort ServerPort { get; set; }
        public int ConnectionTimeoutInSeconds { get; set; }
        public int ConnectionWatchDogInSeconds { get; set; }
        public int PingSizeBytes { get; set; }
        public int PingTimeoutInSeconds { get; set; }
        public int PingIntervalInSeconds { get; set; }
        public int ReconnectDelaysInSeconds { get; set; }
    }
}