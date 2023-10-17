using Communication.Core;
using Communication.Core.Interfaces;
using System.Threading.Tasks;

namespace Signalr.Server.Interfaces
{
    public interface ISignalrServerController : IServerController
    {
        Task<bool> RaiseTransportMessageReceivedAndSendResponseAsync(TransportMessage message);
        Task AddConnectedUserWithConnectionIdAsync(string userId, string connectionId);
        Task RemoveDisconnectedUserWithConnectionIdAsync(string userId, string connectionId);
        Task<int> GetCountOfConnectedUsersAsync();
    }
}