using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;
using api_powergate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;

namespace api_powergate.Infrastructure.Repositories
{
    public class CanalDeCargaRepository : ICanalDeCargaRepository
    {
        private readonly ApiPowergateContext _context;

        public CanalDeCargaRepository(ApiPowergateContext context)
        {
            _context = context;
        }

        public async Task<CanalDeCarga> GetById(int id)
        {
            var result = await _context.CanalDeCargas.FirstOrDefaultAsync(c => c.Id == id);
            return result;
        }
    }
}
