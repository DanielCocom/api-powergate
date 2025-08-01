using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;

namespace api_powergate.Infrastructure.Realtime
{
    public class WebSocketManager
    {
        private readonly ConcurrentDictionary<string, List<WebSocket>> _deviceSubscriptions = new();
        private readonly ILogger<WebSocketManager> _logger;
        private readonly IServiceProvider _serviceProvider;

        public WebSocketManager(ILogger<WebSocketManager> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        public void Subscribe(string deviceId, WebSocket webSocket)
        {
            if (webSocket == null) return;
            _deviceSubscriptions.AddOrUpdate(
                deviceId,
                new List<WebSocket> { webSocket },
                (key, existingList) =>
                {
                    existingList.Add(webSocket);
                    return existingList;
                });

            _logger.LogInformation($"Cliente suscrito a dispositivo {deviceId}");

            // Obtén la instancia de Esp32WebSocketManager solo cuando la necesites
            var esp32Manager = _serviceProvider.GetService<Esp32WebSocketManager>();
            if (esp32Manager != null && esp32Manager.IsDeviceOnline(deviceId))
            {
                var onlineMessage = System.Text.Json.JsonSerializer.Serialize(new
                {
                    type = "device_status",
                    deviceId,
                    status = "online",
                    timestamp = DateTime.UtcNow
                });

                webSocket.SendAsync(
                    new ArraySegment<byte>(Encoding.UTF8.GetBytes(onlineMessage)),
                    WebSocketMessageType.Text,
                    true,
                    CancellationToken.None);
            }
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
