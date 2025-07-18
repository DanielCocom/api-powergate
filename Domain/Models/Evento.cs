using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Domain.Models;

[Table("evento")]
[Index("CanalId", Name = "canal_id")]
public partial class Evento
{
    [Key]
    [Column("id")]
    public long Id { get; set; }

    [Column("canal_id")]
    public int? CanalId { get; set; }

    [Column("tipo", TypeName = "enum('presencia','nfc','manual','programado')")]
    public string? Tipo { get; set; }

    [Column("timestamp", TypeName = "datetime")]
    public DateTime? Timestamp { get; set; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; set; }

    [ForeignKey("CanalId")]
    [InverseProperty("Eventos")]
    public virtual CanalDeCarga? Canal { get; set; }
}
