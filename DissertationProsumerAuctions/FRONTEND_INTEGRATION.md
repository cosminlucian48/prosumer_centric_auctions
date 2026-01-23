# Frontend Integration Guide

## API Endpoints

### Simulation Control

#### Initialize Simulation
```http
POST /api/simulation/initialize
Content-Type: application/json

{
  "numberOfProsumers": 5
}
```

#### Start Simulation
```http
POST /api/simulation/start
```

#### Pause Simulation
```http
POST /api/simulation/pause
```

#### Resume Simulation
```http
POST /api/simulation/resume
```

#### Stop Simulation
```http
POST /api/simulation/stop
```

#### Set Simulation Speed
```http
POST /api/simulation/speed
Content-Type: application/json

{
  "delayMs": 5000
}
```

#### Get Simulation Status
```http
GET /api/simulation/status
```

Response:
```json
{
  "initialized": true,
  "running": true,
  "paused": false,
  "tick": 42,
  "delay": 5000
}
```

### Data Endpoints

#### Get Prosumer Load Data
```http
GET /api/data/prosumer/{id}/load?timestamp=2024-01-01 12:00:00
```

#### Get Prosumer Generation Data
```http
GET /api/data/prosumer/{id}/generation?timestamp=2024-01-01 12:00:00
```

#### Get Energy Market Prices
```http
GET /api/data/energy-market/prices?timestamp=2024-01-01 12:00:00
```

## SignalR Real-Time Updates

Connect to the SignalR hub at: `ws://localhost:5000/simulationHub`

### JavaScript/TypeScript Example

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/simulationHub")
    .build();

// Join the simulation updates group
await connection.start();
await connection.invoke("JoinSimulationGroup");

// Listen for events
connection.on("SimulationStarted", () => {
    console.log("Simulation started");
});

connection.on("SimulationPaused", () => {
    console.log("Simulation paused");
});

connection.on("SimulationResumed", () => {
    console.log("Simulation resumed");
});

connection.on("SimulationStopped", () => {
    console.log("Simulation stopped");
});

connection.on("SimulationSpeedChanged", (delayMs: number) => {
    console.log(`Speed changed to ${delayMs}ms`);
});
```

### React Example

```typescript
import { useEffect, useState } from 'react';
import * as signalR from '@microsoft/signalr';

function SimulationControl() {
    const [connection, setConnection] = useState<signalR.HubConnection | null>(null);
    const [status, setStatus] = useState({ running: false, paused: false, tick: 0 });

    useEffect(() => {
        const newConnection = new signalR.HubConnectionBuilder()
            .withUrl('http://localhost:5000/simulationHub')
            .build();

        newConnection.start()
            .then(() => newConnection.invoke('JoinSimulationGroup'))
            .then(() => setConnection(newConnection));

        newConnection.on('SimulationStarted', () => {
            setStatus(prev => ({ ...prev, running: true, paused: false }));
        });

        newConnection.on('SimulationPaused', () => {
            setStatus(prev => ({ ...prev, paused: true }));
        });

        newConnection.on('SimulationResumed', () => {
            setStatus(prev => ({ ...prev, paused: false }));
        });

        return () => {
            newConnection.stop();
        };
    }, []);

    const pauseSimulation = async () => {
        await fetch('http://localhost:5000/api/simulation/pause', { method: 'POST' });
    };

    const resumeSimulation = async () => {
        await fetch('http://localhost:5000/api/simulation/resume', { method: 'POST' });
    };

    return (
        <div>
            <p>Status: {status.running ? (status.paused ? 'Paused' : 'Running') : 'Stopped'}</p>
            <p>Current Tick: {status.tick}</p>
            <button onClick={pauseSimulation}>Pause</button>
            <button onClick={resumeSimulation}>Resume</button>
        </div>
    );
}
```

## CORS Configuration

The API is configured to allow requests from common frontend development ports:
- `http://localhost:3000` (React default)
- `http://localhost:5173` (Vite default)
- `http://localhost:4200` (Angular default)
- `http://localhost:8080` (Vue CLI default)

To add more origins, update the CORS policy in `Program.cs`.
