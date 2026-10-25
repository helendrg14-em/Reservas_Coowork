using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models;
using Workhub.Models.Enums;
using Workhub.ViewModels;

namespace Workhub.Services;

public class ReservaGestionService(ApplicationContext db, IReservaService disponibilidad,
    ICostoService costos, IFolioService folios)
{
    public async Task<int> GuardarAsync(ReservaForm form, int usuarioId, bool administrador)
    {
        var folioIntento = folios.Generar();
        return await Transacciones.Ejecutar(db, async () =>
        {
            await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
            // se bloquea para que dos personas no reserven el mismo horario al mismo tiempo
            await Transacciones.Bloquear(db, "WorkHub:Reservas");
            Reserva reserva;
            if (form.Id == 0)
            {
                var guardada = await db.Reservas.AsNoTracking().SingleOrDefaultAsync(r => r.Folio == folioIntento);
                if (guardada is not null) return guardada.IdReserva;
                reserva = new Reserva { Folio = folioIntento, FechaCreacion = RelojBolivia.Ahora };
                db.Reservas.Add(reserva);
            }
            else
            {
                reserva = await db.Reservas.Include(r => r.Pago).Include(r => r.ReservaEquipamientos)
                    .SingleOrDefaultAsync(r => r.IdReserva == form.Id)
                    ?? throw new OperacionException("La reserva ya no existe.");
                if (!administrador && reserva.IdUsuario != usuarioId)
                    throw new OperacionException("No puede modificar esta reserva.");
                if (reserva.Estado != EstadoReserva.Pendiente || reserva.Pago is not null)
                    throw new OperacionException("Solo se editan reservas pendientes sin pagos registrados.");
            }
            if (form.FechaInicio < RelojBolivia.Ahora || form.FechaFin <= form.FechaInicio ||
                form.FechaFin - form.FechaInicio > TimeSpan.FromDays(7))
                throw new OperacionException("Seleccione un horario futuro de hasta 7 días.");
            var clienteId = administrador ? form.IdUsuario : usuarioId;
            if (!await db.Usuarios.AnyAsync(u => u.IdUsuario == clienteId))
                throw new OperacionException("Seleccione un cliente válido.");
            var espacio = await db.Espacios.FindAsync(form.IdEspacio)
                ?? throw new OperacionException("El espacio ya no existe.");
            if (!await disponibilidad.EspacioDisponibleAsync(form.IdEspacio, form.FechaInicio, form.FechaFin, form.Id))
                throw new OperacionException("El espacio ya está reservado en ese horario. Elija otro horario o espacio.");
            if (form.Equipos.Any(e => e.Cantidad < 0 || e.Cantidad > 20) ||
                form.Equipos.Select(e => e.IdEquipo).Distinct().Count() != form.Equipos.Count)
                throw new OperacionException("Revise las cantidades y los equipos seleccionados.");
            var seleccionados = form.Equipos.Where(e => e.Cantidad > 0).ToList();
            var ids = seleccionados.Select(e => e.IdEquipo).ToArray();
            var equipos = await db.Equipamientos.Where(e => ids.Contains(e.IdEquipo)).ToDictionaryAsync(e => e.IdEquipo);
            if (equipos.Count != seleccionados.Count) throw new OperacionException("Un equipo seleccionado ya no existe.");
            reserva.IdUsuario = clienteId; reserva.Espacio = espacio; reserva.IdEspacio = espacio.IdEspacio;
            reserva.FechaInicio = form.FechaInicio; reserva.FechaFin = form.FechaFin;
            // quito los equipos que ya no se eligieron y actualizo o agrego los demas
            foreach (var anterior in reserva.ReservaEquipamientos.Where(e => !ids.Contains(e.IdEquipo)).ToList())
            {
                db.ReservaEquipamientos.Remove(anterior);
                reserva.ReservaEquipamientos.Remove(anterior);
            }
            foreach (var item in seleccionados)
            {
                var link = reserva.ReservaEquipamientos.SingleOrDefault(e => e.IdEquipo == item.IdEquipo);
                if (link is null)
                {
                    link = new ReservaEquipamiento { IdEquipo = item.IdEquipo, PrecioHora = equipos[item.IdEquipo].PrecioHora,
                        FechaHora = RelojBolivia.Ahora };
                    reserva.ReservaEquipamientos.Add(link);
                }
                link.Cantidad = item.Cantidad;
            }
            // el costo siempre se calcula en el servidor, no se confia en el formulario
            reserva.CostoTotal = costos.CalcularCostoTotal(reserva);
            if (reserva.CostoTotal <= 0 || reserva.CostoTotal > 99999999.99m)
                throw new OperacionException("El costo excede el importe admitido. Reduzca la duración o los equipos.");
            await db.SaveChangesAsync();
            await tx.CommitAsync();
            return reserva.IdReserva;
        });
    }

    public Task CambiarAsync(int id, int usuarioId, bool administrador, string accion) => Transacciones.Ejecutar(db, async () =>
    {
        await using var tx = await db.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted);
        await Transacciones.Bloquear(db, "WorkHub:Reservas");
        var r = await db.Reservas.Include(r => r.Pago).SingleOrDefaultAsync(r => r.IdReserva == id)
            ?? throw new OperacionException("La reserva no existe.");
        if (!administrador && r.IdUsuario != usuarioId) throw new OperacionException("No puede operar esta reserva.");
        if (accion == "eliminar")
        {
            if (r.Pago is not null || r.Estado != EstadoReserva.Pendiente)
                throw new OperacionException("Solo se eliminan reservas pendientes sin pagos. Use cancelar para conservar el historial.");
            db.Reservas.Remove(r);
        }
        else if (accion == "cancelar")
        {
            if (r.Estado is EstadoReserva.Completada or EstadoReserva.Cancelada)
                throw new OperacionException("La reserva ya está cerrada.");
            if (r.Pago?.Estado == EstadoPago.Aprobado)
                throw new OperacionException("Primero un administrador debe registrar el reembolso del pago.");
            r.Estado = EstadoReserva.Cancelada;
        }
        else if (accion == "completar" && administrador)
        {
            if (r.Estado != EstadoReserva.Confirmada || r.FechaFin > RelojBolivia.Ahora)
                throw new OperacionException("Solo se completan reservas confirmadas cuyo horario ya terminó.");
            r.Estado = EstadoReserva.Completada;
        }
        else throw new OperacionException("Operación no permitida.");
        await db.SaveChangesAsync();
        await tx.CommitAsync();
    });
}
