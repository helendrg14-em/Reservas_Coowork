using Microsoft.EntityFrameworkCore;
using Workhub.Models;

namespace Workhub.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Reserva> Reservas => Set<Reserva>();
    public DbSet<Espacio> Espacios => Set<Espacio>();
    public DbSet<Equipamiento> Equipamientos => Set<Equipamiento>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<ReservaEquipamiento> ReservaEquipamientos => Set<ReservaEquipamiento>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // ===== USUARIO =====
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Correo).HasColumnName("correo").HasMaxLength(150).IsRequired();
            entity.Property(e => e.Telefono).HasColumnName("telefono").HasMaxLength(20).IsRequired();
            entity.Property(e => e.PasswordHash).HasColumnName("password_hash").IsRequired();

            entity.HasIndex(e => e.Correo).IsUnique();
        });

        // ===== ESPACIO =====
        modelBuilder.Entity<Espacio>(entity =>
        {
            entity.ToTable("ESPACIO");
            entity.HasKey(e => e.IdEspacio);
            entity.Property(e => e.IdEspacio).HasColumnName("id_espacio");
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Tipo).HasColumnName("tipo").HasConversion<int>();
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.PrecioHora).HasColumnName("precio_hora").HasPrecision(10, 2);

            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        // ===== EQUIPAMIENTO =====
        modelBuilder.Entity<Equipamiento>(entity =>
        {
            entity.ToTable("EQUIPAMIENTO");
            entity.HasKey(e => e.IdEquipo);
            entity.Property(e => e.IdEquipo).HasColumnName("id_equipo");
            entity.Property(e => e.Codigo).HasColumnName("codigo").HasMaxLength(20).IsRequired();
            entity.Property(e => e.Nombre).HasColumnName("nombre").HasMaxLength(100).IsRequired();
            entity.Property(e => e.Tipo).HasColumnName("tipo").HasMaxLength(50).IsRequired();
            entity.Property(e => e.PrecioHora).HasColumnName("precio_hora").HasPrecision(10, 2);

            entity.HasIndex(e => e.Codigo).IsUnique();
        });

        // ===== RESERVA =====
        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("RESERVA");
            entity.HasKey(e => e.IdReserva);
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Folio).HasColumnName("folio").HasMaxLength(30).IsRequired();
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.FechaCreacion).HasColumnName("fecha_creacion");
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<int>();
            entity.Property(e => e.CostoTotal).HasColumnName("costo_total").HasPrecision(10, 2);
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdEspacio).HasColumnName("id_espacio");

            entity.HasIndex(e => e.Folio).IsUnique();
            entity.HasIndex(e => new { e.IdEspacio, e.FechaInicio, e.FechaFin });

            entity.HasOne(e => e.Usuario)
                .WithMany(u => u.Reservas)
                .HasForeignKey(e => e.IdUsuario)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne(e => e.Espacio)
                .WithMany(s => s.Reservas)
                .HasForeignKey(e => e.IdEspacio)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();

            entity.HasOne(e => e.Pago)
                .WithOne(p => p.Reserva)
                .HasForeignKey<Pago>(p => p.IdReserva)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // ===== PAGO =====
        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("PAGO");
            entity.HasKey(e => e.IdPago);
            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.CodigoTransaccion).HasColumnName("codigo_transaccion").HasMaxLength(40).IsRequired();
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Monto).HasColumnName("monto").HasPrecision(10, 2);
            entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago").HasConversion<int>();
            entity.Property(e => e.Estado).HasColumnName("estado").HasConversion<int>();
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");

            entity.HasIndex(e => e.CodigoTransaccion).IsUnique();
            entity.HasIndex(e => e.IdReserva).IsUnique();
        });

        // ===== RESERVA_EQUIPAMIENTO (INCLUYE) =====
        modelBuilder.Entity<ReservaEquipamiento>(entity =>
        {
            entity.ToTable("INCLUYE");
            entity.HasKey(e => new { e.IdReserva, e.IdEquipo });
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.IdEquipo).HasColumnName("id_equipo");
            entity.Property(e => e.PrecioHora).HasColumnName("precio_hora").HasPrecision(10, 2);
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.FechaHora).HasColumnName("fecha_hora");

            entity.HasOne(e => e.Reserva)
                .WithMany(r => r.ReservaEquipamientos)
                .HasForeignKey(e => e.IdReserva)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired();

            entity.HasOne(e => e.Equipamiento)
                .WithMany(eq => eq.ReservaEquipamientos)
                .HasForeignKey(e => e.IdEquipo)
                .OnDelete(DeleteBehavior.Restrict)
                .IsRequired();
        });
    }
}
