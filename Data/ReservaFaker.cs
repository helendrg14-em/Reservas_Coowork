using Bogus;
using Workhub.Models;

namespace Workhub.Data;

public sealed class ReservaFaker : Faker<Reserva>
{
    public ReservaFaker(IReadOnlyList<Usuario> usuarios, IReadOnlyList<Espacio> espacios) : base("es")
    {
        UseSeed(2402026);
        RuleFor(r => r.Usuario, f => f.PickRandom<Usuario>(usuarios));
        RuleFor(r => r.Espacio, f => f.PickRandom<Espacio>(espacios));
        // Las fechas se asignan por franjas separadas para no generar solapamientos falsos.
    }
}
