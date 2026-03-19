namespace DissertationProsumerAuctions.Services
{
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
    }
}
