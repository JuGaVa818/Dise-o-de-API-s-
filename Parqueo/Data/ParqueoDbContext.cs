using Microsoft.EntityFrameworkCore;

namespace Parqueo.Data;

public sealed class ParqueoDbContext(DbContextOptions<ParqueoDbContext> options)
    : DbContext(options)
{
    public DbSet<Automovil> Automoviles => Set<Automovil>();
    public DbSet<Parqueo> Parqueos => Set<Parqueo>();
    public DbSet<IngresoAutomovil> IngresosAutomoviles => Set<IngresoAutomovil>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Automovil>(entity =>
        {
            entity.ToTable("PRQ_Automoviles", table =>
            {
                table.HasCheckConstraint(
                    "CK_PRQ_Automoviles_anio", "anio BETWEEN 1886 AND 2100");
                table.HasCheckConstraint(
                    "CK_PRQ_Automoviles_tipo", "tipo IN ('sedán', '4x4', 'moto')");
            });
            entity.HasKey(item => item.Id).HasName("PK_PRQ_Automoviles");
            entity.Property(item => item.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(item => item.Color).HasColumnName("color").HasMaxLength(40).IsRequired();
            entity.Property(item => item.Anio).HasColumnName("anio").HasColumnType("smallint");
            entity.Property(item => item.Fabricante).HasColumnName("fabricante").HasMaxLength(80).IsRequired();
            entity.Property(item => item.Tipo).HasColumnName("tipo").HasMaxLength(20).IsRequired();
        });

        modelBuilder.Entity<Parqueo>(entity =>
        {
            entity.ToTable("PRQ_Parqueo", table =>
                table.HasCheckConstraint(
                    "CK_PRQ_Parqueo_precio_por_hora", "precio_por_hora >= 0"));
            entity.HasKey(item => item.Id).HasName("PK_PRQ_Parqueo");
            entity.Property(item => item.Id).HasColumnName("id").ValueGeneratedOnAdd();
            entity.Property(item => item.NombreProvincia)
                .HasColumnName("nombre_provincia").HasMaxLength(80).IsRequired();
            entity.Property(item => item.Nombre).HasColumnName("nombre").HasMaxLength(120).IsRequired();
            entity.Property(item => item.PrecioPorHora)
                .HasColumnName("precio_por_hora").HasPrecision(10, 2);
        });

        modelBuilder.Entity<IngresoAutomovil>(entity =>
        {
            entity.ToTable("PRQ_Ingreso_Automoviles", table =>
                table.HasCheckConstraint(
                    "CK_PRQ_Ingreso_Automoviles_fechas",
                    "fecha_hora_salida IS NULL OR fecha_hora_salida >= fecha_hora_entrada"));
            entity.HasKey(item => item.Consecutivo).HasName("PK_PRQ_Ingreso_Automoviles");
            entity.Property(item => item.Consecutivo)
                .HasColumnName("consecutivo").ValueGeneratedOnAdd();
            entity.Property(item => item.IdParqueo).HasColumnName("id_parqueo");
            entity.Property(item => item.IdAutomovil).HasColumnName("id_automovil");
            entity.Property(item => item.FechaHoraEntrada)
                .HasColumnName("fecha_hora_entrada").HasColumnType("timestamp without time zone");
            entity.Property(item => item.FechaHoraSalida)
                .HasColumnName("fecha_hora_salida").HasColumnType("timestamp without time zone");
            entity.Ignore(item => item.DuracionEstadiaMinutos);
            entity.Ignore(item => item.DuracionEstadiaHoras);
            entity.Ignore(item => item.MontoTotalPagar);
            entity.HasOne(item => item.Parqueo)
                .WithMany(item => item.Ingresos)
                .HasForeignKey(item => item.IdParqueo)
                .HasConstraintName("FK_PRQ_Ingreso_Automoviles_PRQ_Parqueo")
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(item => item.Automovil)
                .WithMany(item => item.Ingresos)
                .HasForeignKey(item => item.IdAutomovil)
                .HasConstraintName("FK_PRQ_Ingreso_Automoviles_PRQ_Automoviles")
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}
