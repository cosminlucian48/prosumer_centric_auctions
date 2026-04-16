namespace ProsumerAuctionPlatform.Models
{
    public readonly record struct ProsumerCapabilities(
        bool HasBattery,
        bool HasGenerator,
        bool HasLoad);
}