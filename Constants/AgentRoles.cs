namespace ProsumerAuctionPlatform.Constants
{
    /// <summary>
    /// Role label constants used by the prosumer role registry.
    /// Each component agent announces its role in the component_ready message payload,
    /// and ProsumerAgent maps that role to the sender's actual agent name.
    /// </summary>
    public static class AgentRoles
    {
        public const string Battery     = "battery";
        public const string Generator   = "generator";
        public const string Load        = "load";
        public const string EnergyMarket = "energy_market";
        public const string Auction     = "auction";
    }
}
