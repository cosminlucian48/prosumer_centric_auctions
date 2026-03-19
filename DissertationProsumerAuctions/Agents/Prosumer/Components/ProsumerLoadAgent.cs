using ActressMas;
using DissertationProsumerAuctions.Constants;
using DissertationProsumerAuctions.DatabaseConnections;
using System.Linq;

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
            Send(_myProsumerName, MessageTypes.ComponentReady);
        }

        private void UpdateProsumerLoadRate()
        {
            try
            {
                // Block on async call since Act() must be synchronous
                var results = DatabaseConnection.Instance
                    .GetProsumerLoadByIdAsync(_myProsumerId, _currentTimestamp)
                    .GetAwaiter().GetResult();

                var response = results.FirstOrDefault();
                if (response == null) return;

                Send(_myProsumerName, Utils.Str(MessageTypes.LoadUpdate, response.Load));
            }
            catch (Exception ex)
            {
                MasLog.Event(this, "error", ex.Message);
            }
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
                case MessageTypes.Tick:
                    HandleTick(parameters);
                    break;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error processing message in ProsumerLoadAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleProsumerStart()
        {
            UpdateProsumerLoadRate();
        }
        
        private void HandleTick(string parameters)
        {
            if (!Utils.TryParseTickMessage(parameters, out int tickIndex, out DateTime simulationTime))
            {
                Serilog.Log.Warning("Failed to parse tick parameters: {Parameters}", parameters);
                return;
            }

            _currentTickIndex = tickIndex;
            
            // Only query database every 15 ticks (15 minutes = database data interval)
            if (tickIndex % 15 == 0)
            {
                _currentTimestamp = simulationTime.ToString("hh:mm:ss tt");
                UpdateProsumerLoadRate();
            }
        }
    }
}
