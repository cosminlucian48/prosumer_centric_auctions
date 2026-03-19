# Simulation Layer (ActressMas)

The simulation layer contains all agents and environment logic implemented in C# using the ActressMas framework. This layer handles message passing, scheduling, and the simulation loop.

## Components

- **[Agents](./agents/)** - All agent implementations
- **[Environment](./environment/)** - Environment and world setup
- **[Models](./models/)** - Data models used by agents
- **[Utils](./utils/)** - Utility classes and helpers

## Architecture

All agents run in a turn-based manner within the `World` environment. Agents communicate via message passing and respond to tick-based time management.
