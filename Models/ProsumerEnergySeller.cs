using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions
{
    internal class ProsumerEnergySeller
    {
        public double EnergyToSell { get; set; }   
        public double FloorPrice{ get; set; }   
        public double StartingPrice { get; set; }   
        public string ProsumerName { get; set; }   
        public ProsumerEnergySeller(string prosumerName, double energyToSell, double floorPrice, double startingPrice) 
        {
            ProsumerName = prosumerName;
            EnergyToSell = energyToSell;
            FloorPrice = floorPrice;
            StartingPrice = startingPrice;
        }

        public override string ToString()
        {
            return $"{ProsumerName} EnergyToSell = {EnergyToSell} FloorPrice = {FloorPrice} StartingPrice = {StartingPrice}\n";
        }
    }
}
