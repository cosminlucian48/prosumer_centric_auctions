namespace ProsumerAuctionPlatform.Constants
{
    /// <summary>
    /// Constants for message types used in agent communication, grouped by domain.
    /// </summary>
    public static class MessageTypes
    {
        /// <summary>Simulation lifecycle messages (tick broadcasts, component readiness, startup signals).</summary>
        public static class Lifecycle
        {
            public const string Tick           = "tick";
            public const string ComponentReady = "component_ready";
            public const string ProsumerStart  = "prosumer_start";
            public const string Started        = "started";
        }

        /// <summary>Energy market messages (price broadcasts, prosumer discovery).</summary>
        public static class Market
        {
            public const string FindProsumers     = "find_prosumers";
            public const string EnergyMarketPrice = "energy_market_price";
        }

        /// <summary>Battery component messages (prosumer ↔ battery agent).</summary>
        public static class Battery
        {
            public const string BatterySOC             = "battery_soc";
            public const string StoreEnergy            = "store";
            public const string EnergyStored           = "energy_stored";
            public const string ConsumeEnergy          = "consume";
            public const string EnergyConsumed         = "energy_consumed";
            public const string BatteryMaximumCapacity = "battery_maximum_capacity";
        }

        /// <summary>Energy reading messages (component agents → prosumer agent).</summary>
        public static class Readings
        {
            public const string LoadUpdate       = "load_update";
            public const string GenerationUpdate = "generation_update";
        }

        /// <summary>Grid settlement messages (prosumer ↔ energy market / grid).</summary>
        public static class Grid
        {
            public const string StartAuctioning        = "startAuctioning";
            public const string SellEnergy             = "sell_energy";
            public const string SellEnergyConfirmation = "sell_energy_confirmation";
            public const string BuyEnergyConfirmation  = "buy_energy_confirmation";
        }

        /// <summary>Dutch auction messages (prosumer ↔ auctioneer agent).</summary>
        public static class Auction
        {
            public const string ExcessToSell = "excess_to_sell";
            public const string DeficitToBuy = "deficit_to_buy";
            public const string SellingPrice  = "selling_price";
            public const string EnergyBid    = "energy_bid";
        }
    }
}
