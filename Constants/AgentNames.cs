namespace ProsumerAuctionPlatform.Constants
{
    /// <summary>
    /// Constants for agent names used throughout the system.
    /// </summary>
    public static class AgentNames
    {
        public const string DutchAuctioneer = "dutchauctioneer";
        public const string EnergyMarket = "energymarket1";
        public const string Tick = "tick";
        // TODO: Replace with proper constant when VPP agent is implemented
        public const string VPP = "vpp";
        
        public static string GetProsumerName(int prosumerId) => $"prosumer{prosumerId}";
        public static string GetBatteryName(string prosumerName) => $"battery{prosumerName}";
        public static string GetGeneratorName(string prosumerName) => $"generator{prosumerName}";
        public static string GetLoadName(string prosumerName) => $"load{prosumerName}";
    }
}
