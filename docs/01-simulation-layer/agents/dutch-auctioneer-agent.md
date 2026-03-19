# DutchAuctioneerAgent

## Overview

The `DutchAuctioneerAgent` manages Dutch auction transactions between energy buyers and sellers. It implements a descending price auction mechanism where the price starts high and decreases until buyers accept.

## Responsibilities

- Collects bids and offers from prosumers
- Runs Dutch auction algorithm
- Broadcasts clearing prices
- Manages auction phases (Stopped, GettingParticipants, Bidding)
- Matches buyers and sellers
- Executes energy transactions

## State

### Properties
- `CurrentEnergyPrice` (double) - Current energy price
- `LastTimestamp` (DateTime) - Last timestamp processed
- `CurrentPhase` (AuctionPhase) - Current auction phase

### Auction Phases
- `Stopped` - No active auction
- `GettingParticipants` - Collecting buyers and sellers
- `Bidding` - Active bidding phase

### Internal State
- `_bids` (Dictionary<ProsumerEnergyBuyer, double>) - Current bids from buyers
- `_buyers` (List<ProsumerEnergyBuyer>) - Registered buyers
- `_sellers` (List<ProsumerEnergySeller>) - Registered sellers
- `_resolvedSellers` (List<ProsumerEnergySeller>) - Sellers with completed transactions
- `_energyTransactions` (List<EnergyTransaction>) - Completed transactions
- `_sellingPrice` (double) - Current selling price
- `_decrement` (double) - Price decrement amount
- `_decrementPercent` (double) - Price decrement percentage
- `_auctionStepsCounter` (int) - Counter for auction steps
- `_participantSignUpTickCount` (int) - Ticks since participant sign-up started
- `_biddingTickCount` (int) - Ticks since last bidding iteration

## Lifecycle

### Setup()
- Logs agent startup
- Initializes auction state

### Act(Message message)
Processes incoming messages:
- `DeficitToBuy` - Buyer registration with energy amount
- `ExcessToSell` - Seller registration with energy, floor price, starting price
- `EnergyBid` - Bid from buyer at current price
- `Tick` - Time-based auction progression

## Behaviors

### Participant Collection
- Collects buyers and sellers during sign-up phase
- Waits for minimum participants (2 buyers, 2 sellers)
- Transitions to bidding phase when ready

### Dutch Auction Algorithm
- Starts with maximum starting price from sellers
- Decrements price at regular intervals
- Accepts bids when price is acceptable to buyers
- Matches buyers and sellers based on energy amounts

### Price Management
- Calculates initial selling price from seller starting prices
- Decrements price by percentage (initially 1%)
- Doubles decrement percentage every 2 auction steps
- Broadcasts new prices to all buyers

### Transaction Matching
- Matches buyers and sellers when bids received
- Prioritizes sellers by floor price
- Creates energy transactions
- Clears bids after matching

## Message Handling

### Incoming Messages
- `DeficitToBuy` - Buyer wants to buy energy
- `ExcessToSell` - Seller wants to sell energy
- `EnergyBid` - Buyer accepts current price
- `Tick` - Auction timing updates

### Outgoing Messages
- `SellingPrice` - Broadcasts current auction price to buyers

## Dependencies

- `ProsumerAgent` - Buyers and sellers
- `TickAgent` - Time management

## Implementation Status

✅ **Implemented** - Core functionality complete
⚠️ **Partial** - Transaction execution logic needs completion
