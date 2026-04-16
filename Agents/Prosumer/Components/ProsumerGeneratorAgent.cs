using ActressMas;
using ProsumerAuctionPlatform.Constants;
using ProsumerAuctionPlatform.DatabaseConnections;
using System.Linq;

namespace ProsumerAuctionPlatform.Agents.Prosumer.Components
{
    internal class ProsumerGeneratorAgent : Agent
    {
        private readonly string _myProsumerName;
        private readonly int _myProsumerId;
        private string _currentTimestamp;
        private bool _hasReceivedFirstTick;
        private bool _hasSentInitialReading;

        public ProsumerGeneratorAgent(string prosumerName)
        {
            _myProsumerName = prosumerName;
            _currentTimestamp = "";
            _myProsumerId = int.Parse(prosumerName.Remove(0, 8));
        }

        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Prosumer Generator started!");
            Send(_myProsumerName, MessageTypes.ComponentReady);
        }


        private void UpdateProsumerGenerationRate()
        {
            try
            {
                // Block on async call since Act() must be synchronous
                var results = DatabaseConnection.Instance
                    .GetProsumerGenerationByIdAsync(_myProsumerId, _currentTimestamp)
                    .GetAwaiter().GetResult();
                var response = results.FirstOrDefault();

                if (response == null) return;
                Send(_myProsumerName, $"{MessageTypes.GenerationUpdate} {response.GenerationRate}");
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
                Serilog.Log.Error(ex, "Error processing message in ProsumerGeneratorAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleProsumerStart()
        {
            // If tick 0 already arrived, avoid duplicate startup read.
            if (_hasReceivedFirstTick)
            {
                return;
            }

            UpdateProsumerGenerationRate();
            _hasSentInitialReading = true;
        }
        
        private void HandleTick(string parameters)
        {
            if (!Utils.TryParseTickMessage(parameters, out int tickIndex, out DateTime simulationTime))
            {
                Serilog.Log.Warning("Failed to parse tick parameters: {Parameters}", parameters);
                return;
            }

            _hasReceivedFirstTick = true;
            
            // Only query database every 15 ticks (15 minutes = database data interval)
            if (tickIndex % 15 == 0)
            {
                // If startup already sent the first data point, skip the tick 0 duplicate.
                if (tickIndex == 0 && _hasSentInitialReading)
                {
                    return;
                }

                _currentTimestamp = simulationTime.ToString("hh:mm:ss tt");
                UpdateProsumerGenerationRate();
            }
        }
    }
}
