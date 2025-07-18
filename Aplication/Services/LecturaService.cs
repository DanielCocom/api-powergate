using api_powergate.Aplication.Dtos;
using api_powergate.Domain.Interfaces;
using api_powergate.Domain.Models;

namespace api_powergate.Aplication.Services
{
    public class LecturaService
    {


        private readonly ILecturaRepository _lecturaRepo;
        private readonly ICanalDeCargaRepository _canalRepo;

        public LecturaService(ILecturaRepository lecturaRepo, ICanalDeCargaRepository canalRepo)
        {
            _lecturaRepo = lecturaRepo;
            _canalRepo = canalRepo;
        }

        public async Task<long> RegistrarLecturaAsync(RegistrarLecturaDto dto)
        {
            var canal = await _canalRepo.GetById(dto.CanalId);
            if (canal == null || canal.Habilitado == false)
                throw new Exception("Canal no válido o está deshabilitado.");

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

            // TODO (opcional): iniciar/cerrar sesión de uso

            return lectura.Id;
        }

    }
}
