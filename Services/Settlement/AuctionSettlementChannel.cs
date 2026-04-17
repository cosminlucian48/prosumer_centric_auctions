using ProsumerAuctionPlatform.Constants;

namespace ProsumerAuctionPlatform.Services.Settlement
{
    /// <summary>
    /// Placeholder auction settlement channel. Not yet active.
    /// To activate once DutchAuctioneerAgent is wired back into the world:
    ///   1. Accept an <c>IProsumerSettlementContext</c> in the constructor.
    ///   2. Seed a per-prosumer price preference at construction:
    ///      <c>_pricePreference = Utils.RandNoGen.NextDouble() * (1.2 - 0.8) + 0.8</c>
    ///      Used to compute bid threshold: <c>ctx.GridSellPrice * _pricePreference</c>.
    ///   3. Implement TrySettleSurplus / TrySettleDeficit to send bids to the auctioneer
    ///      and handle <c>MessageTypes.Auction.SellingPrice</c> confirmations.
    ///   4. Set IsAvailable to true once the auctioneer is registered.
    ///   5. Insert at position 0 of both chains in ProsumerAgent — zero other changes needed.
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
