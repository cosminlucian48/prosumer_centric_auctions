# Energy Market Agent

## Overview

The **Energy Market Agent** represents the interface between prosumers/consumers and the electrical grid for buying and selling energy. In real-world terms, it embodies the role of a **retail energy market** or **utility company**: it publishes prices at which prosumers can buy from the grid (retail) and sell to the grid (feed-in), and it may execute or record those transactions. It is the counterpart to peer-to-peer (P2P) trading: when a prosumer does not cover its need or surplus via P2P, it turns to this agent as a reliable, always-available trading partner.

This document describes how the agent should behave to resemble real-world energy markets, independent of the current codebase. Implementation details are deferred until the design is fixed.

---

## Role in the System

### What the Agent Represents

In practice, “the grid” for a prosumer is experienced through:

- **Retail prices** (buying from grid)
- **Feed-in / net metering rules** (selling to grid)
- **Tariff structure** (flat, time-of-use, real-time, etc.)
- **Settlement** (how much is paid or credited)

The Energy Market Agent is the **single point of contact** for all grid-related price information and, in the simulation, for grid buy/sell transactions. It does not necessarily model the wholesale market in detail; it exposes **retail- and feed-in-relevant** prices and behaviour to prosumers.

### Relation to Real-World Concepts

| Real-world concept | In the simulation |
|--------------------|-------------------|
| Utility / retailer | Energy Market Agent (publishes prices, may execute trades) |
| Wholesale market (e.g. day-ahead, real-time) | Optional: can be the *source* of the agent’s prices (e.g. from a price series or formula) |
| DSO/TSO (grid operator) | Can be abstracted into “grid capacity” or “grid status” handled by this agent or a separate Grid Operator agent |
| Retail tariff (ToU, flat, RTP) | Implemented by how the agent computes and publishes buy/sell prices over time |
| Feed-in tariff / net metering | Implemented by the sell price (or credit) the agent offers |

---

## Responsibilities

### 1. Publish Grid Prices

- **Buy price (retail)**: Price at which prosumers/consumers can buy energy from the grid (€/kWh or equivalent).
- **Sell price (feed-in)**: Price or credit at which prosumers can sell excess energy to the grid.

Prices may depend on:

- **Time**: e.g. time-of-use (ToU) or real-time pricing (RTP).
- **Settlement interval**: Real markets often use 15-minute or hourly settlement; the agent should publish prices at a defined interval (e.g. every 15 minutes of simulation time).
- **Tariff type**: Flat, ToU, tiered, or real-time, depending on configuration.

The agent **broadcasts** or **sends** these prices to all registered prosumers (or to those subscribed to the market) so they can make decisions (consume, store, sell to grid, participate in P2P).

### 2. Optional: Execute Grid Buy/Sell Transactions

- **Buy from grid**: Prosumer requests to buy a certain amount of energy; the agent confirms and records the transaction (volume, price, cost).
- **Sell to grid**: Prosumer requests to sell a certain amount; the agent confirms and records (volume, price, revenue/credit).

If the agent does not execute trades itself, it only provides prices; another component (e.g. a dedicated “Grid Settlement” or “Grid Operator” agent) might execute and settle. For clarity, the design should state whether this agent only publishes prices or also executes and records transactions.

### 3. Represent Grid Availability and Reliability

- The grid is **always available** as a counterparty (no “market closed” for standard buy/sell).
- Optionally, the agent can broadcast **grid status** (e.g. normal, constrained, high prices) so that prosumers can adapt (e.g. reduce demand or prefer P2P).

### 4. Optional: Capacity and Limits

- **Buy from grid**: Usually assumed unlimited for small prosumers (no cap per prosumer in the baseline design).
- **Sell to grid**: Real grids may limit feed-in (capacity, voltage, or market rules). The agent can enforce a **maximum feed-in per period** or **per prosumer**, and refuse or curtail excess.

### 5. Optional: Settlement and Billing

- Record of all grid buy/sell transactions (who, when, volume, price, amount).
- Settlement can be **instantaneous** (per 15-minute period) or **aggregated** (e.g. daily/monthly bill or credit), depending on the simulation design.

---

## Real-World Pricing Structures

The agent should support at least one of the following, configurable per scenario.

### 1. Flat Rate

