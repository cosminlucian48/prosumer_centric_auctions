# Energy Market Agent – Documentation Index

This folder contains implementation and design documentation for the **Energy Market Agent** and its supporting classes.

## Documents

| Document | Description |
|----------|-------------|
| [Implementation](./implementation.md) | How to implement the agent: single agent vs helpers, flow, and recommendations |
| [Architecture](./architecture.md) | Architecture decisions: agent boundaries, class roles, dependencies |
| [Classes](./classes/README.md) | Supporting classes (PriceState, Tariff, GridTransaction, GridStatus, ParticipantRegistry) |

## Main Agent Spec

The behavioural specification of the Energy Market Agent (what it does, not how it is built) is in:

- **[Energy Market Agent](../energy-market-agent.md)** – Role, responsibilities, real-world alignment, message protocol, state

## Quick Summary

- **One agent**: Energy Market Agent; no separate helper *agents* (e.g. no separate Grid Operator agent unless needed later).
- **Helper classes**: PriceState, Tariff (and implementations), GridTransaction, GridStatus, ParticipantRegistry – used *inside* the agent or as shared models.
- **Flow**: Agent uses a Tariff to compute prices, stores them in PriceState, broadcasts on tick; optionally handles buy/sell messages and records GridTransactions; optionally uses GridStatus and feed-in limits.
