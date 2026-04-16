using ActressMas;
using ProsumerAuctionPlatform.Constants;
using ProsumerAuctionPlatform.DatabaseConnections;
using ProsumerAuctionPlatform.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ProsumerAuctionPlatform.Agents.Auctions.DutchAuctioneer
{
    /// <summary>
    /// Agent responsible for managing Dutch auction transactions between energy buyers and sellers.
    /// Uses tick-based time management instead of timers.
    /// </summary>
    internal class DutchAuctioneerAgent : Agent
    {
        public enum AuctionPhase { Stopped, GettingParticipants, Bidding};
        public enum AuctionParticipant { Buyer, Seller};
        
        public double CurrentEnergyPrice { get; private set; } = 0.0;
        private readonly Dictionary<ProsumerEnergyBuyer, double> _bids = new Dictionary<ProsumerEnergyBuyer, double>(); // bidder, price
        private readonly List<ProsumerEnergyBuyer> _buyers = new List<ProsumerEnergyBuyer>();
        private readonly List<ProsumerEnergySeller> _sellers = new List<ProsumerEnergySeller>();
        private readonly List<ProsumerEnergySeller> _resolvedSellers = new List<ProsumerEnergySeller>();
        private int _auctionStepsCounter = 0;
        public DateTime LastTimestamp { get; private set; }
        public AuctionPhase CurrentPhase { get; private set; } = AuctionPhase.Stopped;
        private readonly List<EnergyTransaction> _energyTransactions = new List<EnergyTransaction>();

        private double _sellingPrice;
        private double _decrement;
        private double _decrementPercent = 0.01;

        // Tick-based timing
        // - Participant sign-up uses configured tick count (e.g., 3 ticks = 3 minutes)
        // - Bidding iteration uses fixed 2 ticks between price updates
        private int _participantSignUpTickCount = 0;
        private int _biddingTickCount = 0;
        private readonly int _participantSignUpTicks;
        private readonly int _biddingIntervalTicks = 2; // 2 ticks = 2 minutes between bidding iterations

        public DutchAuctioneerAgent(int participantSignUpTicks) : base()
        {
            // Convert participant sign-up interval to ticks
            // Use EnergyMarketParticipantsSignUpInterval directly as ticks (e.g., 3 = 3 minutes)
            _participantSignUpTicks = participantSignUpTicks;

            LastTimestamp = new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day, 0, 0, 0);
        }

        public override void Setup()
        {
            MasLog.Event(this, "message", "Hi - Dutch Auctioneer Agent started");
        }

        public override void Act(Message message)
        {
            try
            {
                MasLog.Received(this, message, $"[{message.Sender} -> {Name}]: {message.Content}");

                if (!Utils.TryParseMessage(message.Content, out string action, out string parameters))
                {
                    MasLog.Event(this, "error", $"Failed to parse message: {message.Content}");
                    return;
                }

                switch (action)
                {
                    case MessageTypes.Started:
                        break;
                    case MessageTypes.DeficitToBuy:
                        HandleNewParticipant(AuctionParticipant.Buyer, message.Sender, parameters);
                        break;
                    case MessageTypes.ExcessToSell:
                        HandleNewParticipant(AuctionParticipant.Seller, message.Sender, parameters);
                        break;
                    case MessageTypes.EnergyBid:
                        HandleEnergyBid(message.Sender, parameters);
                        break;
                    case MessageTypes.Tick:
                        HandleTick(parameters);
                        break;
                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error processing message in DutchAuctioneerAgent {AgentName}: {Message}", Name, message?.Content);
            }
        }

        private void HandleNewParticipant(AuctionParticipant participantType, string sender, string parameters)
        {
            if (CurrentPhase == AuctionPhase.Stopped)
            {
                CurrentPhase = AuctionPhase.GettingParticipants;
                _participantSignUpTickCount = 0; // Reset counter when starting participant collection
                MasLog.InfoDebug(this, "message", $"[{this.Name}] State switched from Stopped to GettingParticipants. Sellers and buyers can signup.");
            }
            
            if (CurrentPhase == AuctionPhase.GettingParticipants)
            {
                switch (participantType)
                {
                    case AuctionParticipant.Buyer:
                        if (double.TryParse(parameters, out double energyToBuy))
                        {
                            _buyers.Add(new ProsumerEnergyBuyer(sender, energyToBuy));
                            MasLog.InfoDebug(this, "message", $"[{this.Name}] got new buyer {sender}");
                        }
                        else
                        {
                            Serilog.Log.Warning("Invalid energy to buy value: {Value}", parameters);
                        }
                        break;
                    case AuctionParticipant.Seller:
                        //should create helper function
                        string[] auxValues = parameters.Split(" ");
                        if (auxValues.Length >= 3 && 
                            double.TryParse(auxValues[0], out double t1) &&
                            double.TryParse(auxValues[1], out double t2) &&
                            double.TryParse(auxValues[2], out double t3))
                        {
                            _sellers.Add(new ProsumerEnergySeller(sender, t1, t2, t3));
                            MasLog.InfoDebug(this, "message", $"[{this.Name}] got new seller {sender}={parameters}");
                        }
                        else
                        {
                            Serilog.Log.Warning("Invalid seller parameters: {Parameters}", parameters);
                        }
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MasLog.InfoDebug(this, "message", $"[{this.Name}] Auction already started!");
            }
        }

        private void startAuction()
        {
            _sellingPrice = _sellers.Max(ps => ps.StartingPrice); // start from maximum StartingPrice
            _decrement = _decrementPercent * _sellingPrice; // initially decrement is 1%
            List<string> buyersNames = _buyers.Select(prosumer => prosumer.ProsumerName).ToList();
            SendToMany(buyersNames, $"{MessageTypes.SellingPrice} {_sellingPrice}");
            MasLog.InfoDebug(this, "debug", $"{string.Join(" ", _buyers.Select(buyer => buyer.ProsumerName))}");
            _auctionStepsCounter += 1;
            _biddingTickCount = 0; // Reset bidding tick counter when starting auction
        }

        private void HandleTick(string parameters)
        {
            if (!Utils.TryParseTickMessage(parameters, out int tickIndex, out DateTime simulationTime))
            {
                Serilog.Log.Warning("Failed to parse tick message in DutchAuctioneerAgent: {Parameters}", parameters);
                return;
            }

            if (CurrentPhase == AuctionPhase.GettingParticipants)
            {
                _participantSignUpTickCount++;
                if (_participantSignUpTickCount >= _participantSignUpTicks)
                {
                    // Check if auction can start
                    bool conditionToStart = _sellers.Count > 1 && _buyers.Count > 1;
                    this.CurrentPhase = conditionToStart ? AuctionPhase.Bidding : AuctionPhase.Stopped;
                    MasLog.InfoDebug(this, "debug", conditionToStart ? $"\n[{this.Name}] State = Bidding. Bidding will start now." : $"\n[{this.Name}] State = Stopped. Not enough sellers and buyers signed up.");
                    MasLog.InfoDebug(this, "debug", $"[{this.Name}] sellers={_sellers.Count} or buyers={_buyers.Count}");

                    if (conditionToStart)
                    {
                        startAuction();
                    }
                    else
                    {
                        // Reset counter if not starting, will continue collecting participants
                        _participantSignUpTickCount = 0;
                    }
                }
            }
            else if (CurrentPhase == AuctionPhase.Bidding)
            {
                _biddingTickCount++;
                if (_biddingTickCount >= _biddingIntervalTicks)
                {
                    HandleReiterateBidding();
                    _biddingTickCount = 0; // Reset counter for next iteration
                }
            }
        }


        private void HandleReiterateBidding()
        {
            if (_auctionStepsCounter % 2 == 0)
            {
                _decrementPercent = _decrementPercent * 2;
                _decrement = _decrementPercent * _sellingPrice;
            }

            if (_bids.Count == 0)
            {
                _sellingPrice = _sellingPrice - _decrement; // start from maximum StartingPrice
                List<string> buyersNames = _buyers.Select(prosumer => prosumer.ProsumerName).ToList();
                SendToMany(buyersNames, $"{MessageTypes.SellingPrice} {_sellingPrice}");
                // Bidding will continue on next tick (handled by HandleTick)
            }
            else
            {
                // Convert collections to lists for processing
                var sellersList = _sellers.ToList();
                var bidsList = _bids.ToList();
                    
                    double totalEnergyToSell = sellersList.Select(seller => seller.EnergyToSell).Sum();
                    double currentIterationEnergyToBuy = bidsList.Select(kvp => kvp.Key.EnergyToBuy).Sum();
                    double bidValue = bidsList.FirstOrDefault().Value;//TODO
                    MasLog.InfoDebug(this, "debug", $"Energy to sell {totalEnergyToSell}; energy to buy = {currentIterationEnergyToBuy}; Current Bids = {string.Join(" ", bidsList.Select(kvp => kvp.Key.EnergyToBuy))}");

                    //if total bids dont cover total energy for sale
                    if(currentIterationEnergyToBuy < totalEnergyToSell)
                    {
                        sellersList.Sort((x,y) => x.FloorPrice.CompareTo(y.FloorPrice));
                        double lowestFloorPrice = sellersList.First().FloorPrice;
                        while(currentIterationEnergyToBuy > 0)
                        {
                            //WIP
                            List<ProsumerEnergySeller> lowestFloorPriceSellers = sellersList.Where(seller => seller.FloorPrice == lowestFloorPrice).ToList();
                            ProsumerEnergySeller seller = lowestFloorPriceSellers[Utils.RandNoGen.Next(lowestFloorPriceSellers.Count)];
                            EnergyTransaction newEnergyTransaction;
                            if(currentIterationEnergyToBuy - seller.EnergyToSell >= 0)
                            {
                                currentIterationEnergyToBuy = currentIterationEnergyToBuy - seller.EnergyToSell;
                                MasLog.InfoDebug(this, "debug", $"Deleted one seller : {seller.ProsumerName}");
                                
                                // Remove seller from list (update the local list for processing)
                                sellersList.Remove(seller);
                                
                                //_resolvedSellers.Add(seller);
                                newEnergyTransaction = new EnergyTransaction(seller.ProsumerName, "", seller.EnergyToSell, bidValue);
                            }
                            else
                            {
                                seller.EnergyToSell -= currentIterationEnergyToBuy;

                                newEnergyTransaction = new EnergyTransaction(seller.ProsumerName, "", currentIterationEnergyToBuy, bidValue);
                                currentIterationEnergyToBuy = 0;
                            }

                            _energyTransactions.Add(newEnergyTransaction);
                            MasLog.InfoDebug(this, "debug", $"new transaction: {newEnergyTransaction.Seller} Quantity:{newEnergyTransaction.Quantity} Price:{newEnergyTransaction.Price}");
                        }
                        _bids.Clear();
                        MasLog.InfoDebug(this, "debug", $"1.Bids cleanup {_bids.Count}");
                    }
                    else
                    {
                        foreach(ProsumerEnergySeller seller in sellersList)
                        {
                            EnergyTransaction newEnergyTransaction = new EnergyTransaction(seller.ProsumerName, "", seller.EnergyToSell, bidValue);
                        }

                        endAuction();
                    }
                    MasLog.InfoDebug(this, "debug", $"New total energy to sell {sellersList.Select(seller => seller.EnergyToSell).Sum()}/{totalEnergyToSell}");


                }
                _auctionStepsCounter += 1;
        }

        private void HandleEnergyBid(string sender, string bidValue)
        {
            if (!double.TryParse(bidValue, out double bid))
            {
                Serilog.Log.Warning("Invalid bid value: {Value}", bidValue);
                return;
            }

            ProsumerEnergyBuyer buyer = _buyers.FirstOrDefault(prosumer => prosumer.ProsumerName == sender);
            if (buyer != null)
            {
                _bids[buyer] = bid;
            }
            else
            {
                Serilog.Log.Warning("Buyer not found: {Sender}", sender);
            }
        }

        private bool checkIfAuctionCanBeClosed()
        {
            return false;
        }

        private void endAuction()
        {
            MasLog.InfoDebug(this, "debug", $"entry endAuction()");

            performAuctionTransactions();
            
            _sellers.Clear();
            _buyers.Clear();

            this.CurrentPhase = AuctionPhase.Stopped;
            
            MasLog.InfoDebug(this, "debug", $"exit endAuction()");
        }

        private void performAuctionTransactions()
        {
            // TODO: Implement auction transaction execution logic
        }

    }
}
