# Communication Protocols - Agent Interaction Schemas

## Overview

This document defines the communication protocols and interaction schemas between different agent types in the prosumer-centric energy trading system.

## Agent Types

1. **ProsumerAgent** - Main prosumer coordinator
2. **GenerationAgent** - Energy generation component
3. **ConsumptionAgent** - Energy consumption component
4. **BatteryAgent** - Battery storage component
5. **AuctionManager** - Auction coordinator
6. **GridAgent** - Grid interface
7. **TickAgent** - Time management
8. **WeatherAgent** - Weather simulation
9. **MonitorAgent** - Data collection

## Message Protocol Structure

### Message Format

```
Message {
    Sender: string
    Receiver: string
    Content: string
    ConversationId: string (optional)
}

Content Format: "action parameter1 parameter2 ..."
```

### Message Types

All message types should be defined as constants in `MessageTypes` class.

## Communication Schemas

### 1. Time Synchronization

**TickAgent → All Agents**

```
Message: "tick {tickIndex} {simulationTime}"
Example: "tick 15 12:30:00 PM"

Frequency: Every simulation minute
Purpose: Synchronize all agents to simulation time
```

**Agent Response**
- Update internal time state
- Trigger time-based actions
- No response message required

### 2. Prosumer Component Coordination

**ProsumerAgent ↔ Component Agents**

#### Component Registration
```
ComponentAgent → ProsumerAgent
Message: "component_ready"
Purpose: Notify parent that component is ready
```

#### Component Start
```
ProsumerAgent → ComponentAgent
Message: "prosumer_start"
Purpose: Signal that all components are ready
```

#### Energy Updates
```
GenerationAgent → ProsumerAgent
Message: "generation_update {generationRate}"
Example: "generation_update 2.5"

ConsumptionAgent → ProsumerAgent
Message: "load_update {loadValue}"
Example: "load_update 3.2"

BatteryAgent → ProsumerAgent
Message: "battery_soc {socPercentage}"
Example: "battery_soc 75.5"
```

#### Battery Operations
```
ProsumerAgent → BatteryAgent
Message: "store {energyAmount}"
Example: "store 1.5"

BatteryAgent → ProsumerAgent
Message: "energy_stored {storedAmount}"
Example: "energy_stored 1.5"

BatteryAgent → ProsumerAgent
Message: "battery_maximum_capacity {remainingCapacity}"
Example: "battery_maximum_capacity 0.3"
```

### 3. Grid Interaction

**ProsumerAgent ↔ GridAgent**

#### Price Updates
```
GridAgent → All Prosumers
Message: "energy_market_price {price}"
Example: "energy_market_price 0.15"
Frequency: Every 15 minutes (or on change)
```

#### Energy Purchase Request
```
ProsumerAgent → GridAgent
Message: "buy_from_grid {energyAmount}"
Example: "buy_from_grid 2.5"

GridAgent → ProsumerAgent
Message: "buy_confirmation {energyAmount} {price} {totalCost}"
Example: "buy_confirmation 2.5 0.15 0.375"
```

#### Energy Sale Request
```
ProsumerAgent → GridAgent
Message: "sell_to_grid {energyAmount}"
Example: "sell_to_grid 1.8"

GridAgent → ProsumerAgent
Message: "sell_confirmation {energyAmount} {price} {totalRevenue}"
Example: "sell_confirmation 1.8 0.08 0.144"
```

### 4. Auction Participation

**ProsumerAgent ↔ AuctionManager**

#### Auction Registration

**Dutch Auction - Seller Registration**
```
ProsumerAgent → AuctionManager
Message: "excess_to_sell {energyAmount} {floorPrice} {startingPrice}"
Example: "excess_to_sell 5.0 0.10 0.20"
```

**Dutch Auction - Buyer Registration**
```
ProsumerAgent → AuctionManager
Message: "deficit_to_buy {energyAmount}"
Example: "deficit_to_buy 3.5"
```

**Double Auction - Bid Submission**
```
ProsumerAgent → AuctionManager
Message: "bid {energyAmount} {price}"
Example: "bid 2.5 0.12"
```

**Double Auction - Ask Submission**
```
ProsumerAgent → AuctionManager
Message: "ask {energyAmount} {price}"
Example: "ask 3.0 0.14"
```

#### Price Announcements
```
AuctionManager → All Buyers
Message: "selling_price {currentPrice}"
Example: "selling_price 0.18"
Frequency: Every bidding interval (e.g., every 2 minutes)
```

#### Bid Submission
```
ProsumerAgent → AuctionManager
Message: "energy_bid {bidPrice}"
Example: "energy_bid 0.18"
```

#### Auction Results
```
AuctionManager → Winning Buyer
Message: "auction_won {energyAmount} {price} {sellerName}"
Example: "auction_won 2.5 0.18 seller_prosumer1"

AuctionManager → Winning Seller
Message: "auction_won {energyAmount} {price} {buyerName}"
Example: "auction_won 2.5 0.18 buyer_prosumer2"
```

