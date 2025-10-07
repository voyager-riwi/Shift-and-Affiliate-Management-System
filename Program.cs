using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Hubs;
using System_EPS.Services;
using System.Text.Json.Serialization;

// Importante: La clase TicketService y TicketsHub deben existir.
// Si TicketsHub ya existe, puedes renombrarlo a QueueHub o usarlo.
// Aquí asumiremos que debemos usar QueueHub para la lógica de cola.

// Compatibilidad con Timestamps de Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- SECCIÓN DE REGISTRO DE SERVICIOS ---

// Registra los controladores y vistas, y configura la serialización de JSON
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

app.UseHttpsRedirection(); // Desactivado para pruebas locales en http
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Mapea los API controllers para que estén disponibles en rutas como /api/Tickets
app.MapControllers();

// Mapea la ruta por defecto para las vistas (MVC)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// CORRECCIÓN VITAL: Mapea el Hub de la cola (QueueHub) a la ruta /queueHub
// Esto coincide con el IHubContext<QueueHub> del controlador y el JS de la vista.
app.MapHub<QueueHub>("/queueHub"); 

app.Run();
