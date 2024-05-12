using ActressMas;
using DissertationProsumerAuctions.DatabaseConnections;
using DissertationProsumerAuctions.Models;
using Microsoft.VisualBasic.FileIO;
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
        public enum AuctionPhase { Stopped, GettingParticipants, Bidding};
        public enum AuctionParticipant { Buyer, Seller};
        private System.Timers.Timer _timer, _biddingTimer;
        public double currentEnergPrice = 0.0;
        public int energyMarketPriceAnnouncementInterval = 2 * Utils.Delay;
        public int energyMarketParticipantsSignUpInterval = Utils.EnergyMarketParticipantsSignUpInterval * Utils.Delay;
        private Dictionary<ProsumerEnergyBuyer, double> _bids = new Dictionary<ProsumerEnergyBuyer, double>(); // bidder, price
        private List<ProsumerEnergyBuyer> buyers = new List<ProsumerEnergyBuyer>();
        private List<ProsumerEnergySeller> sellers = new List<ProsumerEnergySeller>();
        private List<ProsumerEnergySeller> _resolvedSellers = new List<ProsumerEnergySeller>();
        private int _auctionStepsCounter = 0;
        public DateTime lastTimestamp;
        public AuctionPhase auctionPhase = AuctionPhase.Stopped;
        private List<EnergyTransaction> energyTransactions = new List<EnergyTransaction>();

        private double _sellingPrice;
        private double _decrement;
        private double _decrementPercent = 0.01;

        public DutchAuctioneerAgent() : base()
        {
            _timer = new System.Timers.Timer();
            _timer.Elapsed += t_Elapsed;
            _timer.Interval = energyMarketParticipantsSignUpInterval;

            _biddingTimer = new System.Timers.Timer();
            _biddingTimer.Elapsed += t_ElapsedBidding;
            _biddingTimer.Interval = 2 * Utils.Delay;

            lastTimestamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        private void t_ElapsedBidding(object sender, ElapsedEventArgs e)
        {
            HandleReiterateBidding();
        }

        private async void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            _timer.Stop();
            
            bool conditionToStart = sellers.Count > 1 && buyers.Count > 1;
            this.auctionPhase= conditionToStart ? AuctionPhase.Bidding: AuctionPhase.Stopped;

            Console.WriteLine(conditionToStart ? $"\n[{this.Name}] State = Bidding. Bidding will start now." : $"\n[{this.Name}] State = Stopped. Not enough sellers and buyers signed up.");
            Console.WriteLine($"[{this.Name}] sellers={sellers.Count} or buyers={buyers.Count}");

            if (conditionToStart) startAuction();
            return;
        }

        public override void Setup()
        {
            Console.WriteLine("[{0}] Hi - Dutch Auctioneer Agent started", this.Name);
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
                    HandleNewParticipant(AuctionParticipant.Buyer, message.Sender, parameters);
                    break;
                case "excess_to_sell":
                    HandleNewParticipant(AuctionParticipant.Seller, message.Sender, parameters);
                    break;
                case "energy_bid":
                    HandleEnergyBid(message.Sender, parameters);
                    break;
                default:
                    break;
            }
        }

        private void HandleNewParticipant(AuctionParticipant participantType, string sender, string parameters)
        {
            if (auctionPhase == AuctionPhase.Stopped)
            {
                auctionPhase = AuctionPhase.GettingParticipants;
                Console.WriteLine($"\n[{this.Name}] State = GettingParticipants. Sellers and buyers can signup.");
                _timer.Start();
            }
            
            if (auctionPhase == AuctionPhase.GettingParticipants)
            {
                switch (participantType)
                {
                    case AuctionParticipant.Buyer:
                        buyers.Add(new ProsumerEnergyBuyer(sender, Double.Parse(parameters)));
                        Console.WriteLine($"[{this.Name}] got new buyer {sender}");
                        break;
                    case AuctionParticipant.Seller:
                        //should create helper function
                        string[] auxValues = parameters.Split(" ");
                        double t1 = Double.Parse(auxValues[0]);
                        double t2 = Double.Parse(auxValues[1]);
                        double t3 = Double.Parse(auxValues[2]);
                        sellers.Add(new ProsumerEnergySeller(sender, t1, t2, t3));
                        Console.WriteLine($"[{this.Name}] got new seller {sender}={parameters}");
                        break;
                    default:
                        break;
                }
            }
            else
            {
                Console.WriteLine($"[{this.Name}] Auction already started!");
            }
        }

        private void startAuction()
        {
            _sellingPrice = sellers.Max(ps => ps.StartingPrice); // start from maximum StartingPrice
            _decrement = _decrementPercent * _sellingPrice; // initially decrement is 1%
            List<string> buyersNames = buyers.Select(prosumer => prosumer.ProsumerName).ToList();
            SendToMany(buyersNames, Utils.Str("selling_price", $"{_sellingPrice}"));
            //Console.WriteLine($"{string.Join(" ", buyers.Select(seller => seller.ProsumerName))}");
            _auctionStepsCounter += 1;
            _biddingTimer.Start();
        }


        private void HandleReiterateBidding()
        {
            if (_auctionStepsCounter % 2 == 0)
            {
                _decrementPercent = _decrementPercent * 2;
                _decrement = _decrementPercent * _sellingPrice;
            }

            _biddingTimer.Stop();
            if (_bids.Count == 0)
            {
                _sellingPrice = _sellingPrice - _decrement; // start from maximum StartingPrice
                List<string> buyersNames = buyers.Select(prosumer => prosumer.ProsumerName).ToList();
                SendToMany(buyersNames, Utils.Str("selling_price", $"{_sellingPrice}"));
                _biddingTimer.Start();
            }
            else
            {
                double totalEnergyToSell = sellers.Select(seller => seller.EnergyToSell).ToList().Sum();
                double currentIterationEnergyToBuy = _bids.Keys.Select(buyer => buyer.EnergyToBuy).ToList().Sum();
                double bidValue = _bids.FirstOrDefault().Value;//TODO
                Console.WriteLine($"Energy to sell {totalEnergyToSell}; energy to buy = {currentIterationEnergyToBuy}; Current Bids = {string.Join(" ", _bids.Keys.Select(buyer => buyer.EnergyToBuy).ToList())}");

                //if total bids dont cover total energy for sale
                if(currentIterationEnergyToBuy < totalEnergyToSell)
                {
                    sellers.Sort((x,y) => x.FloorPrice.CompareTo(y.FloorPrice));
                    double lowestFloorPrice = sellers.First().FloorPrice;
                    while(currentIterationEnergyToBuy > 0)
                    {
                        //WIP
                        List<ProsumerEnergySeller> lowestFloorPriceSellers = sellers.Where(seller => seller.FloorPrice == lowestFloorPrice).ToList();
                        ProsumerEnergySeller seller = lowestFloorPriceSellers[Utils.RandNoGen.Next(lowestFloorPriceSellers.Count)];
                        EnergyTransaction newEnergyTransaction;
                        if(currentIterationEnergyToBuy - seller.EnergyToSell >= 0)
                        {
                            currentIterationEnergyToBuy = currentIterationEnergyToBuy - seller.EnergyToSell;
                            Console.WriteLine($"Deleted one seller");
                            sellers.RemoveAll(s => s.ProsumerName == seller.ProsumerName); //to be tested
                            //_resolvedSellers.Add(seller);
                            newEnergyTransaction = new EnergyTransaction(seller.ProsumerName, "", seller.EnergyToSell, bidValue);
                        }
                        else
                        {
                            seller.EnergyToSell -= currentIterationEnergyToBuy;

                            newEnergyTransaction = new EnergyTransaction(seller.ProsumerName, "", currentIterationEnergyToBuy, bidValue);
                            currentIterationEnergyToBuy = 0;
                        }

                        energyTransactions.Add(newEnergyTransaction);
                        Console.WriteLine($"new transaction: {newEnergyTransaction.Seller} Quantity:{newEnergyTransaction.Quantity} Price:{newEnergyTransaction.Price}");
                    }
                    _bids.Clear();
                    Console.WriteLine($"1.Bids cleanup {_bids.Count}");
                }
                else
                {
                    foreach(ProsumerEnergySeller seller in sellers)
                    {
                        EnergyTransaction newEnergyTransaction = new EnergyTransaction(seller.ProsumerName, "", seller.EnergyToSell, bidValue);
                    }

                    endAuction();
                }

                Console.WriteLine($"New total energy to sell {sellers.Select(seller => seller.EnergyToSell).ToList().Sum()}/{totalEnergyToSell}");


            }
            _auctionStepsCounter += 1;
        }

        private void HandleEnergyBid(string sender, string bidValue)
        {
            ProsumerEnergyBuyer buyer = buyers.FirstOrDefault(prosumer => prosumer.ProsumerName == sender);
            _bids.Add(buyer, Double.Parse(bidValue));
        }

        private bool checkIfAuctionCanBeClosed()
        {
            return false;
        }

        private void endAuction()
        {
            Console.WriteLine("Ending auction");

            performAuctionTransactions();
            
            sellers.Clear();
            buyers.Clear();

            this.auctionPhase = AuctionPhase.Stopped;

            Console.WriteLine("Ending auction");
        }

        private void performAuctionTransactions()
        {

        }

    }
}
