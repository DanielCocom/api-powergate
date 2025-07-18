using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Domain.Models;

[Table("programacion_horaria")]
[Index("CanalId", Name = "canal_id")]
public partial class ProgramacionHorarium
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("canal_id")]
    public int? CanalId { get; set; }

    [Column("hora_inicio", TypeName = "time")]
    public TimeOnly? HoraInicio { get; set; }

    [Column("hora_fin", TypeName = "time")]
    public TimeOnly? HoraFin { get; set; }

    [Column("dias_activos")]
    [StringLength(20)]
    public string? DiasActivos { get; set; }

    [Column("activo")]
    public bool? Activo { get; set; }

    [ForeignKey("CanalId")]
    [InverseProperty("ProgramacionHoraria")]
    public virtual CanalDeCarga? Canal { get; set; }
}
