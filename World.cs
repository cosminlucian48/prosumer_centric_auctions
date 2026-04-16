using ActressMas;
using ProsumerAuctionPlatform.Agents.Prosumer;
using ProsumerAuctionPlatform.Agents.Support;
using ProsumerAuctionPlatform.Constants;
using System;
using ProsumerAuctionPlatform.Agents.EnergyMarket;
using ProsumerAuctionPlatform.Agents.Prosumer.Components;
using ProsumerAuctionPlatform.Models;

namespace ProsumerAuctionPlatform
{
    internal class World : EnvironmentMas
    {
        private readonly int _simulationDelayMs;

        public World(int numberOfTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null, bool parallel = true)
            : base(numberOfTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
            _simulationDelayMs = delayAfterTurn;

            var energyMarketAgent = new EnergyMarketAgent();
            Add(energyMarketAgent, AgentNames.EnergyMarket);
            
            var tickAgent = new TickAgent();
            Add(tickAgent, AgentNames.Tick);
        }
        public void AddProsumer(Agent prosumer, string prosumerName, ProsumerCapabilities capabilities)
        {
            base.Add(prosumer, prosumerName);

            if (capabilities.HasLoad)
            {
                var loadAgent = new ProsumerLoadAgent(prosumerName);
                base.Add(loadAgent, AgentNames.GetLoadName(prosumerName));
            }

            if (capabilities.HasGenerator)
            {
                var generatorAgent = new ProsumerGeneratorAgent(prosumerName);
                base.Add(generatorAgent, AgentNames.GetGeneratorName(prosumerName));
            }

            if (capabilities.HasBattery)
            {
                var batteryAgent = new ProsumerBatteryAgent(prosumerName, _simulationDelayMs);
                base.Add(batteryAgent, AgentNames.GetBatteryName(prosumerName));
            }
        }

        public void Add(Agent agent, string agentName)
        {
            base.Add(agent, agentName);
        }
    }
}
