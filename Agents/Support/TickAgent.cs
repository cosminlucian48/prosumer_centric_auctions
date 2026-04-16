using ActressMas;
using ProsumerAuctionPlatform.Constants;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ProsumerAuctionPlatform.Agents.Support
{
    /// <summary>
    /// Agent responsible for broadcasting time ticks to all agents in the system.
    /// Each tick represents 1 minute of simulation time.
    /// This is the only agent that uses timers for time management.
    /// </summary>
    internal class TickAgent : Agent
    {
        private CancellationTokenSource _cancellationTokenSource;
        private Task _backgroundTask;
        private int _tickIndex = 0;
        private int _tickDelayMs = 10000; // 10 seconds real-time delay between ticks
        public DateTime StartTimestamp { get; private set; }
        
        public TickAgent() : base()
        {
        }
        
        public override void Setup()
        {
            StartTimestamp = new DateTime(
                2024, 1, 1, 
                0, 0, 0,
                DateTimeKind.Utc
            );
            
            _cancellationTokenSource = new CancellationTokenSource();
            
            // Run async loop in background task with proper error handling
            // Each tick represents 1 minute of simulation time
            _backgroundTask = Task.Run(async () =>
            {
                try
                {
                    while (!_cancellationTokenSource.Token.IsCancellationRequested)
                    {
                        // Check if agent is still in the environment
                        // If not, stop the background task
                        if (Environment != null && !Environment.AllAgents().Contains(Name))
                        {
                            Serilog.Log.Information("TickAgent removed from environment, stopping background task");
                            break;
                        }
                        
                        // Calculate simulation time: 1 tick = 1 minute
                        DateTime currentTimestamp = StartTimestamp.AddMinutes(_tickIndex);
                        
                        // Broadcast tick message with format: "tick {tickIndex} {simulationTime}"
                        Broadcast($"{MessageTypes.Lifecycle.Tick} {_tickIndex} {currentTimestamp.ToString("hh:mm:ss tt")}");
                        
                        _tickIndex++;
                        
                        // Use cancellation token for delay
                        await Task.Delay(_tickDelayMs, _cancellationTokenSource.Token);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when cancellation is requested
                    Serilog.Log.Information("TickAgent background task cancelled");
                }
                catch (Exception ex)
                {
                    Serilog.Log.Error(ex, "Error in TickAgent loop");
                }
                finally
                {
                    // Clean up resources when task ends
                    _cancellationTokenSource?.Dispose();
                    _cancellationTokenSource = null;
                }
            }, _cancellationTokenSource.Token);
        }
        
        public override void Act(Message m)
        {
            // TickAgent doesn't respond to messages
        }
        
        /// <summary>
        /// Stops the ticking loop and cancels the background task.
        /// Should be called before removing the agent from the environment to ensure proper cleanup.
        /// </summary>
        public void StopTicking()
        {
            if (_cancellationTokenSource != null && !_cancellationTokenSource.Token.IsCancellationRequested)
            {
                _cancellationTokenSource.Cancel();
                
                // Wait for background task to complete (with timeout)
                try
                {
                    _backgroundTask?.Wait(TimeSpan.FromSeconds(2));
                }
                catch (AggregateException ex)
                {
                    // Ignore cancellation exceptions
                    if (!(ex.InnerException is OperationCanceledException))
                    {
                        Serilog.Log.Warning(ex, "Error waiting for TickAgent background task to complete");
                    }
                }
            }
        }
        
        public void SetTickDelay(int ms)
        {
            _tickDelayMs = ms;
        }
    }
}