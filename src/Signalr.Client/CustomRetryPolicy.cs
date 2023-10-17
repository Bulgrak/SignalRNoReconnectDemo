using Microsoft.AspNetCore.SignalR.Client;
using System;

namespace Signalr.Client
{
    public class CustomRetryPolicy : IRetryPolicy
    {
        private readonly int _reconnectDelaysInSeconds;

        public CustomRetryPolicy(int reconnectDelaysInSeconds)
        {
            _reconnectDelaysInSeconds = reconnectDelaysInSeconds;
        }

        public TimeSpan? NextRetryDelay(RetryContext retryContext)
        {
            return TimeSpan.FromSeconds(_reconnectDelaysInSeconds == default ? 3 : _reconnectDelaysInSeconds);
        }
    }
}
