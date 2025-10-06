using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Enums;
using System_EPS.Models;

namespace System_EPS.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly ApplicationDbContext _context;

    public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
    {
        _logger = logger;
        _context = context;
    }

    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }
    
    public IActionResult Kiosk()
    {
        return View();
    }
    
    public async Task<IActionResult> Dashboard()
    {
        var serviceDesks = await _context.ServiceDesks
            .Where(d => d.Status == DeskStatus.Open)
            .ToListAsync();
            
        ViewBag.ServiceDesks = serviceDesks;

        return View();
    }

    // ▼▼▼ MÉTODO CLAVE ▼▼▼
    // Asegúrate de que este método exista.
    public IActionResult RegisterAffiliate()
    {
        return View();
    }
    // ▲▲▲ FIN DEL MÉTODO CLAVE ▲▲▲

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}