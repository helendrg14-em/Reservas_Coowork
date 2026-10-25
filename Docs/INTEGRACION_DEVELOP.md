# Integración del equipo — 25 de octubre de 2026

## Ramas integradas

| Responsable | Rama | Revisión integrada |
|---|---|---|
| Helen | Helen-Login | 2206f1c |
| Dellien | codex/dellien-workhub | 269e23f |
| Miguel | miguel/equipamiento | 9db045b |
| Matias | matias-reservas | 91ee97f |
| Jorge | jorge-pagos-reportes | 5e5742c |

Se hicieron merges separados sin squash ni rebase. Los commits y autores originales permanecen en la historia. La integración se publica únicamente en develop.

## Resolución de conflictos

Se conservan los controladores y vistas de cuenta e inicio de Helen, compatibles con /Cuenta, /Cuenta/Salir y DashboardVm. Se integra el atributo Rol y el modelo de perfil ampliado traídos por la rama de Dellien, sus catálogos y la tarjeta de espacios con acceso directo a reservar. Se preserva la función de cambio de contraseña con una vista compatible con la interfaz actual.

Program.cs combina autenticación, costos, catálogos, archivos, reservas y pagos. Se conserva el script completo de Matias con filtro de clientes, protección contra doble envío, códigos y fotografía. El código compartido duplicado por cambios de finales de línea se normalizó sin sustituir la aplicación completa por un ZIP.

Los permisos se determinan por USUARIO.rol (1=Admin, 2=Cliente). La cookie usa Administrador/Cliente y relee el rol en cada solicitud. El registro público no admite elegir un rol. La migración agrega rol si falta y conserva los administradores de la configuración anterior solo en esa primera conversión. Reiniciar no restablece privilegios revocados. No se agrega una tabla de roles.

Se corrigió el orden del esquema y la carga Bogus para iniciar tanto con bases nuevas como existentes y con dos arranques simultáneos. Los códigos de equipamiento se generan dentro de una transacción y un bloqueo compartido para evitar duplicados concurrentes.

## Verificación

- Compilación .NET 10: cero errores y advertencias.
- 65 comprobaciones de cuentas, permisos, catálogos, reservas, costos y reportes.
- 28 comprobaciones de inicialización, reinicio, concurrencia y restricciones fuera de Development.
- 34 comprobaciones del flujo de reserva y pago del cliente.
- 36 comprobaciones de administración, adjuntos privados, paginación y bloqueos SQL.
- 16 comprobaciones de migración de base antigua, roles persistidos, revocación y cambio de contraseña.

Total: 179 comprobaciones funcionales satisfactorias, ejecutadas contra bases temporales que se eliminaron al terminar. La base habitual del equipo no se modificó. El registro de pago sigue siendo simulado y no procesa cobros bancarios.

## Revisar antes de pasar a main

Actualizar develop, ejecutar INICIAR_WORKHUB.cmd y recorrer los módulos con las cuentas de administrador y cliente. Conservar un respaldo de la base existente y los adjuntos al desplegar cambios. Abrir el Pull Request de develop hacia main y usar Create a merge commit para mantener el historial individual. La publicación en main queda a decisión del equipo.
