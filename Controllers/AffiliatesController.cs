using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Models;
using System.Threading.Tasks;
using System.Linq;

namespace System_EPS.Controllers; 

[ApiController]
[Route("api/[controller]")]
public class AffiliatesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AffiliatesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/Affiliates/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        try
        {
            var affiliate = await _context.Affiliates.FindAsync(id);
            
            if (affiliate == null)
            {
                return NotFound(new { message = $"No se encontró un afiliado con el ID {id}." });
            }
            
            return Ok(affiliate);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al obtener el afiliado", error = ex.Message });
        }
    }
    
    // GET: /api/Affiliates/ByDocument/{documentId}
    [HttpGet("ByDocument/{documentId}")]
    public async Task<IActionResult> GetByDocument(string documentId)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(documentId))
            {
                return BadRequest(new { message = "El número de documento es requerido." });
            }

            var affiliate = await _context.Affiliates
                .FirstOrDefaultAsync(a => a.DocumentId == documentId);

            if (affiliate == null)
            {
                return NotFound(new { message = $"No se encontró un afiliado con el documento '{documentId}'." });
            }

            return Ok(affiliate);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al buscar el afiliado", error = ex.Message });
        }
    }

    // POST: /api/Affiliates
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Affiliate affiliate)
    {
        try
        {
            // Validar el modelo
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(new { message = "Errores de validación", errors });
            }

            // Validar que no exista otro afiliado con el mismo documento
            var existingAffiliate = await _context.Affiliates
                .FirstOrDefaultAsync(a => a.DocumentId == affiliate.DocumentId);
            
            if (existingAffiliate != null)
            {
                return BadRequest(new { message = $"Ya existe un afiliado con el documento '{affiliate.DocumentId}'." });
            }

            // Inicializar la colección de Tickets como vacía
            affiliate.Tickets = new List<Ticket>();

            // Guardar en la base de datos
            _context.Affiliates.Add(affiliate);
            await _context.SaveChangesAsync();
            
            return CreatedAtAction(nameof(GetById), new { id = affiliate.Id }, affiliate);
        }
        catch (DbUpdateException dbEx)
        {
            return StatusCode(500, new { message = "Error al guardar en la base de datos", error = dbEx.InnerException?.Message ?? dbEx.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al crear el afiliado", error = ex.Message });
        }
    }

    // PUT: /api/Affiliates/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] Affiliate affiliate)
    {
        try
        {
            if (id != affiliate.Id)
            {
                return BadRequest(new { message = "El ID de la URL no coincide con el ID del afiliado." });
            }

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();
                
                return BadRequest(new { message = "Errores de validación", errors });
            }

            // Verificar que el afiliado existe
            var existingAffiliate = await _context.Affiliates.FindAsync(id);
            if (existingAffiliate == null)
            {
                return NotFound(new { message = $"No se encontró un afiliado con el ID {id}." });
            }

            _context.Entry(affiliate).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!await AffiliateExists(id))
            {
                return NotFound(new { message = $"El afiliado con ID {id} ya no existe." });
            }
            throw;
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al actualizar el afiliado", error = ex.Message });
        }
    }

    // DELETE: /api/Affiliates/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var affiliate = await _context.Affiliates.FindAsync(id);
            
            if (affiliate == null)
            {
                return NotFound(new { message = $"No se encontró un afiliado con el ID {id}." });
            }
            
            _context.Affiliates.Remove(affiliate);
            await _context.SaveChangesAsync();
            
            return NoContent();
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "Error al eliminar el afiliado", error = ex.Message });
        }
    }

    // GET: /api/Affiliates/test - Endpoint de prueba
    [HttpGet("test")]
    public IActionResult TestEndpoint()
    {
        return Ok(new { message = "El endpoint de prueba funciona correctamente!", timestamp = DateTime.Now });
    }

    // Método auxiliar para verificar si un afiliado existe
    private async Task<bool> AffiliateExists(int id)
    {
        return await _context.Affiliates.AnyAsync(e => e.Id == id);
    }
}