- Single price for buying and optionally a single price for selling, independent of time.
- **Use case**: Simple baseline, low data requirements.

### 2. Time-of-Use (ToU)

- Different buy (and optionally sell) prices for **peak**, **off-peak**, and sometimes **shoulder** periods.
- Example: higher price 7–21h, lower 21–7h.
- **Use case**: Encourages load shifting and storage; common in retail tariffs.

### 3. Real-Time Pricing (RTP)

- Prices updated frequently (e.g. every 15 or 60 minutes) and may reflect demand/supply or wholesale prices.
- Can be driven by historical data (e.g. from a database or CSV) or by a simple formula (e.g. demand-based).
- **Use case**: Research, demand response, alignment with wholesale markets.

### 4. Net Metering

- Prosumer does not “sell” at a separate price; instead, excess generation is **credited** at the **retail buy price** (or a fraction of it), often 1:1.
- The agent can represent this as: **sell price = buy price** (or sell price = k × buy price), and optionally aggregate over a billing period.

### 5. Feed-in Tariff (FIT)

- Fixed sell price (e.g. €/kWh) for fed-in energy, possibly different from the buy price and constant over long periods.
- **Use case**: Policy-driven support for renewables; sell price often lower than buy price.

### Price Spread (Buy vs Sell)

- In reality, **retail buy price** is usually **higher** than **feed-in sell price** (often 2–3×).
- This creates an incentive for self-consumption and storage.
- The agent should make **buy price** and **sell price** configurable and distinct (unless net metering is explicitly chosen).

---

## State (Recommended)

The following state is recommended so that the agent behaves like a real-world retail market interface.

### Price State

- **Current buy price** (retail, €/kWh)
- **Current sell price** (feed-in or credit, €/kWh)
- **Last update time** (simulation time)
- **Tariff type** (flat, ToU, RTP, net metering, FIT)
- **Price history** (optional): e.g. last 24h or 96 intervals (15-minute) for analysis and forecasting.

### Registered Participants

- **List of prosumer/consumer IDs** that receive price updates (and possibly participate in grid transactions).

### Transaction State (if the agent executes trades)

- **Active requests**: e.g. pending buy/sell requests.
- **Transaction log**: buyer/seller, volume, price, time, direction (buy/sell).

### Optional: Grid and Capacity State

- **Grid status**: e.g. normal / constrained / emergency.
- **Feed-in limit** (total or per prosumer) for the current period.
- **Settlement period**: e.g. 15 minutes; current period index or timestamp.

---

## Time and Settlement Granularity

- **Price update interval**: Align with real practice (e.g. **15 minutes** or 1 hour). In simulation, this can be every N ticks (e.g. 15 ticks if 1 tick = 1 minute).
- **Settlement interval**: Same as price interval (e.g. 15-minute settlement) is a common choice and simplifies accounting.
- **Optional**: Day-ahead vs real-time: the agent can publish “day-ahead” and “real-time” prices if the scenario requires it; otherwise a single “current” price per interval is sufficient.

---

## Behaviours (Detailed)

### 1. Initialisation (Setup)

- Load or set **tariff parameters** (e.g. flat prices, ToU bands, or RTP data source).
- Initialise **buy price** and **sell price** for the first period.
- Optionally **discover** or **register** prosumers (e.g. broadcast “find prosumers” and collect “prosumer start” or “register” messages).
- Optionally load **price series** from database or file (e.g. historical wholesale or retail prices).

### 2. Periodic Price Update (e.g. Every 15 Minutes of Simulation Time)

- Compute or retrieve **buy price** and **sell price** for the **current period** (based on tariff type and current time).
- **Broadcast** or **send** the new prices to all registered prosumers (e.g. message type “energy_market_price” with buy and sell, or two separate messages if needed).
- Store **last timestamp** and **current prices** in state.
- If using **price history**, append current prices.

### 3. Prosumer Registration

- When a prosumer joins (e.g. “prosumer start” or “register” message), add it to the **registered participants** list.
- **Send the current prices** to the new prosumer immediately so it can make decisions without waiting for the next broadcast.

### 4. Optional: Handling Buy/Sell Requests

