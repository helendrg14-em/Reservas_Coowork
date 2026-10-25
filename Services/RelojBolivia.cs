namespace Workhub.Services;

public static class RelojBolivia
{
    // Se almacenan horas locales de Bolivia, sin depender de la zona del servidor.
    public static DateTime Ahora => DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-4), DateTimeKind.Unspecified);
}
