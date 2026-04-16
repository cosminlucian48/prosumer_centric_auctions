namespace ProsumerAuctionPlatform.Services
{
    public class ProsumerDefinitionOptions
    {
        public string Name { get; set; } = string.Empty;

        public ProsumerCapabilitiesOptions Capabilities { get; set; } = new();

        public ProsumerBatteryOptions? Battery { get; set; }
    }
}