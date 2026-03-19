using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ProsumerAuctionPlatform.Models
{
    /// <summary>
    /// Data model for prosumer generation information.
    /// Uses composite primary key of ProsumerId and Time.
    /// </summary>
    public class ProsumerGeneratorDataModel
    {
        /// <summary>
        /// Gets or sets the prosumer identifier.
        /// </summary>
        [PrimaryKey]
        public int ProsumerId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp for the generation data.
        /// </summary>
        [PrimaryKey]
        public string Time { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the generation rate at the given time.
        /// </summary>
        public double GenerationRate { get; set; }

        public override string ToString()
        {
            return $"ProsumerId: {ProsumerId}, Time: {Time}, GenerationRate: {GenerationRate}";
        }
    }
}
