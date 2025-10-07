using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System_EPS.Data;

namespace System_EPS.Controllers
{
    public class PrintController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PrintController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Vista para el tiquete de turno
        public async Task<IActionResult> Ticket(int id)
        {
            var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();
            return View("~/Views/Print/Ticket.cshtml", ticket);
        }

        // NUEVA VISTA para el carnet de afiliado
        public async Task<IActionResult> Carnet(int id)
        {
            var affiliate = await _context.Affiliates.FindAsync(id);
            if (affiliate == null) return NotFound();
            return View("~/Views/Print/Carnet.cshtml", affiliate);
        }
    }
}