# World (Environment)

## Overview

The `World` class extends `EnvironmentMas` and represents the multi-agent environment where all agents execute. It manages agent lifecycle, message passing, and simulation execution.

## Responsibilities

- Manages agent lifecycle (add, remove)
- Coordinates agent execution
- Handles message routing
- Manages simulation turns
- Initializes core agents (EnergyMarket, Tick)

## Structure

### Core Agents
- `EnergyMarketAgent` - Grid energy market
- `TickAgent` - Time management

### Prosumer Management
- `AddProsumer()` - Adds a prosumer and its component agents
- Component agents added automatically:
  - `ProsumerBatteryAgent`
  - `ProsumerGeneratorAgent`
  - `ProsumerLoadAgent`

## Configuration

### Constructor Parameters
- `numberOfTurns` (int) - Maximum number of simulation turns (0 = infinite)
- `delayAfterTurn` (int) - Delay in milliseconds after each turn
- `randomOrder` (bool) - Whether agents run in random order
- `rand` (Random) - Random number generator for reproducibility
- `parallel` (bool) - Whether agents run in parallel (default: true)

## Agent Execution

### Parallel Execution
- When `parallel = true`, agents run simultaneously
- Each agent's `Act()` method processes messages sequentially
- Framework ensures thread safety

### Sequential Execution
- When `parallel = false`, agents run one after another
- Execution order can be random or sorted

## Implementation Status

✅ **Implemented** - Core functionality complete
