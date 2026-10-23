using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Workhub.Models;

namespace Workhub.Controllers;

public class HomeController : Controller
{
    [AllowAnonymous]
    public IActionResult Index() => View();

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() => View(new ErrorViewModel
    {
        RequestId = HttpContext.TraceIdentifier
    });
}
