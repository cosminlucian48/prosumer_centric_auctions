using Microsoft.AspNetCore.Mvc;
using DissertationProsumerAuctions.Services;
using DissertationProsumerAuctions.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace DissertationProsumerAuctions.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulationController : ControllerBase
    {
        private readonly SimulationService _simulationService;
        private readonly IHubContext<SimulationHub> _hubContext;
        
        public SimulationController(SimulationService simulationService, IHubContext<SimulationHub> hubContext)
        {
            _simulationService = simulationService;
            _hubContext = hubContext;
        }
        
        /// <summary>
        /// Initialize the simulation with a specified number of prosumers
        /// </summary>
        [HttpPost("initialize")]
        public IActionResult Initialize([FromBody] InitializeRequest request)
        {
            try
            {
                _simulationService.InitializeWorld(request.NumberOfProsumers);
                return Ok(new { message = "Simulation initialized successfully", numberOfProsumers = request.NumberOfProsumers });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Start the simulation
        /// </summary>
        [HttpPost("start")]
        public IActionResult Start()
        {
            try
            {
                _simulationService.StartSimulation();
                _hubContext.Clients.Group("SimulationUpdates").SendAsync("SimulationStarted");
                return Ok(new { message = "Simulation started" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Pause the simulation
        /// </summary>
        [HttpPost("pause")]
        public IActionResult Pause()
        {
            try
            {
                var world = _simulationService.GetWorld();
                if (world == null)
                {
                    return NotFound(new { error = "Simulation not initialized" });
                }
                
                world.PauseSimulation();
                _hubContext.Clients.Group("SimulationUpdates").SendAsync("SimulationPaused");
                return Ok(new { message = "Simulation paused", status = "paused" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Resume the simulation
        /// </summary>
        [HttpPost("resume")]
        public IActionResult Resume()
        {
            try
            {
                var world = _simulationService.GetWorld();
                if (world == null)
                {
                    return NotFound(new { error = "Simulation not initialized" });
                }
                
                world.ResumeSimulation();
                _hubContext.Clients.Group("SimulationUpdates").SendAsync("SimulationResumed");
                return Ok(new { message = "Simulation resumed", status = "running" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Stop the simulation
        /// </summary>
        [HttpPost("stop")]
        public IActionResult Stop()
        {
            try
            {
                var world = _simulationService.GetWorld();
                if (world == null)
                {
                    return NotFound(new { error = "Simulation not initialized" });
                }
                
                world.StopSimulation();
                _hubContext.Clients.Group("SimulationUpdates").SendAsync("SimulationStopped");
                return Ok(new { message = "Simulation stopped", status = "stopped" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Set the simulation speed (delay between ticks in milliseconds)
        /// </summary>
        [HttpPost("speed")]
        public IActionResult SetSpeed([FromBody] SpeedRequest request)
        {
            try
            {
                var world = _simulationService.GetWorld();
                if (world == null)
                {
                    return NotFound(new { error = "Simulation not initialized" });
                }
                
                if (request.DelayMs < 0)
                {
                    return BadRequest(new { error = "Delay must be non-negative" });
                }
                
                world.SetSimulationSpeed(request.DelayMs);
                _hubContext.Clients.Group("SimulationUpdates").SendAsync("SimulationSpeedChanged", request.DelayMs);
                return Ok(new { message = "Simulation speed updated", delayMs = request.DelayMs });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Get the current simulation status
        /// </summary>
        [HttpGet("status")]
        public IActionResult GetStatus()
        {
            try
            {
                var world = _simulationService.GetWorld();
                if (world == null)
                {
                    return Ok(new 
                    { 
                        initialized = false,
                        running = false,
                        paused = false,
                        tick = 0,
                        delay = 0
                    });
                }
                
                var status = new 
                { 
                    initialized = true,
                    running = world.IsSimulationRunning,
                    paused = world.IsSimulationPaused,
                    tick = world.CurrentTickIndex,
                    delay = world.CurrentTickDelay
                };
                
                // Also broadcast status update via SignalR
                _hubContext.Clients.Group("SimulationUpdates").SendAsync("SimulationStatus", status);
                
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
        
        /// <summary>
        /// Subscribe to real-time status updates (via SignalR)
        /// This endpoint explains how to use SignalR for real-time updates
        /// </summary>
        [HttpGet("status/subscribe")]
        public IActionResult SubscribeInfo()
        {
            return Ok(new
            {
                message = "Connect to SignalR hub at /simulationHub to receive real-time updates",
                hubUrl = "/simulationHub",
                events = new[]
                {
                    "SimulationStarted",
                    "SimulationPaused", 
                    "SimulationResumed",
                    "SimulationStopped",
                    "SimulationSpeedChanged",
                    "SimulationStatus"
                },
                instructions = new
                {
                    step1 = "Connect to ws://localhost:5000/simulationHub",
                    step2 = "Call 'JoinSimulationGroup' method",
                    step3 = "Listen for events: SimulationStarted, SimulationPaused, etc.",
                    example = "See FRONTEND_INTEGRATION.md for code examples"
                }
            });
        }
    }
    
    public class InitializeRequest
    {
        public int NumberOfProsumers { get; set; }
    }
    
    public class SpeedRequest
    {
        public int DelayMs { get; set; }
    }
}
