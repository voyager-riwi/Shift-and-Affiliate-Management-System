using Microsoft.AspNetCore.Mvc;

namespace Camera.Controllers;

public class CameraController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
    [HttpPost]
    public IActionResult UploadPhoto(string photoBase64)
    {
        if (string.IsNullOrEmpty(photoBase64))
            return BadRequest("No se recibió la foto");

        // Quitar el encabezado de data:image/png;base64
        var base64Data = photoBase64.Split(',')[1];
        var bytes = Convert.FromBase64String(base64Data);

        // Guardar en local -> Ver mas adelante alguna nube s3 etc
        var fileName = $"foto_{DateTime.Now.Ticks}.png";
        var filePath = Path.Combine("wwwroot/fotos", fileName);

        System.IO.File.WriteAllBytes(filePath, bytes);

        ViewBag.Message = $"Fotico guardada en: {filePath}";
        return View("Index");
    }
}