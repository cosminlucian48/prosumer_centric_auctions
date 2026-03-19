using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.Models
{
    /// <summary>
    /// Represents an energy transaction between a seller and buyer.
    /// </summary>
    internal class EnergyTransaction
    {
        /// <summary>
        /// Gets or sets the quantity of energy in the transaction.
        /// </summary>
        public double Quantity { get; set; }

        /// <summary>
        /// Gets or sets the price per unit of energy.
        /// </summary>
        public double Price { get; set; }

        /// <summary>
        /// Gets or sets the name of the seller agent.
        /// </summary>
        public string Seller { get; set; } = string.Empty;

        /// <summary>
        /// Gets or sets the name of the buyer agent.
        /// </summary>
        public string Buyer { get; set; } = string.Empty;

        public EnergyTransaction(string seller, string buyer, double quantity, double price)
        {
            Seller = seller ?? string.Empty;
            Buyer = buyer ?? string.Empty;
            Quantity = quantity;
            Price = price;
        }

        public override string ToString()
        {
            return $"{Quantity} {Price}";
        }
    }
}
