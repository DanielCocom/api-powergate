using api_powergate.Aplication.Services;
using api_powergate.Domain.Interfaces;
using api_powergate.Infrastructure.Data;
using api_powergate.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using System;

namespace api_powergate.Infrastructure.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("ApiPowergateDbConnection");
            Console.WriteLine($"Cadena de conexión utilizada: {connectionString}");

            services.AddDbContext<ApiPowergateContext>(options =>
                options.UseMySql(
                    connectionString,
                    new MySqlServerVersion(new Version(9, 3, 0))
                )
            );
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

            // Servicios de aplicación
            services.AddTransient<ILecturaService,LecturaService>();
            services.AddTransient<IDispositivoService, DispositivoService>();
            services.AddTransient<AuthService>();

            return services;
        }
    }
}
