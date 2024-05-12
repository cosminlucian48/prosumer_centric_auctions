using ActressMas;
using DissertationProsumerAuctions.DatabaseConnections;
using DissertationProsumerAuctions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DissertationProsumerAuctions.Agents.EnergyMarket
{
    internal class EnergyMarketAgent : Agent
    {
        private System.Timers.Timer _timer;
        public double currentEnergPrice = 0.0;
        public int energyMarketPriceAnnouncementInterval = Utils.EnergyPriceNumberOfDelays * Utils.Delay;
        private Dictionary<string, List<double>> LocalEnergyDifference;
        private List<String> prosumers = new List<String>();
        public DateTime lastTimestamp;

        public EnergyMarketAgent() : base()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = energyMarketPriceAnnouncementInterval;

            lastTimestamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        private async void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            updateProsumerGridEnergyPrice();
            return;
        }

        private async void updateProsumerGridEnergyPrice(string prosumer="")
        {
            List<EnergyMarketPriceDataModel> results = await DatabaseConnection.Instance.GetEnergyMarketPricesbyTime(lastTimestamp.ToString("hh:mm:ss tt"));
            EnergyMarketPriceDataModel response = results.FirstOrDefault();

            if (response == null) return;
            this.currentEnergPrice = response.Price;

            if (prosumer == "")
            {
                SendToMany(this.prosumers, Utils.Str("energy_market_price", this.currentEnergPrice));
            }
            else
            {
                Send(prosumer, Utils.Str("energy_market_price", this.currentEnergPrice));
            }

            lastTimestamp = lastTimestamp.AddMinutes(15);
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}] Hi - Energy Market Agent started", this.Name);
            Broadcast("find_prosumers");
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
                case "prosumer_start":
                    HandleProsumerStart(message.Sender);
                    break;
                default:
                    break;
            }
        }

        private void HandleProsumerStart(string prosumerName)
        {
            this.prosumers.Add(prosumerName);
            updateProsumerGridEnergyPrice(prosumerName);
            _timer.Start();
        }
    }
}
