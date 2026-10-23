using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Services;

var builder = WebApplication.CreateBuilder(args);

var connectionString = builder.Configuration.GetConnectionString("WorkHub");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Falta configurar la cadena de conexión 'WorkHub'.");
}

// Toda la aplicación exige sesión iniciada salvo las acciones marcadas con [AllowAnonymous].
builder.Services.AddControllersWithViews(options =>
{
    var politica = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
    options.Filters.Add(new AuthorizeFilter(politica));
});
builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseSqlServer(connectionString));
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Cuenta/Login";
        options.LogoutPath = "/Cuenta/Logout";
        options.AccessDeniedPath = "/Cuenta/AccesoDenegado";
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Cookie.HttpOnly = true;
    });
builder.Services.AddScoped<IPasswordService, PasswordService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<IReservaService, ReservaService>();
builder.Services.AddScoped<ICostoService, CostoService>();
builder.Services.AddScoped<IFolioService, FolioService>();

var app = builder.Build();

// Carga los datos iniciales solo cuando la base ya fue creada con las migraciones.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationContext>();
    if (db.Database.GetAppliedMigrations().Any())
    {
        SeedData.Initialize(scope.ServiceProvider);
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
