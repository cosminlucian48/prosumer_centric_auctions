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
        // Energy settlement state for the basic battery/grid mode.
        private double _pendingSurplusEnergy;
        private double _pendingDeficitEnergy;
        private double _outstandingStoreRequest;
        private double _outstandingConsumeRequest;

        private double _currentGridBuyPrice;
        private double _currentGridSellPrice;

        private double _currentBill = 0.0;

        private bool _flagMessageFromLoadAgent = false;
        private bool _flagMessageFromGeneratorAgent = false;

        // TODO: Re-enable this auction preference when the Dutch auction flow is wired back into the world.
        private double _auctionEnergyPriceVariationFromGrid;

        // TODO: Reuse this state flag when auction participation is restored.
        private bool _isAuctioning = false;

        private readonly Dictionary<string, bool> _prosumerSetupReadiness = new Dictionary<string, bool>();
        private bool _hasStartedProsumerComponents;

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

                // TODO: This is currently retained for future auction bidding logic.
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
                MasLog.InfoDebug(
                    this,
                    "debug",
                    $"Pending surplus {_pendingSurplusEnergy}; pending deficit {_pendingDeficitEnergy}; bill {this._currentBill}");

                if (!Utils.TryParseMessage(message.Content, out string action, out string parameters))
                {
                    MasLog.Event(this, "error", $"Failed to parse message: {message.Content}");
                    return;
                }

                switch (action)
                {
                    case MessageTypes.Tick:
                        break;
                    case MessageTypes.ComponentReady:
                        HandleDependencyReady(message.Sender);
                        break;
                    case MessageTypes.FindProsumers:
                        HandleDependencyReady(message.Sender);
                        break;
                    case MessageTypes.BatterySOC:
                        HandleBatterySOC(parameters);
                        break;
                    case MessageTypes.LoadUpdate:
                        HandleLoadUpdate(parameters);
                        break;
                    case MessageTypes.GenerationUpdate:
                        HandleGenerationUpdate(parameters);
                        break;
                    case MessageTypes.EnergyStored:
                        HandleEnergyStored(parameters);
                        break;
                    case MessageTypes.EnergyConsumed:
                        HandleEnergyConsumed(parameters);
                        break;
                    case MessageTypes.BatteryMaximumCapacity:
                        HandleBatteryAtMaximumCapacity(parameters);
                        break;
                    case MessageTypes.SellEnergyConfirmation:
                        HandleSellEnergyConfirmation(parameters);
                        break;
                    case MessageTypes.BuyEnergyConfirmation:
                        HandleBuyEnergyConfirmation(parameters);
                        break;
                    case MessageTypes.EnergyMarketPrice:
                        HandleEnergyMarketPrice(parameters);
                        break;
                    case MessageTypes.StartAuctioning:
                        HandleStartAuctioning();
                        break;
                    case MessageTypes.SellingPrice:
                        HandleSellingPrice(message.Sender, parameters);
                        break;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error processing message in ProsumerAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleDependencyReady(string sender)
        {
            if (_prosumerSetupReadiness.ContainsKey(sender))
            {
                _prosumerSetupReadiness[sender] = true;
            }

            if (_hasStartedProsumerComponents)
            {
                return;
            }

            // Startup currently depends on the three component agents plus the energy market
            // publishing its discovery message. Keep this gate explicit until startup is refactored.
            bool allComponentsAreReady = _prosumerSetupReadiness.Values.All(componentIsReady => componentIsReady);

            if (allComponentsAreReady)
            {
                _hasStartedProsumerComponents = true;
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
            double netEnergy = this._currentGeneratedEnergyTotal - this._currentLoadEnergyTotal;

            if (Math.Abs(netEnergy) > double.Epsilon)
            {
                Send(this.Name, MessageTypes.StartAuctioning);
            }
            else
            {
                ResetEnergyWindow();
            }
        }

        private void HandleStartAuctioning()
        {
            // TODO(tech-debt): Restore the auction branch here after DutchAuctioneer is added back to the world.
            // Historical auction behavior intentionally kept below as comments for future reactivation.
            _isAuctioning = false;

            if (this._currentGeneratedEnergyTotal > this._currentLoadEnergyTotal)
            {
                double excessEnergy = this._currentGeneratedEnergyTotal - this._currentLoadEnergyTotal;
                _pendingSurplusEnergy += excessEnergy;

                // Auction path kept for reference:
                // double floorPrice = this._currentGridSellPrice * 0.8;
                // double startingPrice = this._currentGridSellPrice * 1.2;
                // this._isAuctioning = true;
                // Send(AgentNames.DutchAuctioneer,
                //     Utils.Str(MessageTypes.ExcessToSell, excessEnergy, floorPrice, startingPrice));

                // Basic-mode fallback: try local storage first, then settle overflow against the grid.
                TryStorePendingSurplus();
            }
            else if (this._currentGeneratedEnergyTotal < this._currentLoadEnergyTotal)
            {
                double energyDeficit = this._currentLoadEnergyTotal - this._currentGeneratedEnergyTotal;

                // Auction path kept for reference:
                // this._isAuctioning = true;
                // Send(AgentNames.DutchAuctioneer, Utils.Str(MessageTypes.DeficitToBuy, energyDeficit));

                // Basic-mode fallback: try discharging battery first, then buy remaining deficit from the grid.
                _pendingDeficitEnergy += energyDeficit;
                TryCoverPendingDeficit();
            }

            ResetEnergyWindow();
        }

        private void HandleEnergyStored(string energyTriedToStore)
        {
            if (double.TryParse(energyTriedToStore, out double energyStored))
            {
                _pendingSurplusEnergy = Math.Max(0, _pendingSurplusEnergy - energyStored);
                _outstandingStoreRequest = Math.Max(0, _outstandingStoreRequest - energyStored);
            }
            else
            {
                Serilog.Log.Warning("Invalid energy stored value: {Value}", energyTriedToStore);
            }
        }

        private void HandleEnergyConsumed(string consumedEnergyValue)
        {
            if (!double.TryParse(consumedEnergyValue, out double consumedEnergy))
            {
                Serilog.Log.Warning("Invalid energy consumed value: {Value}", consumedEnergyValue);
                return;
            }

            if (_pendingDeficitEnergy <= 0)
            {
                return;
            }

            _pendingDeficitEnergy = Math.Max(0, _pendingDeficitEnergy - consumedEnergy);
            _outstandingConsumeRequest = Math.Max(0, _outstandingConsumeRequest - consumedEnergy);

            // If battery could not satisfy the requested amount, buy the remainder from grid.
            if (_outstandingConsumeRequest > 0)
            {
                BuyEnergyFromGrid(_outstandingConsumeRequest);
                _pendingDeficitEnergy = Math.Max(0, _pendingDeficitEnergy - _outstandingConsumeRequest);
                _outstandingConsumeRequest = 0;
            }

            TryCoverPendingDeficit();
        }

        private void HandleBatteryAtMaximumCapacity(string capacityDifference)
        {
            if (double.TryParse(capacityDifference, out double overflowEnergy))
            {
                if (overflowEnergy > 0)
                {
                    // TODO: Replace this direct grid settlement with the intended VPP or auction route.
                    SellEnergyToGrid(overflowEnergy);
                    _pendingSurplusEnergy = Math.Max(0, _pendingSurplusEnergy - overflowEnergy);
                    _outstandingStoreRequest = Math.Max(0, _outstandingStoreRequest - overflowEnergy);
                }

                TryStorePendingSurplus();
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
                _pendingSurplusEnergy = Math.Max(0, _pendingSurplusEnergy - energy);
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
                _pendingDeficitEnergy = Math.Max(0, _pendingDeficitEnergy - energy);
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
            double targetAuctionBidPrice = _currentGridSellPrice * _auctionEnergyPriceVariationFromGrid;

            Serilog.Log.Information(
                "Ignoring selling price message from {Auctioneer} while prosumer is running in basic battery/grid mode: {Price}. Auction state retained for later reactivation. Target bid threshold would be {TargetAuctionBidPrice}.",
                auctioneer,
                auctionEnergyPrice,
                targetAuctionBidPrice);

            // TODO: Restore the previous bid acceptance logic once the auctioneer is active again.
            _isAuctioning = false;
        }

        private void BuyEnergyFromGrid(double energyDeficit)
        {
            // TODO: Move this to a dedicated grid/VPP integration once market settlement is modeled explicitly.
            this._currentBill -= energyDeficit * this._currentGridBuyPrice;
        }

        private void SellEnergyToGrid(double excessEnergy)
        {
            // TODO: Move this to a dedicated grid/VPP integration once export settlement is modeled explicitly.
            this._currentBill += excessEnergy * this._currentGridSellPrice;
        }

        private void ResetEnergyWindow()
        {
            this._currentGeneratedEnergyTotal = 0.0;
            this._currentLoadEnergyTotal = 0.0;
        }

        private void TryStorePendingSurplus()
        {
            if (_outstandingStoreRequest > 0)
            {
                return;
            }

            if (_pendingSurplusEnergy <= double.Epsilon)
            {
                return;
            }

            _outstandingStoreRequest = _pendingSurplusEnergy;
            Send(AgentNames.GetBatteryName(this.Name), Utils.Str(MessageTypes.StoreEnergy, _outstandingStoreRequest));
        }

        private void TryCoverPendingDeficit()
        {
            if (_outstandingConsumeRequest > 0)
            {
                return;
            }

            if (_pendingDeficitEnergy <= double.Epsilon)
            {
                return;
            }

            if (_currentBatteryStorageCapacity <= double.Epsilon)
            {
                BuyEnergyFromGrid(_pendingDeficitEnergy);
                _pendingDeficitEnergy = 0;
                return;
            }

            _outstandingConsumeRequest = _pendingDeficitEnergy;
            Send(AgentNames.GetBatteryName(this.Name), Utils.Str(MessageTypes.ConsumeEnergy, _outstandingConsumeRequest));
        }
    }
}
