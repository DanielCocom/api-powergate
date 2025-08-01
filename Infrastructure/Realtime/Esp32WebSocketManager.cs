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

        public Esp32WebSocketManager(ILogger<Esp32WebSocketManager> logger, WebSocketManager webManager)
        {
            _logger = logger;
            _webManager = webManager;
        }

        public async Task HandleConnectionAsync(WebSocket webSocket, string deviceId)
        {
            // Intentar añadir el socket. Si ya existe (p.ej., reconexión rápida), manejarlo.
            // Para asegurar una única conexión por deviceId, podrías considerar cerrar la anterior aquí.
            if (!_sockets.TryAdd(deviceId, webSocket))
            {
                _logger.LogWarning($"Dispositivo {deviceId} intentó reconectar. Cerrando conexión anterior si existe.");
                if (_sockets.TryGetValue(deviceId, out var existingSocket) && existingSocket != webSocket)
                {
                    // Esto es una simplificación, en un sistema real, querrías una lógica de cierre más robusta
                    // para la conexión anterior, asegurando que se desaloje correctamente.
                    try { await existingSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "New connection", CancellationToken.None); }
                    catch { /* Ignorar */ }
                    _sockets.TryUpdate(deviceId, webSocket, existingSocket); // Actualizar al nuevo socket
                }
                else
                {
                    // Si el mismo socket intenta conectar de nuevo, solo loguear y continuar
                    _logger.LogInformation($"Dispositivo {deviceId} ya registrado con la misma conexión.");
                }
            }

            _logger.LogInformation($"Dispositivo {deviceId} conectado. Estado: {webSocket.State}");

            // Notificar conexión (FUERA del loop, solo una vez al conectar)
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

            var buffer = new byte[1024 * 4]; // Búfer para mensajes

            // Usamos un control para asegurar que HandleDisconnection se llame solo una vez y de forma controlada
            bool connectionHandled = false;

            try
            {
                // Bucle principal para recibir mensajes mientras el socket está abierto
                while (webSocket.State == WebSocketState.Open)
                {
                    WebSocketReceiveResult result;
                    try
                    {
                        // Espera a recibir un mensaje del WebSocket
                        result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                    }
                    catch (WebSocketException ex)
                    {
                        // Captura la excepción cuando el "remote party" cierra prematuramente
                        _logger.LogWarning($"Dispositivo {deviceId} se desconectó abruptamente durante ReceiveAsync (código: {ex.WebSocketErrorCode}, mensaje: {ex.Message}).");
                        break; // Salir del bucle para ir al finally
                    }
                    catch (OperationCanceledException)
                    {
                        // Esto ocurriría si CancellationToken.IsCancellationRequested fuera true
                        _logger.LogInformation($"Operación de recepción cancelada para el dispositivo {deviceId}.");
                        break; // Salir del bucle
                    }
                    catch (Exception ex)
                    {
                        // Captura cualquier otra excepción inesperada durante la recepción
                        _logger.LogError(ex, $"Error inesperado al recibir datos del dispositivo {deviceId}.");
                        break; // Salir del bucle
                    }

                    // Después de recibir, verifica el tipo de mensaje y el estado del socket
                    if (result.MessageType == WebSocketMessageType.Text)
                    {
                        var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                        _logger.LogInformation($"Mensaje de {deviceId}: {message}");

                        // Procesar mensaje según tipo
                        await ProcessMessageAsync(deviceId, message);
                    }
                    else if (result.MessageType == WebSocketMessageType.Close)
                    {
                        // El cliente solicitó un cierre limpio
                        _logger.LogInformation($"Dispositivo {deviceId} solicitó cerrar la conexión (código: {result.CloseStatus}).");
                        await webSocket.CloseAsync(
                            result.CloseStatus.Value,
                            result.CloseStatusDescription,
                            CancellationToken.None);
                        break; // Salir del bucle
                    }
                    // Puedes añadir manejo para WebSocketMessageType.Binary si esperas binarios
                    // O WebSocketMessageType.Pong si quieres manejar pings/pongs manualmente (aunque KeepAliveInterval lo hace automático)
                }
            }
            catch (Exception ex)
            {
                // Este catch manejará cualquier excepción que escape del bucle principal
                _logger.LogError(ex, $"Excepción principal no manejada en el ciclo de conexión para el dispositivo {deviceId}");
            }
            finally
            {
                // Este bloque se ejecutará siempre, tanto si el bucle termina normalmente como si hay una excepción.
                // Aseguramos que HandleDisconnection se llame una sola vez de forma controlada.
                if (!connectionHandled)
                {
                    await HandleDisconnection(deviceId);
                    connectionHandled = true; // Marca que la desconexión ha sido manejada
                }
            }
        }

        // Resto de tu código (SendCommandAsync, IsOnlineAsync, HandleDisconnection, ProcessMessageAsync, etc.)
        // No hay cambios importantes en estas funciones a menos que tu lógica de desconexión en HandleDisconnection
        // requiera ser más robusta, pero por ahora está bien.

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

                    // Si el socket no está abierto, removerlo.
                    _sockets.TryRemove(deviceId, out _);
                    _logger.LogWarning($"Intento de envío a socket {deviceId} no abierto. Removido del diccionario.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error enviando comando a {deviceId}. Removiendo socket.");
                    _sockets.TryRemove(deviceId, out _); // Remover en caso de error de envío también
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
                _logger.LogInformation($"Dispositivo {deviceId} desconectado. Intentando cerrar el socket.");

                try
                {
                    // Intentar cerrar el socket si aún está abierto.
                    // Esto evita excepciones si ya está cerrado o en Closing/Closed.
                    if (socket.State != WebSocketState.Closed && socket.State != WebSocketState.Aborted)
                    {
                        // Puedes usar un token de cancelación con un timeout si CloseAsync se cuelga
                        using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                        await socket.CloseAsync(
                            WebSocketCloseStatus.NormalClosure,
                            "Servidor cerrando",
                            cts.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning($"Cierre del socket para {deviceId} cancelado por timeout.");
                }
                catch (Exception ex)
                {
                    // Ignorar errores en el cierre final del socket
                    _logger.LogError(ex, $"Error durante el cierre final del socket para {deviceId}.");
                }
                finally
                {
                    // Es buena práctica disponer del socket si no se va a usar más.
                    socket.Dispose();
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
            else
            {
                _logger.LogInformation($"Dispositivo {deviceId} ya había sido marcado como desconectado o no encontrado.");
            }
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

                            // Si ESP32 envía pings, podrías manejarlos aquí (aunque KeepAliveInterval los hace implícitos)
                            // case "ping":
                            //     if (_sockets.TryGetValue(deviceId, out var socket))
                            //     {
                            //         var pongBytes = Encoding.UTF8.GetBytes("pong");
                            //         await socket.SendAsync(new ArraySegment<byte>(pongBytes), WebSocketMessageType.Text, true, CancellationToken.None);
                            //     }
                            //     break;
                    }
                }
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, $"Error al parsear JSON del mensaje de {deviceId}: {message}");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error procesando mensaje de {deviceId}: {message}");
            }
        }

        // Implementación de IDeviceConnectionTracker (Mantener como están)
        public Task MarkConnectedAsync(int dispositivoId, string connectionId)
        {
            return Task.CompletedTask;
        }

        public Task MarkDisconnectedAsync(int dispositivoId, string connectionId)
        {
            return Task.CompletedTask;
        }

        public Task<List<string>> GetConnectionAsync(int dispositivoId, string connectionId)
        {
            var connections = _sockets.ContainsKey(dispositivoId.ToString())
                ? new List<string> { $"ws-{dispositivoId}" } // Este ID es solo un placeholder, no es un ID de conexión real del WebSocket
                : new List<string>();
            return Task.FromResult(connections);
        }
    }
}