// Program.cs

using api_powergate.Infrastructure.Data;
using api_powergate.Infrastructure.DependencyInjection;
using api_powergate.Infrastructure.Realtime;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.HttpOverrides;
using System.Net.WebSockets;

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
                       "http://127.0.0:*", // Para desarrollo local
                       "https://localhost:*", // Para desarrollo local
                       "http://*:*" // <-- ¡AÑADIDO! Escuchar desde todos los puertos
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


app.UseWebSockets(new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromSeconds(60), // Envía un ping cada 60 segundos si no hay actividad
    // Puedes ajustar este valor
});
// Configura el endpoint para ESP32
app.Use(async (context, next) =>
{
    if (context.Request.Path == "/ws/esp32" && context.WebSockets.IsWebSocketRequest)
    {
        var deviceId = context.Request.Query["deviceId"];
        if (string.IsNullOrEmpty(deviceId))
        {
            context.Response.StatusCode = 400;
            return;
        }

        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var manager = context.RequestServices.GetRequiredService<Esp32WebSocketManager>();
        await manager.HandleConnectionAsync(webSocket, deviceId);
    }
    else if (context.Request.Path == "/ws/web" && context.WebSockets.IsWebSocketRequest)
    {
        var webSocket = await context.WebSockets.AcceptWebSocketAsync();
        var manager = context.RequestServices.GetRequiredService<api_powergate.Infrastructure.Realtime.WebSocketManager>();

        // Manejar suscripción
        var deviceId = context.Request.Query["deviceId"];
        manager.Subscribe(deviceId, webSocket);

        // Mantener conexión abierta
        var buffer = new byte[1024];
        while (webSocket.State == WebSocketState.Open)
        {
            await webSocket.ReceiveAsync(
                new ArraySegment<byte>(buffer),
                CancellationToken.None);
        }

        manager.Unsubscribe(deviceId, webSocket);
    }
    else
    {
        await next();
    }
});



app.MapControllers();
app.Run();