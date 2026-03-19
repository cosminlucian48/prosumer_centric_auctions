# PriceState

## Purpose

**PriceState** holds the **current** grid prices (buy and sell) and the **time** they apply to. Optionally it keeps a short **history** of prices for the same interval (e.g. last 24 hours or 96 × 15-minute periods) for analysis, logging, or simple forecasting.

The Energy Market Agent updates PriceState whenever the tariff (or external price source) is evaluated (e.g. every 15 minutes of simulation time) and reads from it when broadcasting prices or when processing buy/sell requests.

## Responsibilities

- Store **current buy price** (retail, €/kWh or equivalent).
- Store **current sell price** (feed-in or credit, €/kWh).
- Store **effective time** (simulation time or period index) for which these prices apply.
- Optionally store **history** of (time, buy, sell) for recent periods.

## Suggested API (contract)

### Properties (or getters)

| Member | Type | Description |
|--------|------|-------------|
| `BuyPrice` | `double` | Current retail (buy) price per kWh. |
| `SellPrice` | `double` | Current feed-in (sell) price per kWh. |
| `EffectiveTime` | `DateTime` or `int` (period index) | Simulation time (or period) these prices apply to. |
| `LastUpdatedAt` | `DateTime` or tick index | When the state was last updated. |

### Methods

| Method | Description |
|--------|-------------|
| `void Update(double buyPrice, double sellPrice, DateTime effectiveTime)` | Set current prices and time. Called by the agent after querying the Tariff. |
| `void Clear()` | Reset state (e.g. for tests). |

### Optional: history

| Member | Type | Description |
|--------|------|-------------|
| `PriceHistory` | `IReadOnlyList<PriceSnapshot>` or similar | Last N periods of (time, buy, sell). |
| `void AppendHistory(double buy, double sell, DateTime time)` | Append one period to history (agent may call after Update). |
| `int MaxHistoryLength` | Configurable cap (e.g. 96 for 24h at 15-min). |

`PriceSnapshot` can be a small record: `(DateTime Time, double BuyPrice, double SellPrice)`.

## Invariants

- `BuyPrice >= 0`, `SellPrice >= 0`.
- In real-world-like setups, `BuyPrice > SellPrice` unless net metering (then they may be equal).
- `EffectiveTime` should match the settlement interval (e.g. 15-minute boundary).

## Who uses it

- **Energy Market Agent**: writes via `Update` (and optionally `AppendHistory`) after each price period; reads `BuyPrice` and `SellPrice` when sending `EnergyMarketPrice` or when executing buy/sell.

## Implementation notes

- Can be a simple POCO or a small class with methods.
- Thread-safety: in ActressMas, the agent runs sequentially per agent, so no locking is required unless the same PriceState is shared across agents (not recommended).
- History can be a circular buffer or a list with a max size to avoid unbounded growth.

## Example (minimal)

```csharp
public class PriceState
{
    public double BuyPrice { get; private set; }
    public double SellPrice { get; private set; }
    public DateTime EffectiveTime { get; private set; }

    public void Update(double buyPrice, double sellPrice, DateTime effectiveTime)
    {
        BuyPrice = buyPrice;
        SellPrice = sellPrice;
        EffectiveTime = effectiveTime;
    }
}
```
