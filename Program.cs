using api_powergate.Infrastructure.Data;
using api_powergate.Infrastructure.DependencyInjection;
using api_powergate.Infrastructure.Realtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configuración de ForwardedHeaders para ngrok
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

// CORS ajustado para ngrok
builder.Services.AddCors(options => {
    options.AddPolicy("AllowAll", policy => {
        policy.WithOrigins(
                "https://*.ngrok-free.app", // Todos los subdominios ngrok
    "http://localhost:*", // Cualquier puerto local
    "https://localhost:*"
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
app.UseCors("AllowAll"); // ¡Antes de UseAuthorization!
app.UseAuthentication();
app.UseAuthorization();

app.MapHub<DeviceHub>("/ws/deviceHub");
app.MapControllers();
app.Run();