using ActressMas;
using DissertationProsumerAuctions.DatabaseConnections;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DissertationProsumerAuctions.Agents.Auctions.DutchAuctioneer
{
    internal class DutchAuctioneerAgent : Agent
    {
        private System.Timers.Timer _timer;
        public double currentEnergPrice = 0.0;
        public int energyMarketPriceAnnouncementInterval = Utils.EnergyRateNumberOfDelays * Utils.Delay;
        private Dictionary<string, List<double>> LocalEnergyDifference;
        private List<String> sellers = new List<String>();
        private Dictionary<string, double> buyers = new Dictionary<string, double>();
        //private Dictionary<string, string> sellers = new Dictionary<string, string>();
        public DateTime lastTimestamp;

        public DutchAuctioneerAgent() : base()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = energyMarketPriceAnnouncementInterval;

            lastTimestamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        private async void t_Elapsed(object sender, ElapsedEventArgs e)
        {

            return;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}] Hi - Dutch Auctioneer Agent started", this.Name);
            _timer.Start();
        }

        public override void Act(Message message)
        {
            Console.WriteLine("\t[{1} -> {0}]: {2}", this.Name, message.Sender, message.Content);

            string action; string parameters;
            Utils.ParseMessage(message.Content, out action, out parameters);

            switch (action)
            {
                case "started":
                    break;
                case "deficit_to_buy":
                    string[] t = parameters.Split(); 
                    buyers.Add(message.Sender, parameters);
                    
                    foreach (KeyValuePair<string, string> kvp in buyers)
                    {
                        Console.WriteLine("buyer Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                    }
                    Console.WriteLine("end");
                    break;
                case "excess_to_sell":
                    sellers.Add(message.Sender, parameters);
                    foreach (KeyValuePair<string, string> kvp in sellers)
                    {
                        Console.WriteLine("seller Key = {0}, Value = {1}", kvp.Key, kvp.Value);
                    }
                    Console.WriteLine("end");
                    break;
                default:
                    break;
            }
        }

    }
}
