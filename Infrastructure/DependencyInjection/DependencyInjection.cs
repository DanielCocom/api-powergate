using api_powergate.Aplication.Services;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Interfaces.ws;
using api_powergate.Infrastructure.Data;
using api_powergate.Infrastructure.Realtime;
using api_powergate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using WebSocketManager = api_powergate.Infrastructure.Realtime.WebSocketManager;

namespace api_powergate.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            Console.WriteLine($"Cadena de conexión utilizada: {connectionString}");

            services.AddDbContext<ApiPowergateContext>(options =>
                options.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString),
                    mySqlOptions =>
                    {
                        mySqlOptions.EnableRetryOnFailure(
                            maxRetryCount: 10,
                            maxRetryDelay: TimeSpan.FromSeconds(30),
                            errorNumbersToAdd: null
                        );
                    }));

            services.AddCors(options =>
            {
                options.AddPolicy("PermitirTodo", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Repositorios
            services.AddScoped<ILecturaRepository, LecturaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();
            services.AddScoped<IDispositivoRepository, DispositivoRepository>();
            services.AddScoped<ICanalDeCargaRepository, CanalDeCargaRepository>();
            services.AddSingleton<Esp32WebSocketManager>();
            services.AddSingleton<WebSocketManager>();
            services.AddSingleton<IRelayCommander, HybridRelayCommander>();

            // Servicios de aplicación
            services.AddTransient<ILecturaService, LecturaService>();
            services.AddTransient<IDispositivoService, DispositivoService>();
            services.AddTransient<AuthService>();


            // Realtime
            services.AddSignalR();
            services.AddSingleton<IDeviceConnectionTracker, Esp32WebSocketManager>();
            services.AddScoped<IRelayCommander, WebSocketRelayCommander>();

            // Services de application
            services.AddScoped<CanalCargaService>();

            return services;
        }
    }
}
