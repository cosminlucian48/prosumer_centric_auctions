namespace ProsumerAuctionPlatform.Models
{
    public readonly record struct ProsumerBatteryOverrides(
        double? InitialStateOfChargePercent,
        double? MaximumCapacity,
        double? ChargingEfficiency,
        double? DischargingEfficiency);
}
