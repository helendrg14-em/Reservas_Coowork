using System.Data;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.ViewModels;
namespace Workhub.Services;

public class PagoService(ApplicationContext db)
{
    public async Task<int> RegistrarAsync(PagoForm form, int? propietarioId = null, string? comprobante = null)
    {
        // Sin N° de operación (p. ej. efectivo) se genera una referencia interna única.
        var referencia = string.IsNullOrWhiteSpace(form.CodigoTransaccion)
            ? $"WH-{RelojBolivia.Ahora:yyyyMMdd}-{Guid.NewGuid():N}"[..24].ToUpperInvariant()
            : form.CodigoTransaccion.Trim().ToUpperInvariant();
        return await Transacciones.Ejecutar(db, async () =>
        {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await Transacciones.Bloquear(db, "WorkHub:Reservas");
        var reserva = await db.Reservas.Include(r => r.Pago).SingleOrDefaultAsync(r => r.IdReserva == form.IdReserva)
            ?? throw new OperacionException("La reserva no existe.");
        if (propietarioId.HasValue && reserva.IdUsuario != propietarioId.Value)
            throw new OperacionException("No puede pagar una reserva de otro usuario.");
        if (reserva.Pago?.CodigoTransaccion == referencia && reserva.Pago.Comprobante == comprobante) return reserva.Pago.IdPago;
        if (reserva.Pago is not null) throw new OperacionException("La reserva ya tiene un pago registrado.");
        if (reserva.Estado != EstadoReserva.Pendiente) throw new OperacionException("Solo puede pagar una reserva pendiente.");
        if (reserva.FechaInicio <= RelojBolivia.Ahora) throw new OperacionException("El horario ya empezó. Reprograme la reserva antes de pagar.");
        if (reserva.CostoTotal <= 0) throw new OperacionException("La reserva debe tener un costo positivo.");
        if (await db.Pagos.AnyAsync(p => p.CodigoTransaccion == referencia))
            throw new OperacionException("Ese N° de operación ya fue registrado en otro pago.");
        var pago = new Pago { IdReserva = reserva.IdReserva, Monto = reserva.CostoTotal,
            MetodoPago = form.MetodoPago, CodigoTransaccion = referencia, Comprobante = comprobante,
            FechaPago = RelojBolivia.Ahora, Estado = EstadoPago.Aprobado };
        db.Pagos.Add(pago);
        reserva.Estado = EstadoReserva.Confirmada;
        await db.SaveChangesAsync(); await tx.CommitAsync(); return pago.IdPago;
        });
    }
    // Devuelve el archivo de comprobante reemplazado (si lo hubo) para que se elimine.
    public Task<string?> EditarAsync(int id, PagoForm form, string? comprobante = null) => Transacciones.Ejecutar(db, async () =>
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await Transacciones.Bloquear(db, "WorkHub:Reservas");
        var pago = await db.Pagos.FindAsync(id) ?? throw new OperacionException("El pago no existe.");
        if (pago.Estado != EstadoPago.Aprobado) throw new OperacionException("No se editan pagos reembolsados.");
        var referencia = string.IsNullOrWhiteSpace(form.CodigoTransaccion)
            ? pago.CodigoTransaccion : form.CodigoTransaccion.Trim().ToUpperInvariant();
        if (await db.Pagos.AnyAsync(p => p.IdPago != id && p.CodigoTransaccion == referencia))
            throw new OperacionException("Ese N° de operación ya fue registrado en otro pago.");
        // No se permite cambiar monto, reserva, fecha o estado desde el formulario.
        pago.CodigoTransaccion = referencia; pago.MetodoPago = form.MetodoPago;
        var anterior = comprobante is null || pago.Comprobante == comprobante ? null : pago.Comprobante;
        if (comprobante is not null) pago.Comprobante = comprobante;
        await db.SaveChangesAsync(); await tx.CommitAsync();
        return anterior;
    });
    public Task ReembolsarAsync(int id) => Transacciones.Ejecutar(db, async () =>
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await Transacciones.Bloquear(db, "WorkHub:Reservas");
        var pago = await db.Pagos.Include(p => p.Reserva).SingleOrDefaultAsync(p => p.IdPago == id)
            ?? throw new OperacionException("El pago no existe.");
        if (pago.Estado != EstadoPago.Aprobado || pago.Reserva.Estado == EstadoReserva.Completada)
            throw new OperacionException("Solo se reembolsan pagos aprobados de reservas no completadas.");
        pago.Estado = EstadoPago.Reembolsado;
        pago.Reserva.Estado = EstadoReserva.Cancelada;
        await db.SaveChangesAsync(); await tx.CommitAsync();
    });
}
