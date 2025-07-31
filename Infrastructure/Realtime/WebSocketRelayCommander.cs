using api_powergate.Domain.Interfaces.ws;
using Microsoft.AspNetCore.SignalR;
using System.Text.Json;

namespace api_powergate.Infrastructure.Realtime
{
    public class WebSocketRelayCommander : IRelayCommander
    {
        
    private readonly Esp32WebSocketManager _wsManager;
        private readonly ILogger<WebSocketRelayCommander> _logger;

        public WebSocketRelayCommander(
            Esp32WebSocketManager wsManager,
            ILogger<WebSocketRelayCommander> logger)
        {
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
                var message = JsonSerializer.Serialize(payload);
                await _wsManager.SendCommandAsync(dispositivoId.ToString(), message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error al enviar comando al dispositivo {dispositivoId}");
                throw;
            }
        }
    }
}

