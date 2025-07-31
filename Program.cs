// Program.cs

using api_powergate.Infrastructure.Data;
using api_powergate.Infrastructure.DependencyInjection;
using api_powergate.Infrastructure.Realtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configuración de ForwardedHeaders para ngrok (y otros proxies/balanceadores de carga como los de Render)
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS ajustado para permitir tu frontend local y, si tuvieras, tu frontend publicado
builder.Services.AddCors(options => {
    options.AddPolicy("AllowSpecificOrigins", policy => { // Cambié el nombre de la política a algo más específico
        policy.WithOrigins(
                "http://127.0.0.1:5500", // <-- ¡AÑADIDO! Tu origen de desarrollo local
                  "http://127.0.0.1:5173", // <-- ¡AÑADIDO! Tu origen de desarrollo local
                "https://api-powergate.onrender.com", // <-- Opcional, pero buena práctica si el frontend está en el mismo dominio o subdominio
                "https://*.ngrok-free.app", // Para pruebas con ngrok
                "http://localhost:5173", // Para desarrollo local
                "https://localhost:*" // Para desarrollo local
            )
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials(); // Obligatorio para SignalR
    });
});

var app = builder.Build();

app.UseForwardedHeaders(); // Middleware para proxies inversos

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseHttpsRedirection(); // Opcional: Comenta si usas ngrok con HTTPS
app.UseCors("AllowSpecificOrigins"); // ¡Asegúrate de usar el nombre de la política que definiste!
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<DeviceHub>("/ws/deviceHub");
app.MapControllers();
app.Run();