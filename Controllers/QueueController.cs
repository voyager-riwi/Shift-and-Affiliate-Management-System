using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System_EPS.Services;

namespace System_EPS.Controllers
{
    [Route("[controller]")]
    public class QueueController : Controller
    {
        private readonly ITicketService _ticketService;

        public QueueController(ITicketService ticketService)
        {
            _ticketService = ticketService;
        }

        // Este método ahora DELEGA al servicio en lugar de tener lógica propia
        [HttpPost("CallNext")]
        public async Task<IActionResult> CallNext([FromForm] int serviceDeskId)
        {
            if (serviceDeskId <= 0)
            {
                return BadRequest(new { message = "ID de puesto inválido." });
            }

            var ticket = await _ticketService.CallNextTicketAsync(serviceDeskId);
            
            if (ticket == null)
            {
                return NotFound(new { message = "No hay tickets en espera." });
            }

            // Importante: Incluir la información del puesto
            var deskNumber = ticket.ServiceDesk?.DeskNumber ?? "N/A";
            var affiliateName = ticket.Affiliate?.FullName ?? "Visitante";

            return Ok(new 
            {
                ticketCode = ticket.TicketCode,
                deskNumber = deskNumber,
                afiliado = affiliateName
            });
        }
    }
}