using api_powergate.Aplication.Dtos;
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
                if (canal == null || canal.Habilitado == false)
                    throw new Exception("Canal no válido o está deshabilitado.");


                // VALIDAR ANTES DE CREAR EL OBJETO
                var lectura = new Lectura
                {
                    CanalId = dto.CanalId,
                    Timestamp = dto.Timestamp,
                    Voltaje = dto.Voltaje,
                    Corriente = dto.Corriente,
                    Potencia = dto.Potencia,
                    EnergiaSesion = dto.EnergiaSesion,
                    Presencia = dto.Presencia,
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
            // TODO (opcional): iniciar/cerrar sesión de uso
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
                        Timestamp = l.Timestamp ?? DateTime.MinValue,
                        Voltaje = l.Voltaje ?? 0,
                        Corriente = l.Corriente ?? 0,
                        Potencia = l.Potencia ?? 0,
                        EnergiaSesion = l.EnergiaSesion ?? 0,
                        Presencia = l.Presencia ?? false,
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


    }
}
