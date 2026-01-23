using DissertationProsumerAuctions.Agents.Prosumer;
using Serilog;

namespace DissertationProsumerAuctions.Services
{
    public class SimulationService
    {
        private World? _world;
        private readonly object _lockObject = new object();
        
        public World GetOrCreateWorld(int numberOfTurns = 0, int delayAfterTurn = 0)
        {
            if (_world == null)
            {
                lock (_lockObject)
                {
                    if (_world == null)
                    {
                        _world = new World(numberOfTurns, delayAfterTurn);
                        Log.Information("World created with {NumberOfTurns} turns and {Delay}ms delay", numberOfTurns, delayAfterTurn);
                    }
                }
            }
            return _world;
        }
        
        public World? GetWorld()
        {
            return _world;
        }
        
        public void InitializeWorld(int numberOfProsumers)
        {
            var world = GetOrCreateWorld(100, Utils.Delay);
            
            Log.Information("Initializing simulation with {NumberOfProsumers} prosumers", numberOfProsumers);
            
            for (int i = 1; i <= numberOfProsumers; i++)
            {
                var prosumerAgent = new ProsumerAgent();
                world.AddProsumer(prosumerAgent, $"prosumer{i}");
            }
            
            Log.Information("Simulation initialized successfully with {NumberOfProsumers} prosumers", numberOfProsumers);
        }
        
        public void StartSimulation()
        {
            var world = GetWorld();
            if (world == null)
            {
                Log.Warning("Cannot start simulation: World not initialized. Call InitializeWorld first.");
                return;
            }
            
            if (world.IsSimulationRunning)
            {
                Log.Warning("Simulation is already running");
                return;
            }
            
            Log.Information("Starting simulation in background task...");
            
            // Start the world in a background task
            Task.Run(() =>
            {
                try
                {
                    Log.Information("Simulation world.Start() called - agents should begin processing");
                    world.Start();
                    Log.Information("Simulation world.Start() completed");
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error occurred while running simulation");
                }
            });
        }
    }
}
