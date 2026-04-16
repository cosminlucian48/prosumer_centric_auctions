using Microsoft.Extensions.Configuration;
using ProsumerAuctionPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProsumerAuctionPlatform.Services
{
    /// <summary>
    /// Implementation of configuration service using Microsoft.Extensions.Configuration.
    /// </summary>
    public class ConfigurationService : IConfigurationService
    {
        private readonly IConfiguration _configuration;
        private IReadOnlyList<ProsumerDefinition>? _prosumerDefinitions;

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

        public int NumberOfProsumers => GetProsumerDefinitions().Count;

        public int EnergyMarketParticipantsSignUpInterval => 
            _configuration.GetValue<int>("EnergyMarketParticipantsSignUpInterval", 3);

        public int Delay => 
            _configuration.GetValue<int>("Delay", 1500);

        public string SeqUrl =>
            _configuration.GetValue<string>("Seq:Url", "http://localhost:5341")!;

        public string DbPath =>
            _configuration.GetValue<string>("Database:Path", "./myDatabase.db")!;

        /// <summary>
        /// Gets the list of prosumer definitions from configuration, applying lazy loading and caching.
        /// </summary>
        public IReadOnlyList<ProsumerDefinition> GetProsumerDefinitions()
        {
            // CACHE CHECK: Return cached result if already loaded and validated.
            if (_prosumerDefinitions != null)
            {
                return _prosumerDefinitions;
            }

            // LAZY LOAD: Parse the "Prosumers" section from configuration into binding models.
            var options = new List<ProsumerDefinitionOptions>();
            _configuration.GetSection("Prosumers").Bind(options);

            if (options.Count == 0)
            {
                throw new InvalidOperationException(
                    "Config error: at least one prosumer must be defined in Prosumers.");
            }

            // Track used names to detect and reject duplicates.
            var usedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var definitions = new List<ProsumerDefinition>(options.Count);

            // Validate and transform each prosumer entry from the binding model to the domain model.
            foreach (var option in options)
            {
                if (string.IsNullOrWhiteSpace(option.Name))
                {
                    throw new InvalidOperationException(
                        "Config error: each prosumer entry requires a non-empty Name.");
                }

                var prosumerName = option.Name.Trim();

                if (!usedNames.Add(prosumerName))
                {
                    throw new InvalidOperationException(
                        $"Config error: duplicate prosumer name '{prosumerName}' in Prosumers.");
                }

                // HasLoad is mandatory; all prosumers must have a load component.
                if (!option.Capabilities.HasLoad)
                {
                    throw new InvalidOperationException(
                        $"Config error: Prosumers[{prosumerName}].Capabilities.HasLoad must be true because each prosumer requires a load component.");
                }

                // Transform the binding model (from config) to the domain model (used by the simulation).
                definitions.Add(new ProsumerDefinition(
                    Name: prosumerName,
                    Capabilities: new ProsumerCapabilities(
                        HasBattery: option.Capabilities.HasBattery,
                        HasGenerator: option.Capabilities.HasGenerator,
                        HasLoad: option.Capabilities.HasLoad),
                    HasAuction: option.Capabilities.HasAuction));
            }

            // CACHE STORE: Save the validated list so future calls return it immediately.
            _prosumerDefinitions = definitions;
            return _prosumerDefinitions;
        }

        public ProsumerCapabilities GetProsumerCapabilities()
        {
            var firstProsumer = GetProsumerDefinitions().First();

            if (!firstProsumer.Capabilities.HasLoad)
            {
                throw new InvalidOperationException(
                    "Config error: Prosumers[0].Capabilities.HasLoad must be true because each prosumer requires a load component.");
            }

            return firstProsumer.Capabilities;
        }

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

            _ = GetProsumerDefinitions();
        }
    }
}
