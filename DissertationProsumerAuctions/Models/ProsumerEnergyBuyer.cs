using DissertationProsumerAuctions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions
{
    internal class ProsumerEnergyBuyer
    {
        public double EnergyToBuy { get; set; }
        public double FloorPrice { get; set; }
        public double StartingPrice { get; set; }
        public string ProsumerName { get; set; }

        public List<EnergyTransaction> bids { get; set; }
        public ProsumerEnergyBuyer(string prosumerName, double energyToBuy = 0, double floorPrice=0, double startingPrice=0)
        {
            ProsumerName = prosumerName;
            EnergyToBuy = energyToBuy;
            FloorPrice = floorPrice;
            StartingPrice = startingPrice;
        }
    }
}
