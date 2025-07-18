using api_powergate.Domain.Models;

namespace api_powergate.Domain.Interfaces
{
    public interface ICanalDeCargaRepository
    {
        Task<CanalDeCarga?> GetById(int id);
    }

}
