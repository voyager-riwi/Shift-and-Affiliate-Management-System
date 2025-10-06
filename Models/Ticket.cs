// En la carpeta: Models/Ticket.cs
using System;
using System.ComponentModel.DataAnnotations;
using System_EPS.Enums;

namespace System_EPS.Models;

public class Ticket
{
    public int Id { get; set; }

    [Display(Name = "Código del Tiquete")]
    public string TicketCode { get; set; }

    [Display(Name = "Fecha de Creación")] 
    public DateTime CreatedAt { get; set; }

    [Display(Name = "Fecha de Atención")] 
    public DateTime? ServedAt { get; set; }
    
    [Display(Name = "Estado")] 
    public TicketStatus Status { get; set; }

    // --- Relaciones y Claves Foráneas ---

    [Display(Name = "Afiliado")] 
    public int? AffiliateId { get; set; }
    public Affiliate Affiliate { get; set; }
    
    [Display(Name = "Puesto de Atención")]
    public int? ServiceDeskId { get; set; }
    public ServiceDesk ServiceDesk { get; set; }
}