# ProsumerLoadAgent

## Overview

The `ProsumerLoadAgent` monitors and reports energy consumption (load) for a prosumer. It queries the database for load data based on simulation time.

## Responsibilities

- Monitors energy consumption rates
- Queries database for load data
- Reports load updates to parent ProsumerAgent
- Updates load data every 15 minutes (simulation time)

## State

### Properties
- `_myProsumerName` (string) - Name of the parent prosumer
- `_myProsumerId` (int) - ID of the parent prosumer

### Internal State
- `_currentTimestamp` (string) - Current simulation timestamp
- `_currentTickIndex` (int) - Current tick index

## Lifecycle

### Setup()
- Logs agent startup
- Sends `ComponentReady` message to parent ProsumerAgent

### Act(Message message)
Processes incoming messages:
- `ProsumerStart` - Triggers initial load rate update
- `Tick` - Time updates, triggers database query every 15 ticks

## Behaviors

### Load Monitoring
- Queries database for load data at 15-minute intervals
- Retrieves load value for current timestamp
- Sends load updates to parent ProsumerAgent

### Database Integration
- Uses `DatabaseConnection.Instance.GetProsumerLoadByIdAsync()`
- Queries based on prosumer ID and timestamp
- Handles database errors gracefully

## Message Handling

### Incoming Messages
- `ProsumerStart` - Initial load update trigger
- `Tick` - Time-based update trigger

### Outgoing Messages
- `ComponentReady` - Sent during setup
- `LoadUpdate` - Load value updates to parent

## Dependencies

- `ProsumerAgent` - Parent agent coordination
- `DatabaseConnection` - Database access for load data

## Implementation Status

✅ **Implemented** - Core functionality complete
