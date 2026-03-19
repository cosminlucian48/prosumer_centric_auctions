# ProsumerAgent

## Overview

The `ProsumerAgent` represents a prosumer (producer-consumer) entity that both produces and consumes energy. It coordinates with its component agents (Battery, Generator, Load) to manage energy balance and participates in energy trading.

## Responsibilities

- Monitors production (based on weather/generation data) and consumption
- Stores excess energy in battery
- Sells/buys energy via negotiation or market mechanisms
- Coordinates with component agents (Battery, Generator, Load)
- Manages energy balance and trading decisions

## State

### Properties
- `ProsumerId` (int) - Unique identifier for the prosumer
- `CurrentEnergyPrice` (double) - Current energy price from market

### Internal State
- `_currentLoadEnergyTotal` (double) - Total current load energy
- `_currentGeneratedEnergyTotal` (double) - Total current generated energy
- `_currentBatteryStorageCapacity` (double) - Current battery capacity
- `_energyInTransit` (double) - Energy currently in transit (being stored/sold)
- `_currentGridBuyPrice` (double) - Current grid buy price
- `_currentGridSellPrice` (double) - Current grid sell price
- `_currentBill` (double) - Current energy bill
- `_auctionEnergyPriceVariationFromGrid` (double) - Price variation factor for auctions
- `_isAuctioning` (bool) - Whether currently participating in auction
- `_prosumerSetupReadiness` (Dictionary<string, bool>) - Tracks component readiness

## Lifecycle

### Setup()
- Parses prosumer ID from agent name
- Initializes component readiness tracking
- Sets up auction price variation factor
- Logs agent startup

### Act(Message message)
Processes incoming messages:
- `Tick` - Time updates (no action)
- `ComponentReady` - Component readiness notifications
- `FindProsumers` - Component setup coordination
- `BatterySOC` - Battery state of charge updates
- `LoadUpdate` - Load energy updates
- `GenerationUpdate` - Generation energy updates
- `EnergyStored` - Energy storage confirmations
- `BatteryMaximumCapacity` - Battery capacity limit reached
- `SellEnergyConfirmation` - Energy sale confirmations
- `BuyEnergyConfirmation` - Energy purchase confirmations
- `EnergyMarketPrice` - Grid energy price updates
- `StartAuctioning` - Trigger to start auction participation
- `SellingPrice` - Auction price updates

## Behaviors

### Energy Balance Management
- Tracks load and generation separately
- Calculates energy deficit/excess
- Triggers auction participation when imbalance detected

### Auction Participation
- Registers as buyer when energy deficit
- Registers as seller when energy excess
- Evaluates auction prices against grid prices
- Places bids when price is favorable

### Component Coordination
- Waits for all components (Battery, Generator, Load, EnergyMarket) to be ready
- Sends `ProsumerStart` message when all ready
- Coordinates energy flow between components

## Message Handling

### Incoming Messages
- Receives updates from component agents
- Receives price updates from EnergyMarketAgent
- Receives auction updates from DutchAuctioneerAgent

### Outgoing Messages
- Sends to component agents for coordination
- Sends to DutchAuctioneerAgent for auction participation
- Sends to VPP agent (when implemented) for excess energy

## Dependencies

- `ProsumerBatteryAgent` - Battery management
- `ProsumerGeneratorAgent` - Generation monitoring
- `ProsumerLoadAgent` - Load monitoring
- `EnergyMarketAgent` - Grid price information
- `DutchAuctioneerAgent` - Auction participation

## Implementation Status

✅ **Implemented** - Core functionality complete
