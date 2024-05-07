using ActressMas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace DissertationProsumerAuctions.Agents.Prosumer
{
    internal class ProsumerBatteryAgent : Agent
    {
        private System.Timers.Timer _timer;
        public int counter = 0;
        public string myProsumerName = "";
        public double currentCapacity = 0.0; // storage value as WH.   1kwh of generated energy = 1 000 wh
        public double maximumCapacity = 15.0;
        public int chargingEfficiency = 1;
        public int dischargingEfficiency = 1;
        public int batterySOCNotificationInterval = 1 * Utils.Delay;
        public int myProsumerId = 0;
        private Dictionary<string, List<double>> LocalEnergyDifference;

        public ProsumerBatteryAgent(string prosumerName) : base()
        {
            myProsumerName = prosumerName;
            myProsumerId = Int32.Parse(prosumerName.Remove(0, 8));

            _timer = new System.Timers.Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = batterySOCNotificationInterval;

            this.currentCapacity = this.maximumCapacity;


            //new NotificationHub().NotifyClients(groupName, "This message is for clients who are part of the 'Womens' group.");


        }

        private void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            //Send(this.myProsumerName, Utils.Str("battery_soc", this.currentCapacity));
            return;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}] Hi - Prosumer battery started", this.Name);
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
                case "store":
                    HandleStoreEnergy(parameters);
                    break;
                default:
                    break;
            }
        }

        private void HandleStoreEnergy(string parameters)
        {
            double energyToStore = Double.Parse(parameters);
            if (this.currentCapacity + energyToStore < this.maximumCapacity)
            {
                this.currentCapacity += energyToStore;
                Send(myProsumerName, Utils.Str("energy_stored", energyToStore));
            }
            else
            {
                double capacityDifference = 0.0;
                if (this.currentCapacity < this.maximumCapacity)
                {
                    capacityDifference = this.maximumCapacity - this.currentCapacity;
                    this.currentCapacity += capacityDifference;
                }
                Send(myProsumerName, Utils.Str("battery_maximum_capacity", capacityDifference));
            }
        }
    }
}
