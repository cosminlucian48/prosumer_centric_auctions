namespace ProsumerAuctionPlatform.Services.Settlement
{
    /// <summary>
    /// Exposes the prosumer state and communication primitives that settlement
    /// channels need, without making ProsumerAgent fields public.
    /// </summary>
    internal interface IProsumerSettlementContext
    {
        bool HasBattery { get; }
        double BatterySOC { get; }
        double GridBuyPrice { get; }
        double GridSellPrice { get; }

        void SendToRole(string role, string content);

        /// <summary>Adjust the prosumer's running bill. Positive = income, negative = cost.</summary>
        void AddToBill(double delta);
    }
}
