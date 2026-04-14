using Microsoft.Extensions.Configuration;
using System;

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

        public string SeqUrl =>
            _configuration.GetValue<string>("Seq:Url", "http://localhost:5341")!;

        public string DbPath =>
            _configuration.GetValue<string>("Database:Path", "./myDatabase.db")!;

        public void Validate()
        {
            if (!Uri.TryCreate(SeqUrl, UriKind.Absolute, out _))
            {
                throw new InvalidOperationException(
                    "Config error: Seq:Url must be a valid absolute URL.");
            }

            if (string.IsNullOrWhiteSpace(DbPath))
            {
                throw new InvalidOperationException(
                    "Config error: Database:Path is required.");
            }
        }
    }
}
