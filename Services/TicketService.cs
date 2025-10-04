using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Enums;
using System_EPS.Models;
using System_EPS.Hubs;
namespace System_EPS.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<TicketsHub> _hubContext;

    public TicketService(ApplicationDbContext context, IHubContext<TicketsHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<Ticket> CreateNextTicketAsync(int affiliateId)
    {
        // Lógica para crear un nuevo tiquete (simplificada)
        var newTicket = new Ticket
        {
            // logica pkara generar turno ramdom
            TicketCode = $"A-{DateTime.Now.Ticks % 1000}", 
            CreatedAt = DateTime.UtcNow,
            Status = TicketStatus.Waiting,
            AffiliateId = affiliateId 
      
        };

        _context.Tickets.Add(newTicket);
        await _context.SaveChangesAsync();

        // Anunciar a todos los clientes (la TV) que la lista de espera cambió
        await _hubContext.Clients.All.SendAsync("UpdateWaitingList");

        return newTicket;
    }

    public async Task<Ticket> CallNextTicketAsync(int serviceDeskId)
    {
        var nextTicket = await _context.Tickets
            .Where(t => t.Status == TicketStatus.Waiting)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (nextTicket != null)
        {
            nextTicket.Status = TicketStatus.Called;
            nextTicket.ServiceDeskId = serviceDeskId;
            nextTicket.ServedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            // Anunciar el tiquete llamado y actualizar la lista
            await _hubContext.Clients.All.SendAsync("TicketCalled", nextTicket.TicketCode, serviceDeskId);
            await _hubContext.Clients.All.SendAsync("UpdateWaitingList");
        }

        return nextTicket;
    }
    
    public async Task<IEnumerable<Ticket>> GetWaitingTicketsAsync()
    {
        // Busca en la base de datos todos los tiquetes cuyo estado sea "Waiting",
        // los ordena por fecha de creación y los devuelve como una lista.
        return await _context.Tickets
            .Where(t => t.Status == TicketStatus.Waiting)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }
}