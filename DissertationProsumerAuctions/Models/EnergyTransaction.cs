using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions.Models
{
    internal class EnergyTransaction
    {
        public double Quantity{  get; set; }
        public double Price {  get; set; }
        public string Seller { get; set; }
        public string Buyer { get; set; }

        public EnergyTransaction(string seller, string buyer, double quantity, double price)
        {
            Quantity = quantity;
            Seller = seller;
            Buyer = buyer;
            Price = price;
        }

        public override string ToString()
        {
            return $"{Quantity} {Price}";
        }
    }
}
