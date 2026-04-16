namespace ProsumerAuctionPlatform.Services
{
    public class ProsumerCapabilitiesOptions
    {
        public bool HasBattery { get; set; } = true;
        public bool HasGenerator { get; set; } = true;
        public bool HasLoad { get; set; } = true;
        public bool HasAuction { get; set; } = false;
    }
}
