using ActressMas;
using DissertationProsumerAuctions.Agents.Prosumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DissertationProsumerAuctions.Agents.EnergyMarket;
using DissertationProsumerAuctions.Agents.Prosumer.Components;
using DissertationProsumerAuctions.Agents.Support;

namespace DissertationProsumerAuctions
{
    public class World : EnvironmentMas
    {
        private TickAgent _tickAgent;
        private bool _hasStarted = false;
        
        public World(int numberOfTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null, bool parallel = true)
            : base(numberOfTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
            var energyMarketAgent = new EnergyMarketAgent();
            Add(energyMarketAgent, "energymarket1");
            
            _tickAgent = new TickAgent();
            Add(_tickAgent, "tick");
        }
        
        // Override Start to track that the world has been started
        public new void Start()
        {
            _hasStarted = true;
            base.Start();
        }
        
        // Public methods for controlling simulation speed and state
        public void StopSimulation()
        {
            _tickAgent?.StopTicking();
        }
        
        public void PauseSimulation()
        {
            _tickAgent?.PauseTicking();
        }
        
        public void ResumeSimulation()
        {
            _tickAgent?.ResumeTicking();
        }
        
        public void SetSimulationSpeed(int delayMs)
        {
            _tickAgent?.SetTickDelay(delayMs);
        }
        
        // Get current simulation state
        public bool IsSimulationRunning => _hasStarted && (_tickAgent?.IsRunning ?? false);
        public bool IsSimulationPaused => _tickAgent?.IsPaused ?? false;
        public int CurrentTickIndex => _tickAgent?.CurrentTickIndex ?? 0;
        public int CurrentTickDelay => _tickAgent?.TickDelayMs ?? 10000;
        public void AddProsumer(Agent prosumer, string prosumerName)
        {
            base.Add(prosumer, prosumerName);

            var batteryAgent = new ProsumerBatteryAgent(prosumerName);
            var generatorAgent = new ProsumerGeneratorAgent(prosumerName);
            var loadAgent = new ProsumerLoadAgent(prosumerName);
            base.Add(batteryAgent, string.Format("battery{0}", prosumerName));
            base.Add(generatorAgent, string.Format("generator{0}", prosumerName));
            base.Add(loadAgent, string.Format("load{0}", prosumerName));
        }

        public void Add(Agent agent, string agentName)
        {
            base.Add(agent, agentName);
        }
    }
}
