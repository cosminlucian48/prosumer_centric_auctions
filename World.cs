using ActressMas;
using DissertationProsumerAuctions.Agents.Prosumer;
using DissertationProsumerAuctions.Agents.Support;
using DissertationProsumerAuctions.Constants;
using System;
using DissertationProsumerAuctions.Agents.EnergyMarket;
using DissertationProsumerAuctions.Agents.Prosumer.Components;

namespace DissertationProsumerAuctions
{
    internal class World : EnvironmentMas
    {
        public World(int numberOfTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null, bool parallel = true)
            : base(numberOfTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
            var energyMarketAgent = new EnergyMarketAgent();
            Add(energyMarketAgent, AgentNames.EnergyMarket);
            
            var tickAgent = new TickAgent();
            Add(tickAgent, AgentNames.Tick);
        }
        public void AddProsumer(Agent prosumer, string prosumerName)
        {
            base.Add(prosumer, prosumerName);

            var batteryAgent = new ProsumerBatteryAgent(prosumerName);
            var generatorAgent = new ProsumerGeneratorAgent(prosumerName);
            var loadAgent = new ProsumerLoadAgent(prosumerName);
            base.Add(batteryAgent, AgentNames.GetBatteryName(prosumerName));
            base.Add(generatorAgent, AgentNames.GetGeneratorName(prosumerName));
            base.Add(loadAgent, AgentNames.GetLoadName(prosumerName));
        }

        public void Add(Agent agent, string agentName)
        {
            base.Add(agent, agentName);
        }
    }
}
