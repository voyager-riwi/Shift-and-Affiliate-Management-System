using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace System_EPS.Models;

public class Affiliate
{
    public int Id { get; set; }

    [Display(Name = "Nombre Completo")] 
    [Required(ErrorMessage = "El nombre completo es requerido")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "Documento")]
    [Required(ErrorMessage = "El documento es requerido")]
    public string DocumentId { get; set; } = string.Empty;

    [Display(Name = "Correo Electrónico")] 
    public string? Email { get; set; }
    
    [Display(Name = "Número de Teléfono")] 
    public string? PhoneNumber { get; set; }

    // Ignora esta propiedad al serializar/deserializar JSON
    [JsonIgnore]
    public ICollection<Ticket>? Tickets { get; set; }
    
    [Display(Name = "Foto")]
    public string? PhotoBase64 { get; set; }
}