using api_powergate.Aplication.Dtos.Canal;
using api_powergate.Common;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Interfaces.ws;
using api_powergate.Infrastructure.Data;
using api_powergate.Infrastructure.Realtime;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using WebSocketManager = api_powergate.Infrastructure.Realtime.WebSocketManager;


namespace api_powergate.Aplication.Services
{
    public class CanalCargaService : ICanalCargaService
    {
        private readonly ApiPowergateContext _context;
        private IDeviceConnectionTracker _connTracker;
        private IRelayCommander _relayCommander;
        private readonly Esp32WebSocketManager _wsManager;
        private readonly WebSocketManager _webSocketManager;
        public CanalCargaService(ApiPowergateContext apiPowergateContext, IRelayCommander relayCommander, IDeviceConnectionTracker connTracker, Esp32WebSocketManager esp32WebSocketManager, WebSocketManager webSocketManager)
        {
            _context = apiPowergateContext ?? throw new ArgumentNullException(nameof(apiPowergateContext));
            _webSocketManager = webSocketManager;
            _relayCommander = relayCommander;
            _connTracker = connTracker ?? throw new ArgumentNullException(nameof(connTracker));
            _wsManager = esp32WebSocketManager ?? throw new ArgumentNullException(nameof(esp32WebSocketManager));
        }
        public async Task<Response<List<CanalEstadoDto>>> GetEstadoRele(int id_dispositivo)
        {
            Response<List<CanalEstadoDto>> response = new Response<List<CanalEstadoDto>>();

            try
            {
                var canal = await _context.CanalDeCargas
                   .Where(c => c.DispositivoId == id_dispositivo)
                   .Select(c => new CanalEstadoDto
                   {
                       CanalId = c.Id,
                       Nombre = c.Nombre,
                       ReleActivo = c.Habilitado
                   })
                   .ToListAsync();

                if (canal == null || canal.Count == 0)
                {
                    response.IsSuccess = false;
                    response.Message = "No se encontró el canal de carga.";
                    return response;
                }

                response.Data = canal;
                response.IsSuccess = true;
                response.Message = "Estado del rele obtenido correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error al obtener el estado del rele: {ex.Message}";
                return response;
            }

        }

        public async Task<Response<CanalEstadoDto>> CambiarEstadoRele(int canalId, bool estado)
        {
            Response<CanalEstadoDto> response = new();
            try
            {
                var canal = await _context.CanalDeCargas
                    .Include(c => c.Dispositivo)
                    .FirstOrDefaultAsync(c => c.Id == canalId);

                if (canal?.Dispositivo == null)
                {
                    response.Data = null;
                    response.IsSuccess = false;
                    response.Message = canal == null ? "Canal no encontrado." : "Dispositivo asociado no encontrado.";
                    return response;
                }

                // Actualizar estado en base de datos
                canal.Habilitado = estado;
                await _context.SaveChangesAsync();

                var commandId = Guid.NewGuid().ToString("N");
                var dispositivoId = canal.DispositivoId;
                var online = await _wsManager.IsOnlineAsync(dispositivoId);

                if (online)
                {
                    await _relayCommander.SendToggleAsync(
                        dispositivoId,
                        canal.Id,
                        estado,
                        commandId);

                    await _context.SaveChangesAsync();
                }

                // Notificar a los clientes web suscritos al canal
             
                //Mensaje que se envia
                var notification = new
                {
                    type = "canal_estado",
                    canalId = canal.Id,
                    nombre = canal.Nombre,
                    releActivo = canal.Habilitado,
                    mensaje = estado ? "Canal encendido" : "Canal apagado",
                    timestamp = DateTime.UtcNow
                };
                await _webSocketManager.BroadcastToDeviceSubscribers(
                    canal.Id.ToString(),
                    JsonSerializer.Serialize(notification)
                );

                response.Data = new CanalEstadoDto
                {
                    CanalId = canal.Id,
                    Nombre = canal.Nombre,
                    ReleActivo = canal.Habilitado,
                };

                response.IsSuccess = true;
                response.Message = online
                    ? "Estado del relé actualizado y comando enviado al dispositivo."
                    : "Estado del relé actualizado (dispositivo offline).";

                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error al cambiar el estado del relé: {ex.Message}";
                return response;
            }
        }

    }
}