- **Buy request**: Prosumer sends volume (kWh). Agent confirms availability (e.g. always true), records transaction (volume, current buy price, total cost), sends confirmation (e.g. “buy_confirmation” with volume, price, cost).
- **Sell request**: Prosumer sends volume. Agent checks **feed-in limit** (if implemented); if accepted, records transaction and sends “sell_confirmation” (volume, price, revenue/credit). If rejected, send rejection with reason (e.g. “feed-in limit reached”).

### 5. Optional: Grid Status

- If **grid status** is modelled, the agent may broadcast status changes (e.g. “grid_constrained”) so prosumers can reduce export or adjust behaviour.

---

## Message Protocol (Recommended)

### Incoming

- **Prosumer registration / start**: e.g. `ProsumerStart` or `Register` (prosumer ID). Agent adds to list and sends current prices.
- **Tick**: Used to advance time; every N ticks the agent updates prices and broadcasts.
- **Buy request** (optional): e.g. `BuyFromGrid {volume}`.
- **Sell request** (optional): e.g. `SellToGrid {volume}`.

### Outgoing

- **Find prosumers** (optional, at setup): e.g. `FindProsumers` to discover participants.
- **Energy market price**: e.g. `EnergyMarketPrice {buyPrice} {sellPrice}` or separate messages for buy/sell. Include timestamp or period index if useful.
- **Buy confirmation** (optional): e.g. `BuyConfirmation {volume} {price} {totalCost}`.
- **Sell confirmation** (optional): e.g. `SellConfirmation {volume} {price} {revenue}`.
- **Sell rejection** (optional): e.g. `SellRejected {reason}`.
- **Grid status** (optional): e.g. `GridStatus {status}`.

---

## Dependencies

- **Tick agent**: Provides simulation time (and possibly tick index) so the Energy Market Agent can align price updates and settlement to a 15-minute (or other) interval.
- **Optional**: **Database or file** for historical price series (e.g. wholesale or retail prices per 15-minute interval).
- **Optional**: **Configuration** for tariff type, price parameters, feed-in limits, and settlement period.

---

## Real-World Alignment Checklist

- [ ] **Two prices**: Separate buy (retail) and sell (feed-in) prices; buy > sell unless net metering.
- [ ] **Time-based pricing**: Support at least ToU or RTP; price updates at fixed intervals (e.g. 15 min).
- [ ] **Settlement interval**: Define (e.g. 15 min) and use consistently for pricing and optional transaction recording.
- [ ] **Always available**: Grid buy/sell is always possible (except optional feed-in limit).
- [ ] **Registration**: Prosumers register or are discovered; only they receive price updates (or broadcast to all).
- [ ] **Optional feed-in limit**: Cap on sell volume per period or per prosumer.
- [ ] **Optional transaction recording**: Log of grid buy/sell for settlement and analysis.
- [ ] **Optional grid status**: Simple status signal for constrained or emergency conditions.

---

## Implementation and Supporting Classes

For **how** to implement this agent (single agent vs helper agents, flow, and recommended **helper classes** such as PriceState, Tariff, GridTransaction, GridStatus, ParticipantRegistry), see:

- **[Energy Market – Implementation](energy-market/implementation.md)** – Implementation recommendation and flow.
- **[Energy Market – Architecture](energy-market/architecture.md)** – Architecture decisions (single agent, no helper agents, use of classes).
- **[Energy Market – Classes](energy-market/classes/README.md)** – Supporting classes: PriceState, Tariff, GridTransaction, GridTransactionLog, GridStatus, FeedInLimit, ParticipantRegistry.

---

## Implementation Status (To Be Filled Later)

- **Price publication**: To be implemented according to the above (single buy/sell, 15-minute updates, tariff type).
- **Prosumer registration**: To be implemented (list of participants, send prices on join).
- **Grid buy/sell execution**: Optional; to be implemented if the agent is responsible for transactions.
- **Feed-in limit and grid status**: Optional; to be implemented if required by the scenario.

---

## References and Further Reading

- Design doc: **Grid Agent** (`design/grid-agent.md`) for high-level role and pricing strategies.
- Design doc: **Real-World Practices** (`design/real-world-practices.md`) for typical retail/feed-in spreads and tariff examples.
- Real markets: ISO New England (day-ahead, real-time, 5-min settlement), CAISO (15-minute market), Nord Pool (hourly/day-ahead), EPEX SPOT (15-min products). The Energy Market Agent abstracts the **retail** view of such markets for prosumers.
