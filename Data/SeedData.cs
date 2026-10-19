using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.Services;

namespace Workhub.Data;

public static class SeedData
{
    public static void Initialize(IServiceProvider serviceProvider)
    {
        var db = serviceProvider.GetService(typeof(ApplicationContext)) as ApplicationContext;
        var passwords = serviceProvider.GetService(typeof(IPasswordService)) as IPasswordService;
        if (db is null || passwords is null) return;

        if (!db.Usuarios.Any())
        {
            db.Usuarios.Add(new Usuario
            {
                Nombre = "Administrador WorkHub",
                Correo = "admin@workhub.local",
                Telefono = "70000000",
                PasswordHash = passwords.Hash("Admin123*")
            });
        }

        if (!db.Espacios.Any())
        {
            db.Espacios.AddRange(
                new Espacio { Codigo = "ESC-01", Nombre = "Escritorio 1", Tipo = TipoEspacio.EscritorioFlexible, Capacidad = 1, PrecioHora = 15m },
                new Espacio { Codigo = "OFI-01", Nombre = "Oficina privada A", Tipo = TipoEspacio.OficinaPrivada, Capacidad = 4, PrecioHora = 45m },
                new Espacio { Codigo = "SAL-01", Nombre = "Sala de reuniones principal", Tipo = TipoEspacio.SalaReuniones, Capacidad = 10, PrecioHora = 80m }
            );
        }

        if (!db.Equipamientos.Any())
        {
            db.Equipamientos.AddRange(
                new Equipamiento { Codigo = "EQ-PROY", Nombre = "Proyector Full HD", Tipo = "Audiovisual", PrecioHora = 10m },
                new Equipamiento { Codigo = "EQ-PIZ", Nombre = "Pizarra móvil", Tipo = "Mobiliario", PrecioHora = 3m },
                new Equipamiento { Codigo = "EQ-VC", Nombre = "Kit de videoconferencia", Tipo = "Audiovisual", PrecioHora = 15m }
            );
        }

        db.SaveChanges();
    }
}
