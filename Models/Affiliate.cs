// En la carpeta: Models/Affiliate.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace System_EPS.Models;

public class Affiliate
{
    public int Id { get; set; }

    [Display(Name = "Nombre Completo")] 
    public string FullName { get; set; }

    [Display(Name = "Documento")]
    public string DocumentId { get; set; }

    [Display(Name = "Correo Electrónico")] 
    public string Email { get; set; }
    
    [Display(Name = "Número de Teléfono")] 
    public string PhoneNumber { get; set; }

    //Un afiliado puede tener una colección de tiquetes.
    public ICollection<Ticket> Tickets { get; set; }
}