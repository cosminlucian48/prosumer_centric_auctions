using System;
using System.IO;
using ActressMas;
using ProsumerAuctionPlatform;
using ProsumerAuctionPlatform.Agents.Auctions.DutchAuctioneer;
using ProsumerAuctionPlatform.Agents.EnergyMarket;
using ProsumerAuctionPlatform.Agents.Prosumer;
using ProsumerAuctionPlatform.DatabaseConnections;
using ProsumerAuctionPlatform.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

class Program
{
    private static void Main(string[] args)
    {
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        // Configuration precedence: appsettings -> appsettings.{env} -> env vars -> user secrets.
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile($"appsettings.{environment}.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
#if DEBUG
            .AddUserSecrets<Program>(optional: true)
#endif
            .Build();

        // Initialize configuration service
        var configService = new ConfigurationService(configuration);
        configService.Validate();

        // Ensure database connection uses the typed config source only.
        DatabaseConnection.Configure(configService.DbPath);

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()             // still see logs in terminal
            .WriteTo.Seq(configService.SeqUrl)   // send logs to Seq
            .CreateLogger();

        Log.Information("Seq sink URL: {SeqUrl}", configService.SeqUrl);
        Log.Information("Environment profile: {Environment}", environment);

        Log.Information("Starting MAS simulation...");

        // Initialize MAS environment
        var world = new World(0, configService.Delay);

        var prosumerDefinitions = configService.GetProsumerDefinitions();

        
        var dutchAuctioneerAgent = new DutchAuctioneerAgent(configService.EnergyMarketParticipantsSignUpInterval);

        foreach (var prosumer in prosumerDefinitions)
        {
            var prosumerAgent = new ProsumerAgent(prosumer.Capabilities);
            world.AddProsumer(prosumerAgent, prosumer.Name, prosumer.Capabilities);
        }
        
        // world.Add(dutchAuctioneerAgent);

        Log.Information($"Starting MAS world {Utils.CorrelationId}...");
        world.Start();

        Log.Information("MAS simulation finished.");
    }
}