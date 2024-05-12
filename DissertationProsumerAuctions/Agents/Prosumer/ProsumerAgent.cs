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

        private double currentGridBuyPrice;
        private double currentGridSellPrice;

        private double currentBill = 0.0;

        private bool flagMessageFromLoadAgent = false;
        private bool flagMessageFromGeneratorAgent = false;

        private double auctionEnergyPriceVariationFromGrid;

        private bool isAuctioning = false;

        private Dictionary<string, bool> prosumerSetupReadiness;

        public ProsumerAgent() : base() { }
        public override void Setup()
        {
            prosumerId = Int32.Parse(this.Name.Remove(0, 8));
            Console.WriteLine("[{0} {1}] Hi", this.Name, prosumerId);
            prosumerSetupReadiness = new Dictionary<string, bool>
            {
                {$"loadprosumer{prosumerId}", false },
                {$"generatorprosumer{prosumerId}", false },
                {$"batteryprosumer{prosumerId}", false },
                {$"energymarket1", false }
            };

            auctionEnergyPriceVariationFromGrid = Utils.RandNoGen.NextDouble() * (1.2 - 0.8) + 0.8;
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);
            //Console.WriteLine("Energy in transit {0}; current bill {1}", this.energyInTransit, this.currentBill);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "component_ready":
                case "find_prosumers":
                    HandleProsumerComponentSetup(message.Sender, action); break;
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
                case "energy_market_price":
                    HandleEnergyMarketPrice(parameters); break;
                case "startAuctioning":
                    HandleStartAuctioning();  break;
                case "selling_price":
                    HandleSellingPrice(message.Sender, parameters); break;
                default:
                    break;
            }
        }

        private void HandleProsumerComponentSetup(string sender, string action)
        {
            prosumerSetupReadiness.TryGetValue(sender, out bool value);
            prosumerSetupReadiness[sender] = value || true;
            bool allComponentsAreReady = prosumerSetupReadiness.Values.All(componentIsReady => componentIsReady);

            if (allComponentsAreReady)
            {
                SendToMany(prosumerSetupReadiness.Keys.ToList(), "prosumer_start");
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
            /*if (this.currentGeneratedEnergyTotal > this.currentLoadEnergyTotal)
            {
                double energyToStore = this.currentGeneratedEnergyTotal - this.currentLoadEnergyTotal;
                this.energyInTransit += energyToStore;

                this.currentGeneratedEnergyTotal = 0.0;
                this.currentLoadEnergyTotal = 0.0;
                Send($"battery{this.Name}", Utils.Str("store", energyToStore));
            }*/

            if (this.currentGeneratedEnergyTotal - this.currentLoadEnergyTotal != 0)
            {
                Send(this.Name, "startAuctioning");
            }
            else
            {
                this.currentGeneratedEnergyTotal = 0.0;
                this.currentLoadEnergyTotal = 0.0;
            }
        }

        private void HandleStartAuctioning()
        {
            if (this.currentGeneratedEnergyTotal > this.currentLoadEnergyTotal)
            {
                double excessEnergy = this.currentGeneratedEnergyTotal - this.currentLoadEnergyTotal;
                this.energyInTransit += excessEnergy;
                double floorPrice = this.currentGridSellPrice * 0.8;
                double startingPrice = this.currentGridSellPrice * 1.2;

                this.isAuctioning = true;
                Send("dutchauctioneer", Utils.Str("excess_to_sell", excessEnergy, floorPrice, startingPrice)); // command + energy units + floor price
            }
            else if (this.currentGeneratedEnergyTotal < this.currentLoadEnergyTotal)
            {
                double energyDeficit = this.currentLoadEnergyTotal - this.currentGeneratedEnergyTotal;
                this.energyInTransit -= energyDeficit;

                this.isAuctioning = true;
                Send("dutchauctioneer", Utils.Str("deficit_to_buy", energyDeficit)); // command + energy units + ceiling price
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

        private void HandleEnergyMarketPrice(string gridEnergyprice)
        {
            this.currentGridBuyPrice = Double.Parse(gridEnergyprice);
            this.currentGridSellPrice = this.currentGridBuyPrice*2;
            //Console.WriteLine($"{this.currentGridBuyPrice} {this.currentGridSellPrice}");
        }

        private void HandleSellingPrice(string auctioneer, string auctionEnergyPrice)
        {
            if (isAuctioning)
            {
                double _auctionEnergyPrice = Double.Parse(auctionEnergyPrice);
                if(_auctionEnergyPrice <= currentGridSellPrice * auctionEnergyPriceVariationFromGrid)
                {
                    Send(auctioneer, Utils.Str("energy_bid",auctionEnergyPrice));
                    isAuctioning = false;
                }
                else
                {
                    Console.WriteLine($" Price not good. {_auctionEnergyPrice} > my price: {currentGridSellPrice * auctionEnergyPriceVariationFromGrid}");
                }
            }
            else
            {
                
            }
        }
    }
}
