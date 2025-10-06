using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Hubs;
using System_EPS.Services;
using System.Text.Json.Serialization; // <-- AÑADIDO: Necesario para la opción de JSON

// Compatibilidad con Timestamps de Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- SECCIÓN DE REGISTRO DE SERVICIOS ---

// Registra los controladores y vistas, y configura la serialización de JSON
// para ignorar los bucles de referencias.
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Registra el DbContext para la conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// Registra el servicio de lógica de tiquetes para inyección de dependencias
builder.Services.AddScoped<ITicketService, TicketService>();

// Registra los servicios necesarios para que SignalR funcione
builder.Services.AddSignalR();


var app = builder.Build();

// --- SECCIÓN DE CONFIGURACIÓN DEL PIPELINE HTTP ---

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// app.UseHttpsRedirection(); // Desactivado para pruebas locales en http
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Mapea los API controllers para que estén disponibles en rutas como /api/Tickets
app.MapControllers();

// Mapea el Hub de SignalR a una ruta específica (/ticketHub)
app.MapHub<TicketsHub>("/ticketHub"); // <-- CORREGIDO: Nombre en singular 'TicketHub'

// Mapea la ruta por defecto para las vistas (MVC)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();