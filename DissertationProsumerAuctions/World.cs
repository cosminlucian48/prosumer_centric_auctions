using ActressMas;
using DissertationProsumerAuctions.Agents.Prosumer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions
{
    internal class World : EnvironmentMas
    {
        public World(int numberOfTurns = 0, int delayAfterTurn = 0, bool randomOrder = true, Random rand = null, bool parallel = true)
            : base(numberOfTurns, delayAfterTurn, randomOrder, rand, parallel)
        {
        }
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
