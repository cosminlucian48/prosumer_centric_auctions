using ActressMas;
using ProsumerAuctionPlatform.Constants;

namespace ProsumerAuctionPlatform.Agents.Prosumer.Components
{
    internal class ProsumerBatteryAgent : Agent
    {
        private readonly string _myProsumerName;
        private double _currentCapacity; // storage value as WH.   1kwh of generated energy = 1 000 wh
        private readonly double _maximumCapacity;
        // TODO(battery-model): Apply charging efficiency losses when storing energy.
        private readonly double _chargingEfficiency;
        // TODO(battery-model): Apply discharging efficiency losses when consuming energy.
        private readonly double _dischargingEfficiency;
        // private Dictionary<string, List<double>> _localEnergyDifference;

        public ProsumerBatteryAgent(
            string prosumerName,
            double initialStateOfChargePercent,
            double maximumCapacity,
            double chargingEfficiency,
            double dischargingEfficiency)
        {
            _myProsumerName = prosumerName;
            
            _maximumCapacity = maximumCapacity;
            _chargingEfficiency = chargingEfficiency;
            _dischargingEfficiency = dischargingEfficiency;
            _currentCapacity = _maximumCapacity * Math.Clamp(initialStateOfChargePercent, 0.0, 1.0);
        }

        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Prosumer Battery started!");
            Send(_myProsumerName, $"{MessageTypes.Lifecycle.ComponentReady} {AgentRoles.Battery}");
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

                LogBatteryState("Before", action, parameters);

                switch (action)
                {
                    case MessageTypes.Lifecycle.ProsumerStart:
                        HandleProsumerStart();
                        break;
                    case MessageTypes.Battery.StoreEnergy:
                        HandleStoreEnergy(parameters);
                        break;
                    case MessageTypes.Battery.ConsumeEnergy:
                        HandleConsumeEnergy(parameters);
                        break;
                }

                LogBatteryState("After", action, parameters);
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
                Send(_myProsumerName, $"{MessageTypes.Battery.EnergyStored} {storedEnergy}");
            }

            double remainingEnergy = energyToStore - storedEnergy;
            if (remainingEnergy > 0)
            {
                Send(_myProsumerName, $"{MessageTypes.Battery.BatteryMaximumCapacity} {remainingEnergy}");
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
                Send(_myProsumerName, $"{MessageTypes.Battery.EnergyConsumed} 0");
                return;
            }

            double energyConsumed = Math.Min(_currentCapacity, energyToConsume);
            _currentCapacity -= energyConsumed;

            Send(_myProsumerName, $"{MessageTypes.Battery.EnergyConsumed} {energyConsumed}");
        }

        private void SendBatterySocUpdate()
        {
            Send(_myProsumerName, $"{MessageTypes.Battery.BatterySOC} {_currentCapacity}");
        }

        private void LogBatteryState(string phase, string action, string parameters)
        {
            MasLog.InfoDebug(
                this,
                "debug",
                $"{phase} {action} {parameters}: capacity {_currentCapacity}; max {_maximumCapacity}; charge_eff {_chargingEfficiency}; discharge_eff {_dischargingEfficiency}");
        }
    }
}
