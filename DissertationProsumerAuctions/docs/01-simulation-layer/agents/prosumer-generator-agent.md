# ProsumerGeneratorAgent

## Overview

The `ProsumerGeneratorAgent` monitors and reports energy generation for a prosumer. It queries the database for generation data based on simulation time.

## Responsibilities

- Monitors energy generation rates
- Queries database for generation data
- Reports generation updates to parent ProsumerAgent
- Updates generation data every 15 minutes (simulation time)

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
- `ProsumerStart` - Triggers initial generation rate update
- `Tick` - Time updates, triggers database query every 15 ticks

## Behaviors

### Generation Monitoring
- Queries database for generation data at 15-minute intervals
- Retrieves generation rate for current timestamp
- Sends generation updates to parent ProsumerAgent

### Database Integration
- Uses `DatabaseConnection.Instance.GetProsumerGenerationByIdAsync()`
- Queries based on prosumer ID and timestamp
- Handles database errors gracefully

## Message Handling

### Incoming Messages
- `ProsumerStart` - Initial generation update trigger
- `Tick` - Time-based update trigger

### Outgoing Messages
- `ComponentReady` - Sent during setup
- `GenerationUpdate` - Generation rate updates to parent

## Dependencies

- `ProsumerAgent` - Parent agent coordination
- `DatabaseConnection` - Database access for generation data

## Implementation Status

✅ **Implemented** - Core functionality complete
