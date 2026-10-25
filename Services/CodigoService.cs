using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Models.Enums;

namespace Workhub.Services;

// Genera códigos correlativos por prefijo: ESC-04, OFI-03, SAL-04, EQ-07...
public class CodigoService(ApplicationContext db)
{
    public static string Prefijo(TipoEspacio tipo) => tipo switch
    {
        TipoEspacio.OficinaPrivada => "OFI",
        TipoEspacio.SalaReuniones => "SAL",
        _ => "ESC"
    };

    public async Task<string> EspacioAsync(TipoEspacio tipo) =>
        Siguiente(Prefijo(tipo), await db.Espacios.Select(e => e.Codigo).ToListAsync());

    public async Task<Dictionary<TipoEspacio, string>> EspaciosPorTipoAsync()
    {
        var codigos = await db.Espacios.Select(e => e.Codigo).ToListAsync();
        return Enum.GetValues<TipoEspacio>().ToDictionary(t => t, t => Siguiente(Prefijo(t), codigos));
    }

    public async Task<string> EquipoAsync() =>
        Siguiente("EQ", await db.Equipamientos.Select(e => e.Codigo).ToListAsync());

    private static string Siguiente(string prefijo, IEnumerable<string> existentes)
    {
        var maximo = existentes
            .Where(c => c.StartsWith(prefijo + "-", StringComparison.OrdinalIgnoreCase))
            .Select(c => int.TryParse(c[(prefijo.Length + 1)..], out var n) ? n : 0)
            .DefaultIfEmpty(0).Max();
        return $"{prefijo}-{maximo + 1:00}";
    }
}
