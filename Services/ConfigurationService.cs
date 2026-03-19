using Microsoft.Extensions.Configuration;
using System.IO;

namespace ProsumerAuctionPlatform.Services
{
    /// <summary>
    /// Implementation of configuration service using Microsoft.Extensions.Configuration.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;

        public ConfigurationService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public int EnergyLoadRateNumberOfDelays => 
            _configuration.GetValue<int>("EnergyLoadRateNumberOfDelays", 45);

        public int EnergyGenerationRateNumberOfDelays => 
            _configuration.GetValue<int>("EnergyGenerationRateNumberOfDelays", 45);

        public int EnergyPriceNumberOfDelays => 
            _configuration.GetValue<int>("EnergyPriceNumberOfDelays", 45);

        public int NumberOfProsumers => 
            _configuration.GetValue<int>("NumberOfProsumers", 5);

        public int EnergyMarketParticipantsSignUpInterval => 
            _configuration.GetValue<int>("EnergyMarketParticipantsSignUpInterval", 3);

        public int Delay => 
            _configuration.GetValue<int>("Delay", 1500);
    }
}
