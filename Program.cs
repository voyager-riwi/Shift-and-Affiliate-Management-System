using Microsoft.EntityFrameworkCore;
using System_EPS.Data;
using System_EPS.Hubs;
using System_EPS.Services;
using System.Text.Json.Serialization;

// Compatibilidad con Timestamps de Npgsql
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// ========================================
// SECCIÓN DE REGISTRO DE SERVICIOS
// ========================================

// Registra los controladores y vistas con configuración JSON
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        // IGNORAR CICLOS DE REFERENCIA en JSON
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

// Registra el DbContext para la conexión a la base de datos
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ========================================
// SERVICIOS PERSONALIZADOS
// ========================================

// Servicio de Tickets
builder.Services.AddScoped<ITicketService, TicketService>();

// ⭐ NUEVO: Servicio de Impresión
builder.Services.AddScoped<IPrintService, PrintService>();

// ========================================
// SIGNALR CON IGNORAR CICLOS
// ========================================

builder.Services.AddSignalR()
    .AddJsonProtocol(options =>
    {
        // IGNORAR CICLOS DE REFERENCIA en SignalR
        options.PayloadSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
        options.PayloadSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    });

var app = builder.Build();

// ========================================
// CONFIGURACIÓN DEL PIPELINE HTTP
// ========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

//COMENTAR ESTA LÍNEA para modo híbrido HTTP/HTTPS
// app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// Mapear API Controllers
app.MapControllers();

// Mapear rutas MVC por defecto
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

// Mapear Hub de SignalR (QueueHub)
app.MapHub<QueueHub>("/queueHub");

app.Run();