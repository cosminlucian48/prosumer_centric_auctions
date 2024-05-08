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

        var prosumerAgent1 = new ProsumerAgent();
        var prosumerAgent2 = new ProsumerAgent();
        var prosumerAgent3 = new ProsumerAgent();

        var energyMarketAgent = new EnergyMarketAgent();
        var dutchAuctioneerAgent = new DutchAuctioneerAgent();
        world.AddProsumer(prosumerAgent1, "prosumer1");
        world.AddProsumer(prosumerAgent2, "prosumer2");
        world.AddProsumer(prosumerAgent3, "prosumer3");
        world.Add(energyMarketAgent, "energymarket1");
        world.Add(dutchAuctioneerAgent, "dutchauctioneer");


        world.Start();
    }
}