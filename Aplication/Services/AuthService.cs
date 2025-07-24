using api_powergate.Aplication.Dtos;
using api_powergate.Aplication.Dtos.Usuario;
using api_powergate.Common;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;

public class AuthService
{
    private readonly IUsuarioRepository _usuarioRepository;

    public AuthService(IUsuarioRepository usuarioRepository)
    {
        _usuarioRepository = usuarioRepository;
    }

    public async Task<Response<UsuarioDto>> RegistrarUsuario(AuthDtos.RegisterRequest registerRequest)
    {
        var response = new Response<UsuarioDto>();

        try
        {
            // Validar si ya existe el correo
            var usuarioExiste = await _usuarioRepository.GetUsuarioPorCorreo(registerRequest.correo);
            if (usuarioExiste != null)
            {
                response.IsSuccess = false;
                response.Message = "El correo ya está registrado.";
                return response;
            }

            // Encriptar contraseña
            var contrasenaHash = HashPassword(registerRequest.contrasena);

            var nuevoUsuario = new Usuario
            {
                Nombre = "Default",
                Correo = registerRequest.correo,
                ClaveHash = contrasenaHash,
                FechaRegistro = DateTime.UtcNow
            };

            var guardado = await _usuarioRepository.AddUsuario(nuevoUsuario);

            if (!guardado)
            {
                response.IsSuccess = false;
                response.Message = "No se pudo registrar el usuario. Intenta más tarde.";
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Usuario registrado con éxito.";
            response.Data = new UsuarioDto { correo = nuevoUsuario.Correo };
            return response;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = $"Error interno: {ex.Message}";
            return response;
        }
    }

    public async Task<Response<UsuarioDto>> LoginUsuario(AuthDtos.LoginRequest loginRequest)
    {
        var response = new Response<UsuarioDto>();

        try
        {
            var usuario = await _usuarioRepository.GetUsuarioPorCorreo(loginRequest.correo);

            if (usuario == null)
            {
                response.IsSuccess = false;
                response.Message = "El correo no está registrado.";
                return response;
            }

            var credencialesValidas = VerifyPassword(loginRequest.contrasena, usuario.ClaveHash);

            if (!credencialesValidas)
            {
                response.IsSuccess = false;
                response.Message = "La contraseña es incorrecta.";
                return response;
            }

            response.IsSuccess = true;
            response.Message = "Inicio de sesión exitoso.";
            response.Data = new UsuarioDto { correo = usuario.Correo };
            return response;
        }
        catch (Exception ex)
        {
            response.IsSuccess = false;
            response.Message = $"Error al iniciar sesión: {ex.Message}";
            return response;
        }
    }

    private string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        return BCrypt.Net.BCrypt.Verify(password, storedHash);
    }
}
