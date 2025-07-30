using api_powergate.Aplication.Dtos.Canal;
using api_powergate.Aplication.Dtos.Dispositivo;
using api_powergate.Common;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;
using api_powergate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace api_powergate.Aplication.Services
{
    public class DispositivoService : IDispositivoService
    {
        private readonly IDispositivoRepository _dispositivoRepository;
        private readonly ApiPowergateContext _context;
        public DispositivoService(IDispositivoRepository dispositivoRepository, ApiPowergateContext apiPowergateContext)
        {
            _dispositivoRepository = dispositivoRepository;
            _context = apiPowergateContext ?? throw new ArgumentNullException(nameof(apiPowergateContext));
        }
        public async Task<Response<EstadoDispositivoDto?>> ObtenerEstado(int dispositivoId)
        {
            var response = new Response<EstadoDispositivoDto?>();

            try
            {
                
                var dispositivo = await _context.Dispositivos
                    .AsNoTracking() 
                    .Include(d => d.CanalDeCargas)
                    .FirstOrDefaultAsync(d => d.Id == dispositivoId);

                
                if (dispositivo == null)
                {
                    response.IsSuccess = false;
                    response.Message = "El Id del dispositivo no existe.";
                    response.Data = null; 
                    return response;
                }

            
                if (dispositivo.CanalDeCargas == null || !dispositivo.CanalDeCargas.Any())
                {
                 
                    response.IsSuccess = false;
                    response.Message = $"El dispositivo con Id {dispositivoId} no tiene canales de carga asociados.";
                    response.Data = null;
                    return response;
              
                }

            
                var lecturasUltimasPorCanal = new List<Lectura>();
                foreach (var canal in dispositivo.CanalDeCargas)
                {
                    var ultimaLectura = await _context.Lecturas
                        .AsNoTracking()
                        .Where(l => l.CanalId == canal.Id)
                        .OrderByDescending(l => l.Timestamp)
                        .FirstOrDefaultAsync(); // Obtiene solo la última lectura para este canal

                    if (ultimaLectura != null)
                    {
                        lecturasUltimasPorCanal.Add(ultimaLectura);
                    }
                }

            
                var potenciaTotal = lecturasUltimasPorCanal.Sum(l => l.Potencia ?? 0);
                var energiaTotalSesion = lecturasUltimasPorCanal.Sum(l => l.EnergiaSesion ?? 0);
                var tienePresencia = lecturasUltimasPorCanal.Any(l => l.Presencia == true);

      
                var dto = new EstadoDispositivoDto
                {
                    DispositivoId = dispositivo.Id,
                    Nombre = dispositivo.Nombre ?? "", 
                    Ubicacion = dispositivo.Ubicacion,
                    PotenciaTotal = potenciaTotal,
                    EnergiaTotalSesion = energiaTotalSesion,
                    TienePresencia = tienePresencia,

                    Canales = dispositivo.CanalDeCargas.Select(c =>
                    {
                        return new CanalEstadoDto
                        {
                            CanalId = c.Id,
                            Nombre = c.Nombre ?? "",
                            ReleActivo = c.Habilitado  
                        };
                    }).ToList()
                  
                };

                response.IsSuccess = true;
                response.Message = "Estado del dispositivo obtenido correctamente.";
                response.Data = dto;

                return response;
            }
            catch (Exception ex)
            {
           
                response.IsSuccess = false;
                response.Message = $"Ocurrió un error inesperado al obtener el estado del dispositivo: {ex.Message}";
              
                response.Data = null;
                return response;
            }
        }
    }


}

