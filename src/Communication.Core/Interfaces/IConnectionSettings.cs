namespace Communication.Core.Interfaces
{
    public interface IConnectionSettings
    {
        string ServerAddress { get; set; }
        ushort ServerPort { get; set; }
        int ConnectionTimeoutInSeconds { get; set; }
        int ConnectionWatchDogInSeconds { get; set; }
        int PingSizeBytes { get; set; }
        int PingTimeoutInSeconds { get; set; }
        int PingIntervalInSeconds { get; set; }
        int ReconnectDelaysInSeconds { get; set; }
    }
}
