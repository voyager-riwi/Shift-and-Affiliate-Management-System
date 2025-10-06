using Microsoft.AspNetCore.Mvc;
using System_EPS.Services;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet]
    public async Task<IActionResult> GetWaitingTickets()
    {
        var tickets = await _ticketService.GetWaitingTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("history/today")]
    public async Task<IActionResult> GetTodaysHistory()
    {
        var history = await _ticketService.GetTodaysHistoryAsync();
        return Ok(history);
    }

    // ▼▼▼ MÉTODO CORREGIDO ▼▼▼
    [HttpPost("{documentId?}")]
    public async Task<IActionResult> CreateTicket(string documentId = null) // Se añade '= null' para hacerlo opcional
    {
        var ticket = await _ticketService.CreateNextTicketAsync(documentId);
        return Ok(ticket);
    }

    [HttpPost("next/{serviceDeskId}")]
    public async Task<IActionResult> CallNextTicket(int serviceDeskId)
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