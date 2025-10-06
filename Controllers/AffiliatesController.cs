using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System.Threading.Tasks;

[ApiController]
[Route("api/[controller]")]
public class AffiliatesController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public AffiliatesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: /api/Affiliates/ByDocument/12345
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
}