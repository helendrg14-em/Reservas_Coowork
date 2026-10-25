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
        await tx.CommitAsync(ct);
    }
}