#### Transaction Confirmation
```
AuctionManager → Buyer
Message: "buy_energy_confirmation {energyAmount} {price} {totalCost}"
Example: "buy_energy_confirmation 2.5 0.18 0.45"

AuctionManager → Seller
Message: "sell_energy_confirmation {energyAmount} {price} {totalRevenue}"
Example: "sell_energy_confirmation 2.5 0.18 0.45"
```

### 5. Weather Updates

**WeatherAgent → GenerationAgents**

```
WeatherAgent → All GenerationAgents
Message: "weather_update {solarIrradiance} {windSpeed} {temperature}"
Example: "weather_update 800 5.2 22.5"
Frequency: Every 15 minutes
```

**GenerationAgent Response**
- Update generation rates based on weather
- Recalculate expected generation
- No response message required

### 6. Monitoring and Data Collection

**MonitorAgent ↔ All Agents**

#### State Requests
```
MonitorAgent → ProsumerAgent
Message: "request_state"

ProsumerAgent → MonitorAgent
Message: "state_update {prosumerId} {generation} {consumption} {batterySOC} {energyBalance}"
Example: "state_update 1 2.5 3.2 75.5 -0.7"
```

#### Transaction Logging
```
AuctionManager → MonitorAgent
Message: "transaction_log {buyerId} {sellerId} {energyAmount} {price} {timestamp}"
Example: "transaction_log buyer_1 seller_2 2.5 0.18 12:30:00"
```

#### System Metrics
```
MonitorAgent → All Agents (periodic)
Message: "request_metrics"

Agents → MonitorAgent
Message: "metrics_update {agentType} {metricsJson}"
```

## Sequence Diagrams

### Energy Deficit Scenario

```
ProsumerAgent    ConsumptionAgent    BatteryAgent    AuctionManager    GridAgent
     |                  |                 |                |               |
     |--load_update---->|                 |                |               |
     |                  |                 |                |               |
     |<--generation-----|                 |                |               |
     |   update         |                 |                |               |
     |                  |                 |                |               |
     | Calculate Deficit                 |                |               |
     |                  |                 |                |               |
     |--check_battery--->|                |                |               |
     |                  |                 |                |               |
     |<--battery_soc----|                 |                |               |
     |                  |                 |                |               |
     |--deficit_to_buy--->|               |                |               |
     |                  |                 |                |               |
     |                  |                 |--selling_price-->|            |
     |                  |                 |                |               |
     |<--selling_price--|                 |                |               |
     |                  |                 |                |               |
     |--energy_bid------>|               |                |               |
     |                  |                 |                |               |
     |<--auction_won-----|                |                |               |
     |                  |                 |                |               |
     |<--buy_confirmation|                |                |               |
```

### Energy Excess Scenario

```
ProsumerAgent    GenerationAgent    BatteryAgent    AuctionManager    GridAgent
     |                  |                 |                |               |
     |<--generation-----|                 |                |               |
     |   update         |                 |                |               |
     |                  |                 |                |               |
     | Calculate Excess                  |                |               |
     |                  |                 |                |               |
     |--store_energy--->|                 |                |               |
     |                  |                 |                |               |
     |<--battery_full---|                 |                |               |
     |                  |                 |                |               |
     |--excess_to_sell--->|               |                |               |
     |                  |                 |                |               |
     |                  |                 |--auction_start-|               |
     |                  |                 |                |               |
     |<--selling_price--|                 |                |               |
     |                  |                 |                |               |
     |                  |                 |<--energy_bid---|               |
     |                  |                 |                |               |
     |<--auction_won----|                 |                |               |
     |                  |                 |                |               |
     |<--sell_confirmation               |                |               |
```

## Best Practices

### 1. Message Consistency
- Use constants for all message types
- Standardize message format
- Validate message content

### 2. Error Handling
- Handle missing agents gracefully
- Timeout for unresponsive agents
- Retry mechanisms for critical messages

### 3. Asynchronous Communication
- Don't block on message responses
- Use callbacks or state machines
- Handle out-of-order messages

### 4. Conversation Tracking
- Use conversation IDs for multi-message exchanges
- Track conversation state
- Handle conversation timeouts

### 5. Message Logging
- Log all messages for debugging
- Support message replay
- Enable message tracing

## Implementation Guidelines

### Message Parsing
```csharp
public static bool TryParseMessage(string content, out string action, out string parameters)
{
    // Parse "action param1 param2 ..." format
}
```

### Message Construction
```csharp
public static string Str(string action, params object[] parameters)
{
    // Construct "action param1 param2 ..." format
}
```

### Message Sending
```csharp
// Single agent
Send(receiverName, messageContent);

// Multiple agents
SendToMany(receiverList, messageContent);

// Broadcast
Broadcast(messageContent, includeSender: false);
```
