using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions
{
    public class EnergyMarketPriceDataModel
    {
        [PrimaryKey, AutoIncrement]
        public string Time { get; set; }
        public double Price { get; set; }
        public override string ToString()
        {
            return $"EnergyMarketPrice Time: {Time}, Price: {Price}";
        }
    }
}
