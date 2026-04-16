namespace ProsumerAuctionPlatform.Models
{
    public readonly record struct ProsumerDefinition(
        string Name,
        ProsumerCapabilities Capabilities,
        bool HasAuction,
        ProsumerBatteryOverrides? Battery);
}