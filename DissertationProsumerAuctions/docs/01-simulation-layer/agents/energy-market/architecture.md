# Energy Market Agent – Architecture

## Architecture decisions

### 1. Single agent boundary

- **Decision**: The “retail grid” is represented by **one** agent: the Energy Market Agent.
- **Rationale**: In reality, prosumers interact with one retailer/utility for grid buy/sell. One agent keeps the MAS simple and avoids inter-agent coordination. If later we need a distinct grid operator (DSO), we can add a second agent and document the protocol between them.

### 2. No helper agents

- **Decision**: Do **not** introduce separate agents for “price publisher”, “grid operator”, or “settlement”.
- **Rationale**: Those would be artificial splits; in practice the same retailer does pricing, acceptance of buy/sell, and settlement. One agent with clear internal structure (and helper classes) is sufficient and easier to maintain.

### 3. Helper classes inside the same domain

- **Decision**: Use **classes** (PriceState, Tariff, GridTransaction, GridStatus, FeedInLimit, ParticipantRegistry) within the same process. The Energy Market Agent holds instances of these and uses them in its message handlers.
- **Rationale**: Separation of concerns (e.g. “how is price computed?” vs “when do we send a message?”) is achieved by classes and interfaces, not by more agents. This keeps message passing simple and logic testable.

### 4. Tariff as pluggable strategy

- **Decision**: Price is computed by an **ITariff** (or equivalent) implementation (Flat, ToU, RTP, etc.). The agent depends on ITariff, not on a concrete tariff.
- **Rationale**: Different scenarios (flat, ToU, RTP) can be swapped via configuration without changing the agent code. Same pattern can support net metering (sell price = buy price) or feed-in tariff (fixed sell price).

### 5. Optional transaction execution and settlement

- **Decision**: The agent **may** execute grid buy/sell (handle BuyFromGrid / SellToGrid) and record **GridTransaction** in a **GridTransactionLog**. This is optional so that a “price-only” mode is possible (e.g. prosumers only use the published price for decisions, and settlement is modelled elsewhere).
- **Rationale**: Supports both “minimal” (price publication only) and “full” (price + execution + settlement) implementations without introducing another agent.

### 6. Optional grid status and feed-in limit

- **Decision**: **GridStatus** and **FeedInLimit** are optional. When present, the agent uses them to refuse or curtail feed-in and may broadcast grid status.
- **Rationale**: Keeps the core agent simple; advanced behaviour (constraints, emergencies) is added via optional classes and logic.

---

## Component diagram (logical)

```
                    ┌─────────────────────────────────────┐
                    │     Energy Market Agent              │
                    │                                     │
  Prosumers ───────►│  ┌─────────────┐  ┌──────────────┐ │
  (messages)        │  │ PriceState   │  │ ITariff      │ │
                    │  │ (buy, sell,  │◄─│ (Flat, ToU,  │ │
                    │  │  time)      │  │  RTP, …)     │ │
                    │  └─────────────┘  └──────────────┘ │
                    │         │                │         │
                    │  ┌──────▼────────────────▼──────┐  │
                    │  │ ParticipantRegistry           │  │
                    │  │ (list of prosumer IDs)        │  │
                    │  └──────────────────────────────┘  │
                    │         │                          │
                    │  ┌──────▼──────────────┐           │
                    │  │ GridTransactionLog  │ (opt)     │
                    │  │ GridStatus          │ (opt)     │
                    │  │ FeedInLimit         │ (opt)     │
                    │  └────────────────────┘           │
                    └─────────────────────────────────────┘
```

- **External**: Only the Energy Market Agent is visible to other agents (prosumers, tick).
- **Internal**: PriceState, ITariff, ParticipantRegistry, and optionally GridTransactionLog, GridStatus, FeedInLimit are used inside the agent.

---

## Dependencies (inbound)

- **TickAgent**: Supplies simulation time (and tick index). The Energy Market Agent uses this to decide when to update prices (e.g. every 15 ticks) and what time to pass to the Tariff.
- **Configuration**: Tariff type and parameters, settlement interval, optional feed-in limit, optional price series (file/DB) for RTP.
- **Optional**: Database or file for historical price series (if RTP is used).

## Dependencies (outbound)

- **Prosumers**: The Energy Market Agent sends price messages (and optionally confirmations/rejections) to prosumers. It does not depend on other domain agents (e.g. auctioneer); prosumers compare grid price with P2P and decide.

---

## What belongs in the agent vs in a class

| Responsibility | In agent | In class |
|----------------|----------|----------|
| When to broadcast price | ✅ Agent (on tick) | |
| What price to broadcast | | ✅ Tariff + PriceState |
| Who receives the price | ✅ Agent (Send/Broadcast) | ✅ ParticipantRegistry (who is “who”) |
| Recording a grid transaction | ✅ Agent (on message) | ✅ GridTransaction, GridTransactionLog |
| Deciding buy/sell price for time T | | ✅ Tariff |
| Storing current prices | | ✅ PriceState |
| Checking feed-in limit | ✅ Agent (before accepting sell) | ✅ FeedInLimit |
| Grid status (normal/constrained) | ✅ Agent (may broadcast) | ✅ GridStatus |

---

## Summary

- **One agent**, no helper agents.
- **Helper classes** for state and rules: PriceState, Tariff, GridTransaction, GridTransactionLog, GridStatus, FeedInLimit, ParticipantRegistry.
- **Clear split**: agent = orchestration and messaging; classes = data and pricing/limit logic.
