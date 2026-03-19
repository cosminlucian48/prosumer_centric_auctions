# Energy Market Agent – Implementation Recommendation

## Opinion: Single Agent, No Helper Agents

**Recommendation: implement a single Energy Market Agent that owns all grid-retail behaviour.** Do not split this into multiple *agents* (e.g. separate “Price Agent”, “Grid Operator Agent”, “Settlement Agent”) unless you later need distinct actors (e.g. separate DSO and retailer).

### Why one agent?

1. **Single point of contact** – Prosumers already have one “grid” counterparty in reality (the retailer/utility). One agent keeps the model simple and the message protocol clear.
2. **No extra coordination** – Multiple agents would need to agree on prices, limits, and settlement. One agent avoids cross-agent synchronization and race conditions.
3. **Easier testing and debugging** – One agent, well-structured with internal classes, is easier to unit-test and trace than a distributed design.
4. **Sufficient for most research/teaching** – For prosumer-centric auctioning and comparison with P2P, a single “grid interface” agent is enough. You can later introduce a separate Grid Operator agent if you need explicit grid constraints or DSO behaviour.

### When to add another agent?

Consider a **separate Grid Operator Agent** only if you need:

- Independent grid capacity or congestion logic.
- Grid status (e.g. constraints, emergencies) that is not derived from the same process that sets retail prices.
- Different update frequencies (e.g. grid status every minute, prices every 15 minutes).

Until then, the Energy Market Agent can hold optional **GridStatus** and **FeedInLimit** internally (see [GridStatus](./classes/grid-status.md)).

---

## What the agent should do (summary)

1. **Price publication** – Use a **Tariff** (or price source) to compute current **buy** and **sell** prices; store them in **PriceState**; broadcast (or send) to registered prosumers at a fixed interval (e.g. every 15 minutes of simulation time).
2. **Registration** – Maintain a **ParticipantRegistry** (list of prosumer IDs); on registration, send current prices once.
3. **Optional: execute grid buy/sell** – On `BuyFromGrid` / `SellToGrid` messages, validate (e.g. feed-in limit), record a **GridTransaction**, send confirmation or rejection.
4. **Optional: grid status and limits** – Use **GridStatus** and **FeedInLimit** to refuse or curtail feed-in when “constrained”.

The agent **does not** need a helper *agent* for any of this; it should use **helper classes** (see below and [Classes](./classes/README.md)).

---

## Recommended helper classes (not agents)

These are **classes** (or interfaces + implementations) used by the Energy Market Agent. They can live in the same assembly (e.g. `Models` or `EnergyMarket` namespace).

| Class | Purpose |
|-------|---------|
| **PriceState** | Holds current buy/sell price, timestamp, optional short history. |
| **ITariff** / **Tariff** | Computes buy/sell price for a given time (flat, ToU, RTP, etc.). |
| **GridTransaction** | One grid buy or sell record (prosumer, volume, price, direction, time). |
| **GridTransactionLog** (or **SettlementLog**) | List/aggregate of grid transactions for settlement or analysis. |
| **GridStatus** | Enum or small class: Normal, Constrained, Emergency (optional). |
| **FeedInLimit** | Optional: max feed-in per period or per prosumer. |
| **ParticipantRegistry** | List (or set) of registered prosumer IDs; optionally metadata (e.g. tariff choice). |

The agent **orchestrates** these: it owns one Tariff, one PriceState, one ParticipantRegistry, one GridTransactionLog (if transactions are implemented), and optionally GridStatus and FeedInLimit.

---

## Suggested flow inside the agent

### Initialisation (Setup)

1. Load configuration (tariff type, parameters, settlement interval, optional feed-in limit).
2. Build **ITariff** (e.g. FlatTariff, ToUTariff, or RTPTariff from DB/file).
3. Initialise **PriceState** with first prices (e.g. for time 0).
4. Initialise **ParticipantRegistry** (empty).
5. Optionally initialise **GridStatus** = Normal, **FeedInLimit**.
6. Optionally broadcast `FindProsumers` (if discovery is used).

### On tick (e.g. every 15 ticks = 15 minutes)

1. Get current simulation time (from tick message).
2. Ask **Tariff** for buy/sell prices at that time; update **PriceState**.
3. Broadcast `EnergyMarketPrice {buyPrice} {sellPrice}` to all IDs in **ParticipantRegistry** (or broadcast to all agents if registration is not used).
4. If **GridStatus** is modelled and time-based, update it here.
5. Optional: reset **FeedInLimit** for the new period (if limit is per period).

### On ProsumerStart / Register

1. Add prosumer ID to **ParticipantRegistry**.
2. Send current prices once: `EnergyMarketPrice {buyPrice} {sellPrice}` to that prosumer.

### On BuyFromGrid (optional)

1. Parse volume.
2. Get current buy price from **PriceState**.
3. Create **GridTransaction** (buy, prosumer, volume, price, time).
4. Append to **GridTransactionLog**.
5. Send `BuyConfirmation {volume} {price} {totalCost}` to prosumer.

### On SellToGrid (optional)

1. Parse volume.
2. Check **FeedInLimit** (if used): if over limit, send `SellRejected` and return.
3. Get current sell price from **PriceState**.
4. Create **GridTransaction** (sell, prosumer, volume, price, time).
5. Append to **GridTransactionLog**; update **FeedInLimit** consumed (if per period).
6. Send `SellConfirmation {volume} {price} {revenue}` to prosumer.

---

## File / namespace layout suggestion

```
EnergyMarket/
  EnergyMarketAgent.cs          # The agent (Act, Setup, message handlers)
  PriceState.cs                 # Current prices + optional history
  Tariff/
    ITariff.cs
    FlatTariff.cs
    TimeOfUseTariff.cs
    RealTimePricingTariff.cs
  GridTransaction.cs            # Single transaction record
  GridTransactionLog.cs         # List/aggregation of transactions
  GridStatus.cs                 # Enum or class
  FeedInLimit.cs                # Optional
  ParticipantRegistry.cs        # List of registered prosumer IDs
```

Or under existing `Models/` and `Agents/EnergyMarket/`:

```
Models/
  PriceState.cs
  GridTransaction.cs
  GridStatus.cs
  FeedInLimit.cs
  ParticipantRegistry.cs
  Tariff/ ...
Agents/EnergyMarket/
  EnergyMarketAgent.cs
  GridTransactionLog.cs         # If agent-specific
```

---

## Summary

- **One agent**: Energy Market Agent; no helper *agents*.
- **Helper classes**: PriceState, Tariff (and implementations), GridTransaction, GridTransactionLog, GridStatus, FeedInLimit, ParticipantRegistry.
- **Flow**: Tariff → PriceState → broadcast on tick; optional buy/sell handling with GridTransaction and FeedInLimit; registration via ParticipantRegistry.

Detailed contracts for each class are in [Classes](./classes/README.md).
