using Microsoft.EntityFrameworkCore;
using System.Data;
using Workhub.Services;

namespace Workhub.Data;

// La base se crea con EnsureCreated, que no agrega columnas a una base existente.
// Este paso idempotente incorpora las columnas nuevas sin borrar datos.
public static class EsquemaWorkHub
{
    public static async Task ActualizarAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
        if (!await db.Database.CanConnectAsync(ct)) return;
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted,ct);
        await Transacciones.Bloquear(db,"WorkHub:Esquema");
        await db.Database.ExecuteSqlRawAsync("""
            IF OBJECT_ID(N'dbo.ESPACIO') IS NOT NULL AND COL_LENGTH(N'dbo.ESPACIO', N'foto') IS NULL
                ALTER TABLE dbo.ESPACIO ADD foto nvarchar(200) NULL;
            IF OBJECT_ID(N'dbo.PAGO') IS NOT NULL AND COL_LENGTH(N'dbo.PAGO', N'comprobante') IS NULL
                ALTER TABLE dbo.PAGO ADD comprobante nvarchar(200) NULL;
            """, ct);
        var existeRol = await db.Database.SqlQueryRaw<int>(
            "SELECT CASE WHEN COL_LENGTH(N'dbo.USUARIO', N'rol') IS NULL THEN 0 ELSE 1 END AS [Value]").SingleAsync(ct);
        if (existeRol == 0)
        {
            await db.Database.ExecuteSqlRawAsync("ALTER TABLE dbo.USUARIO ADD rol int NOT NULL CONSTRAINT DF_USUARIO_rol DEFAULT(2);", ct);
            // Solo la migración inicial conserva los administradores de la configuración anterior.
            // En reinicios posteriores el rol almacenado es la única fuente de permisos.
            var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var correos = config.GetSection("Administracion:Correos").Get<string[]>() ?? [];
            foreach (var correo in correos)
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE dbo.USUARIO SET rol=1 WHERE correo={correo}", ct);
        }
        await tx.CommitAsync(ct);
    }
}
