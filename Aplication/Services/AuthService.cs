using api_powergate.Aplication.Dtos;
using api_powergate.Aplication.Dtos.Usuario;
using api_powergate.Common;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;
using BCrypt.Net; // Importa la librería de bcrypt

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
            var usuarioExiste = await _usuarioRepository.GetUsuarioPorCorreo(registerRequest.correo);
            if (usuarioExiste != null)
            {
                response.IsSuccess = false;
                response.Message = "El correo ya esta registrado";
                return response;
            }

            // Aquí se usa bcrypt para generar el hash
            var contrasenaHash = HashPassword(registerRequest.contrasena);

            var usuario = new Usuario()
            {
                Nombre = "Default",
                Correo = registerRequest.correo,
                ClaveHash = contrasenaHash,
                FechaRegistro = DateTime.Now
            };

            var guardaUsuario = await _usuarioRepository.AddUsuario(usuario);

            if (guardaUsuario)
            {
                response.Data = null;
                response.IsSuccess = true;
                response.Message = "Se ha registrado el usuario con éxito";
                return response;
            }

            response.IsSuccess = false;
            response.Message = "No se pudo registrar, intentalo más tarde";
            response.Data = null;

        }
        catch (Exception exe)
        {
            response.IsSuccess = false;
            response.Message = exe.Message;
        }

        return response;
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
                response.Message = "El correo no se ha registrado";
                response.Data = null;
                return response;
            }

          
            var verificarContrasena = VerifyPassword(loginRequest.contrasena, usuario.ClaveHash);

            if (verificarContrasena)
            {
                response.Data = new UsuarioDto { corroe = loginRequest.correo };
                response.IsSuccess = true;
                response.Message = "Inicio de sesión exitoso";
                return response;
            }

            response.Data = null;
            response.IsSuccess = false;
            response.Message = "No se pudo registrar al usuario";

        }
        catch (Exception exe)
        {
            response.IsSuccess = false;
            response.Message = exe.Message;
            response.Data = null;
        }
        return response;
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
