using System.Collections.Generic;
using System.Threading.Tasks;
using System_EPS.Models;

namespace System_EPS.Services;

public interface ITicketService
{
    // CAMBIO: La firma ahora acepta un string opcional
    Task<Ticket> CreateNextTicketAsync(string documentId = null);

    Task<Ticket> CallNextTicketAsync(int serviceDeskId);
    Task<IEnumerable<Ticket>> GetWaitingTicketsAsync();
    Task<IEnumerable<Ticket>> GetTodaysHistoryAsync();
}