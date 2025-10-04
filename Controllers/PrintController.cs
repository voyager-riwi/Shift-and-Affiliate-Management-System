using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System_EPS.Data;

namespace System_EPS.Controllers
{
    public class PrintController : Controller
    {
        private readonly ApplicationDbContext _context;

        // El constructor recibe el contexto de la base de datos
        // gracias a la inyección de dependencias que configuramos en Program.cs
        public PrintController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Esta acción se activará con una URL como: /Print/Ticket/5
        public async Task<IActionResult> Ticket(int id)
        {
            // Busca el tiquete en la base de datos usando el Id
            var ticket = await _context.Tickets.FindAsync(id);

            // Si no se encuentra un tiquete con ese Id, devuelve un error 404
            if (ticket == null)
            {
                return NotFound();
            }

            // Si lo encuentra, devuelve la vista de impresión que creamos,
            // pasándole el objeto 'ticket' para que pueda mostrar sus datos.
            return View("~/Views/Print/Ticket.cshtml", ticket);
        }
    }
}