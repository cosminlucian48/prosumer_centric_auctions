using ActressMas;
using ProsumerAuctionPlatform.Constants;
using ProsumerAuctionPlatform.Models;
using ProsumerAuctionPlatform.Services.Settlement;
using System;
using System.Collections.Generic;
using System.Linq;
using Serilog;

namespace ProsumerAuctionPlatform.Agents.Prosumer
{
    internal class ProsumerAgent : Agent, IProsumerSettlementContext
    {
        public int ProsumerId { get; private set; } = 0;
        private double _currentLoadEnergyTotal;
        private double _currentGeneratedEnergyTotal;
        private double _currentBatteryStorageCapacity;
        // Energy settlement state.
        private double _pendingSurplusEnergy;
        private double _pendingDeficitEnergy;

        private double _currentGridBuyPrice;
        private double _currentGridSellPrice;

        // Settlement channel chain.
        private readonly BatterySettlementChannel _batteryChannel;
        private readonly GridSettlementChannel _gridChannel;
        private readonly List<ISettlementChannel> _surplusChain;
        private readonly List<ISettlementChannel> _deficitChain;

        private double _currentBill = 0.0;

        private bool _flagMessageFromLoadAgent = false;
        private bool _flagMessageFromGeneratorAgent = false;

        private readonly Dictionary<string, bool> _prosumerSetupReadiness = new Dictionary<string, bool>();
        private readonly Dictionary<string, string> _roleRegistry = new Dictionary<string, string>();
        private bool _hasStartedProsumerComponents;
        private readonly ProsumerCapabilities _capabilities;

        public ProsumerAgent() : this(new ProsumerCapabilities(true, true, true))
        {
        }

        public ProsumerAgent(ProsumerCapabilities capabilities) : base()
        {
            _capabilities = capabilities;
            _batteryChannel = new BatterySettlementChannel(this);
            _gridChannel = new GridSettlementChannel(this);
            _surplusChain = new List<ISettlementChannel>
            {
                _batteryChannel,
                // new AuctionSettlementChannel(),  // TODO: insert at position 0 when auction is active
                _gridChannel,
            };
            _deficitChain = new List<ISettlementChannel>
            {
                _batteryChannel,
                // new AuctionSettlementChannel(),  // TODO: insert at position 0 when auction is active
                _gridChannel,
            };
        }

        // --- IProsumerSettlementContext ---
        bool IProsumerSettlementContext.HasBattery      => _capabilities.HasBattery;
        double IProsumerSettlementContext.BatterySOC    => _currentBatteryStorageCapacity;
        double IProsumerSettlementContext.GridBuyPrice  => _currentGridBuyPrice;
        double IProsumerSettlementContext.GridSellPrice => _currentGridSellPrice;
        void IProsumerSettlementContext.SendToRole(string role, string content) => SendToRole(role, content);
        void IProsumerSettlementContext.AddToBill(double delta) => _currentBill += delta;

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

                if (_capabilities.HasLoad)
                {
                    _prosumerSetupReadiness[AgentRoles.Load] = false;
                }

                if (_capabilities.HasGenerator)
                {
                    _prosumerSetupReadiness[AgentRoles.Generator] = false;
                }

                if (_capabilities.HasBattery)
                {
                    _prosumerSetupReadiness[AgentRoles.Battery] = false;
                }

                _prosumerSetupReadiness[AgentRoles.EnergyMarket] = false;
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

                if (!Utils.TryParseMessage(message.Content, out string action, out string parameters))
                {
                    MasLog.Event(this, "error", $"Failed to parse message: {message.Content}");
                    return;
                }

                LogProsumerState("Before", action, parameters);

                switch (action)
                {
                    case MessageTypes.Lifecycle.Tick:
                        break;
                    case MessageTypes.Lifecycle.ComponentReady:
                        HandleDependencyReady(message.Sender, parameters);
                        break;
                    case MessageTypes.Market.FindProsumers:
                        HandleDependencyReady(message.Sender, AgentRoles.EnergyMarket);
                        break;
                    case MessageTypes.Battery.BatterySOC:
                        HandleBatterySOC(parameters);
                        break;
                    case MessageTypes.Readings.LoadUpdate:
                        HandleLoadUpdate(parameters);
                        break;
                    case MessageTypes.Readings.GenerationUpdate:
                        HandleGenerationUpdate(parameters);
                        break;
                    case MessageTypes.Battery.EnergyStored:
                        HandleEnergyStored(parameters);
                        break;
                    case MessageTypes.Battery.EnergyConsumed:
                        HandleEnergyConsumed(parameters);
                        break;
                    case MessageTypes.Battery.BatteryMaximumCapacity:
                        HandleBatteryAtMaximumCapacity(parameters);
                        break;
                    case MessageTypes.Market.EnergyMarketPrice:
                        HandleEnergyMarketPrice(parameters);
                        break;
                    case MessageTypes.Lifecycle.NetEnergyReady:
                        HandleNetEnergySettlement();
                        break;
                }

                LogProsumerState("After", action, parameters);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error processing message in ProsumerAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleDependencyReady(string sender, string role)
        {
            _roleRegistry[role] = sender;
            if (_prosumerSetupReadiness.ContainsKey(role))
                _prosumerSetupReadiness[role] = true;

            if (_hasStartedProsumerComponents)
                return;

            bool allReady = _prosumerSetupReadiness.Values.All(r => r);
            if (allReady)
            {
                _hasStartedProsumerComponents = true;
                SendToMany(
                    _roleRegistry.Values.ToList(),
                    MessageTypes.Lifecycle.ProsumerStart);
            }
        }

