# Grid Agent - Best Practices

## Overview

The Grid Agent represents the main electrical grid (utility company) that provides backup energy supply and absorbs excess energy. It acts as a reliable, always-available trading partner for prosumers.

## Responsibilities

### 1. Energy Supply

**When Grid Supplies Energy**
- Prosumer has energy deficit (consumption > generation + battery)
- P2P trading insufficient or unavailable
- Prosumer chooses grid over P2P (price, reliability)

**Characteristics**
- Always available (reliability)
- Unlimited capacity (in theory)
- Standardized pricing
- Instant delivery

### 2. Energy Absorption

**When Grid Absorbs Energy**
- Prosumer has energy excess (generation + battery > consumption)
- P2P trading unavailable or unprofitable
- Battery at maximum capacity

**Characteristics**
- May have limits (grid capacity)
- Feed-in tariffs or net metering
- Price typically lower than buy price
- May be restricted during low demand

### 3. Price Management

**Price Structures**

**Flat Rate**
- Constant price regardless of time
- Simple for consumers
- No time-based optimization

**Time-of-Use (ToU)**
- Different prices for peak/off-peak periods
- Encourages load shifting
- Common in commercial/industrial

**Real-Time Pricing (RTP)**
- Dynamic prices based on market conditions
- Reflects actual grid conditions
- Requires smart meters and communication

**Tiered Pricing**
- Different prices for different consumption levels
- Encourages conservation
- Common in residential

**Net Metering**
- Credit for excess generation
- Typically 1:1 credit ratio
- Monthly or annual settlement

### 4. Grid Services

**Frequency Regulation**
- Maintain grid frequency (50/60 Hz)
- Requires fast response
- Compensated service

**Voltage Support**
- Maintain voltage levels
- Local grid support
- May require specific equipment

**Demand Response**
- Reduce load during peak periods
- Incentive-based
- Voluntary participation

## Grid Agent Architecture

### Recommended Structure

```
GridAgent
├── PriceManager
│   ├── Price Calculation
│   ├── Tariff Management
│   └── Price Broadcasting
├── TransactionManager
│   ├── Buy Transactions
│   ├── Sell Transactions
│   └── Settlement
├── CapacityManager
│   ├── Grid Capacity Tracking
│   ├── Load Management
│   └── Constraint Handling
└── ServiceManager
    ├── Grid Services
    └── Service Contracts
```

## State Management

### Required State

**Price State**
- Current buy price
- Current sell price
- Price history
- Tariff structure

**Capacity State**
- Available capacity
- Current load
- Peak capacity
- Reserve margin

**Transaction State**
- Active transactions
- Transaction history
- Settlement records
- Billing information

**Operational State**
- Grid status (normal, constrained, emergency)
- Service availability
- Maintenance schedules

## Pricing Strategies

### 1. Cost-Plus Pricing
- Based on generation cost + margin
- Stable prices
- Predictable

### 2. Market-Based Pricing
- Based on wholesale market prices
- Variable prices
- Reflects actual costs

### 3. Time-Varying Pricing
- Prices change with demand
- Encourages load shifting
- More efficient

### 4. Dynamic Pricing
- Real-time price updates
- Reflects grid conditions
- Requires communication infrastructure

## Real-World Practices

### Grid Buy Prices (Retail)
- Typically higher than wholesale
- Includes transmission, distribution, retail margin
- Varies by region and tariff
- Range: $0.10 - $0.30 per kWh (varies by country)

### Grid Sell Prices (Feed-in)
- Typically lower than buy price
- May be fixed (feed-in tariff) or variable
- Net metering: credit at retail rate
- Range: $0.03 - $0.15 per kWh

### Price Spread
- Buy price typically 2-3x sell price
- Creates incentive for self-consumption
- Varies by region and policy

### Grid Constraints
- Grid may limit feed-in during low demand
- Capacity constraints during peak
- Voltage/frequency limits
- Regional variations

## Communication Protocols

### Price Updates
- Broadcast prices to all prosumers
- Update frequency: every 15 minutes to hourly
- Include price forecasts when available

### Transaction Requests
- Prosumers request buy/sell
- Grid confirms availability
- Execute transaction
- Settlement

### Grid Status
- Broadcast grid status
- Alert during constraints
- Emergency notifications

## Implementation Recommendations

### 1. Always Available
- Grid should always respond to requests
- No rejection (unless capacity constraint)
- Reliable service

### 2. Price Transparency
- Clear pricing structure
- Regular price updates
- Historical price data

### 3. Capacity Modeling
- Model grid capacity constraints
- Handle peak demand scenarios
- Support demand response

### 4. Multiple Tariffs
- Support different tariff structures
- Allow prosumers to choose
- Model real-world complexity

### 5. Settlement
- Track all transactions
- Periodic settlement (daily, monthly)
- Billing and payment

## Best Practices

### 1. Reliability
- Grid should be most reliable option
- Always available as backup
- Handle all transaction requests

### 2. Pricing
- Realistic price spreads
- Time-varying prices
- Transparent pricing

### 3. Capacity
- Model constraints realistically
- Handle peak scenarios
- Support demand response

### 4. Services
- Support grid services (optional)
- Model service contracts
- Compensate for services

### 5. Integration
- Seamless integration with P2P trading
- Prosumers can compare prices
- Easy switching between grid and P2P

## Use Cases

### Backup Supply
- When P2P trading insufficient
- When battery depleted
- Emergency situations

### Excess Absorption
- When P2P trading unavailable
- When battery full
- When grid price acceptable

### Price Benchmark
- Prosumers compare P2P prices
- Decision making reference
- Market price discovery

### Grid Services
- Frequency regulation
- Voltage support
- Demand response participation
