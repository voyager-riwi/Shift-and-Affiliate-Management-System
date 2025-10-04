// En la carpeta: Models/ServiceDesk.cs
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System_EPS.Enums; 
namespace System_EPS.Models;

public class ServiceDesk
{
    public int Id { get; set; }
    
    [Display(Name = "Número de Caja")]
    public string DeskNumber { get; set; } // Ejemplo: "Caja 01", "Ventanilla 3"
    
    [Display(Name = "Estado")]
    public DeskStatus Status { get; set; }

   //Una caja puede atender una colección de tiquetes.
    public ICollection<Ticket> Tickets { get; set; }
}