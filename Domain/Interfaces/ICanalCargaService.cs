using api_powergate.Aplication.Dtos.Canal;
using api_powergate.Common;

namespace api_powergate.Domain.Interfaces
{
    public interface ICanalCargaService
    {
        Task<Response<List<CanalCargaEstadoDto>>> GetEstadoRele(int id_dispositivo);
    }
}
