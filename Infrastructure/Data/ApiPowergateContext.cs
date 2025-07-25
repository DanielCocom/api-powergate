using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Pomelo.EntityFrameworkCore.MySql.Scaffolding.Internal;
using api_powergate.Domain.Models;

namespace api_powergate.Infrastructure.Data;

public partial class ApiPowergateContext : DbContext
{
    public ApiPowergateContext()
    {
    }

    public ApiPowergateContext(DbContextOptions<ApiPowergateContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CanalDeCarga> CanalDeCargas { get; set; }

    public virtual DbSet<ConfiguracionCanal> ConfiguracionCanals { get; set; }

    public virtual DbSet<Dispositivo> Dispositivos { get; set; }

    public virtual DbSet<Evento> Eventos { get; set; }

    public virtual DbSet<Lectura> Lecturas { get; set; }

    public virtual DbSet<ProgramacionHorarium> ProgramacionHoraria { get; set; }

    public virtual DbSet<SesionUso> SesionUsos { get; set; }

    public virtual DbSet<Usuario> Usuarios { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder
            .UseCollation("utf8mb4_0900_ai_ci")
            .HasCharSet("utf8mb4");

        modelBuilder.Entity<CanalDeCarga>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Habilitado).HasDefaultValueSql("'1'");

            entity.HasOne(d => d.Dispositivo).WithMany(p => p.CanalDeCargas).HasConstraintName("canal_de_carga_ibfk_1");
        });

        modelBuilder.Entity<ConfiguracionCanal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.IntervaloMuestreoSegundos).HasDefaultValueSql("'10'");
            entity.Property(e => e.RequiereNfc).HasDefaultValueSql("'0'");
            entity.Property(e => e.RequierePresencia).HasDefaultValueSql("'0'");

            entity.HasOne(d => d.Canal).WithMany(p => p.ConfiguracionCanals).HasConstraintName("configuracion_canal_ibfk_1");
        });

        modelBuilder.Entity<Dispositivo>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        modelBuilder.Entity<Evento>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Canal).WithMany(p => p.Eventos).HasConstraintName("evento_ibfk_1");
        });

        modelBuilder.Entity<Lectura>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Canal).WithMany(p => p.Lecturas).HasConstraintName("lectura_ibfk_1");
        });

        modelBuilder.Entity<ProgramacionHorarium>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.Activo).HasDefaultValueSql("'1'");

            entity.HasOne(d => d.Canal).WithMany(p => p.ProgramacionHoraria).HasConstraintName("programacion_horaria_ibfk_1");
        });

        modelBuilder.Entity<SesionUso>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.HasOne(d => d.Canal).WithMany(p => p.SesionUsos).HasConstraintName("sesion_uso_ibfk_1");
        });

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PRIMARY");

            entity.Property(e => e.FechaRegistro).HasDefaultValueSql("CURRENT_TIMESTAMP");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
