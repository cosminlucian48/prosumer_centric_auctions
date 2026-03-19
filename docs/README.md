# ProsumerAuctionPlatform Documentation

This documentation is organized by architecture layers as defined in the project design.

## Structure

- **[01-simulation-layer](./01-simulation-layer/)** - ActressMas simulation core
- **[02-backend-interface-layer](./02-backend-interface-layer/)** - API/WebSocket bridge
- **[03-web-dashboard](./03-web-dashboard/)** - Frontend visualization and controls

## Project Overview

This project implements a multi-agent energy trading simulation using ActressMas framework, where agents representing prosumers, the grid, and market operators interact to balance supply and demand via multiple trading mechanisms.

### Primary Goals
- Design and implement a multi-agent energy trading simulation using ActressMas
- Enable agents to interact via CNP, double auction, and bilateral negotiation
- Provide real-time monitoring and visualization capabilities

### Secondary Goals
- Web-based dashboard for monitoring, visualization, and interaction
- Dynamic simulation control (add/remove agents, change weather, etc.)
- Comparative analysis of trading algorithms
