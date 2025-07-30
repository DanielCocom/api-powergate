using api_powergate.Domain.Interfaces.ws;
using System.Collections.Concurrent;
using System.Globalization;

namespace api_powergate.Infrastructure.Realtime
{
    public class InMemoryDeviceConnectionTracker : IDeviceConnectionTracker
    {
        private readonly ConcurrentDictionary<int, string> _map = new();
        public Task MarkConnectedAsync(int dispositivoId, string connectionId)
        {
            _map[dispositivoId] = connectionId;
            return Task.CompletedTask;
        }
        public Task MarkDisconnectedAsync(int dispositivoId, string connectionId)
        {
            _map.TryRemove(dispositivoId, out _);
            return Task.CompletedTask;
        }
        public Task<bool> IsOnlineAsync(int dispositivoId)
            => Task.FromResult(_map.ContainsKey(dispositivoId));

        public Task<List<string>> GetConnectionAsync(int dispositivoId, string connectionId)
        {
            return Task.FromResult(_map.TryGetValue(dispositivoId, out var cid)
                ? new List<string> { cid }
                : new List<string>());
        }
    }
}
