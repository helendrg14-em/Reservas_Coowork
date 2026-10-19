# WorkHub

Aplicación web para administrar reservas de espacios de coworking y equipamiento. El proyecto usa ASP.NET Core MVC y Entity Framework Core con SQL Server.

## Equipo y módulos

| Integrante | Módulo asignado |
| --- | --- |
| Helen | Usuarios: login, registro y autenticación |
| Jorge | Pagos y reportes |
| Dellien | Espacios: CRUD y filtros |
| Miguel | Equipamiento y relación «incluye» |
| Matías | Reservas |

Agreguen el usuario de GitHub de cada integrante cuando esté confirmado.

## Requisitos

- .NET SDK 10
- SQL Server LocalDB para desarrollo local, o una instancia compatible de SQL Server

## Ejecutar localmente

1. Clona el repositorio.
2. Verifica la cadena `ConnectionStrings:WorkHub` en `appsettings.json`. Para una instancia distinta de LocalDB, guarda la cadena local en `appsettings.Development.json` o en el secreto de usuario `ConnectionStrings:WorkHub`; no subas contraseñas ni credenciales.
3. Desde la carpeta del proyecto, instala la herramienta de EF si aún no está disponible y crea la base de datos:

   ```powershell
   dotnet tool install --global dotnet-ef
   dotnet ef migrations add InitialCreate
   dotnet ef database update
   dotnet run
   ```

4. Abre la URL local indicada por `dotnet run`.

La primera migración debe generarse una sola vez y subirse al repositorio. Después, los integrantes actualizan la base con `dotnet ef database update` cuando reciban cambios de modelo.

## Modelo y DER

El contexto configura las entidades `Usuario`, `Reserva`, `Espacio`, `Pago` y `Equipamiento`, además de la entidad asociativa `ReservaEquipamiento` para la relación N:M «incluye». Esta entidad conserva cantidad, fecha y precio por hora del equipo al momento de reservar.

El modelo implementa las cardinalidades del DER: usuario y espacio tienen muchas reservas; cada reserva puede tener un pago; y una reserva puede incluir varios equipos. Se agregaron folio de reserva y códigos de espacio/equipo como atributos prácticos para identificación. La contraseña se almacena como hash.

## Estado inicial

La solución contiene el arranque MVC, modelos, contexto de datos, servicios compartidos y vistas de entrada para que cada integrante implemente su módulo. Los CRUD, la autenticación completa, los pagos y los dos reportes requeridos aún deben desarrollarse. Los reportes acordados son:

- Reservas por período y estado, con filtros por fecha y resumen de cantidades.
- Ingresos por período y método de pago, con filtros por fecha y total recaudado.

## Trabajo en GitHub

Trabajen en ramas por módulo y creen commits con cambios reales y mensajes descriptivos. Antes de integrar una rama, actualicen desde `main` y resuelvan los conflictos conservando el modelo compartido del DER.
