using System.ComponentModel.DataAnnotations;

namespace api_powergate.Aplication.Dtos
{
    public class AuthDtos
    {
        
        public class RegisterRequest
        {
            [Required(ErrorMessage = "El correo es requerido.")]
            public string correo { get; set; }
            [Required(ErrorMessage = "La contraseña es requerida.")]
            [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres.")]
            public string contrasena { get; set; }
        }

        
        public class LoginRequest
        {
            [Required(ErrorMessage = "El correo es necesario.")]
            public string correo { get; set; }

            [Required(ErrorMessage = "La contraseña es requerida.")]
            public string contrasena { get; set; }
        }
        public class AuthResponse
        {
            public string Username { get; set; }
            public string Message { get; set; }
            public string Token { get; set; } // Aquí iría el token JWT
        }

    }
}
