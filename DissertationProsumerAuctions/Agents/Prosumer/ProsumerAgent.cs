using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.Agents.Prosumer
{
    internal class ProsumerAgent : Agent
    {
        public int counter = 0;
        public int prosumerId = 0;
        private double currentLoadEnergyTotal;
        private double currentGeneratedEnergyTotal;
        private double currentBatteryStorageCapacity;
        private double energyInTransit;

        private double currentBill = 0.0;

        private bool flagMessageFromLoadAgent = false;
        private bool flagMessageFromGeneratorAgent = false;

        public ProsumerAgent() : base() { }
        public override void Setup()
        {
            prosumerId = Int32.Parse(this.Name.Remove(0, 8));
            Console.WriteLine("[{0} {1}] Hi", this.Name, prosumerId);

        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
            Console.WriteLine("Energy in transit {0}; current bill {1}", this.energyInTransit, this.currentBill);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "started":
                    break;
                case "battery_soc":
                    HandleBatterySOC(parameters); break;
                case "load_update":
                    HandleLoadUpdate(parameters); break;
                case "generation_update":
                    HandleGenerationUpdate(parameters); break;
                case "energy_stored":
                    HandleEnergyStored(parameters); break;
                case "battery_maximum_capacity":
                    HandleBatteryAtMaximumCapacity(parameters); break;
                case "sell_energy_confirmation":
                    HandleSellEnergyConfirmation(parameters); break;
                case "buy_energy_confirmation":
                    HandleBuyEnergyConfirmation(parameters); break;
                default:
                    break;
            }
        }

        private void HandleBatterySOC(string currentBatteryStorageCapacity)
        {
            this.currentBatteryStorageCapacity = Double.Parse(currentBatteryStorageCapacity);
        }
        private void HandleLoadUpdate(string newLoadValue)
        {
            this.currentLoadEnergyTotal += Double.Parse(newLoadValue);
            this.flagMessageFromLoadAgent = !this.flagMessageFromLoadAgent;
            if (this.flagMessageFromLoadAgent == this.flagMessageFromGeneratorAgent)
            {
                HandleEnergyConsume();
            }
        }
        private void HandleGenerationUpdate(string newGenerationValue)
        {
            this.currentGeneratedEnergyTotal += Double.Parse(newGenerationValue);
            this.flagMessageFromGeneratorAgent = !this.flagMessageFromGeneratorAgent;
            if (this.flagMessageFromGeneratorAgent == this.flagMessageFromLoadAgent)
            {
                HandleEnergyConsume();
            }
        }

        private void HandleEnergyConsume()
        {
            if (this.currentGeneratedEnergyTotal > this.currentLoadEnergyTotal)
            {

                double energyToStore = this.currentGeneratedEnergyTotal - this.currentLoadEnergyTotal;
                this.energyInTransit += energyToStore;

                this.currentGeneratedEnergyTotal = 0.0;
                this.currentLoadEnergyTotal = 0.0;
                Send($"battery{this.Name}", Utils.Str("store", energyToStore));
            }
        }

        private void HandleEnergyStored(string energyTriedToStore)
        {
            this.energyInTransit -= Double.Parse(energyTriedToStore);
        }

        private void HandleBatteryAtMaximumCapacity(string capacityDifference)
        {
            double storedEnergy = Double.Parse(capacityDifference);
            this.energyInTransit -= storedEnergy;
            Send("vpp", Utils.Str("sell_energy", this.energyInTransit));
        }

        private void HandleSellEnergyConfirmation(string parameters)
        {

            string energySold; string moneyReceived;
            Utils.ParseMessage(parameters, out energySold, out moneyReceived);
            this.currentBill += Double.Parse(moneyReceived);
            this.energyInTransit -= Double.Parse(energySold);
        }

        private void HandleBuyEnergyConfirmation(string parameters)
        {
            string energyBought; string moneyToPay;
            Utils.ParseMessage(parameters, out energyBought, out moneyToPay);
            this.currentLoadEnergyTotal -= Double.Parse(energyBought);
            this.currentBill -= Double.Parse(moneyToPay);
        }
    }
}
