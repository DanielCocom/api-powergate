using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace api_powergate.Infrastructure.Realtime
{
    public class WebSocketManager
    {
        private readonly ConcurrentDictionary<string, List<WebSocket>> _deviceSubscriptions = new();
        private readonly ILogger<WebSocketManager> _logger;

        public WebSocketManager(ILogger<WebSocketManager> logger)
        {
            _logger = logger;
        }

        public void Subscribe(string deviceId, WebSocket webSocket)
        {
            _deviceSubscriptions.AddOrUpdate(
                deviceId,
                new List<WebSocket> { webSocket },
                (key, existingList) =>
                {
                    existingList.Add(webSocket);
                    return existingList;
                });

            _logger.LogInformation($"Cliente suscrito a dispositivo {deviceId}");
        }

        public void Unsubscribe(string deviceId, WebSocket webSocket)
        {
            if (_deviceSubscriptions.TryGetValue(deviceId, out var sockets))
            {
                sockets.Remove(webSocket);
                if (!sockets.Any()) _deviceSubscriptions.TryRemove(deviceId, out _);
            }
        }

        public async Task BroadcastToDeviceSubscribers(string deviceId, string message)
        {
            if (_deviceSubscriptions.TryGetValue(deviceId, out var sockets))
            {
                var bytes = Encoding.UTF8.GetBytes(message);
                var tasks = new List<Task>();

                foreach (var socket in sockets.ToList())
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        tasks.Add(socket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None));
                    }
                    else
                    {
                        Unsubscribe(deviceId, socket);
                    }
                }

                await Task.WhenAll(tasks);
            }
        }
    }
}
