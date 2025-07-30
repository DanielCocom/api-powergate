using api_powergate.Aplication.Dtos;
using api_powergate.Aplication.Dtos.Canal;
using api_powergate.Common;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;
using api_powergate.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Aplication.Services
{
    public class LecturaService : ILecturaService
    {


        private readonly ILecturaRepository _lecturaRepo;
        private readonly ICanalDeCargaRepository _canalRepo;
        private readonly ApiPowergateContext _context;

        public LecturaService(ILecturaRepository lecturaRepo, ICanalDeCargaRepository canalRepo, ApiPowergateContext apiPowergateContext)
        {
            _lecturaRepo = lecturaRepo;
            _canalRepo = canalRepo;
            _context = apiPowergateContext;
        }

        public async Task<Response<bool>> RegistrarLectura(RegistrarLecturaDto dto)
        {

            Response<bool> response = new Response<bool>();
            try
            {
                var canal = await _canalRepo.GetById(dto.CanalId);
                if (canal == null)
                    throw new Exception("Canal no válido o está deshabilitado.");

                // si esta activo no deja pasar la energia
                canal = await _canalRepo.GetById(dto.CanalId);
                if (dto.ReleActivo)
                {
                    canal.Habilitado = false;
                }
                else
                {
                    canal.Habilitado = true;
                }
                await _context.SaveChangesAsync();


                // VALIDAR ANTES DE CREAR EL OBJETO
                var lectura = new Lectura
                {
                    CanalId = dto.CanalId,
                    Timestamp = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, TimeZoneInfo.FindSystemTimeZoneById("Central Standard Time (Mexico)")), // Asignar la fecha y hora actual en UTC
                    Voltaje = dto.Voltaje,
                    Corriente = dto.Corriente,
                    Potencia = dto.Potencia,
                    ReleActivo = dto.ReleActivo

                };

                await _lecturaRepo.AddLectura(lectura);

                response.IsSuccess = true;
                response.Message = "Lectura registrada correctamente.";
                response.Data = true;

                return response;

            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = ex.Message;
                return response;
            }

        }

        public async Task<Response<List<RegistrarLecturaDto>>> ObtenerLecturasPorDispositivoAsync(int dispositivoId)
        {
            var response = new Response<List<RegistrarLecturaDto>>();

            try
            {

                var existeDispositivo = await _context.Dispositivos.AnyAsync(d => d.Id == dispositivoId);
                if (!existeDispositivo)
                {
                    response.IsSuccess = false;
                    response.Message = $"El dispositivo con ID {dispositivoId} no existe.";
                    return response;
                }

                // Consulta de lecturas
                var lecturas = await _context.Lecturas
                    .Where(l => l.Canal.DispositivoId == dispositivoId)
                    .OrderByDescending(l => l.Timestamp)
                    .Select(l => new RegistrarLecturaDto
                    {
                        CanalId = l.CanalId,
                        Voltaje = l.Voltaje ?? 0,
                        Corriente = l.Corriente ?? 0,
                        Potencia = l.Potencia ?? 0,
                        ReleActivo = l.ReleActivo ?? false
                    })
                    .ToListAsync();

                response.Data = lecturas;
                response.IsSuccess = true;
                response.Message = lecturas.Any()
                    ? "Lecturas obtenidas correctamente."
                    : "El dispositivo no tiene lecturas registradas.";

                return response;
            }
            catch (Exception ex)
            {
                // Loguear el error en algún sistema de logging si es posible
                response.IsSuccess = false;
                response.Message = $"Ocurrió un error al obtener las lecturas: {ex.Message}";
                return response;
            }
        }

        public async Task<Response<decimal>> CalcularEnergiaDeLaSesionAsync(int canalId, DateTime inicio, DateTime fin)
        {
            var response = new Response<decimal>();

            try
            {
                var lecturas = await _context.Lecturas
                    .Where(l => l.CanalId == canalId && l.Timestamp >= inicio && l.Timestamp <= fin)
                    .ToListAsync();

                if (!lecturas.Any())
                {
                    response.IsSuccess = false;
                    response.Message = "No se encontraron lecturas en el rango especificado.";
                    return response;
                }

                // Calcular la energía de la sesión como la suma de potencia * tiempo entre lecturas  
                decimal energiaTotal = 0;
                for (int i = 1; i < lecturas.Count; i++)
                {
                    var tiempoHoras = (lecturas[i].Timestamp.Value - lecturas[i - 1].Timestamp.Value).TotalHours;
                    energiaTotal += (lecturas[i - 1].Potencia ?? 0) * (decimal)tiempoHoras;
                }

                response.Data = energiaTotal;
                response.IsSuccess = true;
                response.Message = "Energía calculada correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Ocurrió un error al calcular la energía: {ex.Message}";
                return response;
            }
        }

        public async Task<Response<List<CanalCargaEstadoDto>>> GetEstadoRele(int id_dispositivo)
        {
            Response<List<CanalCargaEstadoDto>> response = new Response<List<CanalCargaEstadoDto>>();

            try
            {
                var canal = await _context.CanalDeCargas
                   .Where(c => c.DispositivoId == id_dispositivo)
                   .Select(c => new CanalCargaEstadoDto
                   {
                       Nombre = c.Nombre,
                       ReleActivo = c.Habilitado ?? false // Asumimos que Habilitado indica si el rele está activo
                   })
                   .ToListAsync();

                if (canal == null)
                {
                    response.IsSuccess = false;
                    response.Message = "No se encontró el canal de carga.";
                    return response;
                }

                response.Data = canal;
                response.IsSuccess = true;
                response.Message = "Estado del rele obtenido correctamente.";
                return response;
            }
            catch (Exception ex)
            {
                response.IsSuccess = false;
                response.Message = $"Error al obtener el estado del rele: {ex.Message}";
                return response;
            }

        }
    }
}
