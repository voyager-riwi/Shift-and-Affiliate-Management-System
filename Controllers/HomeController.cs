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

    // El constructor recibe ILogger y ApplicationDbContext
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
    
    // Acción para cargar el panel de control del operador
    public async Task<IActionResult> Dashboard()
    {
        // Busca en la BD todos los puestos de atención que estén abiertos
        var serviceDesks = await _context.ServiceDesks
            .Where(d => d.Status == DeskStatus.Open)
            .ToListAsync();
            
        // Pasa la lista de puestos a la vista para construir el dropdown
        ViewBag.ServiceDesks = serviceDesks;

        return View();
    }

    // NUEVA ACCIÓN: Carga la pantalla del turnero (Display Board)
    // Se accede en /Home/TurnDisplay
    public IActionResult TurnDisplay()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}