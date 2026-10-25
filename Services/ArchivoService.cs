namespace Workhub.Services;

// Guarda fotos de espacios (públicas, en wwwroot) y comprobantes de pago (privados, en App_Data).
public class ArchivoService(IWebHostEnvironment env)
{
    public const long TamanoMaximo = 5 * 1024 * 1024;
    private const string CarpetaFotos = "uploads/espacios";

    private static readonly Dictionary<string, string> Tipos = new(StringComparer.OrdinalIgnoreCase)
    {
        [".jpg"] = "image/jpeg", [".jpeg"] = "image/jpeg", [".png"] = "image/png",
        [".webp"] = "image/webp", [".pdf"] = "application/pdf"
    };

    private string CarpetaComprobantes => Path.Combine(env.ContentRootPath, "App_Data", "comprobantes");

    public async Task<string> GuardarFotoEspacioAsync(IFormFile archivo)
    {
        var nombre = await GuardarAsync(archivo, Path.Combine(env.WebRootPath, CarpetaFotos), permitirPdf: false);
        return $"/{CarpetaFotos}/{nombre}";
    }

    public void EliminarFotoEspacio(string? ruta)
    {
        if (string.IsNullOrEmpty(ruta) || !ruta.StartsWith($"/{CarpetaFotos}/")) return;
        Eliminar(Path.Combine(env.WebRootPath, CarpetaFotos, Path.GetFileName(ruta)));
    }

    public Task<string> GuardarComprobanteAsync(IFormFile archivo) =>
        GuardarAsync(archivo, CarpetaComprobantes, permitirPdf: true);

    public void EliminarComprobante(string? nombre)
    {
        if (!string.IsNullOrEmpty(nombre)) Eliminar(Path.Combine(CarpetaComprobantes, Path.GetFileName(nombre)));
    }

    public (string Ruta, string Tipo)? AbrirComprobante(string? nombre)
    {
        if (string.IsNullOrEmpty(nombre)) return null;
        var ruta = Path.Combine(CarpetaComprobantes, Path.GetFileName(nombre));
        return File.Exists(ruta) && Tipos.TryGetValue(Path.GetExtension(ruta), out var tipo) ? (ruta, tipo) : null;
    }

    private static async Task<string> GuardarAsync(IFormFile archivo, string carpeta, bool permitirPdf)
    {
        var extension = Path.GetExtension(archivo.FileName).ToLowerInvariant();
        if (!Tipos.ContainsKey(extension) || (extension == ".pdf" && !permitirPdf))
            throw new OperacionException(permitirPdf
                ? "El comprobante debe ser una imagen (JPG, PNG, WEBP) o un PDF."
                : "La fotografía debe ser JPG, PNG o WEBP.");
        if (archivo.Length == 0 || archivo.Length > TamanoMaximo)
            throw new OperacionException("El archivo debe pesar como máximo 5 MB.");
        // Comprueba la firma del contenido para no aceptar archivos renombrados.
        var cabecera = new byte[12];
        await using (var lectura = archivo.OpenReadStream())
            await lectura.ReadAtLeastAsync(cabecera, cabecera.Length, throwOnEndOfStream: false);
        if (!FirmaValida(extension, cabecera))
            throw new OperacionException("El contenido del archivo no coincide con su formato.");
        var nombre = $"{Guid.NewGuid():N}{(extension == ".jpeg" ? ".jpg" : extension)}";
        var ruta = Path.Combine(carpeta,nombre);
        try
        {
            Directory.CreateDirectory(carpeta);
            await using var destino = File.Create(ruta);
            await archivo.CopyToAsync(destino);
            return nombre;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Eliminar(ruta);
            throw new OperacionException("No se pudo guardar el archivo. Revise los permisos de la carpeta y vuelva a adjuntarlo.");
        }
    }

    private static bool FirmaValida(string extension, byte[] c) => extension switch
    {
        ".jpg" or ".jpeg" => c[0] == 0xFF && c[1] == 0xD8 && c[2] == 0xFF,
        ".png" => c.AsSpan(0,8).SequenceEqual(new byte[] {137,80,78,71,13,10,26,10}),
        ".webp" => c[0] == 'R' && c[1] == 'I' && c[2] == 'F' && c[3] == 'F' && c[8] == 'W' && c[9] == 'E' && c[10] == 'B' && c[11] == 'P',
        ".pdf" => c[0] == '%' && c[1] == 'P' && c[2] == 'D' && c[3] == 'F' && c[4] == '-',
        _ => false
    };

    private static void Eliminar(string ruta)
    {
        try { if (File.Exists(ruta)) File.Delete(ruta); }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException) { /* Un archivo huérfano no afecta los datos. */ }
    }
}
