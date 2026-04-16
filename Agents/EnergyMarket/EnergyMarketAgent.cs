using ActressMas;
using ProsumerAuctionPlatform.Constants;
using ProsumerAuctionPlatform.DatabaseConnections;
using ProsumerAuctionPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProsumerAuctionPlatform.Agents.EnergyMarket
{
    /// <summary>
    /// Agent responsible for managing energy market prices and broadcasting them to prosumers.
    /// Uses tick-based time management instead of timers.
    /// </summary>
    internal class EnergyMarketAgent : Agent
    {
        public double CurrentEnergyPrice { get; private set; } = 0.0;
        private readonly Dictionary<string, List<double>> _localEnergyDifference = new Dictionary<string, List<double>>();
        private readonly List<string> _prosumers = new List<string>();
        public DateTime LastTimestamp { get; private set; }
        
        // Tick-based timing: 15 ticks = 15 minutes (database data interval)
        private int _tickCount = 0;
        private const int TicksPerPriceUpdate = 15;

        public EnergyMarketAgent() : base()
        {
            LastTimestamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        private void UpdateProsumerGridEnergyPrice(string prosumer = "")
        {
            try
            {
                // Block on async call since Act() must be synchronous
                List<EnergyMarketPriceDataModel> results = DatabaseConnection.Instance
                    .GetEnergyMarketPricesbyTime(LastTimestamp.ToString("hh:mm:ss tt"))
                    .GetAwaiter().GetResult();
                EnergyMarketPriceDataModel response = results.FirstOrDefault();

                if (response == null) return;
                this.CurrentEnergyPrice = response.Price;

                if (string.IsNullOrEmpty(prosumer))
                {
                    SendToMany(this._prosumers.ToList(), $"{MessageTypes.Market.EnergyMarketPrice} {this.CurrentEnergyPrice}");
                }
                else
                {
                    Send(prosumer, $"{MessageTypes.Market.EnergyMarketPrice} {this.CurrentEnergyPrice}");
                }

                // LastTimestamp is already set from tick message, no need to add minutes
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error updating prosumer grid energy price");
            }
        }

        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Energy Market Agent started");
            Broadcast(MessageTypes.Market.FindProsumers);
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

                switch (action)
                {
                    case MessageTypes.Lifecycle.Started:
                        break;
                    case MessageTypes.Lifecycle.ProsumerStart:
                        HandleProsumerStart(message.Sender);
                        break;
                    case MessageTypes.Lifecycle.Tick:
                        HandleTick(parameters);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error processing message in EnergyMarketAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleProsumerStart(string prosumerName)
        {
            this._prosumers.Add(prosumerName);
            // Send initial price to new prosumer
            UpdateProsumerGridEnergyPrice(prosumerName);
        }

        private void HandleTick(string parameters)
        {
            if (!Utils.TryParseTickMessage(parameters, out int tickIndex, out DateTime simulationTime))
            {
                Serilog.Log.Warning("Failed to parse tick message in EnergyMarketAgent: {Parameters}", parameters);
                return;
            }

            _tickCount++;
            
            // Update price every 15 ticks (15 minutes = database data interval)
            if (_tickCount % TicksPerPriceUpdate == 0)
            {
                LastTimestamp = simulationTime;
                MasLog.InfoDebug(this, "debug", $"Tick {tickIndex}: Updating energy market price (every {TicksPerPriceUpdate} ticks)");
                UpdateProsumerGridEnergyPrice();
            }
        }
    }
}
