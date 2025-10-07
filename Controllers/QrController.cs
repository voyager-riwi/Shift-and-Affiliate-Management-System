using Microsoft.AspNetCore.Mvc;
using QRCoder; // La librería que instalamos
using System.Drawing;
using System.IO;

namespace System_EPS.Controllers
{
    public class QrCodeController : Controller
    {
        [HttpGet]
        public IActionResult Generate(int id)
        {
            // La información que guardaremos en el QR es la URL para buscar a este afiliado.
            // Así, cualquier lector de QR (como la cámara de un celular) puede acceder a su info.
            string urlDeBusqueda = Url.Action("GetById", "Affiliates", new { id }, Request.Scheme);

            QRCodeGenerator qrGenerator = new QRCodeGenerator();
            QRCodeData qrCodeData = qrGenerator.CreateQrCode(urlDeBusqueda, QRCodeGenerator.ECCLevel.Q);
            
            PngByteQRCode qrCode = new PngByteQRCode(qrCodeData);
            byte[] qrCodeAsPng = qrCode.GetGraphic(20);

            // Devolvemos la imagen directamente al navegador
            return File(qrCodeAsPng, "image/png");
        }
    }
}