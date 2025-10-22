using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Hubs;
using System_EPS.Services;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace System_EPS.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private readonly ITicketService _ticketService;
        private readonly IHubContext<QueueHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public TicketsController(
            ITicketService ticketService, 
            IHubContext<QueueHub> hubContext,
            ApplicationDbContext context)
        {
            _ticketService = ticketService;
            _hubContext = hubContext;
            _context = context;
        }

        // ✅ CAMBIO: Agregar ruta explícita
        [HttpGet]
        [HttpGet("waiting")] // ← Agregar esta línea para compatibilidad
        public async Task<IActionResult> GetWaitingTickets()
        {
            try
            {
                var tickets = await _ticketService.GetWaitingTicketsAsync();
                return Ok(tickets);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetWaitingTickets: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.StackTrace });
            }
        }

        [HttpGet("history/today")]
        public async Task<IActionResult> GetTodaysHistory()
        {
            try
            {
                var history = await _ticketService.GetTodaysHistoryAsync();
                return Ok(history);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en GetTodaysHistory: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { error = ex.Message, details = ex.StackTrace });
            }
        }

        [HttpPost("{documentId?}")]
        public async Task<IActionResult> CreateTicket(string documentId = null)
        {
            try
            {
                var ticket = await _ticketService.CreateNextTicketAsync(documentId);
                
                var waitingList = await _ticketService.GetWaitingTicketsAsync();
                await _hubContext.Clients.All.SendAsync("UpdateWaitingList", waitingList);

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CreateTicket: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("next/{serviceDeskId}")]
        public async Task<IActionResult> CallNextTicket(int serviceDeskId)
        {
            try
            {
                if (serviceDeskId <= 0)
                {
                    return BadRequest("El Id del puesto de atención no es válido.");
                }

                var ticket = await _ticketService.CallNextTicketAsync(serviceDeskId);
                if (ticket == null)
                {
                    return NotFound("No hay tiquetes en espera.");
                }

                await _hubContext.Clients.All.SendAsync("ReceiveNewCall", ticket);
                var waitingList = await _ticketService.GetWaitingTicketsAsync();
                await _hubContext.Clients.All.SendAsync("UpdateWaitingList", waitingList);

                Console.WriteLine($"✅ Turno {ticket.TicketCode} llamado correctamente");

                return Ok(ticket);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en CallNextTicket: {ex.Message}");
                return StatusCode(500, new { error = ex.Message });
            }
        }

        [HttpPost("reset")]
        public async Task<IActionResult> ResetSystem()
        {
            try
            {
                // Eliminar TODOS los tickets sin filtrar
                var allTickets = await _context.Tickets.ToListAsync();

                _context.Tickets.RemoveRange(allTickets);
                await _context.SaveChangesAsync();

                await _hubContext.Clients.All.SendAsync("SystemReset");

                Console.WriteLine($"✅ Sistema reiniciado: {allTickets.Count} tickets eliminados");

                return Ok(new
                {
                    success = true,
                    message = "Sistema reiniciado correctamente",
                    deletedCount = allTickets.Count
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Error en ResetSystem: {ex.Message}");
                return StatusCode(500, new
                {
                    success = false,
                    message = "Error al reiniciar el sistema",
                    error = ex.Message
                });
            }
        }
    }
}