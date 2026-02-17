using FinSimAI.Hubs;

var builder = WebApplication.CreateBuilder(args);

// SignalR para comunicación en tiempo real
builder.Services.AddSignalR();

var app = builder.Build();

// Servir archivos estáticos (wwwroot/)
app.UseDefaultFiles();
app.UseStaticFiles();

// Endpoint de SignalR
app.MapHub<SimulationHub>("/simulationHub");

// Abrir el navegador automáticamente
var url = "http://localhost:5000";
app.Urls.Add(url);

Console.WriteLine($"\n  ╔══════════════════════════════════════════════╗");
Console.WriteLine($"  ║  FINSIM-AI: Servidor activo en {url}  ║");
Console.WriteLine($"  ╚══════════════════════════════════════════════╝");
Console.WriteLine($"  Abre tu navegador en: {url}\n");

app.Run();
