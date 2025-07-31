using api_powergate.Aplication.Dtos.Canal;
using api_powergate.Common;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Interfaces.ws;
using api_powergate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Aplication.Services
{
    public class CanalCargaService : ICanalCargaService
    {
        private readonly ApiPowergateContext _context;
        private IDeviceConnectionTracker _connTracker;
        private IRelayCommander _relayCommander;
        public CanalCargaService(ApiPowergateContext apiPowergateContext, IRelayCommander relayCommander, IDeviceConnectionTracker connTracker)
        {
            _context = apiPowergateContext ?? throw new ArgumentNullException(nameof(apiPowergateContext));
            _relayCommander = relayCommander;
            _connTracker = connTracker ?? throw new ArgumentNullException(nameof(connTracker));
        }
        public async Task<Response<List<CanalCargaEstadoDto>>> GetEstadoRele(int id_dispositivo)
        {
            Response<List<CanalCargaEstadoDto>> response = new Response<List<CanalCargaEstadoDto>>();

            try
            {
                var canal = await _context.CanalDeCargas
                   .Where(c => c.DispositivoId == id_dispositivo)
                   .Select(c => new CanalCargaEstadoDto
                   {
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

        public async Task<Response<bool>> CambiarEstadoRele(int canalId, bool estado)
        {
            Response<bool> response = new Response<bool>();
            try
            {
                var canal = await _context.CanalDeCargas.FindAsync(canalId);
                if (canal == null)
                {
                    response.IsSuccess = false;
                    response.Message = "Canal no encontrado.";
                    return response;
                }

                canal.Habilitado = estado;
                await _context.SaveChangesAsync();

                int dispositivoId = canal.DispositivoId;

                var online = await _connTracker.IsOnlineAsync(dispositivoId);

               var commandId = Guid.NewGuid().ToString("N");
                if (online) 
                {
                    await _relayCommander.SendToggleAsync(canal.DispositivoId, canal.Id, estado, Guid.NewGuid().ToString());

                }

                response.IsSuccess = true;
                response.Message = "Estado del rele actualizado correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error al cambiar el estado del rele: {ex.Message}";
                return response;
            }
        }


    }
}
