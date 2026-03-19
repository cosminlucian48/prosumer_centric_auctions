# TickAgent

## Overview

The `TickAgent` manages the simulation time for all agents in the system. It is the only agent that uses timers for time management, broadcasting tick messages to all agents at regular intervals.

## Responsibilities

- Manages current timestamp for all agents
- Regularly sends ticks to all agents
- Maintains simulation time progression
- Provides time synchronization across the system

## State

### Properties
- `StartTimestamp` (DateTime) - Simulation start timestamp
- `_tickIndex` (int) - Current tick index (0-based)
- `_tickDelayMs` (int) - Real-time delay between ticks (default: 10000ms = 10 seconds)
- `_cancellationTokenSource` (CancellationTokenSource) - For background task cancellation
- `_backgroundTask` (Task) - Background task running tick loop

## Lifecycle

### Setup()
- Initializes start timestamp (2024-01-01 00:00:00 UTC)
- Starts background task for tick broadcasting
- Background task runs continuously until agent is removed

### Act(Message message)
- TickAgent doesn't respond to messages (empty implementation)

## Behaviors

### Time Management
- Each tick represents 1 minute of simulation time
- Calculates current simulation time: `StartTimestamp + tickIndex minutes`
- Broadcasts tick message with format: `"tick {tickIndex} {simulationTime}"`

### Background Task
- Runs asynchronously in background
- Checks if agent is still in environment each iteration
- Stops automatically if agent is removed
- Handles cancellation gracefully

### Cleanup
- `StopTicking()` method available for manual cleanup
- Automatically stops when agent removed from environment
- Properly disposes of cancellation token

## Message Handling

### Incoming Messages
- None (agent doesn't process messages)

### Outgoing Messages
- `Tick` - Broadcast to all agents with tick index and simulation time

## Dependencies

- None (independent agent)

## Implementation Status

✅ **Implemented** - Core functionality complete

## Notes

- This is the only agent that uses timers/background tasks
- All other agents depend on tick messages for time management
- Background task self-monitors and stops when agent is removed
