using api_powergate.Aplication.Dtos.Canal;
using api_powergate.Aplication.Dtos.Dispositivo;
using api_powergate.Common;
using api_powergate.Domain.Interfaces;
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
        public async  Task <Response<EstadoDispositivoDto?>> ObtenerEstado(int dispositivoId)
        {
            var response = new Response<EstadoDispositivoDto?>();
            try
            {
                var dispositivo = await _context.Dispositivos
            .Include(d => d.CanalDeCargas)
            .FirstOrDefaultAsync(d => d.Id == dispositivoId);

                if (dispositivo == null)
                    return null;

                var canalIds = dispositivo.CanalDeCargas.Select(c => c.Id).ToList();

                var lecturas = await _context.Lecturas
                    .Where(l => canalIds.Contains(l.CanalId))
                    .GroupBy(l => l.CanalId)
                    .Select(g => g.OrderByDescending(l => l.Timestamp).First())
                    .ToListAsync();

                var dto = new EstadoDispositivoDto
                {
                    DispositivoId = dispositivo.Id,
                    Nombre = dispositivo.Nombre ?? "",
                    Ubicacion = dispositivo.Ubicacion,
                    PotenciaTotal = lecturas.Sum(l => l.Potencia ?? 0),
                    EnergiaTotalSesion = lecturas.Sum(l => l.EnergiaSesion ?? 0),
                    TienePresencia = lecturas.Any(l => l.Presencia == true),
                    Canales = dispositivo.CanalDeCargas.Select(c =>
                    {
                        var lectura = lecturas.FirstOrDefault(l => l.CanalId == c.Id);
                        return new CanalEstadoDto
                        {
                            CanalId = c.Id,
                            Nombre = c.Nombre ?? "",
                            Potencia = lectura?.Potencia ?? 0,
                            ReleActivo = lectura?.ReleActivo == true
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
                response.Message = ex.Message;
                response.Data = null;
                return response;
            }
        }

        
    }
}
