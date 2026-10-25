using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Workhub.Services;

public static class ErroresBaseDatos
{
    public static bool Es(Exception e) => Transacciones.EsErrorDatos(e);
}
