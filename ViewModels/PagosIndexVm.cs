using Workhub.Models;
namespace Workhub.ViewModels;
public class PagosIndexVm
{
    public string? Buscar { get; set; }
    public bool PagoClienteHabilitado { get; set; }
    // Cada lista contiene solo la página actual; los totales se calculan sobre todos los registros.
    public List<Reserva> PorPagar { get; set; } = [];
    public List<Reserva> Vencidas { get; set; } = [];
    public List<Pago> Historial { get; set; } = [];
    public decimal MontoPendiente { get; set; }
    public Paginacion PaginacionPendientes { get; set; } = Paginacion.Crear(1, 0, 1);
    public Paginacion PaginacionVencidas { get; set; } = Paginacion.Crear(1, 0, 1);
    public Paginacion PaginacionHistorial { get; set; } = Paginacion.Crear(1, 0, 1);
}
