# WorkHub

Aplicación MVC para administrar espacios de coworking, equipamiento, reservas y pagos.

## Repositorio e integrantes

- [Repositorio público del equipo, accesible para el docente](https://github.com/helendrg14-em/Reservas_Coowork).
- [Versión integrada en develop](https://github.com/helendrg14-em/Reservas_Coowork/tree/develop).
- [Historial de commits y autores](https://github.com/helendrg14-em/Reservas_Coowork/commits/develop).

| Integrante | Usuario de GitHub | Aporte principal |
|---|---|---|
| Helen Rios Gutierrez | [helendrg14-em](https://github.com/helendrg14-em) | Cuentas, autenticación, estructura e inicio |
| Andre Dellien | [andredellien](https://github.com/andredellien) | Espacios, fotografías y códigos |
| Miguel Angelo Mendoza Coronado | [mangelomendozac-prog](https://github.com/mangelomendozac-prog) | Equipamiento y formularios |
| Matias Ruiz Decourt | [Matias-Ruiz-Decourt](https://github.com/Matias-Ruiz-Decourt) | Reservas, disponibilidad y folios |
| Jorge Luis Herbas Mallcu | [JorgeHM28](https://github.com/JorgeHM28) | Pagos, reportes, documentación e integración |

## Iniciar en Windows

Requiere .NET SDK 10 y SQL Server Express LocalDB. Abra `Workhub.slnx` en Visual Studio o ejecute `INICIAR_WORKHUB.cmd`. También puede ejecutar `dotnet restore` y `dotnet run --launch-profile http` desde esta carpeta. Consulte la dirección que muestra la consola.

La conexión local está en `appsettings.json`; las opciones del entorno local están en `appsettings.Development.json`. Se conserva el nombre `WorkHubDemoDb` por compatibilidad con la base existente. No elimine esa base para actualizar. Al iniciar se agregan, si faltan, las columnas de fotografías, comprobantes y rol.

También se necesita Internet para restaurar NuGet y un navegador. Git permite clonar y consultar el historial; Visual Studio y SSMS son opcionales. Comprobar instalaciones con `dotnet --version` y `sqllocaldb info`.

```powershell
git clone --branch develop https://github.com/helendrg14-em/Reservas_Coowork.git
cd Reservas_Coowork
sqllocaldb start MSSQLLocalDB
dotnet restore Workhub.csproj
dotnet build Workhub.csproj
dotnet run --project Workhub.csproj --launch-profile http
```

Abrir **http://localhost:5004** cuando la consola indique que el servidor está escuchando. Detener con `Ctrl+C`. Desde el ZIP, extraer todo y ejecutar `INICIAR_WORKHUB.cmd` dentro de la carpeta `WorkHub`.

## Configurar y crear la base de datos

La conexión incluida usa autenticación integrada de Windows, sin usuario ni contraseña SQL:

```json
"ConnectionStrings": {
  "WorkHub": "Server=(localdb)\\MSSQLLocalDB;Database=WorkHubDemoDb;Trusted_Connection=True;TrustServerCertificate=True"
}
```

**Opción automática recomendada:** ejecutar con el perfil `http` (entorno Development). `DatosDemoSeeder` crea el esquema con EF Core `EnsureCreatedAsync` si no existe. `EsquemaWorkHub` agrega columnas nuevas a bases compatibles anteriores sin borrar datos. Bogus carga una vez 1 administrador, 18 clientes, 8 espacios, 6 equipos y 60 reservas con sus pagos y equipos. Si ya existen usuarios, espacios o equipos, no los rellena ni reemplaza.

La carga inicial exige que el nombre de base de la conexión coincida con `DatosDemo:BasePermitida` y que `DatosDemo:Habilitado` esté activo en Development. Si usa otro nombre para una instalación nueva, cambie ambas opciones. Variables de entorno o user-secrets locales pueden sobrescribir los archivos de configuración.

**Alternativa mediante script:** para una base nueva que todavía no existe, usar SSMS o `sqlcmd` (requiere las herramientas de línea de comandos de SQL Server):

```powershell
sqllocaldb start MSSQLLocalDB
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -Q "CREATE DATABASE [WorkHubDemoDb]"
sqlcmd -S "(localdb)\MSSQLLocalDB" -E -b -d WorkHubDemoDb -i "Data/CrearEsquemaSoloBaseVacia.sql"
dotnet run --project Workhub.csproj --launch-profile http
```

El script crea las seis tablas, claves, relaciones e índices. No incluye datos ni contraseñas y rechaza bases que ya contienen tablas. El siguiente inicio local carga los ejemplos si sigue vacía. El proyecto usa creación automática y **script SQL**, no una cadena de migraciones EF: no requiere `dotnet ef database update`.

**Base existente:** hacer respaldo y conservar la conexión; no ejecutar nuevamente CREATE DATABASE ni el script de creación. Puede revisar el esquema con `Data/DiagnosticarBaseExistente.sql`. Ante un fallo de conexión, comprobar instancia, nombre de base y permisos del usuario de Windows; no borrar la base.

## Acceso inicial

Las siguientes son **cuentas ficticias y públicas de ejemplo**, exclusivamente para ejecución local. No son cuentas personales del equipo, credenciales de GitHub ni de servicios bancarios.

| Cuenta | Correo | Contraseña inicial |
|---|---|---|
| Administrador | admin@workhub.local | Admin123! |
| Cliente | cliente01@workhub.local | Cliente123! |

Bogus carga datos iniciales una sola vez en la base local autorizada y en Development. Existen clientes del 01 al 18. Las credenciales iniciales del dominio local están bloqueadas fuera de Development. No cambie a Production con estas cuentas. El registro público no puede apropiarse del correo administrador configurado.

Las contraseñas de usuarios se guardan como hash. No subir tokens, contraseñas personales ni cadenas con credenciales reales. `.env`, `appsettings.Local.json`, fotografías cargadas y comprobantes privados están excluidos de Git.

## Funciones

- Inicio con fotografías de espacios y acceso a reservar.
- Códigos automáticos para espacios y equipamiento, fotografías propias y paginación.
- Reservas con disponibilidad, equipamiento, cálculo de costos y estados.
- Pagos pendientes, reservas vencidas, registro de pagos y comprobantes privados.
- Reportes de ingresos y reservas con filtros, totales e impresión.

La confirmación del cliente es simulada: registra el pago dentro del sistema y no procesa cobros bancarios. Subir un comprobante no verifica una operación bancaria. El administrador puede registrar y reembolsar pagos dentro del sistema.

Las fotografías cargadas se guardan en `wwwroot/uploads/espacios`; los comprobantes privados en `App_Data/comprobantes`. Incluya ambas carpetas y la base SQL en sus copias de respaldo. El ZIP de código no incluye datos ni archivos personales cargados.

## Reportes y utilidad

### 1. Reservas por período

Ruta `/Reportes/Reservas`, disponible para administración. Consulta SQL Server mediante EF Core y filtra por **fecha de inicio** (desde/hasta) y estado. Muestra folio, cliente, espacio, horario, estado e importe. Resume cantidad de reservas, confirmadas, canceladas e importe no cancelado. Este importe representa reservas y no necesariamente dinero cobrado.

Permite revisar la actividad y las cancelaciones, organizar horarios y atender incidencias. El cliente consulta sus propias reservas en su sección; no accede al informe global de otros usuarios.

### 2. Ingresos por período

Ruta `/Reportes/Ingresos`, disponible para administración. Consulta pagos por **fecha de pago** (desde/hasta) y método: tarjeta, transferencia, QR o efectivo. Muestra comprobante, cliente, fecha, método, estado e importe. Resume cantidad de comprobantes, importe registrado, reembolsados e ingresos vigentes (pagos aprobados).

Sirve para revisar registros de cobros por período y método, distinguir pagos vigentes de reembolsados y contrastarlos con las reservas. No constituye una conciliación bancaria automática.

Ambos reportes incluyen todo el día final, rechazan fechas invertidas y rangos mayores de 366 días. La pantalla pagina de 15 en 15; los totales consideran el resultado completo y la impresión incluye todas sus filas.

## DER y presentación individual

- [DER original, imagen](Docs/DER_original.png).
- [DER actualizado, editable en draw.io](Docs/DER_implementado.drawio).
- [Explicación de modificaciones](Docs/COMPARACION_DER.md).

Demostrar una reserva con equipos, el rechazo de un horario solapado, el costo calculado, la confirmación de pago simulado y su comprobante. Como administrador, mostrar los dos reportes, cambiar filtros y explicar sus totales. Mostrar también las restricciones del cliente y el historial de GitHub.

Cada integrante debe comprender **toda la solución**, no solo sus archivos: relaciones y claves, validaciones, permisos, separación MVC, consultas y persistencia. Preguntas para practicar:

| Integrante | Pregunta de preparación |
|---|---|
| Helen | ¿Cómo se verifica la contraseña y se impide registrarse como administrador? |
| Andre Dellien | ¿Cómo se valida una fotografía y por qué el código lo asigna el servidor? |
| Miguel Angelo | ¿Por qué no se elimina un equipo utilizado y cómo se evitan códigos duplicados? |
| Matias | ¿Cómo se detecta el solapamiento y qué conserva INCLUYE? |
| Jorge Luis | ¿Cómo se impide un segundo pago y cómo difieren los totales de los reportes? |

La pregunta individual puede abarcar cualquier módulo.

## Correspondencia entre ZIP y repositorio

Generar el ZIP desde un **commit publicado**, mediante `git archive`, sin copiar una carpeta con cambios pendientes:

```powershell
git fetch origin
git rev-parse origin/develop
git archive --format=zip --prefix=WorkHub/ --output=../WorkHub_Entrega.zip origin/develop
```

Entregar el identificador completo mostrado por `git rev-parse` junto con el enlace. El ZIP contiene los archivos versionados de esa revisión, sin `.git`, `bin`, `obj`, `.vs`, bases locales ni archivos privados no versionados. El historial queda en GitHub.

Cuando el equipo apruebe el pase a main, integrar con **Create a merge commit** y regenerar el ZIP desde `origin/main`. Cualquier cambio posterior exige regenerar el archivo.

Documentación: `Docs/GUIA_USO.md`, `Docs/COMPARACION_DER.md`, `Docs/DER_implementado.mmd`, `Docs/FOTOGRAFIAS.md` y `Docs/REVISION.md`.

## Integración en develop

Las cinco ramas se integraron conservando sus commits y autores mediante merges, sin squash. Consulte `Docs/INTEGRACION_DEVELOP.md`. Antes de pasar a main, el equipo puede ejecutar la aplicación y revisar sus módulos desde develop.

El permiso se almacena en `USUARIO.rol`: 1=Admin, 2=Cliente. La sesión utiliza el nombre Administrador para las políticas existentes. La lista `Administracion:Correos` se utiliza para migrar una base antigua sin columna rol y reservar correos de registro; no concede permisos en cada inicio de sesión. El registro público siempre asigna Cliente.
