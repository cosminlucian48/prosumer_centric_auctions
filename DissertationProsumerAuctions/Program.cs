using DissertationProsumerAuctions.Services;
using DissertationProsumerAuctions.Hubs;
using DissertationProsumerAuctions;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        theme: Serilog.Sinks.SystemConsole.Themes.AnsiConsoleTheme.Code
    )             // always write to console
    .WriteTo.Seq("http://localhost:5341")   // send logs to Seq (optional - fails gracefully if Seq not running)
    .CreateLogger();

builder.Host.UseSerilog();

// Add services
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddSingleton<SimulationService>();
builder.Services.AddSingleton<SimulationBroadcastService>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4200", "http://localhost:8080")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
app.UseCors("AllowFrontend");
app.UseRouting();
app.UseAuthorization();

app.MapControllers();
app.MapHub<SimulationHub>("/simulationHub");

Log.Information("Starting MAS simulation API...");
Log.Information($"API is running on: {app.Urls}");
Log.Information($"SignalR Hub available at: /simulationHub");
Log.Information($"API endpoints available at: /api/simulation and /api/data");

// Optional: Auto-initialize simulation on startup (comment out if you want manual initialization via API)
var simulationService = app.Services.GetRequiredService<SimulationService>();
simulationService.InitializeWorld(Utils.NumberOfProsumers);
Log.Information($"Auto-initialized simulation with {Utils.NumberOfProsumers} prosumers");

app.Run();