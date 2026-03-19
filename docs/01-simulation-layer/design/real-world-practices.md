# Real-World Practices in Energy Trading

## Overview

This document outlines real-world practices in energy trading, battery usage, prosumer interactions, and grid relationships based on current industry standards and research.

## Battery Usage Practices

### Residential Battery Systems

**Typical Configurations**
- Capacity: 5-15 kWh (most common: 10 kWh)
- Power: 5-10 kW
- Round-trip efficiency: 85-95%
- Depth of Discharge: 80-90%
- Cycle life: 4000-6000 cycles (10-15 years)

**Usage Patterns**

**1. Self-Consumption Optimization (Most Common)**
- Store excess solar generation during day
- Use stored energy during evening/night
- Maximize self-consumption rate (typically 60-80%)
- Minimize grid purchases
- **Rationale**: Grid sell price < Grid buy price

**2. Time-of-Use Optimization**
- Charge during low-price periods (night)
- Discharge during high-price periods (peak)
- Maximize arbitrage
- **Rationale**: Price difference > efficiency losses

**3. Backup Power**
- Maintain minimum charge for outages
- Emergency power supply
- Typically 20-30% reserved capacity

**Real-World Example: Tesla Powerwall**
- 13.5 kWh capacity
- 10 kW power
- 90% round-trip efficiency
- Self-consumption mode by default
- Time-based control available

### Commercial/Industrial Battery Systems

**Typical Configurations**
- Capacity: 100 kWh - 10 MWh
- Power: 50 kW - 5 MW
- More sophisticated control
- Multiple revenue streams

**Usage Patterns**

**1. Peak Shaving**
- Discharge during peak demand
- Reduce demand charges
- Significant cost savings

**2. Grid Services**
- Frequency regulation
- Voltage support
- Demand response
- Revenue generation

**3. Energy Arbitrage**
- Buy low, sell high
- Time-of-use optimization
- Market participation

## Energy Auctioning Between Prosumers

### Current State

**Limited Real-World Implementation**
- Most P2P energy trading is experimental
- Few commercial platforms exist
- Regulatory barriers in many regions
- Technology still developing

### Existing Platforms

**1. Power Ledger (Australia)**
- Blockchain-based platform
- Double auction mechanism
- Real-time trading
- Settlement via blockchain tokens

**2. Vandebron (Netherlands)**
- Direct contracts between producers and consumers
- Bilateral negotiation
- Long-term contracts
- Fixed prices

**3. SonnenCommunity (Germany)**
- Community-based trading
- Virtual energy sharing
- Battery-based community
- Simplified trading

**4. LO3 Energy (Brooklyn Microgrid)**
- Local energy marketplace
- Blockchain-based
- Neighbor-to-neighbor trading
- Real-time pricing

### Trading Mechanisms in Practice

**1. Fixed Price Contracts**
- Long-term agreements
- Predictable pricing
- Common in commercial settings

**2. Dynamic Pricing**
- Real-time price updates
- Market-based pricing
- Requires communication infrastructure

**3. Community-Based Trading**
- Shared resources
- Simplified pricing
- Social benefits

### Challenges

**1. Regulatory**
- Grid connection rules
- Tax implications
- Licensing requirements
- Varies by jurisdiction

**2. Technical**
- Communication infrastructure
- Metering requirements
- Settlement systems
- Grid integration

**3. Economic**
- Transaction costs
- Price discovery
- Market liquidity
- Risk management

## Grid Relationships

### Buying from Grid

**When Prosumers Buy**
- Energy deficit (most common)
- Battery depleted
- P2P trading unavailable
- Grid price competitive

**Pricing Structures**

**Residential (Typical)**
- Flat rate: $0.10-0.15/kWh
- Time-of-use: $0.08-0.25/kWh
- Tiered: Varies by consumption level

**Commercial**
- Demand charges: $5-20/kW
- Energy charges: $0.08-0.12/kWh
- Time-of-use common

