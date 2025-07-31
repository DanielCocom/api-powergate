using api_powergate.Domain.Interfaces.ws;
using System.Collections.Concurrent;
using System.Globalization;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace api_powergate.Infrastructure.Realtime
{
    public class Esp32WebSocketManager : IDeviceConnectionTracker
    {
        private readonly ConcurrentDictionary<string, WebSocket> _sockets = new();
        private readonly ILogger<Esp32WebSocketManager> _logger;
        private readonly WebSocketManager _webManager;

        public Esp32WebSocketManager(ILogger<Esp32WebSocketManager> logger, WebSocketManager webManager )
        {
            _logger = logger;
            _webManager = webManager;
        }

        public async Task HandleConnectionAsync(WebSocket webSocket, string deviceId)
        {
            _sockets.TryAdd(deviceId, webSocket);
            _logger.LogInformation($"Dispositivo {deviceId} conectado");

            // 1. Notificar conexión (FUERA del loop)
            await _webManager.BroadcastToDeviceSubscribers(
                deviceId,
                JsonSerializer.Serialize(new
                {
                    type = "device_status",
                    deviceId,
                    status = "online",
                    timestamp = DateTime.UtcNow
                })
            );

            var buffer = new byte[1024 * 4];

            try
            {
                while (webSocket.State == WebSocketState.Open)
                {
                    var result = await webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        CancellationToken.None);

                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Mensaje de {deviceId}: {message}");

                        // 2. Procesar mensaje según tipo
                        await ProcessMessageAsync(deviceId, message);
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

                var offlineMessage = JsonSerializer.Serialize(new
                {
                    type = "device_status",
                    deviceId,
                    status = "offline",
                    timestamp = DateTime.UtcNow
                });

                await _webManager.BroadcastToDeviceSubscribers(deviceId, offlineMessage);
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
        private async Task ProcessMessageAsync(string deviceId, string message)
        {
            try
            {
                using var jsonDoc = JsonDocument.Parse(message);
                var root = jsonDoc.RootElement;

                if (root.TryGetProperty("type", out var typeProp))
                {
                    var messageType = typeProp.GetString();

                    switch (messageType)
                    {
                        case "telemetry":
                            var voltios = root.GetProperty("voltios").GetDouble();
                            var amperios = root.GetProperty("amperios").GetDouble();
                            var vatios = voltios * amperios; // Cálculo básico de potencia

                            await _webManager.BroadcastToDeviceSubscribers(
                                deviceId,
                                JsonSerializer.Serialize(new
                                {
                                    type = "telemetry",
                                    deviceId,
                                    voltios,
                                    amperios,
                                    vatios, // Añadido
                                    timestamp = DateTime.UtcNow
                                })
                            );
                            break;

                        case "ack":
                            // Procesar ACK si es necesario
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando mensaje de {deviceId}");
            }
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
