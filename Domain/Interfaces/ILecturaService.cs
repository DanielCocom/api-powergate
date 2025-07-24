using api_powergate.Aplication.Dtos;
using api_powergate.Common;
using api_powergate.Domain.Models;

namespace api_powergate.Domain.Interfaces
{
    public interface ILecturaService
    {
        Task<Response<bool>> RegistrarLectura(RegistrarLecturaDto lectura);
        Task<Response<List<RegistrarLecturaDto>>> ObtenerLecturasPorDispositivoAsync(int dispositivoId);
        
    }
}
