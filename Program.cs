using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Hubs;    
using System_EPS.Services;  

// Compatibilidad con Timestamps de Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// --- SECCIÓN DE REGISTRO DE SERVICIOS ---

// Registra los controladores de vistas (MVC)
builder.Services.AddControllersWithViews();

// Registra el DbContext para la conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// --- CÓDIGO NUEVO AÑADIDO ---
// Registra el servicio de lógica de tiquetes para inyección de dependencias
builder.Services.AddScoped<ITicketService, TicketService>();

// Registra los servicios necesarios para que SignalR funcione
builder.Services.AddSignalR();
// --- FIN DEL CÓDIGO NUEVO ---

var app = builder.Build();

// --- SECCIÓN DE CONFIGURACIÓN DEL PIPELINE HTTP ---

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

// --- CÓDIGO NUEVO AÑADIDO ---
// Mapea los API controllers para que estén disponibles en rutas como /api/Tickets
app.MapControllers(); 

// Mapea el Hub de SignalR a una ruta específica (/ticketHub)
app.MapHub<TicketsHub>("/ticketHub");
// --- FIN DEL CÓDIGO NUEVO ---

// Mapea la ruta por defecto para las vistas (MVC)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();