using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Workhub.Models.Enums;

namespace Workhub.Models;

public class Reserva
{
    public int IdReserva { get; set; }

    [Required]
    [StringLength(30)]
    [Display(Name = "Folio")]
    public string Folio { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Fecha y hora de inicio")]
    public DateTime FechaInicio { get; set; }

    [Required]
    [Display(Name = "Fecha y hora de fin")]
    public DateTime FechaFin { get; set; }

    [Display(Name = "Fecha de creación")]
    public DateTime FechaCreacion { get; set; } = DateTime.Now;

    [Required]
    public EstadoReserva Estado { get; set; } = EstadoReserva.Pendiente;

    [Column(TypeName = "decimal(10,2)")]
    [Display(Name = "Costo total")]
    public decimal CostoTotal { get; set; }

    public int IdUsuario { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int IdEspacio { get; set; }
    public Espacio Espacio { get; set; } = null!;

    public Pago? Pago { get; set; }
    public List<ReservaEquipamiento> ReservaEquipamientos { get; set; } = new();

    [NotMapped]
    public double HorasReservadas => (FechaFin - FechaInicio).TotalHours;
}
