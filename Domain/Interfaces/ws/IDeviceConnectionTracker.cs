using System.Globalization;

namespace api_powergate.Domain.Interfaces.ws
{
    public interface IDeviceConnectionTracker
    {
        Task MarkConnectedAsync(int dispositivoid, string connectionId);
        Task MarkDisconnectedAsync(int dispositivoid, string connectionId);
        Task<bool> IsOnlineAsync(int dispositivoid);
        Task<List<string>> GetConnectionAsync(int dispositivoid, string connectionId);

    }
}
