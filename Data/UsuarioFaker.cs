using Bogus;
using Workhub.Models;

namespace Workhub.Data;

// Mismo patrón Faker<T> usado en la práctica HeroeFaker.
public sealed class UsuarioFaker : Faker<Usuario>
{
    public UsuarioFaker() : base("es")
    {
        UseSeed(20261024);
        RuleFor(u => u.Nombre, f => f.Name.FullName());
        RuleFor(u => u.Telefono, f => f.Phone.PhoneNumber("7#######"));
        // Correo y hash se asignan en el seeder: correos únicos y contraseñas con hash real.
    }
}
