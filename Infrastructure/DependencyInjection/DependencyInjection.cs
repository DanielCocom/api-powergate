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
            // Configurar EF Core
            services.AddDbContext<ApiPowergateContext>(options =>
                options.UseMySql(
                    configuration.GetConnectionString("DefaultConnection"),
                    new MySqlServerVersion(new Version(9, 3, 0))
                )
            );

            // Repositorios
            services.AddScoped<ILecturaRepository, LecturaRepository>();
            services.AddScoped<IUsuarioRepository, UsuarioRepository>();

            services.AddScoped<ICanalDeCargaRepository, CanalDeCargaRepository>();

            // Servicios de aplicación
            services.AddTransient<ILecturaService,LecturaService>();
            services.AddTransient<AuthService>();

            return services;
        }
    }
}
