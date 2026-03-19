# Prosumer Auction Platform

**A Multi-Agent Energy Trading Simulation for Dissertation Research**

## Project Overview

This is a dissertation research project that implements a sophisticated multi-agent simulation for energy trading in prosumer networks. The platform models the interaction between energy prosumers (entities that both produce and consume energy) using intelligent autonomous agents that negotiate and trade energy through multiple market mechanisms.

### Core Objectives

- **Primary**: Design and implement a multi-agent energy trading simulation using the ActressMas framework
- **Secondary**: Enable agents to interact via multiple trading mechanisms (CNP, Dutch Auction, Bilateral Negotiation)
- **Tertiary**: Provide real-time monitoring, visualization, and dynamic control via a web-based dashboard

## Key Features

- 🤖 **Multi-Agent Simulation**: ActressMas-based agent framework with prosumers, grid operators, and market agents
- ⚡ **Energy Trading Mechanisms**: Contract Net Protocol (CNP), Double Auctions, and Bilateral Negotiations
- 📊 **Real-time Visualization**: Web dashboard for monitoring trading activity and energy flows
- 🐳 **Docker Support**: Complete containerized development environment with hot-reload capabilities
- 📈 **Comprehensive Logging**: Integrated Seq logging for distributed tracing and analysis

## Architecture

The project is organized into three main layers:

1. **Simulation Layer** (`docs/01-simulation-layer/`)
   - Agent implementations and behavior logic
   - Auction algorithms and trading protocols
   - Energy market simulations

2. **Backend Interface Layer** (`docs/02-backend-interface-layer/`)
   - REST API endpoints
   - SignalR hubs for real-time communication
   - Data services for persistence

3. **Web Dashboard** (`docs/03-web-dashboard/`)
   - Real-time visualization components
   - Interactive controls
   - Monitoring and analytics UI

For detailed architecture documentation, see [docs/README.md](./docs/README.md).

## Getting Started

### Prerequisites

- **.NET 8.0** or higher
- **Docker** and **Docker Compose** (for containerized development)
- **Git**

### Quick Start with Docker

The recommended approach is to use Docker Compose for a fully containerized development environment:

```bash
docker-compose up
```

This will:
- Build the .NET application with live-reload support
- Start a **Seq** logging server (accessible at `http://localhost:5341`)
- Mount your source code for live editing
- Keep build artifacts in Docker volumes to avoid file lock issues

### Local Development (without Docker)

```bash
# Restore dependencies
dotnet restore

# Build the solution
dotnet build DissertationProsumerAuctions.sln

# Run the simulation
dotnet run
```

## Docker & Docker Compose

### Docker Setup

The project includes a `Dockerfile` with multiple build targets:

- **dev**: Development image with live-reload and watch mode enabled
- **release**: Optimized production build

### Docker Compose Configuration

The `docker-compose.yml` defines the complete development stack:

| Service | Purpose | Port |
|---------|---------|------|
| `seq` | Centralized logging and event streaming | 5341 |
| `sqlite-data` | Database volume manager | - |
| `prosumer-dev` | Main application (dev mode) | - |

**Key features**:
- **Live code reloading**: Changes to `.cs` files automatically trigger rebuilds
- **Volume management**: Source code and build artifacts in separate Docker volumes
- **Environment configuration**: Automatic SEQ_URL and database path injection
- **Cross-platform compatibility**: Works on Windows, macOS, and Linux

### Running with Docker Compose

```bash
# Start all services
docker-compose up

# Start in background
docker-compose up -d

# View logs
docker-compose logs -f prosumer-dev

# Stop services
docker-compose down
```

## Project Structure

```
├── Agents/                 # Agent implementations (Prosumers, Market, Grid)
├── Models/                 # Data models and domain entities
├── Services/               # Business logic and configuration services
├── Constants/              # Agent names and message types
├── DatabaseConnections/    # Database abstraction and connection logic
├── Utils/                  # Utility functions and helpers
├── Resources/              # Data files (CSV prices, loads, generation)
├── docs/                   # Comprehensive documentation by architecture layer
├── Dockerfile              # Container image definition
├── docker-compose.yml      # Development environment orchestration
└── Program.cs              # Application entry point
```

## Configuration

Configuration is managed through `appsettings.json`:

```json
{
  "ProducerConfiguration": {
    "NumberOfProsumers": 10,
    "SimulationSpeed": 1000,
    "LogLevel": "Information"
  }
}
```

## Documentation

For in-depth documentation on specific topics, refer to:

- [Full Architecture & Design Documentation](./docs/README.md)
- [Simulation Layer Details](./docs/01-simulation-layer/README.md)
- [Backend Interface Layer](./docs/02-backend-interface-layer/README.md)
- [Web Dashboard](./docs/03-web-dashboard/README.md)
- [Agent Specifications](./docs/01-simulation-layer/agents/)
- [Auction Algorithms](./docs/01-simulation-layer/design/auction-algorithms.md)

## Building & Publishing

### Build the Solution

```bash
dotnet build DissertationProsumerAuctions.sln
```

### Publish Release Build

```bash
dotnet publish DissertationProsumerAuctions.sln
```

### Watch Mode (Local Development)

```bash
dotnet watch run --project DissertationProsumerAuctions.sln
```

## Logging & Monitoring

The application uses **Seq** for centralized structured logging:

- **Console Output**: Real-time logs visible in terminal
- **Seq Dashboard**: `http://localhost:5341` - Access complete event stream with filtering and analysis
- **Structured Logging**: All events include context (agent names, energy amounts, prices, etc.)

## Troubleshooting

### Docker Container Issues

```bash
# Rebuild images without cache
docker-compose build --no-cache

# Remove all containers and volumes
docker-compose down -v

# View detailed logs
docker-compose logs -f prosumer-dev
```

### Build Failures

```bash
# Clean build artifacts
dotnet clean DissertationProsumerAuctions.sln

# Restore packages
dotnet restore DissertationProsumerAuctions.sln
```

## Research & Citation

This is a dissertation project researching multi-agent energy trading systems. If you use this project for research or reference it in publications, please cite appropriately.

## License

This project is part of academic research. Use according to your institution's guidelines.

---

**For more detailed information**, see the [comprehensive documentation](./docs/README.md).
