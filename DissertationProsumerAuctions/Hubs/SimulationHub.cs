using Microsoft.AspNetCore.SignalR;

namespace DissertationProsumerAuctions.Hubs
{
    public class SimulationHub : Hub
    {
        public async Task JoinSimulationGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "SimulationUpdates");
        }
        
        public async Task LeaveSimulationGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "SimulationUpdates");
        }
    }
}
