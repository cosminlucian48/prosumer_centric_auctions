using ActressMas;
using ProsumerAuctionPlatform.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace ProsumerAuctionPlatform.Agents.Prosumer
{
    internal class ProsumerAgent : Agent
    {
        public int ProsumerId { get; private set; } = 0;
        private double _currentLoadEnergyTotal;
        private double _currentGeneratedEnergyTotal;
        private double _currentBatteryStorageCapacity;
        private double _energyInTransit;

        private double _currentGridBuyPrice;
        private double _currentGridSellPrice;

        private double _currentBill = 0.0;

        private bool _flagMessageFromLoadAgent = false;
        private bool _flagMessageFromGeneratorAgent = false;

        private double _auctionEnergyPriceVariationFromGrid;

        private bool _isAuctioning = false;

        private readonly Dictionary<string, bool> _prosumerSetupReadiness = new Dictionary<string, bool>();

        public ProsumerAgent() : base() { }
        public override void Setup()
        {
            try
            {
                if (this.Name.Length > 8 && int.TryParse(this.Name.Remove(0, 8), out int id))
                {
                    ProsumerId = id;
                }
                else
                {
                    Serilog.Log.Warning("Invalid prosumer name format: {Name}", this.Name);
                }
                MasLog.Event(this, "message", "Hi - Prosumer Agent started!");
            _prosumerSetupReadiness[AgentNames.GetLoadName(this.Name)] = false;
            _prosumerSetupReadiness[AgentNames.GetGeneratorName(this.Name)] = false;
            _prosumerSetupReadiness[AgentNames.GetBatteryName(this.Name)] = false;
            _prosumerSetupReadiness[AgentNames.EnergyMarket] = false;
            
            _auctionEnergyPriceVariationFromGrid = Utils.RandNoGen.NextDouble() * (1.2 - 0.8) + 0.8;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error setting up ProsumerAgent {AgentName}", this.Name);
                throw;
            }
        }

        public override void Act(Message message)
        {
            try
            {
                MasLog.Received(this, message, $"[{message.Sender} -> {Name}]: {message.Content}");
                MasLog.InfoDebug(this, "debug", $"Energy in transit {this._energyInTransit}; current bill { this._currentBill}");

                if (!Utils.TryParseMessage(message.Content, out string action, out string parameters))
                {
                    MasLog.Event(this, "error", $"Failed to parse message: {message.Content}");
                    return;
                }

                switch (action)
            {
                case MessageTypes.Tick: break;
                case MessageTypes.ComponentReady: break;
                case MessageTypes.FindProsumers:
                    HandleProsumerComponentSetup(message.Sender, action); break;
                case MessageTypes.BatterySOC:
                    HandleBatterySOC(parameters); break;
                case MessageTypes.LoadUpdate:
                    HandleLoadUpdate(parameters); break;
                case MessageTypes.GenerationUpdate:
                    HandleGenerationUpdate(parameters); break;
                case MessageTypes.EnergyStored:
                    HandleEnergyStored(parameters); break;
                case MessageTypes.BatteryMaximumCapacity:
                    HandleBatteryAtMaximumCapacity(parameters); break;
                case MessageTypes.SellEnergyConfirmation:
                    HandleSellEnergyConfirmation(parameters); break;
                case MessageTypes.BuyEnergyConfirmation:
                    HandleBuyEnergyConfirmation(parameters); break;
                case MessageTypes.EnergyMarketPrice:
                    HandleEnergyMarketPrice(parameters); break;
                case MessageTypes.StartAuctioning:
                    HandleStartAuctioning();  break;
                case MessageTypes.SellingPrice:
                    HandleSellingPrice(message.Sender, parameters); break;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error processing message in ProsumerAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleProsumerComponentSetup(string sender, string action)
        {
            if (_prosumerSetupReadiness.ContainsKey(sender))
            {
                _prosumerSetupReadiness[sender] = true;
            }
            bool allComponentsAreReady = _prosumerSetupReadiness.Values.All(componentIsReady => componentIsReady);

            if (allComponentsAreReady)
            {
                SendToMany(_prosumerSetupReadiness.Keys.ToList(), MessageTypes.ProsumerStart);
            }
            
        }

        private void HandleBatterySOC(string currentBatteryStorageCapacity)
        {
            if (double.TryParse(currentBatteryStorageCapacity, out double capacity))
            {
                this._currentBatteryStorageCapacity = capacity;
            }
            else
            {
                Serilog.Log.Warning("Invalid battery SOC value: {Value}", currentBatteryStorageCapacity);
            }
        }
        private void HandleLoadUpdate(string newLoadValue)
        {
            if (double.TryParse(newLoadValue, out double loadValue))
            {
                this._currentLoadEnergyTotal += loadValue;
                this._flagMessageFromLoadAgent = !this._flagMessageFromLoadAgent;
                if (this._flagMessageFromLoadAgent == this._flagMessageFromGeneratorAgent)
                {
                    HandleEnergyConsume();
                }
            }
            else
            {
                Serilog.Log.Warning("Invalid load value: {Value}", newLoadValue);
            }
        }
        private void HandleGenerationUpdate(string newGenerationValue)
        {
            if (double.TryParse(newGenerationValue, out double generationValue))
            {
                this._currentGeneratedEnergyTotal += generationValue;
                this._flagMessageFromGeneratorAgent = !this._flagMessageFromGeneratorAgent;
                if (this._flagMessageFromGeneratorAgent == this._flagMessageFromLoadAgent)
                {
                    HandleEnergyConsume();
                }
            }
            else
            {
                Serilog.Log.Warning("Invalid generation value: {Value}", newGenerationValue);
            }
        }

        private void HandleEnergyConsume()
        {
            if (this._currentGeneratedEnergyTotal - this._currentLoadEnergyTotal != 0)
            {
                Send(this.Name, MessageTypes.StartAuctioning);
            }
            else
            {
                this._currentGeneratedEnergyTotal = 0.0;
                this._currentLoadEnergyTotal = 0.0;
            }
        }

        private void HandleStartAuctioning()
        {
            if (this._currentGeneratedEnergyTotal > this._currentLoadEnergyTotal)
            {
                double excessEnergy = this._currentGeneratedEnergyTotal - this._currentLoadEnergyTotal;
                this._energyInTransit += excessEnergy;
                double floorPrice = this._currentGridSellPrice * 0.8;
                double startingPrice = this._currentGridSellPrice * 1.2;

                this._isAuctioning = true;
                Send(AgentNames.DutchAuctioneer, Utils.Str(MessageTypes.ExcessToSell, excessEnergy, floorPrice, startingPrice)); // command + energy units + floor price
            }
            else if (this._currentGeneratedEnergyTotal < this._currentLoadEnergyTotal)
            {
                double energyDeficit = this._currentLoadEnergyTotal - this._currentGeneratedEnergyTotal;
                this._energyInTransit -= energyDeficit;

                this._isAuctioning = true;
                Send(AgentNames.DutchAuctioneer, Utils.Str(MessageTypes.DeficitToBuy, energyDeficit)); // command + energy units + ceiling price
            }
        }

        private void HandleEnergyStored(string energyTriedToStore)
        {
            if (double.TryParse(energyTriedToStore, out double energyStored))
            {
                this._energyInTransit -= energyStored;
            }
            else
            {
                Serilog.Log.Warning("Invalid energy stored value: {Value}", energyTriedToStore);
            }
        }

        private void HandleBatteryAtMaximumCapacity(string capacityDifference)
        {
            if (double.TryParse(capacityDifference, out double storedEnergy))
            {
                this._energyInTransit -= storedEnergy;
                // TODO: Replace with proper VPP agent when implemented
                Send(AgentNames.VPP, Utils.Str(MessageTypes.SellEnergy, this._energyInTransit));
            }
            else
            {
                Serilog.Log.Warning("Invalid capacity difference value: {Value}", capacityDifference);
            }
        }

        private void HandleSellEnergyConfirmation(string parameters)
        {
            if (!Utils.TryParseMessage(parameters, out string energySold, out string moneyReceived))
            {
                Serilog.Log.Warning("Failed to parse sell energy confirmation parameters: {Parameters}", parameters);
                return;
            }

            if (double.TryParse(moneyReceived, out double money) && double.TryParse(energySold, out double energy))
            {
                this._currentBill += money;
                this._energyInTransit -= energy;
            }
            else
            {
                Serilog.Log.Warning("Invalid values in sell energy confirmation: energy={Energy}, money={Money}", energySold, moneyReceived);
            }
        }

        private void HandleBuyEnergyConfirmation(string parameters)
        {
            if (!Utils.TryParseMessage(parameters, out string energyBought, out string moneyToPay))
            {
                Serilog.Log.Warning("Failed to parse buy energy confirmation parameters: {Parameters}", parameters);
                return;
            }

            if (double.TryParse(energyBought, out double energy) && double.TryParse(moneyToPay, out double money))
            {
                this._currentLoadEnergyTotal -= energy;
                this._currentBill -= money;
            }
            else
            {
                Serilog.Log.Warning("Invalid values in buy energy confirmation: energy={Energy}, money={Money}", energyBought, moneyToPay);
            }
        }

        private void HandleEnergyMarketPrice(string gridEnergyPrice)
        {
            if (double.TryParse(gridEnergyPrice, out double price))
            {
                this._currentGridBuyPrice = price;
                this._currentGridSellPrice = this._currentGridBuyPrice * 2;
            }
            else
            {
                Serilog.Log.Warning("Invalid grid energy price: {Value}", gridEnergyPrice);
            }
        }

        private void HandleSellingPrice(string auctioneer, string auctionEnergyPrice)
        {
            if (!double.TryParse(auctionEnergyPrice, out double price))
            {
                Serilog.Log.Warning("Invalid auction energy price: {Value}", auctionEnergyPrice);
                return;
            }

            if (_isAuctioning)
            {
                if (price <= _currentGridSellPrice * _auctionEnergyPriceVariationFromGrid)
                {
                    Send(auctioneer, Utils.Str(MessageTypes.EnergyBid, auctionEnergyPrice));
                    _isAuctioning = false;
                }
                else
                {
                    Serilog.Log.Information("Price not good. {AuctionPrice} > my price: {MyPrice}", 
                        price, _currentGridSellPrice * _auctionEnergyPriceVariationFromGrid);
                }
            }
        }
    }
}
