using api_powergate.Aplication.Dtos.Usuario;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;
using api_powergate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace api_powergate.Infrastructure.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly ApiPowergateContext _context;

        public UsuarioRepository(ApiPowergateContext context)
        {
            _context = context;
        }


        public async Task<bool> AddUsuario(Usuario usuario)
        {
            await _context.Usuarios.AddAsync(usuario);
            var changes = await _context.SaveChangesAsync();
            return changes > 0;
        }
        public async Task<Usuario?> GetUsuarioPorCorreo(string correo)
        {
            return await _context.Usuarios.FirstOrDefaultAsync(u => u.Correo == correo);
        }
    }
}
