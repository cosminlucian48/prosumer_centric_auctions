using System;
using ActressMas;
using DissertationProsumerAuctions;
using DissertationProsumerAuctions.Agents.Auctions.DutchAuctioneer;
using DissertationProsumerAuctions.Agents.EnergyMarket;
using DissertationProsumerAuctions.Agents.Prosumer;
using Microsoft.AspNetCore.Builder;
using Serilog;

class Program
{
    private static void Main(string[] args)
    {
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