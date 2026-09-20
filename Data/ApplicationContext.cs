using Microsoft.EntityFrameworkCore;
using Workhub.Models;

namespace Workhub.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Reserva> Reservas { get; set; }
    public DbSet<Espacio> Espacios { get; set; }
    public DbSet<Equipamiento> Equipamientos { get; set; }
    public DbSet<Pago> Pagos { get; set; }
    public DbSet<ReservaEquipamiento> ReservaEquipamientos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.ToTable("USUARIO");
            entity.HasKey(e => e.IdUsuario);
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.Nombre).HasColumnName("nombre");
            entity.Property(e => e.Correo).HasColumnName("correo");
            entity.Property(e => e.Telefono).HasColumnName("telefono");

            entity.HasIndex(e => e.Correo).IsUnique();
        });

        modelBuilder.Entity<Espacio>(entity =>
        {
            entity.ToTable("ESPACIO");
            entity.HasKey(e => e.IdEspacio);
            entity.Property(e => e.IdEspacio).HasColumnName("id_espacio");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
            entity.Property(e => e.Capacidad).HasColumnName("capacidad");
            entity.Property(e => e.PrecioHora).HasColumnName("precio_hora").HasPrecision(18, 2);
        });

        modelBuilder.Entity<Reserva>(entity =>
        {
            entity.ToTable("RESERVA");
            entity.HasKey(e => e.IdReserva);
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Estado).HasColumnName("estado");
            entity.Property(e => e.FechaInicio).HasColumnName("fecha_inicio");
            entity.Property(e => e.FechaFin).HasColumnName("fecha_fin");
            entity.Property(e => e.IdUsuario).HasColumnName("id_usuario");
            entity.Property(e => e.IdEspacio).HasColumnName("id_espacio");

            entity.HasOne(e => e.Usuario)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.IdUsuario)
                .IsRequired();

            entity.HasOne(e => e.Espacio)
                .WithMany(e => e.Reservas)
                .HasForeignKey(e => e.IdEspacio)
                .IsRequired();

            
            entity.HasOne(e => e.Pago)
                .WithOne(e => e.Reserva)
                .HasForeignKey<Pago>(e => e.IdReserva)
                .IsRequired();
        });

        modelBuilder.Entity<Equipamiento>(entity =>
        {
            entity.ToTable("EQUIPAMIENTO");
            entity.HasKey(e => e.IdEquipo);
            entity.Property(e => e.IdEquipo).HasColumnName("id_equipo");
            entity.Property(e => e.Nombre).HasColumnName("nombre");
            entity.Property(e => e.Tipo).HasColumnName("tipo");
            entity.Property(e => e.PrecioHora).HasColumnName("precio_hora").HasPrecision(18, 2);
        });

        modelBuilder.Entity<Pago>(entity =>
        {
            entity.ToTable("PAGO");
            entity.HasKey(e => e.IdPago);
            entity.Property(e => e.IdPago).HasColumnName("id_pago");
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.Monto).HasColumnName("monto").HasPrecision(18, 2);
            entity.Property(e => e.MetodoPago).HasColumnName("metodo_pago");
            entity.Property(e => e.FechaPago).HasColumnName("fecha_pago");
        });

        modelBuilder.Entity<ReservaEquipamiento>(entity =>
        {
            entity.ToTable("INCLUYE");
            entity.HasKey(e => new { e.IdReserva, e.IdEquipo });
            entity.Property(e => e.IdReserva).HasColumnName("id_reserva");
            entity.Property(e => e.IdEquipo).HasColumnName("id_equipo");
            entity.Property(e => e.PrecioHora).HasColumnName("precio_hora").HasPrecision(18, 2);
            entity.Property(e => e.Cantidad).HasColumnName("cantidad");
            entity.Property(e => e.FechaHora).HasColumnName("fecha_hora");

            entity.HasOne(e => e.Reserva)
                .WithMany(e => e.ReservaEquipamientos)
                .HasForeignKey(e => e.IdReserva)
                .IsRequired();

            entity.HasOne(e => e.Equipamiento)
                .WithMany(e => e.ReservaEquipamientos)
                .HasForeignKey(e => e.IdEquipo)
                .IsRequired();
        });
    }
}
