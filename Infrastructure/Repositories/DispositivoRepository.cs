using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;
using api_powergate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Infrastructure.Repositories
{
    public class DispositivoRepository : IDispositivoRepository
    {
        private readonly ApiPowergateContext _context;
        public DispositivoRepository(ApiPowergateContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }
        public Task<bool> AddDispositivo(Dispositivo dispositivo)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteDispositivo(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<Dispositivo>> GetAllDispositivos()
        {
            throw new NotImplementedException();
        }

        public async Task<Dispositivo?> GetDispositivoPorId(int id)
        {
            var dispositvo = await _context.Dispositivos.FirstOrDefaultAsync(x => x.Id == id);  
            return dispositvo;
        }

        public Task<bool> UpdateDispositivo(Dispositivo dispositivo)
        {
            throw new NotImplementedException();
        }
    }
}
