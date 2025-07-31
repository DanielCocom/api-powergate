using api_powergate.Domain.Interfaces.ws;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;

namespace api_powergate.Infrastructure.Realtime
{
    public class Esp32WebSocketManager : IDeviceConnectionTracker
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly ILogger<Esp32WebSocketManager> _logger;

        public Esp32WebSocketManager(ILogger<Esp32WebSocketManager> logger)
        {
            _logger = logger;
        }

        public async Task HandleConnectionAsync(WebSocket webSocket, string deviceId)
        {
            _sockets.TryAdd(deviceId, webSocket);
            _logger.LogInformation($"Dispositivo {deviceId} conectado");

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Mensaje de {deviceId}: {message}");
                        // Procesar ACKs u otros mensajes aquí
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleDisconnection(deviceId);
                    }
                }
            }
            finally
            {
                await HandleDisconnection(deviceId);
            }
        }

        public async Task<bool> SendCommandAsync(string deviceId, string message)
        {
            if (_sockets.TryGetValue(deviceId, out var socket))
            {
                try
                {
                    if (socket.State == WebSocketState.Open)
                    {
                        var bytes = Encoding.UTF8.GetBytes(message);
                        await socket.SendAsync(
                            new ArraySegment<byte>(bytes),
                            WebSocketMessageType.Text,
                            true,
                            CancellationToken.None);
                        return true;
                    }

                    _sockets.TryRemove(deviceId, out _);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error enviando comando a {deviceId}");
                    _sockets.TryRemove(deviceId, out _);
                }
            }
            return false;
        }

        public Task<bool> IsOnlineAsync(int dispositivoId)
        {
            return Task.FromResult(_sockets.ContainsKey(dispositivoId.ToString()));
        }

        private async Task HandleDisconnection(string deviceId)
        {
            if (_sockets.TryRemove(deviceId, out var socket))
            {
                _logger.LogInformation($"Dispositivo {deviceId} desconectado");
                try
                {
                    await socket.CloseAsync(
                        WebSocketCloseStatus.NormalClosure,
                        "Closing",
                        CancellationToken.None);
                }
                catch
                {
                    // Ignorar errores de cierre
                }
            }
        }

        // Implementación de IDeviceConnectionTracker
        public Task MarkConnectedAsync(int dispositivoId, string connectionId)
        {
            // Ya manejado en HandleConnectionAsync
            return Task.CompletedTask;
        }

        public Task MarkDisconnectedAsync(int dispositivoId, string connectionId)
        {
            // Ya manejado en HandleDisconnection
            return Task.CompletedTask;
        }

        public Task<List<string>> GetConnectionAsync(int dispositivoId, string connectionId)
        {
            var connections = _sockets.ContainsKey(dispositivoId.ToString())
                ? new List<string> { $"ws-{dispositivoId}" }
                : new List<string>();
            return Task.FromResult(connections);
        }
    }
}
