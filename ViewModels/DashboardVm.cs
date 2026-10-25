using Workhub.Models;
namespace Workhub.ViewModels;
public class DashboardVm
{
    public int Espacios { get; set; }
    public int Equipos { get; set; }
    public int Pendientes { get; set; }
    public int Vencidas { get; set; }
    public List<Espacio> Destacados { get; set; } = [];
    public int Confirmadas { get; set; }
    public List<Reserva> Proximas { get; set; } = [];
}
