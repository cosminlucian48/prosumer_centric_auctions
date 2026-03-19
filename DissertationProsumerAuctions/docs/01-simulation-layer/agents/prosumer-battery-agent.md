# ProsumerBatteryAgent

## Overview

The `ProsumerBatteryAgent` manages the battery storage component for a prosumer. It handles energy storage operations and capacity management.

## Responsibilities

- Manages battery storage capacity
- Stores excess energy from generation
- Reports battery state of charge (SOC)
- Handles energy storage requests
- Notifies when battery reaches maximum capacity

## State

### Properties
- `_myProsumerName` (string) - Name of the parent prosumer
- `_myProsumerId` (int) - ID of the parent prosumer

### Internal State
- `_currentCapacity` (double) - Current battery capacity in WH (Watt-hours)
- `_maximumCapacity` (double) - Maximum battery capacity (15.0 kWh = 15000 WH)
- `_chargingEfficiency` (int) - Charging efficiency factor
- `_dischargingEfficiency` (int) - Discharging efficiency factor
- `_batterySOCNotificationInterval` (int) - Interval for SOC notifications

## Lifecycle

### Setup()
- Logs agent startup
- Sends `ComponentReady` message to parent ProsumerAgent

### Act(Message message)
Processes incoming messages:
- `ProsumerStart` - Prosumer initialization complete
- `StoreEnergy` - Request to store energy
- `Tick` - Time updates (currently no action)

## Behaviors

### Energy Storage
- Accepts energy storage requests
- Validates capacity before storing
- Stores energy up to maximum capacity
- Reports stored energy amount to parent

### Capacity Management
- Tracks current vs maximum capacity
- Handles partial storage when near capacity limit
- Notifies parent when maximum capacity reached

## Message Handling

### Incoming Messages
- `StoreEnergy` - Energy storage request with amount to store

### Outgoing Messages
- `ComponentReady` - Sent during setup
- `EnergyStored` - Confirmation of stored energy amount
- `BatteryMaximumCapacity` - Notification when capacity limit reached

## Dependencies

- `ProsumerAgent` - Parent agent coordination

## Implementation Status

✅ **Implemented** - Core functionality complete
