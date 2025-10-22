using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Enums;
using System_EPS.Hubs;
using System_EPS.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System;
using Microsoft.Extensions.DependencyInjection;

namespace System_EPS.Services;

public class TicketService : ITicketService
{
    private readonly ApplicationDbContext _context;
    private readonly IHubContext<QueueHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;

    public TicketService(
        ApplicationDbContext context, 
        IHubContext<QueueHub> hubContext,
        IServiceProvider serviceProvider)
    {
        _context = context;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
    }

    public async Task<Ticket> CreateNextTicketAsync(string documentId = null)
    {
        int? affiliateId = null;

        if (!string.IsNullOrWhiteSpace(documentId))
        {
            var affiliate = await _context.Affiliates
                .FirstOrDefaultAsync(a => a.DocumentId == documentId);
            if (affiliate != null)
            {
                affiliateId = affiliate.Id;
            }
        }

        var ticketCount = await _context.Tickets.CountAsync(t => t.CreatedAt.Date == DateTime.Today);
        var ticketNumber = ticketCount + 1;
        
        var newTicket = new Ticket
        {
            TicketCode = $"A{ticketNumber:D3}",
            CreatedAt = DateTime.Now,
            Status = TicketStatus.Waiting,
            AffiliateId = affiliateId
        };

        _context.Tickets.Add(newTicket);
        await _context.SaveChangesAsync();
        
        Console.WriteLine($"✅ Ticket creado: {newTicket.TicketCode} con ID: {newTicket.Id}");
        
        var ticketId = newTicket.Id;
        Console.WriteLine($"🖨️ Iniciando tarea de impresión en segundo plano para ticket ID: {ticketId}");
        
        // ⭐ CREAR EL SCOPE ANTES DEL TASK.RUN
        var scope = _serviceProvider.CreateScope();
        
        _ = Task.Run(async () => 
        {
            try
            {
                Console.WriteLine($"⏳ Esperando 500ms antes de imprimir...");
                await Task.Delay(500);
                
                Console.WriteLine($"🔧 Usando scope para impresión...");
                using (scope)
                {
                    var printService = scope.ServiceProvider.GetRequiredService<IPrintService>();
                    
                    Console.WriteLine($"📞 Llamando a PrintTicketAsync para ticket {ticketId}...");
                    await printService.PrintTicketAsync(ticketId);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ ERROR en tarea de impresión: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                scope?.Dispose();
            }
        });
        
        return newTicket;
    }

    public async Task<Ticket> CallNextTicketAsync(int serviceDeskId)
    {
        var nextTicket = await _context.Tickets
            .Include(t => t.Affiliate)
            .Where(t => t.Status == TicketStatus.Waiting)
            .OrderBy(t => t.CreatedAt)
            .FirstOrDefaultAsync();

        if (nextTicket == null) return null;

        var desk = await _context.ServiceDesks.FindAsync(serviceDeskId);
        if (desk == null) return null;

        nextTicket.Status = TicketStatus.InService;
        nextTicket.ServiceDeskId = serviceDeskId;
        nextTicket.ServedAt = DateTime.Now;
        
        await _context.SaveChangesAsync();
        
        await _context.Entry(nextTicket).Reference(t => t.ServiceDesk).LoadAsync();
        
        Console.WriteLine($"✅ Ticket {nextTicket.TicketCode} llamado en puesto {desk.DeskNumber}");
        
        return nextTicket;
    }
    
    public async Task<IEnumerable<Ticket>> GetWaitingTicketsAsync()
    {
        try
        {
            var tickets = await _context.Tickets
                .Include(t => t.Affiliate)
                .Include(t => t.ServiceDesk)
                .Where(t => t.Status == TicketStatus.Waiting)
                .OrderBy(t => t.CreatedAt)
                .ToListAsync();
            
            Console.WriteLine($"✅ GetWaitingTicketsAsync: {tickets.Count} tickets");
            return tickets;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en GetWaitingTicketsAsync: {ex.Message}");
            throw;
        }
    }

    public async Task<IEnumerable<Ticket>> GetTodaysHistoryAsync()
    {
        try
        {
            var today = DateTime.Today;
            var tomorrow = today.AddDays(1);

            var tickets = await _context.Tickets
                .Include(t => t.Affiliate)
                .Include(t => t.ServiceDesk)
                .Where(t => t.Status == TicketStatus.InService && 
                            t.ServedAt.HasValue &&
                            t.ServedAt.Value >= today &&
                            t.ServedAt.Value < tomorrow)
                .OrderByDescending(t => t.ServedAt)
                .ToListAsync();
            
            Console.WriteLine($"✅ GetTodaysHistoryAsync: {tickets.Count} tickets");
            return tickets;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Error en GetTodaysHistoryAsync: {ex.Message}");
            throw;
        }
    }
}