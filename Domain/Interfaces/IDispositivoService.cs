using api_powergate.Aplication.Dtos.Dispositivo;
using api_powergate.Common;
using api_powergate.Domain.Models;

namespace api_powergate.Domain.Interfaces
{
    public interface IDispositivoService
    {
        Task<Response<EstadoDispositivoDto?>> ObtenerEstado(int dispositivoId);
    }
}
