namespace ProsumerAuctionPlatform.Services
{
    using System.Collections.Generic;
    using ProsumerAuctionPlatform.Models;

    /// <summary>
    /// Interface for configuration service providing type-safe access to configuration values.
    /// </summary>
    public interface IConfigurationService
    {
        /// <summary>
        /// Gets the number of delays for energy load rate updates.
        /// </summary>
        int EnergyLoadRateNumberOfDelays { get; }

        /// <summary>
        /// Gets the number of delays for energy generation rate updates.
        /// </summary>
        int EnergyGenerationRateNumberOfDelays { get; }

        /// <summary>
        /// Gets the number of delays for energy price updates.
        /// </summary>
        int EnergyPriceNumberOfDelays { get; }

        /// <summary>
        /// Gets the number of prosumers in the simulation.
        /// </summary>
        int NumberOfProsumers { get; }

        /// <summary>
        /// Gets the interval for energy market participants sign-up.
        /// </summary>
        int EnergyMarketParticipantsSignUpInterval { get; }

        /// <summary>
        /// Gets the delay value in milliseconds.
        /// </summary>
        int Delay { get; }

        /// <summary>
        /// Gets the Seq endpoint URL for structured logs.
        /// </summary>
        string SeqUrl { get; }

        /// <summary>
        /// Gets the SQLite database path.
        /// </summary>
        string DbPath { get; }

        /// <summary>
        /// Gets startup prosumer definitions with per-prosumer capabilities.
        /// </summary>
        IReadOnlyList<ProsumerDefinition> GetProsumerDefinitions();

        /// <summary>
        /// Gets the default prosumer capability profile.
        /// Kept for compatibility; this value is derived from the first configured prosumer.
        /// </summary>
        ProsumerCapabilities GetProsumerCapabilities();

        /// <summary>
        /// Validates required configuration and throws clear startup errors.
        /// </summary>
        void Validate();
    }
}
