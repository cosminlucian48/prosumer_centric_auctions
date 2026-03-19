# GridStatus and FeedInLimit

## Purpose

- **GridStatus**: Represents the **operational state** of the grid (e.g. Normal, Constrained, Emergency). The Energy Market Agent can use it to refuse or curtail feed-in and to broadcast a simple “grid status” message to prosumers.
- **FeedInLimit**: Represents a **maximum feed-in** (kWh) per period and/or per prosumer. The agent checks it before accepting a sell request and may send `SellRejected` when the limit is reached.

Both are **optional**: the agent can work with only prices and no status/limits.

---

## GridStatus

### Responsibilities

- Hold the current grid status (enum or small set of states).
- Optionally record when the status last changed (for logging or time-based rules).

### Suggested API (contract)

#### Enum

```csharp
public enum GridStatus
{
    Normal,      // No restrictions
    Constrained, // Feed-in may be limited or discouraged
    Emergency    // Severe constraints; feed-in possibly refused
}
```

#### Class (if more than an enum is needed)

| Member | Type | Description |
|--------|------|-------------|
| `CurrentStatus` | `GridStatus` | Current state. |
| `LastUpdatedAt` | `DateTime` or tick | When status was last set. |
| `string Description` | Optional | Short reason (e.g. "High demand"). |

| Method | Description |
|--------|-------------|
| `void Set(GridStatus status)` | Set new status. |
| `bool AllowsFeedIn()` | e.g. true for Normal, false for Emergency, configurable for Constrained. |

### Who uses it

- **Energy Market Agent**: Reads `GridStatus` when processing `SellToGrid`; if status is Emergency (or Constrained with “no feed-in”), may send `SellRejected`. Optionally broadcasts `GridStatus {status}` to prosumers on change or periodically.

### How status is set

- **Configuration**: Fixed status for the whole run (e.g. always Normal).
- **Time-based**: Simple rule (e.g. “Constrained” during peak hours 18:00–20:00).
- **Exogenous**: Later, a separate “Grid Operator” agent or a scenario script could send a message to the Energy Market Agent to set status (requires a dedicated message and handler).
- **Demand-based**: Optional: status = Constrained when total demand in the system exceeds a threshold (would require the agent to know or estimate system demand).

For the first implementation, **time-based** or **fixed Normal** is enough.

---

## FeedInLimit

### Responsibilities

- Enforce a **maximum** amount of energy (kWh) that can be **sold to the grid** in a given period (e.g. per 15-minute period, or per day), either **globally** or **per prosumer**.
- Track how much has already been “consumed” in the current period; when a new sell request arrives, check if adding it would exceed the limit.

### Suggested API (contract)

#### Option A: Global limit per period

| Member | Type | Description |
|--------|------|-------------|
| `MaxFeedInPerPeriodKwh` | `double` | Cap for the whole grid in one period. |
| `double ConsumedThisPeriodKwh` | | Sum of accepted sell volumes in the current period. |
| `int CurrentPeriodIndex` | | Period index (e.g. 15-min slot) to detect period change. |

| Method | Description |
|--------|-------------|
| `bool CanAccept(double volumeKwh, int periodIndex)` | True if `ConsumedThisPeriodKwh + volumeKwh <= MaxFeedInPerPeriodKwh` and period is current (or reset if new period). |
| `void RecordAccepted(double volumeKwh, int periodIndex)` | Add volume to `ConsumedThisPeriodKwh`; reset if period changed. |

#### Option B: Per-prosumer limit per period

- Same idea but store `ConsumedThisPeriodKwh` **per participant** (e.g. `Dictionary<string, double>`).
- `CanAccept(participantId, volumeKwh, periodIndex)` and `RecordAccepted(participantId, volumeKwh, periodIndex)`.

#### Option C: Both

- Global cap **and** per-prosumer cap; accept only if both allow.

### Who uses it

- **Energy Market Agent**: Before accepting a `SellToGrid` request, call `FeedInLimit.CanAccept(...)`. If false, send `SellRejected`. After accepting, call `RecordAccepted(...)`.

### Configuration

- `MaxFeedInPerPeriodKwh` (and per-prosumer limits if any) should be **configurable** (e.g. from appsettings or scenario file). Use a large value or “no limit” (e.g. double.MaxValue or nullable) when feed-in is not limited.

### Implementation notes

- When the **period** changes (e.g. new 15-minute slot), reset “consumed” counters so the limit applies per period.
- FeedInLimit can be a **class** used only by the Energy Market Agent; no need to expose it to other agents.

---

## Summary

| Class | Purpose |
|-------|---------|
| **GridStatus** | Normal / Constrained / Emergency; agent may refuse or curtail feed-in and broadcast status. |
| **FeedInLimit** | Max feed-in per period (global or per prosumer); agent checks before accepting sell and records after accept. |

Both are optional; the agent can run with neither (always allow feed-in and no status broadcast).
