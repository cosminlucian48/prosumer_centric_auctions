namespace ProsumerAuctionPlatform.Services.Settlement
{
    /// <summary>
    /// Fallback settlement channel that buys/sells directly against the grid.
    /// Always available and always last in the chain.
    /// </summary>
    internal class GridSettlementChannel : ISettlementChannel
    {
        private readonly IProsumerSettlementContext _ctx;

        public GridSettlementChannel(IProsumerSettlementContext ctx)
        {
            _ctx = ctx;
        }

        public string Name => "grid";
        public bool IsAvailable => true;
        public bool HasPendingSurplusRequest => false;
        public bool HasPendingDeficitRequest => false;

        public double TrySettleSurplus(double pending)
        {
            if (pending <= double.Epsilon) return 0;
            _ctx.AddToBill(pending * _ctx.GridSellPrice);
            return pending;
        }

        public double TrySettleDeficit(double pending)
        {
            if (pending <= double.Epsilon) return 0;
            _ctx.AddToBill(-(pending * _ctx.GridBuyPrice));
            return pending;
        }
    }
}
