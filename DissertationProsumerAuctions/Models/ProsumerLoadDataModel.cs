using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.Models
{
    /// <summary>
    /// Data model for prosumer load information.
    /// Uses composite primary key of ProsumerId and Time.
    /// </summary>
    public class ProsumerLoadDataModel
    {
        /// <summary>
        /// Gets or sets the prosumer identifier.
        /// </summary>
        [PrimaryKey]
        public int ProsumerId { get; set; }

        /// <summary>
        /// Gets or sets the timestamp for the load data.
        /// </summary>
        [PrimaryKey]
        public string Time { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the load value at the given time.
        /// </summary>
        public double Load { get; set; }

        public override string ToString()
        {
            return $"ProsumerId: {ProsumerId}, Time: {Time}, Load: {Load}";
        }
    }
}
