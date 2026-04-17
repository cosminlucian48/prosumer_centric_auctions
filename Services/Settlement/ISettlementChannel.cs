namespace ProsumerAuctionPlatform.Services.Settlement
{
    /// <summary>
    /// A single step in the energy settlement chain.
    /// Each channel is given the opportunity to absorb pending surplus or deficit energy.
    /// Unresolved energy passes to the next channel in the list.
    /// </summary>
    internal interface ISettlementChannel
    {
        string Name { get; }

        /// <summary>
        /// Whether this channel is capable of handling energy at all
        /// (e.g. false for auction stub, or false for battery when HasBattery = false).
        /// A channel with IsAvailable = false is skipped entirely — the chain continues.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Whether this channel has an outstanding asynchronous surplus request in flight.
        /// When true, the chain stops and waits for a confirmation callback rather than
        /// falling through to the next channel.
        /// </summary>
        bool HasPendingSurplusRequest { get; }

        /// <summary>
        /// Whether this channel has an outstanding asynchronous deficit request in flight.
        /// When true, the chain stops rather than falling through to the next channel.
        /// </summary>
        bool HasPendingDeficitRequest { get; }

        /// <summary>
        /// Attempt to absorb <paramref name="pending"/> units of surplus energy.
        /// Returns the amount dispatched to this channel (may equal pending for async channels
        /// that acknowledge only on confirmation). Returns 0 if nothing was dispatched.
        /// </summary>
        double TrySettleSurplus(double pending);

        /// <summary>
        /// Attempt to cover <paramref name="pending"/> units of deficit energy.
        /// Returns the amount dispatched. Returns 0 if nothing was dispatched.
        /// </summary>
        double TrySettleDeficit(double pending);
    }
}
