using System;
using ActressMas;
using DissertationProsumerAuctions;
using DissertationProsumerAuctions.Agents.Auctions.DutchAuctioneer;
using DissertationProsumerAuctions.Agents.EnergyMarket;
using DissertationProsumerAuctions.Agents.Prosumer;

class Program
{
    private static void Main(string[] args)
    {
        //DataAccess.InitializeDatabase();
        var world = new World(0, Utils.Delay);

        var energyMarketAgent = new EnergyMarketAgent();
        var dutchAuctioneerAgent = new DutchAuctioneerAgent();

        for(int i=1;i <= Utils.NumberOfProsumers; i++)
        {
            var prosumerAgent = new ProsumerAgent();
            world.AddProsumer(prosumerAgent, $"prosumer{i}");
        }


        world.Add(energyMarketAgent, "energymarket1");
        world.Add(dutchAuctioneerAgent, "dutchauctioneer");


        world.Start();
    }
}