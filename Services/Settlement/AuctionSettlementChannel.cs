using ProsumerAuctionPlatform.Constants;

namespace ProsumerAuctionPlatform.Services.Settlement
{
    /// <summary>
    /// Placeholder auction settlement channel. Not yet active.
    /// To activate once DutchAuctioneerAgent is wired back into the world:
    ///   1. Set IsAvailable to true (or wire it to prosumer auction state).
    ///   2. Implement TrySettleSurplus / TrySettleDeficit to send to the auctioneer.
    ///   3. Insert at position 0 of both chains in ProsumerAgent — zero other changes needed.
    /// </summary>
    internal class AuctionSettlementChannel : ISettlementChannel
    {
        public string Name => AgentRoles.Auction;

        // TODO: Set to true and implement TrySettle* once the auction agent is active.
        public bool IsAvailable => false;
        public bool HasPendingSurplusRequest => false;
        public bool HasPendingDeficitRequest => false;

        public double TrySettleSurplus(double pending) => 0;
        public double TrySettleDeficit(double pending) => 0;
    }
}
