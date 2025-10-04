using System_EPS.Models;

namespace System_EPS.Services;

public interface ITicketService
{
    // Método para que el kiosko genere un nuevo tiquete
    Task<Ticket> CreateNextTicketAsync(int affiliateId);

    // Método para que el funcionario llame al siguiente tiquete en espera
    Task<Ticket> CallNextTicketAsync(int serviceDeskId);

    // Método para obtener la lista de tiquetes en espera para la TV
    Task<IEnumerable<Ticket>> GetWaitingTicketsAsync();
}