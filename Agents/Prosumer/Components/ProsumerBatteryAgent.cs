using ActressMas;
using ProsumerAuctionPlatform.Constants;

namespace ProsumerAuctionPlatform.Agents.Prosumer.Components
{
    internal class ProsumerBatteryAgent : Agent
    {
        private readonly string _myProsumerName;
        private double _currentCapacity; // storage value as WH.   1kwh of generated energy = 1 000 wh
        private readonly double _maximumCapacity;
        private readonly int _chargingEfficiency;
        private readonly int _dischargingEfficiency;
        private readonly int _batterySOCNotificationInterval;
        private readonly int _myProsumerId;
        private const double InitialStateOfChargePercent = 0.5;
        // private Dictionary<string, List<double>> _localEnergyDifference;

        public ProsumerBatteryAgent(string prosumerName, int simulationDelayMs)
        {
            _myProsumerName = prosumerName;
            _myProsumerId = int.Parse(prosumerName.Remove(0, 8));
            
            _maximumCapacity = 15.0;
            _chargingEfficiency = 1;
            _dischargingEfficiency = 1;
            _batterySOCNotificationInterval = simulationDelayMs;
            // Start at 50% SOC to avoid immediate overflow exports and allow both charge/discharge flows.
            _currentCapacity = _maximumCapacity * InitialStateOfChargePercent;
        }

        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Prosumer Battery started!");
            Send(_myProsumerName, MessageTypes.ComponentReady);
        }

        public override void Act(Message message)
        {
            try
            {
                MasLog.Received(this, message, $"[{message.Sender} -> {Name}]: {message.Content}");

                if (!Utils.TryParseMessage(message.Content, out var action, out var parameters))
                {
                    MasLog.Event(this, "error", $"Failed to parse message: {message.Content}");
                    return;
                }

                switch (action)
            {
                case MessageTypes.ProsumerStart:
                    HandleProsumerStart();
                    break;
                case MessageTypes.StoreEnergy:
                    HandleStoreEnergy(parameters);
                    break;
                case MessageTypes.ConsumeEnergy:
                    HandleConsumeEnergy(parameters);
                    break;
                case MessageTypes.Tick:
                    HandleTick(parameters);
                    break;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error processing message in ProsumerBatteryAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleStoreEnergy(string parameters)
        {
            if (!double.TryParse(parameters, out double energyToStore))
            {
                Serilog.Log.Warning("Invalid energy to store value: {Value}", parameters);
                return;
            }

            if (energyToStore <= 0)
            {
                return;
            }

            double availableCapacity = _maximumCapacity - _currentCapacity;
            double storedEnergy = Math.Min(availableCapacity, energyToStore);

            if (storedEnergy > 0)
            {
                _currentCapacity += storedEnergy;
                Send(_myProsumerName, $"{MessageTypes.EnergyStored} {storedEnergy}");
                SendBatterySocUpdate();
            }

            double remainingEnergy = energyToStore - storedEnergy;
            if (remainingEnergy > 0)
            {
                Send(_myProsumerName, $"{MessageTypes.BatteryMaximumCapacity} {remainingEnergy}");
            }
        }

        private void HandleProsumerStart()
        {
            SendBatterySocUpdate();
        }

        private void HandleConsumeEnergy(string parameters)
        {
            if (!double.TryParse(parameters, out double energyToConsume))
            {
                Serilog.Log.Warning("Invalid energy to consume value: {Value}", parameters);
                return;
            }

            if (energyToConsume <= 0)
            {
                Send(_myProsumerName, $"{MessageTypes.EnergyConsumed} 0");
                return;
            }

            double energyConsumed = Math.Min(_currentCapacity, energyToConsume);
            _currentCapacity -= energyConsumed;

            Send(_myProsumerName, $"{MessageTypes.EnergyConsumed} {energyConsumed}");
            SendBatterySocUpdate();
        }

        private void HandleTick(string parameters)
        {
            // nothing to do yet
        }

        private void SendBatterySocUpdate()
        {
            Send(_myProsumerName, $"{MessageTypes.BatterySOC} {_currentCapacity}");
        }
    }
}
