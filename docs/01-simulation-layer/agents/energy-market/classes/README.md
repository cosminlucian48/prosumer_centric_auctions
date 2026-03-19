# Energy Market Agent – Supporting Classes

These classes are used by the **Energy Market Agent** to manage prices, tariffs, transactions, grid status, and participant registration. They are **not** agents; they are normal C# classes (or interfaces + implementations) in the same process.

| Class | Document | Purpose |
|-------|----------|---------|
| **PriceState** | [price-state.md](./price-state.md) | Holds current buy/sell price and timestamp; optional short history. |
| **Tariff** | [tariff.md](./tariff.md) | Interface and implementations (Flat, ToU, RTP, NetMetering) to compute price from time. |
| **GridTransaction** | [grid-transaction.md](./grid-transaction.md) | Single grid buy or sell record. |
| **GridTransactionLog** | [grid-transaction.md](./grid-transaction.md) | Log/aggregation of grid transactions for settlement. |
| **GridStatus** | [grid-status.md](./grid-status.md) | Enum or class for grid state (Normal, Constrained, Emergency). |
| **FeedInLimit** | [grid-status.md](./grid-status.md) | Optional: max feed-in per period or per prosumer. |
| **ParticipantRegistry** | [participant-registry.md](./participant-registry.md) | List of registered prosumer IDs receiving price updates. |

## Usage by the agent

- **PriceState**: Updated every price interval from **Tariff**; read when broadcasting prices or when processing buy/sell.
- **Tariff**: Injected or built in Setup; `GetBuyPrice(time)`, `GetSellPrice(time)` (or equivalent) called each period.
- **GridTransaction** / **GridTransactionLog**: Used only if the agent executes buy/sell; each request creates a transaction and appends to the log.
- **GridStatus** / **FeedInLimit**: Optional; used when processing sell requests and when broadcasting grid status.
- **ParticipantRegistry**: Updated on Register/ProsumerStart; iterated when sending price updates (if not broadcasting to all).
