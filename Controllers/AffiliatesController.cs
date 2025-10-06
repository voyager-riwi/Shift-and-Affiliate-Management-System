using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Models;
using System.Threading.Tasks;

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
        var affiliate = await _context.Affiliates.FindAsync(id);
        if (affiliate == null)
        {
            return NotFound();
        }
        return Ok(affiliate);
    }
    
    // GET: /api/Affiliates/ByDocument/{documentId}
    [HttpGet("ByDocument/{documentId}")]
    public async Task<IActionResult> GetByDocument(string documentId)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return BadRequest("El número de documento es requerido.");
        }

        var affiliate = await _context.Affiliates
            .FirstOrDefaultAsync(a => a.DocumentId == documentId);

        if (affiliate == null)
        {
            return NotFound($"No se encontró un afiliado con el documento '{documentId}'.");
        }

        return Ok(affiliate);
    }

    // POST: /api/Affiliates
    [HttpPost]
    public async Task<IActionResult> Create(Affiliate affiliate)
    {
        _context.Affiliates.Add(affiliate);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = affiliate.Id }, affiliate);
    }

    // PUT: /api/Affiliates/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Affiliate affiliate)
    {
        if (id != affiliate.Id)
        {
            return BadRequest();
        }
        _context.Entry(affiliate).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: /api/Affiliates/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var affiliate = await _context.Affiliates.FindAsync(id);
        if (affiliate == null)
        {
            return NotFound();
        }
        _context.Affiliates.Remove(affiliate);
        await _context.SaveChangesAsync();
        return NoContent();
    }
    
    [HttpPost("test")]
    public IActionResult TestEndpoint()
    {
        return Ok("El endpoint de prueba funciona!");
    }
}