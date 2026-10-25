using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.Services;

namespace Workhub.Data;

public static class DatosDemoSeeder
{
    public static async Task InicializarAsync(IServiceProvider services, IConfiguration configuration,
        IWebHostEnvironment environment, CancellationToken ct = default)
    {
        if (!environment.IsDevelopment() || !configuration.GetValue<bool>("DatosDemo:Habilitado")) return;

        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        var passwords = scope.ServiceProvider.GetRequiredService<IPasswordService>();
        var costos = scope.ServiceProvider.GetRequiredService<ICostoService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatosDemo");
        var connection = new SqlConnectionStringBuilder(db.Database.GetConnectionString());
        var permitida = configuration["DatosDemo:BasePermitida"] ?? "WorkHubDemoDb";
        if (!string.Equals(connection.InitialCatalog, permitida, StringComparison.OrdinalIgnoreCase))
        {
            // Cambiar la conexión a una base real nunca autoriza a crear tablas o insertar ejemplos.
            logger.LogWarning("Carga demo omitida: la conexión no apunta a la base de demostración permitida.");
            return;
        }

        // Crea únicamente el esquema de la base demo si aún no existe. Nunca elimina tablas/datos.
        // El bloqueo en master también serializa dos primeros arranques simultáneos.
        var masterBuilder = new SqlConnectionStringBuilder(connection.ConnectionString) { InitialCatalog = "master" };
        await using var master = new SqlConnection(masterBuilder.ConnectionString);
        await master.OpenAsync(ct);
        await using var schemaLock = master.CreateCommand();
        schemaLock.CommandText = """
            DECLARE @resultado int;
            EXEC @resultado = sys.sp_getapplock @Resource=@recurso,
                @LockMode='Exclusive', @LockOwner='Session', @LockTimeout=60000;
            IF @resultado < 0 THROW 50010, 'No se pudo bloquear el inicio de la demo. Reintente.', 1;
            """;
        schemaLock.Parameters.AddWithValue("@recurso", $"WorkHub.Inicializar.{permitida}");
        schemaLock.CommandTimeout = 70;
        await schemaLock.ExecuteNonQueryAsync(ct);
        try
        {
            await db.Database.EnsureCreatedAsync(ct);
            await EsquemaWorkHub.ActualizarAsync(services, ct);
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.Serializable, ct);
            // Una sola carga inicial. No rellena automáticamente registros que alguien eliminó.
            if (await db.Usuarios.AnyAsync(ct) || await db.Espacios.AnyAsync(ct) || await db.Equipamientos.AnyAsync(ct))
            {
                logger.LogInformation("La base demo ya contiene datos. No se insertó ni se sobrescribió ningún registro.");
                return;
            }

            var admin = new Usuario
            {
                Nombre = "Administrador WorkHub", Correo = CredencialesDemo.CorreoAdmin,
                Rol = RolUsuario.Admin, Telefono = "70000000", PasswordHash = passwords.Hash(CredencialesDemo.PasswordAdmin)
            };
            var clientes = new UsuarioFaker().Generate(18);
            for (var i = 0; i < clientes.Count; i++)
            {
                clientes[i].Correo = $"cliente{i + 1:00}{CredencialesDemo.Dominio}";
                clientes[i].PasswordHash = passwords.Hash(CredencialesDemo.PasswordCliente);
            }
            db.Usuarios.Add(admin);
            db.Usuarios.AddRange(clientes);

            var espacios = new List<Espacio>
            {
                new() { Codigo="ESC-01", Nombre="Escritorio Roble", Tipo=TipoEspacio.EscritorioFlexible, Capacidad=1, PrecioHora=15m },
                new() { Codigo="ESC-02", Nombre="Escritorio Nube", Tipo=TipoEspacio.EscritorioFlexible, Capacidad=1, PrecioHora=18m },
                new() { Codigo="ESC-03", Nombre="Escritorio Ventana", Tipo=TipoEspacio.EscritorioFlexible, Capacidad=1, PrecioHora=20m },
                new() { Codigo="OFI-01", Nombre="Oficina Andina", Tipo=TipoEspacio.OficinaPrivada, Capacidad=4, PrecioHora=45m },
                new() { Codigo="OFI-02", Nombre="Oficina Horizonte", Tipo=TipoEspacio.OficinaPrivada, Capacidad=6, PrecioHora=60m },
                new() { Codigo="SAL-01", Nombre="Sala Illimani", Tipo=TipoEspacio.SalaReuniones, Capacidad=8, PrecioHora=80m },
                new() { Codigo="SAL-02", Nombre="Sala Encuentro", Tipo=TipoEspacio.SalaReuniones, Capacidad=12, PrecioHora=100m },
                new() { Codigo="SAL-03", Nombre="Sala Creativa", Tipo=TipoEspacio.SalaReuniones, Capacidad=6, PrecioHora=65m }
            };
            var equipos = new List<Equipamiento>
            {
                new() { Codigo="EQ-01", Nombre="Proyector Full HD", Tipo="Audiovisual", PrecioHora=10m },
                new() { Codigo="EQ-02", Nombre="Pizarra móvil", Tipo="Mobiliario", PrecioHora=3m },
                new() { Codigo="EQ-03", Nombre="Kit de videoconferencia", Tipo="Audiovisual", PrecioHora=15m },
                new() { Codigo="EQ-04", Nombre="Monitor de 27 pulgadas", Tipo="Informática", PrecioHora=8m },
                new() { Codigo="EQ-05", Nombre="Micrófono USB", Tipo="Audio", PrecioHora=5m },
                new() { Codigo="EQ-06", Nombre="Parlante portátil", Tipo="Audio", PrecioHora=6m }
            };
            db.Espacios.AddRange(espacios);
            db.Equipamientos.AddRange(equipos);
            await db.SaveChangesAsync(ct);

            var hoy = RelojBolivia.Ahora.Date;
            var reservas = new ReservaFaker(clientes, espacios).Generate(60);
            for (var i = 0; i < reservas.Count; i++)
            {
                var r = reservas[i];
                var pasada = i < 48;
                var inicio = pasada
                    ? hoy.AddDays(-(i / 2 + 1)).AddHours(i % 2 == 0 ? 9 : 14)
                    : hoy.AddDays((i - 48) / 3 + 1).AddHours(9 + (i - 48) % 3 * 3);
                r.Folio = $"DEMO-{i + 1:0000}";
                r.IdUsuario = r.Usuario.IdUsuario;
                r.IdEspacio = r.Espacio.IdEspacio;
                r.FechaInicio = inicio;
                r.FechaFin = inicio.AddMinutes(60 + i % 4 * 30);
                r.FechaCreacion = pasada ? inicio.AddDays(-5) : hoy.AddDays(-3).AddHours(8);
                r.Estado = pasada
                    ? (i % 4 < 2 ? EstadoReserva.Cancelada : EstadoReserva.Completada)
                    : (i % 3 == 0 ? EstadoReserva.Pendiente : i % 3 == 1 ? EstadoReserva.Confirmada : EstadoReserva.Cancelada);
                for (var j = 0; j < i % 3; j++)
                {
                    var equipo = equipos[(i + j) % equipos.Count];
                    r.ReservaEquipamientos.Add(new ReservaEquipamiento
                    {
                        IdEquipo = equipo.IdEquipo, Cantidad = 1 + (i + j) % 2,
                        PrecioHora = equipo.PrecioHora, FechaHora = r.FechaCreacion
                    });
                }
                r.CostoTotal = costos.CalcularCostoTotal(r);
                // Una reserva confirmada/completada tiene exactamente un pago aprobado.
                // Algunas canceladas mantienen un comprobante reembolsado; pendientes no tienen pago.
                if (r.Estado is EstadoReserva.Confirmada or EstadoReserva.Completada || (pasada && i % 4 == 1))
                {
                    r.Pago = new Pago
                    {
                        CodigoTransaccion = $"DEMO-PAGO-{i + 1:0000}", Monto = r.CostoTotal,
                        MetodoPago = (MetodoPago)(1 + (i / 4 + i) % 4),
                        Estado = r.Estado == EstadoReserva.Cancelada ? EstadoPago.Reembolsado : EstadoPago.Aprobado,
                        FechaPago = r.FechaCreacion.AddDays(1)
                    };
                }
            }
            db.Reservas.AddRange(reservas);
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            logger.LogInformation("Demo preparada: 1 administrador, 18 clientes, 8 espacios, 6 equipos y 60 reservas con sus pagos y equipos.");
        }
        finally
        {
            await using var release = master.CreateCommand();
            release.CommandText = "EXEC sys.sp_releaseapplock @Resource=@recurso, @LockOwner='Session';";
            release.Parameters.AddWithValue("@recurso", $"WorkHub.Inicializar.{permitida}");
            await release.ExecuteNonQueryAsync(CancellationToken.None);
        }
    }
}
