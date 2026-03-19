using ProsumerAuctionPlatform.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProsumerAuctionPlatform.DatabaseConnections
{
    /// <summary>
    /// Interface for database connection operations.
    /// This interface enables dependency injection and improves testability.
    /// </summary>
    public interface IDatabaseConnection
    {
        /// <summary>
        /// Gets the underlying SQLite database connection.
        /// </summary>
        SQLite.SQLiteAsyncConnection Database { get; }

        /// <summary>
        /// Retrieves prosumer load data by ID and timestamp.
        /// </summary>
        /// <param name="id">The prosumer ID.</param>
        /// <param name="timestamp">The timestamp string.</param>
        /// <returns>List of prosumer load data models.</returns>
        Task<List<ProsumerLoadDataModel>> GetProsumerLoadByIdAsync(int id, string timestamp);

        /// <summary>
        /// Retrieves prosumer generation data by ID and timestamp.
        /// </summary>
        /// <param name="id">The prosumer ID.</param>
        /// <param name="timestamp">The timestamp string.</param>
        /// <returns>List of prosumer generation data models.</returns>
        Task<List<ProsumerGeneratorDataModel>> GetProsumerGenerationByIdAsync(int id, string timestamp);

        /// <summary>
        /// Retrieves energy market price data by timestamp.
        /// </summary>
        /// <param name="timestamp">The timestamp string.</param>
        /// <returns>List of energy market price data models.</returns>
        Task<List<EnergyMarketPriceDataModel>> GetEnergyMarketPricesbyTime(string timestamp);
    }
}
