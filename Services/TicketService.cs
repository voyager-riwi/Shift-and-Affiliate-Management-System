using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Enums;
using System_EPS.Hubs; // Asegúrate de que este 'using' esté presente
using System_EPS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace System_EPS.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _context;
    // CAMBIO: Usamos el nombre en singular 'TicketHub'
    private readonly IHubContext<TicketsHub> _hubContext;

    // CAMBIO: Usamos el nombre en singular 'TicketHub'
    public TicketService(ApplicationDbContext context, IHubContext<TicketsHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    // CAMBIO: Reemplazamos el método viejo por este, que acepta un string o nada.
    public async Task<Ticket> CreateNextTicketAsync(string documentId = null)
    {
        int? affiliateId = null;

        // Si se proveyó un documento, buscamos si el afiliado ya existe
        if (!string.IsNullOrWhiteSpace(documentId))
        {
            var affiliate = await _context.Affiliates
                .FirstOrDefaultAsync(a => a.DocumentId == documentId);
            if (affiliate != null)
            {
                affiliateId = affiliate.Id;
            }
        }

        var newTicket = new Ticket
        {
            TicketCode = $"A-{DateTime.Now.Ticks % 1000}",
            CreatedAt = DateTime.Now,
            Status = TicketStatus.Waiting,
            AffiliateId = affiliateId // Será el Id encontrado o null
        };

        _context.Tickets.Add(newTicket);
        await _context.SaveChangesAsync();
        
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
            nextTicket.Status = TicketStatus.Served;
            nextTicket.ServiceDeskId = serviceDeskId;
            nextTicket.ServedAt = DateTime.Now;
            await _context.SaveChangesAsync();
            
            await _hubContext.Clients.All.SendAsync("TicketCalled", nextTicket.TicketCode, serviceDeskId);
            await _hubContext.Clients.All.SendAsync("UpdateWaitingList");
        }

        return nextTicket;
    }
    
    public async Task<IEnumerable<Ticket>> GetWaitingTicketsAsync()
    {
        return await _context.Tickets
            .Where(t => t.Status == TicketStatus.Waiting)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTodaysHistoryAsync()
    {
        var today = DateTime.Today;
        var tomorrow = today.AddDays(1);

        return await _context.Tickets
            .Include(t => t.Affiliate)
            .Where(t => t.Status == TicketStatus.Served &&
                        t.ServedAt.HasValue &&
                        t.ServedAt.Value >= today &&
                        t.ServedAt.Value < tomorrow)
            .OrderByDescending(t => t.ServedAt)
            .ToListAsync();
    }
}