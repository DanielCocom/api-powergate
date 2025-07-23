using api_powergate.Domain.Models;

namespace api_powergate.Domain.Interfaces
{
    public interface IDispositivoRepository
    {
        Task<bool> AddDispositivo(Dispositivo dispositivo);
        Task<Dispositivo?> GetDispositivoPorId(int id);
        Task<IEnumerable<Dispositivo>> GetAllDispositivos();
        Task<bool> UpdateDispositivo(Dispositivo dispositivo);
        Task<bool> DeleteDispositivo(int id);
    }
}
