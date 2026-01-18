using ActressMas;

namespace DissertationProsumerAuctions.Agents.Prosumer.Components
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
            Send(_myProsumerName, "component_ready");
        }

        public override void Act(Message message)
        {
            MasLog.Received(this, message, $"[{message.Sender} -> {Name}]: {message.Content}");

            Utils.ParseMessage(message.Content, out var action, out var parameters);

            switch (action)
            {
                case "prosumer_start":
                    HandleProsumerStart();
                    break;
                case "store":
                    HandleStoreEnergy(parameters);
                    break;
                case "tick":
                    HandleTick(parameters);
                    break;
            }
        }

        private void HandleStoreEnergy(string parameters)
        {
            var energyToStore = double.Parse(parameters);
            if (_currentCapacity + energyToStore < _maximumCapacity)
            {
                _currentCapacity += energyToStore;
                Send(_myProsumerName, Utils.Str("energy_stored", energyToStore));
            }
            else
            {
                var capacityDifference = 0.0;
                if (_currentCapacity < _maximumCapacity)
                {
                    capacityDifference = _maximumCapacity - _currentCapacity;
                    _currentCapacity += capacityDifference;
                }
                Send(_myProsumerName, Utils.Str("battery_maximum_capacity", capacityDifference));
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