**Industrial**
- Negotiated rates
- Real-time pricing
- Demand response programs

### Selling to Grid

**Feed-in Tariffs (FIT)**
- Fixed price for excess generation
- Long-term contracts (10-20 years)
- Declining rates over time
- Common in Europe

**Net Metering**
- 1:1 credit for excess generation
- Monthly or annual settlement
- Common in US residential
- May have capacity limits

**Market-Based Pricing**
- Wholesale market prices
- Variable pricing
- Requires market participation
- More complex

**Typical Rates**
- Feed-in tariff: $0.03-0.10/kWh
- Net metering: Credit at retail rate
- Market price: Variable, often $0.02-0.08/kWh

### Grid Services

**Frequency Regulation**
- Maintain 50/60 Hz frequency
- Fast response required (< 1 second)
- Compensation: $20-50/MW-hour
- Requires specific capabilities

**Voltage Support**
- Maintain voltage levels
- Local grid support
- Compensation varies
- May require equipment upgrades

**Demand Response**
- Reduce load during peak
- Incentive-based
- $50-500 per event
- Voluntary participation

## Best Practices for Implementation

### 1. Realistic Modeling

**Battery Behavior**
- Model efficiency losses accurately
- Include self-discharge
- Respect depth of discharge limits
- Consider degradation over time

**Generation Profiles**
- Use real historical data
- Model weather dependencies
- Include variability
- Time-of-day patterns

**Consumption Profiles**
- Realistic load curves
- Seasonal variations
- Day-of-week patterns
- Random variability

### 2. Pricing Realism

**Price Spreads**
- Grid buy: 2-3x grid sell price
- P2P prices: Between grid buy and sell
- Reflects real-world economics

**Time Variation**
- Peak prices: 2-3x off-peak
- Reflect demand patterns
- Seasonal variations

**Price Discovery**
- Market-based pricing
- Supply/demand dynamics
- Transparent pricing

### 3. Trading Behavior

**Decision Making**
- Compare all options (battery, grid, P2P)
- Consider transaction costs
- Evaluate risk
- Optimize for objectives (cost, sustainability, etc.)

**Trading Frequency**
- Continuous or periodic
- Match real-world capabilities
- Consider communication limitations

**Transaction Costs**
- Include in calculations
- Vary by mechanism
- May affect trading decisions

### 4. Grid Integration

**Reliability**
- Grid as backup
- Always available
- Most reliable option

**Capacity Constraints**
- Model grid limits
- Handle peak scenarios
- Support demand response

**Regulatory Compliance**
- Follow grid connection rules
- Respect capacity limits
- Handle feed-in restrictions

## Research Insights

### From Academic Literature

**1. P2P Trading Benefits**
- 10-30% cost savings potential
- Increased renewable energy utilization
- Reduced grid stress
- Community benefits

**2. Battery Optimization**
- Self-consumption: 60-80% typical
- Time-of-use: 5-15% additional savings
- Grid services: Additional revenue stream

**3. Market Mechanisms**
- Double auction: Most efficient
- Bilateral negotiation: Most flexible
- Dutch auction: Fast execution
- Choice depends on context

**4. Grid Impact**
- Reduced peak demand
- Better renewable integration
- Potential for grid services
- Need for new regulations

## Recommendations for Simulation

### 1. Start Simple
- Basic battery behavior
- Simple pricing
- One auction mechanism
- Add complexity gradually

### 2. Use Real Data
- Historical load profiles
- Real generation data
- Actual price structures
- Weather data

### 3. Model Realistically
- Efficiency losses
- Time constraints
- Communication delays
- Transaction costs

### 4. Support Multiple Scenarios
- Different prosumer types
- Various battery sizes
- Multiple trading mechanisms
- Different grid policies

### 5. Enable Analysis
- Comprehensive logging
- Performance metrics
- Cost analysis
- Comparison capabilities
