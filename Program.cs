using Microsoft.EntityFrameworkCore;
using Workhub.Data;
using Workhub.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();

var app = builder.Build();

app.UseStaticFiles();
app.MapControllerRoute(
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.Run();

public partial class Program { }
