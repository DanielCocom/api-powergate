using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Domain.Models;

[Table("configuracion_canal")]
[Index("CanalId", Name = "canal_id")]
public partial class ConfiguracionCanal
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("canal_id")]
    public int? CanalId { get; set; }

    [Column("requiere_presencia")]
    public bool? RequierePresencia { get; set; }

    [Column("requiere_nfc")]
    public bool? RequiereNfc { get; set; }

    [Column("intervalo_muestreo_segundos")]
    public int? IntervaloMuestreoSegundos { get; set; }

    [ForeignKey("CanalId")]
    [InverseProperty("ConfiguracionCanals")]
    public virtual CanalDeCarga? Canal { get; set; }
}
