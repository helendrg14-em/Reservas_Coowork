using Workhub.Models;
using Workhub.Models.Enums;
namespace Workhub.Services;

public static class Presentacion
{
    public static bool Vencida(Reserva r) => r.Estado == EstadoReserva.Pendiente && r.FechaInicio <= RelojBolivia.Ahora;
    public static bool PuedePagar(Reserva r) => r.Estado == EstadoReserva.Pendiente && r.Pago is null && r.FechaInicio > RelojBolivia.Ahora;
    public static string Estado(Reserva r) => Vencida(r) ? "Vencida" : r.Estado.ToString();
    public static string Tipo(TipoEspacio tipo) => tipo switch
    {
        TipoEspacio.OficinaPrivada => "Oficina privada",
        TipoEspacio.SalaReuniones => "Sala de reuniones",
        _ => "Escritorio flexible"
    };
    public static string Foto(Espacio espacio) => !string.IsNullOrEmpty(espacio.Foto) ? espacio.Foto : espacio.Tipo switch
    {
        TipoEspacio.OficinaPrivada => "/images/oficina.jpg",
        TipoEspacio.SalaReuniones => "/images/sala.jpg",
        _ => "/images/escritorio.jpg"
    };
    public static bool PagoDemo(IWebHostEnvironment environment, IConfiguration configuration) =>
        environment.IsDevelopment() && configuration.GetValue<bool>("Pagos:PermitirSimulacionCliente");
}
