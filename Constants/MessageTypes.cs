namespace ProsumerAuctionPlatform.Constants
{
    /// <summary>
    /// Constants for message types used in agent communication.
    /// </summary>
    public static class MessageTypes
    {
        // Tick system
        public const string Tick = "tick";
        
        // Component lifecycle
        public const string ComponentReady = "component_ready";
        public const string ProsumerStart = "prosumer_start";
        public const string Started = "started";
        
        // Energy updates
        public const string LoadUpdate = "load_update";
        public const string GenerationUpdate = "generation_update";
        public const string BatterySOC = "battery_soc";
        public const string EnergyStored = "energy_stored";
        public const string BatteryMaximumCapacity = "battery_maximum_capacity";
        public const string StoreEnergy = "store";
        public const string SellEnergy = "sell_energy";
        
        // Market
        public const string EnergyMarketPrice = "energy_market_price";
        public const string FindProsumers = "find_prosumers";
        
        // Auction
        public const string ExcessToSell = "excess_to_sell";
        public const string DeficitToBuy = "deficit_to_buy";
        public const string SellingPrice = "selling_price";
        public const string EnergyBid = "energy_bid";
        public const string StartAuctioning = "startAuctioning";
        
        // Transactions
        public const string SellEnergyConfirmation = "sell_energy_confirmation";
        public const string BuyEnergyConfirmation = "buy_energy_confirmation";
    }
}
