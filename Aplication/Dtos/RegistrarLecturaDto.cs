namespace api_powergate.Aplication.Dtos
{
    public class RegistrarLecturaDto
    {
        public int CanalId { get; set; }
        public DateTime Timestamp { get; set; }
        public decimal Voltaje { get; set; }
        public decimal Corriente { get; set; }
        public decimal Potencia { get; set; }
        public decimal EnergiaSesion { get; set; }
        public bool Presencia { get; set; }
        public bool ReleActivo { get; set; }
    }
}
