# Backend Interface Layer

The backend interface layer acts as a bridge between the ActressMas simulation and the web dashboard. It exposes REST or WebSocket endpoints for agent data, simulation control, and scenario parameters.

## Components

- **[API Endpoints](./api-endpoints/)** - REST API endpoints
- **[SignalR Hubs](./signalr-hubs/)** - WebSocket/real-time communication
- **[Data Services](./data-services/)** - Data aggregation and transformation

## Responsibilities

- Expose agent data (energy levels, trades)
- Provide simulation control (start/stop, add/remove agents)
- Handle scenario parameters (weather, prices)
- Transform simulation data for frontend consumption
- Manage real-time updates via WebSocket

## Implementation Status

❌ **Not Implemented** - Placeholder for future implementation

## Planned Technologies

- ASP.NET Core Web API
- SignalR for real-time communication
- RESTful endpoints for control operations