        private void SendToRole(string role, string content)
        {
            if (_roleRegistry.TryGetValue(role, out string? agentName))
                Send(agentName, content);
            else
                Serilog.Log.Warning("No agent registered for role {Role} — message dropped: {Content}", role, content);
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

                if (!_capabilities.HasGenerator)
                {
                    HandleReadingsReady();
                    return;
                }

                this._flagMessageFromLoadAgent = !this._flagMessageFromLoadAgent;
                if (this._flagMessageFromLoadAgent == this._flagMessageFromGeneratorAgent)
                {
                    HandleReadingsReady();
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

                if (!_capabilities.HasLoad)
                {
                    HandleReadingsReady();
                    return;
                }

                this._flagMessageFromGeneratorAgent = !this._flagMessageFromGeneratorAgent;
                if (this._flagMessageFromGeneratorAgent == this._flagMessageFromLoadAgent)
                {
                    HandleReadingsReady();
                }
            }
            else
            {
                Serilog.Log.Warning("Invalid generation value: {Value}", newGenerationValue);
            }
        }

        private void HandleReadingsReady()
        {
            double netEnergy = this._currentGeneratedEnergyTotal - this._currentLoadEnergyTotal;

            if (Math.Abs(netEnergy) > double.Epsilon)
            {
                Send(this.Name, MessageTypes.Lifecycle.NetEnergyReady);
            }
            else
            {
                ResetEnergyWindow();
            }
        }

        private void HandleNetEnergySettlement()
        {
            if (this._currentGeneratedEnergyTotal > this._currentLoadEnergyTotal)
            {
                double excessEnergy = this._currentGeneratedEnergyTotal - this._currentLoadEnergyTotal;
                _pendingSurplusEnergy += excessEnergy;

                // Auction path: insert AuctionSettlementChannel at position 0 of _surplusChain.
                RunSurplusChain(_pendingSurplusEnergy);
            }
            else if (this._currentGeneratedEnergyTotal < this._currentLoadEnergyTotal)
            {
                double energyDeficit = this._currentLoadEnergyTotal - this._currentGeneratedEnergyTotal;
                _pendingDeficitEnergy += energyDeficit;

                // Auction path: insert AuctionSettlementChannel at position 0 of _deficitChain.
                RunDeficitChain(_pendingDeficitEnergy);
            }

            ResetEnergyWindow();
        }

        private void HandleEnergyStored(string energyTriedToStore)
        {
            if (double.TryParse(energyTriedToStore, out double energyStored))
            {
                _currentBatteryStorageCapacity += energyStored;
                _pendingSurplusEnergy = Math.Max(0, _pendingSurplusEnergy - energyStored);
                _batteryChannel.OnSurplusConfirmed(energyStored);
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

            _currentBatteryStorageCapacity = Math.Max(0, _currentBatteryStorageCapacity - consumedEnergy);
            _pendingDeficitEnergy = Math.Max(0, _pendingDeficitEnergy - consumedEnergy);
            _batteryChannel.OnDeficitConfirmed(consumedEnergy);

            // If battery could not satisfy the full requested amount, buy the shortfall from grid.
            double shortfall = _batteryChannel.OutstandingConsumeRequest;
            if (shortfall > 0)
            {
                _gridChannel.TrySettleDeficit(shortfall);
                _pendingDeficitEnergy = Math.Max(0, _pendingDeficitEnergy - shortfall);
                _batteryChannel.ClearOutstandingDeficit();
            }

            RunDeficitChain(_pendingDeficitEnergy);
        }

        private void HandleBatteryAtMaximumCapacity(string capacityDifference)
        {
            if (double.TryParse(capacityDifference, out double overflowEnergy))
            {
                if (overflowEnergy > 0)
                {
                    _gridChannel.TrySettleSurplus(overflowEnergy);
                    _pendingSurplusEnergy = Math.Max(0, _pendingSurplusEnergy - overflowEnergy);
                    _batteryChannel.OnSurplusOverflow(overflowEnergy);
                }

                RunSurplusChain(_pendingSurplusEnergy);
            }
            else
            {
                Serilog.Log.Warning("Invalid capacity difference value: {Value}", capacityDifference);
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

        private void ResetEnergyWindow()
        {
            this._currentGeneratedEnergyTotal = 0.0;
            this._currentLoadEnergyTotal = 0.0;
        }

        private void RunSurplusChain(double amount)
        {
            double remaining = amount;
            foreach (var channel in _surplusChain)
            {
                if (remaining <= double.Epsilon) break;
                if (!channel.IsAvailable) continue;
                if (channel.HasPendingSurplusRequest) break; // async in-flight — wait for confirmation
                double dispatched = channel.TrySettleSurplus(remaining);
                remaining = Math.Max(0, remaining - dispatched);
            }
        }

        private void RunDeficitChain(double amount)
        {
            double remaining = amount;
            foreach (var channel in _deficitChain)
            {
                if (remaining <= double.Epsilon) break;
                if (!channel.IsAvailable) continue;
                if (channel.HasPendingDeficitRequest) break; // async in-flight — wait for confirmation
                double dispatched = channel.TrySettleDeficit(remaining);
                remaining = Math.Max(0, remaining - dispatched);
            }
        }

        private void LogProsumerState(string phase, string action, string parameters)
        {
            MasLog.InfoDebug(
                this,
                "debug",
                $"{phase} {action} {parameters}: load {_currentLoadEnergyTotal}; generation {_currentGeneratedEnergyTotal}; battery_soc {_currentBatteryStorageCapacity}; pending surplus {_pendingSurplusEnergy}; pending deficit {_pendingDeficitEnergy}; outstanding store {_batteryChannel.OutstandingStoreRequest}; outstanding consume {_batteryChannel.OutstandingConsumeRequest}; bill {_currentBill}");
        }
    }
}
