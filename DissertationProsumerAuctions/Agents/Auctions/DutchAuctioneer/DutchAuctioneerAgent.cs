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
    internal class DutchAuctioneer : Agent
    {
        private System.Timers.Timer _timer;
        public double currentEnergPrice = 0.0;
        public int energyMarketPriceAnnouncementInterval = Utils.EnergyRateNumberOfDelays * Utils.Delay;
        private Dictionary<string, List<double>> LocalEnergyDifference;
        private List<String> prosumers = new List<String>();
        public DateTime lastTimestamp;

        public DutchAuctioneer() : base()
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
                default:
                    break;
            }
        }

    }
}
