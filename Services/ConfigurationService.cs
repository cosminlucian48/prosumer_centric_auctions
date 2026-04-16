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

        public double BatteryInitialStateOfChargePercent =>
            _configuration.GetValue<double>("Battery:InitialStateOfChargePercent", 0.5);

        public double BatteryMaximumCapacity =>
            _configuration.GetValue<double>("Battery:MaximumCapacity", 15.0);

        public double BatteryChargingEfficiency =>
            _configuration.GetValue<double>("Battery:ChargingEfficiency", 1.0);

        public double BatteryDischargingEfficiency =>
            _configuration.GetValue<double>("Battery:DischargingEfficiency", 1.0);

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

                ValidateProsumerBatteryOverrides(prosumerName, option.Battery);

                // Transform the binding model (from config) to the domain model (used by the simulation).
                definitions.Add(new ProsumerDefinition(
                    Name: prosumerName,
                    Capabilities: new ProsumerCapabilities(
                        HasBattery: option.Capabilities.HasBattery,
                        HasGenerator: option.Capabilities.HasGenerator,
                        HasLoad: option.Capabilities.HasLoad),
                    HasAuction: option.Capabilities.HasAuction,
                    Battery: option.Battery == null
                        ? null
                        : new ProsumerBatteryOverrides(
                            InitialStateOfChargePercent: option.Battery.InitialStateOfChargePercent,
                            MaximumCapacity: option.Battery.MaximumCapacity,
                            ChargingEfficiency: option.Battery.ChargingEfficiency,
                            DischargingEfficiency: option.Battery.DischargingEfficiency)));
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

            if (BatteryInitialStateOfChargePercent < 0 || BatteryInitialStateOfChargePercent > 1)
            {
                throw new InvalidOperationException(
                    "Config error: Battery:InitialStateOfChargePercent must be in range [0, 1].");
            }

            if (BatteryMaximumCapacity <= 0)
            {
                throw new InvalidOperationException(
                    "Config error: Battery:MaximumCapacity must be greater than 0.");
            }

            if (BatteryChargingEfficiency <= 0 || BatteryChargingEfficiency > 1)
            {
                throw new InvalidOperationException(
                    "Config error: Battery:ChargingEfficiency must be in range (0, 1].");
            }

            if (BatteryDischargingEfficiency <= 0 || BatteryDischargingEfficiency > 1)
            {
                throw new InvalidOperationException(
                    "Config error: Battery:DischargingEfficiency must be in range (0, 1].");
            }

            _ = GetProsumerDefinitions();
        }

        private static void ValidateProsumerBatteryOverrides(string prosumerName, ProsumerBatteryOptions? battery)
        {
            if (battery == null)
            {
                return;
            }

            if (battery.InitialStateOfChargePercent.HasValue &&
                (battery.InitialStateOfChargePercent.Value < 0 || battery.InitialStateOfChargePercent.Value > 1))
            {
                throw new InvalidOperationException(
                    $"Config error: Prosumers[{prosumerName}].Battery.InitialStateOfChargePercent must be in range [0, 1].");
            }

            if (battery.MaximumCapacity.HasValue && battery.MaximumCapacity.Value <= 0)
            {
                throw new InvalidOperationException(
                    $"Config error: Prosumers[{prosumerName}].Battery.MaximumCapacity must be greater than 0.");
            }

            if (battery.ChargingEfficiency.HasValue &&
                (battery.ChargingEfficiency.Value <= 0 || battery.ChargingEfficiency.Value > 1))
            {
                throw new InvalidOperationException(
                    $"Config error: Prosumers[{prosumerName}].Battery.ChargingEfficiency must be in range (0, 1].");
            }

            if (battery.DischargingEfficiency.HasValue &&
                (battery.DischargingEfficiency.Value <= 0 || battery.DischargingEfficiency.Value > 1))
            {
                throw new InvalidOperationException(
                    $"Config error: Prosumers[{prosumerName}].Battery.DischargingEfficiency must be in range (0, 1].");
            }
        }
    }
}
