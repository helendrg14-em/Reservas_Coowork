namespace Workhub.Services;

// Genera un folio único con formato R-yyyyMMdd-xxxxxx (fecha + 6 chars aleatorios).
public class FolioService : IFolioService
{
    public string Generar()
    {
        var fecha = DateTime.Now.ToString("yyyyMMdd");
        var sufijo = Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();
        return $"R-{fecha}-{sufijo}";
    }
}
