using Microsoft.AspNetCore.SignalR;
using DissertationProsumerAuctions.Hubs;

namespace DissertationProsumerAuctions.Services
{
    public class SimulationBroadcastService
    {
        private readonly IHubContext<SimulationHub> _hubContext;
        
        public SimulationBroadcastService(IHubContext<SimulationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        
        public async Task BroadcastTick(int tickIndex, string timestamp)
        {
            await _hubContext.Clients.Group("SimulationUpdates").SendAsync("TickUpdate", new
            {
                tickIndex = tickIndex,
                timestamp = timestamp
            });
        }
        
        public async Task BroadcastSimulationStatus(bool running, bool paused, int currentTick, int delay)
        {
            await _hubContext.Clients.Group("SimulationUpdates").SendAsync("SimulationStatus", new
            {
                running = running,
                paused = paused,
                currentTick = currentTick,
                delay = delay
            });
        }
        
        public async Task BroadcastAgentMessage(string fromAgent, string toAgent, string messageType, object? data = null)
        {
            await _hubContext.Clients.Group("SimulationUpdates").SendAsync("AgentMessage", new
            {
                fromAgent = fromAgent,
                toAgent = toAgent,
                messageType = messageType,
                data = data,
                timestamp = DateTime.UtcNow
            });
        }
        
        public async Task BroadcastAgentEvent(string agentName, string eventType, object? data = null)
        {
            await _hubContext.Clients.Group("SimulationUpdates").SendAsync("AgentEvent", new
            {
                agentName = agentName,
                eventType = eventType,
                data = data,
                timestamp = DateTime.UtcNow
            });
        }
    }
}
