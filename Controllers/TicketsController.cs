using Microsoft.AspNetCore.Mvc;
using System_EPS.Services;

[ApiController]
[Route("api/[controller]")] // Esto asigna la ruta /api/Tickets
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    // El constructor recibe el servicio de lógica gracias a la inyección de dependencias
    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    // GET: /api/Tickets
    // Devuelve la lista de tiquetes en espera (útil para la TV)
    [HttpGet]
    public async Task<IActionResult> GetWaitingTickets()
    {
        var tickets = await _ticketService.GetWaitingTicketsAsync();
        return Ok(tickets);
    }

    // POST: /api/Tickets
    // Crea un nuevo tiquete (lo usará el kiosko)
    [HttpPost("{affiliateId}")] 
    public async Task<IActionResult> CreateTicket(int affiliateId)
    {
        if (affiliateId <= 0)
        {
            return BadRequest("El Id del afiliado no es válido.");
        }
        var ticket = await _ticketService.CreateNextTicketAsync(affiliateId);
        return Ok(ticket);
    }

    // POST: /api/Tickets/next
    // Llama al siguiente tiquete en la fila (lo usará el funcionario)
    [HttpPost("next")]
    public async Task<IActionResult> CallNextTicket([FromBody] int serviceDeskId)
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
        return Ok(ticket);
    }
}