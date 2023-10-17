using Microsoft.AspNetCore.SignalR;

namespace Signalr.Server
{
    public class UserIdProvider : IUserIdProvider
    {
        public string GetUserId(HubConnectionContext connection)
        {
            var httpContext = connection.GetHttpContext();
            var user = httpContext.Request.Query["user"].ToString();
            return user;
        }
    }
}
