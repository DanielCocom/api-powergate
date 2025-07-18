using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;
using api_powergate.Infrastructure.Data;
using System;

namespace api_powergate.Infrastructure.Repositories
{
    public class LecturaRepository : ILecturaRepository
    {
        private readonly ApiPowergateContext _context;

        public LecturaRepository(ApiPowergateContext context)
        {
            _context = context;
        }

        public async Task AddLectura(Lectura lectura)
        {
            _context.Lecturas.Add(lectura);
            await _context.SaveChangesAsync();
        }
    }
}
