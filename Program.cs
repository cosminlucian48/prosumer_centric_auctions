using System;
using System.IO;
using ActressMas;
using ProsumerAuctionPlatform;
using ProsumerAuctionPlatform.Agents.Auctions.DutchAuctioneer;
using ProsumerAuctionPlatform.Agents.EnergyMarket;
using ProsumerAuctionPlatform.Agents.Prosumer;
using ProsumerAuctionPlatform.Services;
using Microsoft.Extensions.Configuration;
using Serilog;

class Program
{
    private static void Main(string[] args)
    {
        // Build configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        // Initialize configuration service
        var configService = new ConfigurationService(configuration);
        Utils.InitializeConfiguration(configService);
        // If SEQ_URL is not provided (for example local non-Docker execution),
        // use host-mapped Seq endpoint on localhost.
        var seqUrl = Environment.GetEnvironmentVariable("SEQ_URL");
        if (string.IsNullOrWhiteSpace(seqUrl))
        {
            seqUrl = "http://localhost:5341";
        }

        Log.Logger = new LoggerConfiguration()
            .Enrich.FromLogContext()
            .WriteTo.Console()             // still see logs in terminal
            .WriteTo.Seq(seqUrl)   // send logs to Seq
            .CreateLogger();

        Log.Information("Seq sink URL: {SeqUrl}", seqUrl);

        Log.Information("Starting MAS simulation...");

        // Initialize MAS environment
        var world = new World(0, Utils.Delay);

        
        var dutchAuctioneerAgent = new DutchAuctioneerAgent();

        for (int i = 1; i <= Utils.NumberOfProsumers; i++)
        {
            var prosumerAgent = new ProsumerAgent();
            world.AddProsumer(prosumerAgent, $"prosumer{i}");
        }
        
        // world.Add(dutchAuctioneerAgent);

        Log.Information($"Starting MAS world {Utils.CorrelationId}...");
        world.Start();

        Log.Information("MAS simulation finished.");
    }
}