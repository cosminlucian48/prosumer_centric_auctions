# GridTransaction and GridTransactionLog

## Purpose

- **GridTransaction**: A single record of a **grid buy** or **grid sell** (who, volume, price, direction, time). Used when the Energy Market Agent **executes** buy/sell requests (not when it only publishes prices).
- **GridTransactionLog**: A collection of such records for a given period (e.g. day, month) or for the whole simulation, used for **settlement**, **billing**, or **analysis**.

## GridTransaction

### Responsibilities

- Store one grid transaction: participant ID, volume (kWh), price (€/kWh), direction (buy/sell), total amount (€), and simulation time (or period index).

### Suggested API (contract)

| Member | Type | Description |
|--------|------|-------------|
| `ParticipantId` | `string` | Prosumer (or consumer) agent name/ID. |
| `VolumeKwh` | `double` | Energy volume in kWh. |
| `PricePerKwh` | `double` | Price in €/kWh (or equivalent). |
| `Direction` | `enum` (Buy / Sell) | Whether grid sold to prosumer (Buy) or bought from prosumer (Sell). |
| `TotalAmount` | `double` | Signed amount in €: positive = cost to prosumer (buy), negative = revenue/credit (sell). Or always positive with direction. |
| `SimulationTime` | `DateTime` or `int` | Time or period index of the transaction. |
| `PeriodIndex` | `int` (optional) | Settlement period index (e.g. 0-based 15-min period of the day). |

### Invariants

- `VolumeKwh > 0`.
- `PricePerKwh >= 0`.
- For Buy: prosumer pays (e.g. `TotalAmount = VolumeKwh * PricePerKwh`). For Sell: prosumer receives (e.g. `TotalAmount = -VolumeKwh * PricePerKwh` or store as positive “revenue”).

### Example (minimal record)

```csharp
public enum GridTransactionDirection { Buy, Sell }

public class GridTransaction
{
    public string ParticipantId { get; }
    public double VolumeKwh { get; }
    public double PricePerKwh { get; }
    public GridTransactionDirection Direction { get; }
    public double TotalAmount { get; }
    public DateTime SimulationTime { get; }

    public GridTransaction(string participantId, double volumeKwh, double pricePerKwh,
        GridTransactionDirection direction, DateTime simulationTime)
    {
        ParticipantId = participantId;
        VolumeKwh = volumeKwh;
        PricePerKwh = pricePerKwh;
        Direction = direction;
        SimulationTime = simulationTime;
        TotalAmount = direction == GridTransactionDirection.Buy
            ? volumeKwh * pricePerKwh
            : -(volumeKwh * pricePerKwh);
    }
}
```

---

## GridTransactionLog

### Responsibilities

- **Append** new grid transactions (when the agent confirms a buy or sell).
- **Enumerate** transactions (e.g. for settlement, reporting, or export).
- Optionally **filter** by participant, time range, or direction.
- Optionally **aggregate** (e.g. total cost per prosumer per day) for settlement or billing.

### Suggested API (contract)

| Method / Property | Description |
|-------------------|-------------|
| `void Append(GridTransaction transaction)` | Record one transaction. |
| `IReadOnlyList<GridTransaction> GetAll()` | All transactions (or return a copy). |
| `IReadOnlyList<GridTransaction> GetByParticipant(string participantId)` | Filter by prosumer. |
| `IReadOnlyList<GridTransaction> GetByTimeRange(DateTime from, DateTime to)` | Filter by time. |
| `double GetNetAmountForParticipant(string participantId)` | Sum of TotalAmount for that participant (positive = net cost, negative = net revenue). |
| `void Clear()` | Optional; e.g. for tests or new settlement period. |

### Implementation notes

- Can be a `List<GridTransaction>` wrapped in a class, or a more efficient structure if the number of transactions is large.
- In a multi-agent setup with a single Energy Market Agent, access is sequential (no concurrent writes from other agents to this log), so locking is not required unless the same log is shared across threads elsewhere.
- For long simulations, consider **periodic export** or **rolling window** so the in-memory log does not grow unbounded (e.g. flush to DB or file at end of day).

## Who uses it

- **Energy Market Agent**: After validating a buy/sell request, creates a `GridTransaction` and appends it to the **GridTransactionLog**; optionally uses the log for settlement reports or for enforcing limits (e.g. daily cap per prosumer).

## Relation to “Settlement”

- **Settlement** in real markets often means computing final positions and payments over a period (e.g. day, month). In this simulation, “settlement” can mean:
  - **Per-transaction**: Each buy/sell is immediately recorded (GridTransaction); no separate settlement agent.
  - **Aggregated**: At the end of a period, use `GridTransactionLog` to compute `GetNetAmountForParticipant(prosumerId)` and optionally produce a “bill” or “credit” summary.

The Energy Market Agent can perform aggregated settlement by reading the log at the end of each period (or when requested) without needing a separate Settlement Agent.
