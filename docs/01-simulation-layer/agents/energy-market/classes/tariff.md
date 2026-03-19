# Tariff

## Purpose

**Tariff** (as an interface **ITariff** and several implementations) is responsible for **computing** the grid **buy** and **sell** price for a given **simulation time**. The Energy Market Agent does not contain tariff logic; it calls the tariff each period and writes the result into **PriceState**.

This keeps pricing rules (flat, time-of-use, real-time, net metering) pluggable and testable without touching the agent.

## Responsibilities

- Given a **time** (and optionally date), return the **buy price** (retail) and **sell price** (feed-in) applicable for that period.
- Encapsulate tariff type (flat, ToU, RTP, net metering, feed-in tariff) and parameters (e.g. peak/off-peak levels, price series).

## Suggested API (contract)

### Interface

```csharp
public interface ITariff
{
    /// <summary>
    /// Get the retail (buy) price in €/kWh for the given time.
    /// </summary>
    double GetBuyPrice(DateTime simulationTime);

    /// <summary>
    /// Get the feed-in (sell) price in €/kWh for the given time.
    /// </summary>
    double GetSellPrice(DateTime simulationTime);

    /// <summary>
    /// Tariff identifier or type (e.g. "Flat", "ToU", "RTP") for logging/config.
    /// </summary>
    string TariffType { get; }
}
```

Alternatively, a single method returning both:

```csharp
void GetPrices(DateTime simulationTime, out double buyPrice, out double sellPrice);
```

## Implementations

### 1. FlatTariff

- **Parameters**: `buyPrice`, `sellPrice` (constants).
- **Behaviour**: Always return the same buy and sell price regardless of time.
- **Use case**: Simple baseline, testing.

### 2. TimeOfUseTariff (ToU)

- **Parameters**: e.g. `peakBuyPrice`, `offPeakBuyPrice`, `peakSellPrice`, `offPeakSellPrice`, and **peak hours** (e.g. 7:00–21:00) or **peak period** (e.g. list of hour ranges).
- **Behaviour**: Return peak prices during peak hours, off-peak otherwise. Optionally support “shoulder” period with a third price.
- **Use case**: Realistic retail tariffs that encourage load shifting.

### 3. RealTimePricingTariff (RTP)

- **Parameters**: Price series (e.g. from DB or file): list of (time or period index, buy price, sell price). Or a **formula** (e.g. based on demand).
- **Behaviour**: Look up price for the given time (or nearest period); interpolate if needed.
- **Use case**: Research, demand response, alignment with wholesale or real data.

### 4. NetMeteringTariff

- **Parameters**: Single retail price (or a ToU buy price), and a rule “sell price = buy price” (or sell = k × buy, e.g. k = 1).
- **Behaviour**: `GetSellPrice(time)` returns the same (or scaled) value as `GetBuyPrice(time)`.
- **Use case**: Net metering regimes where excess generation is credited at retail rate.

### 5. FeedInTariff (FIT)

- **Parameters**: Fixed `sellPrice`; buy price from another tariff (e.g. flat or ToU).
- **Behaviour**: `GetSellPrice` always returns the fixed FIT; `GetBuyPrice` delegates to inner tariff.
- **Use case**: Policy-driven feed-in with fixed remuneration.

## Configuration

- Tariff type and parameters should be **configurable** (e.g. from `appsettings.json` or a dedicated config class). The agent (or a factory) builds the appropriate `ITariff` at startup.
- For RTP, configuration may point to a **price series** (file path or DB connection + query).

## Who uses it

- **Energy Market Agent**: In Setup, builds or receives an `ITariff`. On each price-update tick, calls `GetBuyPrice(time)` and `GetSellPrice(time)`, then updates **PriceState**.

## Invariants

- Prices must be non-negative.
- For non–net metering cases, typically `GetBuyPrice > GetSellPrice` for the same time.

## Implementation notes

- Tariff implementations should be **stateless** (or state limited to cached price series). All time-dependent behaviour is driven by the `simulationTime` argument.
- RTP implementation may need to load and cache price series; avoid blocking the agent for long (e.g. load once at startup or in a background load).
