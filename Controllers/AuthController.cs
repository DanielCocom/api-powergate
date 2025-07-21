using api_powergate.Aplication.Services;
using Microsoft.AspNetCore.Mvc;
using api_powergate.Aplication.Dtos;

namespace api_powergate.Controllers
{
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] AuthDtos.LoginRequest loginDto)
        {
            var response = await _authService.LoginUsuario(loginDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return Unauthorized(response);
            }
        }
        [HttpPost("registro")]
        public async Task<IActionResult> registro([FromBody] AuthDtos.RegisterRequest loginDto)
        {
            var response = await _authService.RegistrarUsuario(loginDto);
            if (response.IsSuccess)
            {
                return Ok(response);
            }
            else
            {
                return Unauthorized(response);
            }
        }


    }
}
