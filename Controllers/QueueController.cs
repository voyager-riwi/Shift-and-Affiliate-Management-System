using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Enums;
using System_EPS.Hubs; 
using System_EPS.Models;

namespace System_EPS.Controllers
{
    // Define el controlador como una API para manejar la acción POST del Dashboard
    [Route("[controller]")]
    public class QueueController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<QueueHub> _hubContext; // Inyección de SignalR

        public QueueController(ApplicationDbContext context, IHubContext<QueueHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Endpoint para que el Dashboard llame al siguiente turno
        // Se accede mediante una petición POST a /Queue/CallNext
        [HttpPost("CallNext")]
        public async Task<IActionResult> CallNext([FromForm] int serviceDeskId)
        {
            // 1. Obtener el siguiente ticket en estado de espera, incluyendo el Afiliado
            var nextTicket = await _context.Tickets
                .Include(t => t.Affiliate) // <--- Crucial para obtener el FullName
                .Where(t => t.Status == TicketStatus.Waiting)
                .OrderBy(t => t.CreatedAt) // Prioriza el ticket más antiguo
                .FirstOrDefaultAsync();

            if (nextTicket == null)
            {
                // No hay tickets para llamar
                return NotFound(new { message = "No hay tickets en espera." });
            }

            // 2. Actualizar el estado del ticket y asignarlo al puesto
            var desk = await _context.ServiceDesks.FindAsync(serviceDeskId);
            if (desk == null) 
                return NotFound(new { message = "Puesto de servicio no encontrado." });

            nextTicket.Status = TicketStatus.InService;
            nextTicket.ServedAt = DateTime.Now; // Marca la hora de inicio de atención
            nextTicket.ServiceDeskId = serviceDeskId;
            await _context.SaveChangesAsync();
            
            // 3. PREPARAR Y ENVIAR LA SEÑAL a TurnDisplay a través de SignalR
            string ticketCode = nextTicket.TicketCode;
            string deskNumber = desk.DeskNumber.ToString(); 
            // Obtiene el nombre completo del afiliado o un valor por defecto
            string affiliateName = nextTicket.Affiliate?.FullName ?? "Afiliado Desconocido"; 

            // Envía la señal a todos los clientes suscritos (TurnDisplay.cshtml)
            await _hubContext.Clients.All.SendAsync("ReceiveNewCall", ticketCode, deskNumber);

            // 4. Devuelve el resultado en formato JSON al Dashboard (petición AJAX)
            return Ok(new 
            {
                ticketCode = ticketCode, 
                deskNumber = deskNumber,
                afiliado = affiliateName
            });
        }
    }
}