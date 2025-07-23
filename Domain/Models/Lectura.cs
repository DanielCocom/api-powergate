using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Domain.Models;

[Table("lectura")]
[Index("CanalId", Name = "canal_id")]
public partial class Lectura
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("canal_id")]
    public int CanalId { get; set; }

    [Column("timestamp", TypeName = "datetime")]
    public DateTime? Timestamp { get; set; }

    [Column("voltaje")]
    [Precision(6, 2)]
    public decimal? Voltaje { get; set; }

    [Column("corriente")]
    [Precision(6, 3)]
    public decimal? Corriente { get; set; }

    [Column("potencia")]
    [Precision(8, 2)]
    public decimal? Potencia { get; set; }

    [Column("energia_sesion")]
    [Precision(10, 3)]
    public decimal? EnergiaSesion { get; set; }

    [Column("presencia")]
    public bool? Presencia { get; set; }

    [Column("rele_activo")]
    public bool? ReleActivo { get; set; }

    [ForeignKey("CanalId")]
    [InverseProperty("Lecturas")]
    public virtual CanalDeCarga? Canal { get; set; }
}
