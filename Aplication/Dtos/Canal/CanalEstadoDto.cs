namespace api_powergate.Aplication.Dtos.Canal
{
    public class CanalEstadoDto
    {
        public int CanalId { get; set; }
        public string Nombre { get; set; } = "";
        public decimal Potencia { get; set; }
        public bool ReleActivo { get; set; }
    }
}
