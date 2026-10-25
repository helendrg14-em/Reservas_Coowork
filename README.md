# WorkHub

Aplicación MVC para administrar espacios de coworking, equipamiento, reservas y pagos.

## Iniciar en Windows

Requiere .NET SDK 10 y SQL Server Express LocalDB. Abra `Workhub.slnx` en Visual Studio o ejecute `INICIAR_WORKHUB.cmd`. También puede ejecutar `dotnet restore` y `dotnet run --launch-profile http` desde esta carpeta. Consulte la dirección que muestra la consola.

La conexión local está en `appsettings.json`; las opciones del entorno local están en `appsettings.Development.json`. Se conserva el nombre `WorkHubDemoDb` por compatibilidad con la base existente. No elimine esa base para actualizar. Al iniciar se agregan, si faltan, las columnas de fotografías, comprobantes y rol, si faltan.

## Acceso inicial

| Cuenta | Correo | Contraseña inicial |
|---|---|---|
| Administrador | admin@workhub.local | Admin123! |
| Cliente | cliente01@workhub.local | Cliente123! |

Bogus carga datos iniciales una sola vez en la base local autorizada y en Development. Existen clientes del 01 al 18. Las credenciales iniciales del dominio local están bloqueadas fuera de Development. No cambie a Production con estas cuentas. El registro público no puede apropiarse del correo administrador configurado.

## Funciones

- Inicio con fotografías de espacios y acceso a reservar.
- Códigos automáticos para espacios y equipamiento, fotografías propias y paginación.
- Reservas con disponibilidad, equipamiento, cálculo de costos y estados.
- Pagos pendientes, reservas vencidas, registro de pagos y comprobantes privados.
- Reportes de ingresos y reservas con filtros, totales e impresión.

La confirmación del cliente es simulada: registra el pago dentro del sistema y no procesa cobros bancarios. Subir un comprobante no verifica una operación bancaria. El administrador puede registrar y reembolsar pagos dentro del sistema.

Las fotografías cargadas se guardan en `wwwroot/uploads/espacios`; los comprobantes privados en `App_Data/comprobantes`. Incluya ambas carpetas y la base SQL en sus copias de respaldo. El ZIP de código no incluye datos ni archivos personales cargados.

## Integración del equipo

Los cinco TXT entregados contienen 26 bloques con archivos completos y mensajes de commit. Helen trabaja cuentas y estructura; Dellien, espacios; Miguel, equipamiento y costos; Matias, reservas; Jorge, pagos, reportes y documentación.

Cada bloque propone unos 15 minutos de trabajo, sin forzar tiempos. Confirme únicamente cambios reales revisados. Si el repositorio ya contiene el mismo código, no cree commits vacíos ni altere fechas para aparentar actividad. Consulte `Docs/PLAN_COMMITS.md`.

Documentación: `Docs/GUIA_USO.md`, `Docs/COMPARACION_DER.md`, `Docs/DER_implementado.mmd`, `Docs/FOTOGRAFIAS.md` y `Docs/REVISION.md`.

## Integración en develop

Las cinco ramas se integraron conservando sus commits y autores mediante merges, sin squash. Consulte `Docs/INTEGRACION_DEVELOP.md`. Antes de pasar a main, el equipo puede ejecutar la aplicación y revisar sus módulos desde develop.

El permiso se almacena en `USUARIO.rol`: 1=Admin, 2=Cliente. La sesión utiliza el nombre Administrador para las políticas existentes. La lista `Administracion:Correos` se utiliza para migrar una base antigua sin columna rol y reservar correos de registro; no concede permisos en cada inicio de sesión. El registro público siempre asigna Cliente.
