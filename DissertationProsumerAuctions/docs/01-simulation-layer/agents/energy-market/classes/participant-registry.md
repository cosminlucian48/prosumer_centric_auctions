# ParticipantRegistry

## Purpose

**ParticipantRegistry** holds the list of **prosumer (or consumer) IDs** that are “registered” with the Energy Market Agent and should receive **price updates**. When the agent does not broadcast to all agents in the environment, it sends price messages only to the IDs in this registry.

Optionally the registry can store a small amount of **metadata** per participant (e.g. tariff choice, registration time) for future use (e.g. per-customer tariffs or analytics).

## Responsibilities

- **Add** a participant ID when the agent receives a registration message (e.g. `ProsumerStart` or `Register`).
- **Remove** a participant ID when a prosumer leaves the simulation (optional; requires a “deregister” message or notification).
- **Enumerate** all registered IDs (e.g. to send price updates).
- **Check** whether an ID is registered (e.g. to ignore buy/sell from unknown participants, if desired).
- Optionally store **metadata** per ID (e.g. tariff, registration time).

## Suggested API (contract)

### Minimal (list of IDs only)

| Member / Method | Description |
|-----------------|-------------|
| `void Add(string participantId)` | Register a prosumer. Idempotent if already present. |
| `void Remove(string participantId)` | Unregister (optional). |
| `bool Contains(string participantId)` | True if registered. |
| `IReadOnlyList<string> GetAll()` | All registered IDs (e.g. for iterating when sending prices). |
| `int Count` | Number of registered participants. |

### With metadata (optional)

| Member / Method | Description |
|-----------------|-------------|
| `void Add(string participantId, ParticipantInfo info = null)` | Register with optional info. |
| `ParticipantInfo GetInfo(string participantId)` | Return metadata or null. |
| `void UpdateInfo(string participantId, ParticipantInfo info)` | Update metadata. |

`ParticipantInfo` can be a small class: e.g. `TariffId`, `RegisteredAt` (DateTime or tick), or custom fields.

## Who uses it

- **Energy Market Agent**:
  - On `ProsumerStart` / `Register`: call `Add(participantId)`; then send current prices to that ID once.
  - On each price-update tick: call `GetAll()` and send `EnergyMarketPrice` to each ID (if not broadcasting to all).
  - Optionally on buy/sell: call `Contains(participantId)` to ignore requests from unregistered participants (or allow all; design choice).

## Invariants

- Participant IDs should be **unique** (no duplicate entries). `Add` can be idempotent: if already present, do nothing or update metadata.
- IDs should match the **agent names** used in the MAS (e.g. `"prosumer1"`, `"prosumer2"`).

## Implementation notes

- **Data structure**: A `HashSet<string>` is enough for uniqueness and fast `Contains`; if order matters (e.g. for deterministic iteration), use `List<string>` and check before add, or an ordered set.
- **Thread-safety**: In ActressMas, only the Energy Market Agent modifies the registry, so no locking is required unless the same registry is shared across agents (not recommended).
- **Persistence**: Usually in-memory only; no need to persist the registry across runs unless you want to “resume” a simulation with the same participants.

## Alternative: No registry (broadcast)

If the agent **broadcasts** price updates to **all** agents in the environment (e.g. `Broadcast(EnergyMarketPrice, includeSender: false)`), then a **ParticipantRegistry** is not strictly necessary for sending. It can still be useful to:

- Know “who is a prosumer” for analytics or for optional per-participant logic (e.g. feed-in limit per prosumer).
- Send **only** to registered participants if the environment contains other agents (e.g. TickAgent, Auctioneer) that should not receive price messages.

So the registry is **recommended** when the environment has multiple agent types; it can be **omitted** if the agent always broadcasts and does not need a list of participants for any other reason.
