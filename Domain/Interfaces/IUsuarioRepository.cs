using api_powergate.Domain.Models;

namespace api_powergate.Domain.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<bool> AddUsuario(Usuario usuario);
        Task<Usuario> GetUsuarioPorCorreo(string correo);
    }
}
