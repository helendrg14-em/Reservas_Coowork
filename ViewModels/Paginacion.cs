namespace Workhub.ViewModels;

// Parametro: nombre del valor en la URL (permite varias listas paginadas en una misma página).
// Ancla: id de la sección a la que vuelve el navegador al cambiar de página.
public record Paginacion(int Pagina, int TotalPaginas, int TotalRegistros, int TamanoPagina,
    string Parametro = "pagina", string? Ancla = null)
{
    public int Saltar => (Pagina - 1) * TamanoPagina;
    public bool Contiene(int indice) => indice >= Saltar && indice < Saltar + TamanoPagina;

    public static Paginacion Crear(int pagina, int totalRegistros, int tamanoPagina,
        string parametro = "pagina", string? ancla = null)
    {
        var paginas = Math.Max(1, (int)Math.Ceiling(totalRegistros / (double)tamanoPagina));
        return new(Math.Clamp(pagina, 1, paginas), paginas, totalRegistros, tamanoPagina, parametro, ancla);
    }
}
