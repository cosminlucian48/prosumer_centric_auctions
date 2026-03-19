using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.Models
{
    /// <summary>
    /// Data model for energy market price information.
    /// </summary>
    public class EnergyMarketPriceDataModel
    {
        /// <summary>
        /// Gets or sets the timestamp for the price data.
        /// </summary>
        [PrimaryKey]
        public string Time { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the energy price at the given time.
        /// </summary>
        public double Price { get; set; }

        public override string ToString()
        {
            return $"EnergyMarketPrice Time: {Time}, Price: {Price}";
        }
    }
}
