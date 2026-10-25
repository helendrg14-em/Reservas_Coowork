using System.Globalization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Services;

var builder = WebApplication.CreateBuilder(args);
var connection = builder.Configuration.GetConnectionString("WorkHub")
    ?? throw new InvalidOperationException("Configure ConnectionStrings:WorkHub.");
builder.Services.AddControllersWithViews(options =>
    options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()));
builder.Services.AddDbContext<ApplicationContext>(options => options.UseSqlServer(connection));
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<ICostoService, CostoService>();
builder.Services.AddScoped<IFolioService, FolioService>();
builder.Services.AddScoped<ReservaGestionService>();
builder.Services.AddScoped<PagoService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<CodigoService>();
builder.Services.AddSingleton<ArchivoService>();
builder.Services.AddScoped<SessionEvents>();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.Cookie.Name = "WorkHub.Demo.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.EventsType = typeof(SessionEvents);
    });
builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.AddPolicy("Administracion", policy => policy.RequireRole("Administrador"));
});
var app = builder.Build();
try
{
    await DatosDemoSeeder.InicializarAsync(app.Services, app.Configuration, app.Environment);
    await EsquemaWorkHub.ActualizarAsync(app.Services);
}
catch (Exception ex) when (ex is Microsoft.Data.SqlClient.SqlException or InvalidOperationException)
{
    app.Logger.LogCritical(ex, "No se pudo preparar WorkHub. Revise LocalDB y la conexión. " +
        "En Windows puede iniciar la instancia con: sqllocaldb start MSSQLLocalDB. " +
        "La demo usa WorkHubDemoDb; no borre su base existente.");
    throw;
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
    app.UseHttpsRedirection();
}
var culture = (CultureInfo)CultureInfo.GetCultureInfo("es-BO").Clone();
culture.NumberFormat.NumberDecimalSeparator = ".";
culture.NumberFormat.NumberGroupSeparator = ",";
app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new(culture, culture),
    SupportedCultures = [culture], SupportedUICultures = [culture],
    RequestCultureProviders = []
});
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.Run();
public partial class Program { }
