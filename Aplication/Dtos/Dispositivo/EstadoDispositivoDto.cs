using api_powergate.Aplication.Dtos.Canal;

namespace api_powergate.Aplication.Dtos.Dispositivo
{
    public class EstadoDispositivoDto
    {
        public int DispositivoId { get; set; }
        public string Nombre { get; set; } = "";
        public string? Ubicacion { get; set; }
        public decimal PotenciaTotal { get; set; }
        public decimal EnergiaTotalSesion { get; set; }
        public bool TienePresencia { get; set; }
        public List<CanalEstadoDto> Canales { get; set; } = new();
    }
}
