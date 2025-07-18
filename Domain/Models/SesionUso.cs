using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Domain.Models;

[Table("sesion_uso")]
[Index("CanalId", Name = "canal_id")]
public partial class SesionUso
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("canal_id")]
    public int? CanalId { get; set; }

    [Column("inicio", TypeName = "datetime")]
    public DateTime? Inicio { get; set; }

    [Column("fin", TypeName = "datetime")]
    public DateTime? Fin { get; set; }

    [Column("energia_consumida")]
    [Precision(10, 3)]
    public decimal? EnergiaConsumida { get; set; }

    [ForeignKey("CanalId")]
    [InverseProperty("SesionUsos")]
    public virtual CanalDeCarga? Canal { get; set; }
}
