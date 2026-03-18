using ActressMas;
using DissertationProsumerAuctions.DatabaseConnections;

namespace DissertationProsumerAuctions.Agents.Prosumer.Components
{
    internal class ProsumerGeneratorAgent : Agent
    {
        private readonly string _myProsumerName;
        private readonly int _myProsumerId;
        private string _currentTimestamp;
        private int _currentTickIndex;

        public ProsumerGeneratorAgent(string prosumerName)
        {
            _myProsumerName = prosumerName;
            _currentTimestamp = "";
            _currentTickIndex = 0;
            _myProsumerId = int.Parse(prosumerName.Remove(0, 8));
        }

        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Prosumer Generator started!");
            Send(_myProsumerName, "component_ready");
        }


        private Task UpdateProsumerGenerationRate()
        {
            return Task.Run(async () =>
            {
                try
                {
                    var results = await DatabaseConnection.Instance
                        .GetProsumerGenerationByIdAsync(_myProsumerId, _currentTimestamp);
                    var response = results.FirstOrDefault();

                    if (response == null) return;
                    Send(_myProsumerName, Utils.Str("generation_update", response.GenerationRate));
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
            _ = UpdateProsumerGenerationRate();
        }
        
        private void HandleTick(string parameters)
        {
            Utils.ParseMessage(parameters, out var tickIndex, out var receivedTimestamp);
            _currentTimestamp = receivedTimestamp;
            _currentTickIndex = Int32.Parse(tickIndex);
            _ = UpdateProsumerGenerationRate();
        }
    }
}
