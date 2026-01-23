using ActressMas;

namespace DissertationProsumerAuctions.Agents.Support
{
    internal class TickAgent : Agent
    {
        private string _agentName; 
        private bool _running = true;
        private bool _paused = false;
        private readonly object _lockObject = new object();
        int _tickIndex = 0;
        private int _tickDelayMs = 5000; // 10seconds
        private CancellationTokenSource _delayCancellationTokenSource;
        public DateTime startTimestamp;
        
        // Public properties for status checking
        public bool IsRunning => _running;
        public bool IsPaused => _paused;
        public int CurrentTickIndex => _tickIndex;
        public int TickDelayMs => _tickDelayMs;
        
        public TickAgent() : base()
        {
            _delayCancellationTokenSource = new CancellationTokenSource();
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
                MasLog.InfoDebug(this, "debug", $"TickAgent: Running");
                lock (_lockObject)
                {
                    if (_paused)
                    {
                        // Wait while paused
                        Monitor.Wait(_lockObject);
                    }
                }
                
                if (!_running) break;
                
                DateTime currentTimestamp = startTimestamp.AddMinutes(_tickIndex * 15);
                
                Broadcast($"tick {_tickIndex} {currentTimestamp.ToString("hh:mm:ss tt")}");
                
                _tickIndex++;
                
                try
                {
                    await Task.Delay(_tickDelayMs, _delayCancellationTokenSource.Token);
                }
                catch (TaskCanceledException)
                {
                    // Delay was cancelled, will restart with new delay
                    _delayCancellationTokenSource = new CancellationTokenSource();
                }
            }
        }
        
        public override void Act(Message m)
        {
            // Handle control messages
            var content = m.Content.ToLower().Trim();
            
            switch (content)
            {
                case "stop":
                    StopTicking();
                    break;
                case "pause":
                    PauseTicking();
                    break;
                case "resume":
                case "start":
                    ResumeTicking();
                    break;
                default:
                    if (content.StartsWith("set_speed "))
                    {
                        var parts = content.Split(' ');
                        if (parts.Length == 2 && int.TryParse(parts[1], out int delayMs))
                        {
                            SetTickDelay(delayMs);
                        }
                    }
                    break;
            }
        }
        
        public void StopTicking()
        {
            MasLog.Event(this, "message", "Stopping tick agent");
            lock (_lockObject)
            {
                _running = false;
                _paused = false;
                Monitor.PulseAll(_lockObject);
            }
            _delayCancellationTokenSource?.Cancel();
        }
        
        public void PauseTicking()
        {
            MasLog.Event(this, "message", "Pausing tick agent");
            lock (_lockObject)
            {
                _paused = true;
            }
        }
        
        public void ResumeTicking()
        {
            MasLog.Event(this, "message", "Resuming tick agent");
            lock (_lockObject)
            {
                _paused = false;
                Monitor.PulseAll(_lockObject);
            }
        }
        
        public void SetTickDelay(int ms)
        {   
            MasLog.Event(this, "message", $"Setting tick delay to {ms}ms");
            if (ms < 0) ms = 0; // Prevent negative delays
            
            lock (_lockObject)
            {
                _tickDelayMs = ms;
            }
            
            // Cancel current delay to apply new delay immediately
            _delayCancellationTokenSource?.Cancel();
        }
    }
}