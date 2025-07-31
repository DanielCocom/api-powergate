using api_powergate.Domain.Interfaces.ws;
using api_powergate.Infrastructure.Realtime;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

public class HybridRelayCommander : IRelayCommander
{
    private readonly IHubContext<DeviceHub> _hub;
    private readonly Esp32WebSocketManager _wsManager;
    private readonly ILogger<HybridRelayCommander> _logger;

    public HybridRelayCommander(
        IHubContext<DeviceHub> hub,
        Esp32WebSocketManager wsManager,
        ILogger<HybridRelayCommander> logger)
    {
        _hub = hub;
        _wsManager = wsManager;
        _logger = logger;
    }

    public async Task SendToggleAsync(int dispositivoId, int? canalId, bool estado, string commandId)
    {
        var payload = new
        {
            type = "relay.toggle",
            commandId,
            canalId,
            state = estado,
            timestamp = DateTime.UtcNow
        };

        try
        {
            // Intenta enviar por WebSocket primero
            var deviceStr = dispositivoId.ToString();
            var message = JsonSerializer.Serialize(payload);

            _logger.LogInformation($"Intentando enviar comando por WebSocket: {message}");

            var wsSuccess = await _wsManager.SendCommandAsync(deviceStr, message);

            if (!wsSuccess)
            {
                _logger.LogWarning("Falló WebSocket, intentando por SignalR");
                await _hub.Clients.Group(DeviceHub.DeviceGroup(dispositivoId))
                          .SendAsync("RelayCommand", payload);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error al enviar comando al dispositivo {dispositivoId}");
            throw;
        }
    }
}