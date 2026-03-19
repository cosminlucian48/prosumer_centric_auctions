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
        // private Dictionary<string, List<double>> _localEnergyDifference;

        public ProsumerBatteryAgent(string prosumerName)
        {
            _myProsumerName = prosumerName;
            _myProsumerId = int.Parse(prosumerName.Remove(0, 8));
            
            _maximumCapacity = 15.0;
            _chargingEfficiency = 1;
            _dischargingEfficiency = 1;
            _batterySOCNotificationInterval = 1 * Utils.Delay;
            _maximumCapacity = 15.0;
            _currentCapacity = _maximumCapacity;
        }

        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Prosumer battery started!");
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

            if (_currentCapacity + energyToStore < _maximumCapacity)
            {
                _currentCapacity += energyToStore;
                Send(_myProsumerName, Utils.Str(MessageTypes.EnergyStored, energyToStore));
            }
            else
            {
                var capacityDifference = 0.0;
                if (_currentCapacity < _maximumCapacity)
                {
                    capacityDifference = _maximumCapacity - _currentCapacity;
                    _currentCapacity += capacityDifference;
                }
                Send(_myProsumerName, Utils.Str(MessageTypes.BatteryMaximumCapacity, capacityDifference));
            }
        }

        private void HandleProsumerStart()
        {
            // nothing to do yet
        }

        private void HandleTick(string parameters)
        {
            // nothing to do yet
        }
    }
}
