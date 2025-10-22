using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System_EPS.Data;
using System_EPS.Models;

namespace System_EPS.Services
{
    public interface IPrintService
    {
        Task<bool> PrintTicketAsync(int ticketId);
    }

    public class PrintService : IPrintService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PrintService> _logger;

        public PrintService(ApplicationDbContext context, ILogger<PrintService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> PrintTicketAsync(int ticketId)
        {
            try
            {
                _logger.LogInformation($"🖨️ Iniciando impresión de ticket {ticketId}...");
                
                var ticket = await _context.Tickets
                    .Include(t => t.Affiliate)
                    .Include(t => t.ServiceDesk)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(t => t.Id == ticketId);

                if (ticket == null)
                {
                    _logger.LogError($"❌ Ticket {ticketId} no encontrado");
                    return false;
                }
                
                _logger.LogInformation($"✅ Ticket {ticket.TicketCode} encontrado, generando HTML...");

                string html = GenerateTicketHtml(ticket);

                string tempFolder = Path.GetTempPath();
                string fileName = $"ticket_{ticket.Id}_{DateTime.Now:yyyyMMddHHmmss}.html";
                string tempFile = Path.Combine(tempFolder, fileName);

                await File.WriteAllTextAsync(tempFile, html);
                _logger.LogInformation($"📄 Archivo temporal creado: {tempFile}");

                if (!OperatingSystem.IsWindows())
                {
                    _logger.LogWarning("⚠️ La impresión automática solo funciona en Windows");
                    _logger.LogInformation($"💡 Abre manualmente este archivo para imprimir: {tempFile}");
                    return false;
                }

                _logger.LogInformation("🖨️ Enviando a impresora...");
                
                string browser = GetDefaultBrowser();
                
                ProcessStartInfo psi = new ProcessStartInfo
                {
                    FileName = browser,
                    Arguments = $"\"{tempFile}\"",
                    UseShellExecute = false,
                    CreateNoWindow = true,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(psi);
                _logger.LogInformation($"✅ Navegador abierto: {browser}");
                
                await Task.Delay(3000);

                try
                {
                    if (process != null && !process.HasExited)
                    {
                        await Task.Delay(5000);
                        if (!process.HasExited)
                        {
                            process.Kill();
                        }
                    }
                }
                catch
                {
                    // Ignorar errores al cerrar el proceso
                }

                await Task.Delay(2000);

                try
                {
                    if (File.Exists(tempFile))
                    {
                        File.Delete(tempFile);
                    }
                }
                catch
                {
                    // Ignorar error al eliminar archivo temporal
                }

                _logger.LogInformation($"✅ Ticket {ticket.TicketCode} enviado a impresora");
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError($"❌ Error al imprimir ticket: {ex.Message}");
                return false;
            }
        }

        private string GetDefaultBrowser()
        {
            try
            {
                using var key = Microsoft.Win32.Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice");
                if (key != null)
                {
                    var progId = key.GetValue("ProgId")?.ToString();
                    if (!string.IsNullOrEmpty(progId))
                    {
                        using var progKey = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey($@"{progId}\shell\open\command");
                        if (progKey != null)
                        {
                            var path = progKey.GetValue("")?.ToString();
                            if (!string.IsNullOrEmpty(path))
                            {
                                path = path.Replace("\"", "").Split(new[] { ".exe" }, StringSplitOptions.None)[0] + ".exe";
                                return path.Trim();
                            }
                        }
                    }
                }
            }
            catch
            {
                // Si falla, usar rutas conocidas
            }

            string[] browsers = {
                @"C:\Program Files\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files (x86)\Google\Chrome\Application\chrome.exe",
                @"C:\Program Files\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe",
                @"C:\Program Files\Mozilla Firefox\firefox.exe",
                @"C:\Program Files (x86)\Mozilla Firefox\firefox.exe"
            };

            foreach (var browser in browsers)
            {
                if (File.Exists(browser))
                {
                    return browser;
                }
            }

            return "msedge.exe";
        }

        // ⭐ AQUÍ ESTÁ EL MÉTODO MODIFICADO
        private string GenerateTicketHtml(Ticket ticket)
        {
            var affiliateName = ticket.Affiliate?.FullName ?? "Visitante";
            var affiliateDoc = ticket.Affiliate?.DocumentId ?? "N/A";

            return $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
    <title>Ticket {ticket.TicketCode}</title>
    <style>
        @page {{
            size: 80mm auto;
            margin: 0;
        }}
        
        @media print {{
            body {{
                width: 80mm;
                margin: 0;
                padding: 0;
            }}
        }}
        
        * {{
            margin: 0;
            padding: 0;
            box-sizing: border-box;
        }}
        
        body {{
            font-family: 'Courier New', monospace;
            text-align: center;
            width: 80mm;
            padding: 8mm 5mm;
            font-size: 11px;
            line-height: 1.3;
            background: white;
        }}
        
        .logo {{
            font-size: 16px;
            font-weight: bold;
            margin-bottom: 2px;
        }}
        
        .subtitle {{
            font-size: 9px;
            margin-bottom: 8px;
        }}
        
        h2 {{
            font-size: 14px;
            margin: 8px 0 5px 0;
            font-weight: bold;
        }}
        
        .ticket-code {{
            font-size: 48px;
            font-weight: bold;
            margin: 8px 0;
            letter-spacing: 3px;
            border: 2px solid #000;
            padding: 8px;
            border-radius: 5px;
        }}
        
        .info-section {{
            text-align: left;
            margin: 8px 0;
        }}
        
        .info-row {{
            margin: 3px 0;
            font-size: 10px;
            display: flex;
        }}
        
        .info-row strong {{
            min-width: 55px;
            font-weight: bold;
        }}
        
        .info-row span {{
            flex: 1;
        }}
        
        .separator {{
            border-top: 1px dashed #000;
            margin: 6px 0;
        }}
        
        .footer {{
            margin-top: 8px;
            font-size: 9px;
            font-style: italic;
            line-height: 1.4;
        }}
        
        .ticket-id {{
            font-size: 8px;
            color: #666;
            margin-top: 5px;
        }}
    </style>
    <script>
        window.onload = function() {{
            setTimeout(function() {{
                window.print();
                setTimeout(function() {{
                    window.close();
                }}, 500);
            }}, 100);
        }};
    </script>
</head>
<body>
    <div class='logo'>🏥 EPS VOYAGER</div>
    <div class='subtitle'>Entidad Promotora de Salud</div>
    
    <div class='separator'></div>
    
    <h2>SU TURNO</h2>
    <div class='ticket-code'>{ticket.TicketCode}</div>
    
    <div class='separator'></div>
    
    <div class='info-section'>
        <div class='info-row'>
            <strong>Nombre:</strong>
            <span>{affiliateName}</span>
        </div>
        <div class='info-row'>
            <strong>Documento:</strong>
            <span>{affiliateDoc}</span>
        </div>
        <div class='info-row'>
            <strong>Fecha:</strong>
            <span>{ticket.CreatedAt:dd/MM/yyyy}</span>
        </div>
        <div class='info-row'>
            <strong>Hora:</strong>
            <span>{ticket.CreatedAt:HH:mm:ss}</span>
        </div>
    </div>
    
    <div class='separator'></div>
    
    <div class='footer'>
        <p>Por favor espere a que su turno</p>
        <p>sea llamado en la pantalla.</p>
        <p>¡Gracias por su paciencia!</p>
    </div>
    
    <div class='separator'></div>
    
    <div class='ticket-id'>Ticket ID: {ticket.Id}</div>
</body>
</html>";
        }
    }
}