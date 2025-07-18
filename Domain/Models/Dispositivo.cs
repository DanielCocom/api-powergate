using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace api_powergate.Domain.Models;

[Table("dispositivo")]
public partial class Dispositivo
{
    [Key]
    [Column("id")]
    public int Id { get; set; }

    [Column("nombre")]
    [StringLength(50)]
    public string? Nombre { get; set; }

    [Column("ubicacion")]
    [StringLength(100)]
    public string? Ubicacion { get; set; }

    [Column("ip_local")]
    [StringLength(45)]
    public string? IpLocal { get; set; }

    [Column("descripcion", TypeName = "text")]
    public string? Descripcion { get; set; }

    [Column("fecha_registro", TypeName = "datetime")]
    public DateTime? FechaRegistro { get; set; }

    [InverseProperty("Dispositivo")]
    public virtual ICollection<CanalDeCarga> CanalDeCargas { get; set; } = new List<CanalDeCarga>();
}
