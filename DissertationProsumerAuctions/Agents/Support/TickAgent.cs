using ActressMas;

namespace DissertationProsumerAuctions.Agents.Prosumer
{
    internal class TickAgent : Agent
    {
        private string _agentName; 
        private bool _running = true;
        int _tickIndex = 0;
        private int _tickDelayMs = 10000; // 10seconds
        public DateTime startTimestamp;
        
        
        public TickAgent() : base()
        {
            
        }
        
        public override async void Setup()
        {   
            startTimestamp = new DateTime(
                2024, 1, 1, 
                0, 0, 0,
                DateTimeKind.Utc
            );
            while (_running)
            {
                DateTime currentTimestamp = startTimestamp.AddMinutes(_tickIndex * 15);
                
                Broadcast($"tick {_tickIndex} {currentTimestamp.ToString("hh:mm:ss tt")}");
                
                _tickIndex++;
                
                await Task.Delay(_tickDelayMs);
            }
        }
        
        public override void Act(Message m)
        {
            // nothing
        }
        
        public void StopTicking()
        {
            _running = false;
        }
        
        public void SetTickDelay(int ms)
        {
            _tickDelayMs = ms;
        }
    }
}