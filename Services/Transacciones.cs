using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Workhub.Data;
namespace Workhub.Services;

public static class Transacciones
{
    // Reintenta la operación completa; nunca solo SaveChanges dentro de una transacción.
    public static Task<T> Ejecutar<T>(ApplicationContext db, Func<Task<T>> operacion) =>
        new SqlServerRetryingExecutionStrategy(db, 3, TimeSpan.FromSeconds(2), null)
            .ExecuteAsync(async () => { db.ChangeTracker.Clear(); return await operacion(); });
    public static Task Ejecutar(ApplicationContext db, Func<Task> operacion) =>
        Ejecutar(db, async () => { await operacion(); return true; });

    public static async Task Bloquear(ApplicationContext db, string recurso)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($@"
            DECLARE @resultado int;
            EXEC @resultado = sys.sp_getapplock @Resource={recurso}, @LockMode='Exclusive',
                 @LockOwner='Transaction', @LockTimeout=3000;
            IF @resultado < 0 THROW 51001, 'Otra operación está en curso. Vuelva a intentar.', 1;");
    }
    public static bool EsErrorDatos(Exception error)
    {
        for (Exception? e = error; e is not null; e = e.InnerException)
            if (e is SqlException or DbUpdateException or RetryLimitExceededException) return true;
        return false;
    }
}
