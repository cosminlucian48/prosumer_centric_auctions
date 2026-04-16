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
        private readonly double _batteryInitialStateOfChargePercent;
        private readonly double _batteryMaximumCapacity;
        private readonly double _batteryChargingEfficiency;
        private readonly double _batteryDischargingEfficiency;

        public World(
            int numberOfTurns = 0,
            int delayAfterTurn = 0,
            bool randomOrder = true,
            Random rand = null,
            bool parallel = true,
            double batteryInitialStateOfChargePercent = 0.5,
            double batteryMaximumCapacity = 15.0,
            double batteryChargingEfficiency = 1.0,
            double batteryDischargingEfficiency = 1.0)
            : base(numberOfTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
            _batteryInitialStateOfChargePercent = batteryInitialStateOfChargePercent;
            _batteryMaximumCapacity = batteryMaximumCapacity;
            _batteryChargingEfficiency = batteryChargingEfficiency;
            _batteryDischargingEfficiency = batteryDischargingEfficiency;

            var energyMarketAgent = new EnergyMarketAgent();
            Add(energyMarketAgent, AgentNames.EnergyMarket);
            
            var tickAgent = new TickAgent();
            Add(tickAgent, AgentNames.Tick);
        }
        public void AddProsumer(
            Agent prosumer,
            string prosumerName,
            ProsumerCapabilities capabilities,
            ProsumerBatteryOverrides? batteryOverrides = null)
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
                double initialStateOfChargePercent = batteryOverrides?.InitialStateOfChargePercent ?? _batteryInitialStateOfChargePercent;
                double maximumCapacity = batteryOverrides?.MaximumCapacity ?? _batteryMaximumCapacity;
                double chargingEfficiency = batteryOverrides?.ChargingEfficiency ?? _batteryChargingEfficiency;
                double dischargingEfficiency = batteryOverrides?.DischargingEfficiency ?? _batteryDischargingEfficiency;

                var batteryAgent = new ProsumerBatteryAgent(
                    prosumerName,
                    initialStateOfChargePercent,
                    maximumCapacity,
                    chargingEfficiency,
                    dischargingEfficiency);
                base.Add(batteryAgent, AgentNames.GetBatteryName(prosumerName));
            }
        }

        public void Add(Agent agent, string agentName)
        {
            base.Add(agent, agentName);
        }
    }
}
