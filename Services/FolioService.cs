namespace Workhub.Services;
public class FolioService : IFolioService
{
    public string Generar() => $"R-{RelojBolivia.Ahora:yyyyMMdd}-{Guid.NewGuid():N}"[..30].ToUpperInvariant();
}
