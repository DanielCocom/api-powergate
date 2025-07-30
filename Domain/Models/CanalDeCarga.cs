using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Domain.Models;

[Table("canal_de_carga")]
[Index("DispositivoId", Name = "dispositivo_id")]
public partial class CanalDeCarga
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("dispositivo_id")]
    public int DispositivoId { get; set; }

    [Column("nombre")]
    [StringLength(50)]
    public string? Nombre { get; set; }

    [Column("pin_rele")]
    public int? PinRele { get; set; }

    [Column("pin_sensor")]
    public int? PinSensor { get; set; }

    [Column("habilitado")]
    public bool? Habilitado { get; set; }

    [InverseProperty("Canal")]
    public virtual ICollection<ConfiguracionCanal> ConfiguracionCanals { get; set; } = new List<ConfiguracionCanal>();

    [ForeignKey("DispositivoId")]
    [InverseProperty("CanalDeCargas")]
    public virtual Dispositivo? Dispositivo { get; set; }

    [InverseProperty("Canal")]
    public virtual ICollection<Evento> Eventos { get; set; } = new List<Evento>();

    [InverseProperty("Canal")]
    public virtual ICollection<Lectura> Lecturas { get; set; } = new List<Lectura>();

    [InverseProperty("Canal")]
    public virtual ICollection<ProgramacionHorarium> ProgramacionHoraria { get; set; } = new List<ProgramacionHorarium>();

    [InverseProperty("Canal")]
    public virtual ICollection<SesionUso> SesionUsos { get; set; } = new List<SesionUso>();
}
