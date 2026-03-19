using ActressMas;
using DissertationProsumerAuctions.DatabaseConnections;

namespace DissertationProsumerAuctions.Agents.Prosumer.Components
{
    internal class ProsumerLoadAgent : Agent
    {

        private readonly string _myProsumerName;
        private readonly int _myProsumerId;
        private string _currentTimestamp;
        private int _currentTickIndex;
        
        public ProsumerLoadAgent(string prosumerName)
        {
            _myProsumerName = prosumerName;
            _myProsumerId = int.Parse(prosumerName.Remove(0, 8));
            _currentTimestamp = "";
        }
        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Prosumer Load started!");
            Send(_myProsumerName, "component_ready");
        }

        private Task UpdateProsumerLoadRate() // impossible with void
        {
            return Task.Run(async () =>
            {
                try
                {
                    var results = await DatabaseConnection.Instance
                        .GetProsumerLoadByIdAsync(_myProsumerId, _currentTimestamp);

                    var response = results.FirstOrDefault();
                    if (response == null) return;

                    Send(_myProsumerName, Utils.Str("load_update", response.Load));
                }
                catch (Exception ex)
                {
                    MasLog.Event(this, "error", ex.Message);
                }
            });
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
                case "tick":
                    HandleTick(parameters);
                    break;
            }
        }

        private void HandleProsumerStart()
        {
            _ = UpdateProsumerLoadRate();
        }
        
        private void HandleTick(string parameters)
        {
            Utils.ParseMessage(parameters, out var tickIndex, out var receivedTimestamp);
            _currentTimestamp = receivedTimestamp;
            _currentTickIndex = Int32.Parse(tickIndex);
            _ = UpdateProsumerLoadRate();
        }
    }
}
