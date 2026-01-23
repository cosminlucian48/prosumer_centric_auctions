# SignalR Real-Time Updates Guide

## Overview

SignalR provides real-time, bidirectional communication between your frontend and the simulation backend. When simulation events occur, your frontend is automatically notified without needing to poll.

## Connection Setup

### JavaScript/TypeScript

```typescript
import * as signalR from "@microsoft/signalr";

const connection = new signalR.HubConnectionBuilder()
    .withUrl("http://localhost:5000/simulationHub")
    .withAutomaticReconnect() // Automatically reconnect if connection drops
    .build();

// Start connection
await connection.start();

// Join the simulation updates group
await connection.invoke("JoinSimulationGroup");
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
            .withAutomaticReconnect()
            .build();

        newConnection.start()
            .then(() => {
                console.log('SignalR Connected');
                return newConnection.invoke('JoinSimulationGroup');
            })
            .then(() => {
                console.log('Joined SimulationUpdates group');
                setConnection(newConnection);
            })
            .catch(err => console.error('SignalR Connection Error: ', err));

        // Cleanup on unmount
        return () => {
            newConnection.stop();
        };
    }, []);

    // Set up event listeners
    useEffect(() => {
        if (!connection) return;

        connection.on('SimulationStarted', () => {
            console.log('Simulation started');
            setStatus(prev => ({ ...prev, running: true, paused: false }));
        });

        connection.on('SimulationPaused', () => {
            console.log('Simulation paused');
            setStatus(prev => ({ ...prev, paused: true }));
        });

        connection.on('SimulationResumed', () => {
            console.log('Simulation resumed');
            setStatus(prev => ({ ...prev, paused: false }));
        });

        connection.on('SimulationStopped', () => {
            console.log('Simulation stopped');
            setStatus(prev => ({ ...prev, running: false, paused: false }));
        });

        connection.on('SimulationSpeedChanged', (delayMs: number) => {
            console.log(`Simulation speed changed to ${delayMs}ms`);
        });

        connection.on('SimulationStatus', (statusData: any) => {
            console.log('Status update:', statusData);
            setStatus({
                running: statusData.running,
                paused: statusData.paused,
                tick: statusData.tick
            });
        });

        return () => {
            // Remove listeners
            connection.off('SimulationStarted');
            connection.off('SimulationPaused');
            connection.off('SimulationResumed');
            connection.off('SimulationStopped');
            connection.off('SimulationSpeedChanged');
            connection.off('SimulationStatus');
        };
    }, [connection]);

    return (
        <div>
            <p>Status: {status.running ? (status.paused ? 'Paused' : 'Running') : 'Stopped'}</p>
            <p>Current Tick: {status.tick}</p>
        </div>
    );
}
```

## Available Events

### 1. SimulationStarted
**Triggered when:** Simulation starts via `/api/simulation/start`

```typescript
connection.on('SimulationStarted', () => {
    // Update UI to show simulation is running
});
```

### 2. SimulationPaused
**Triggered when:** Simulation is paused via `/api/simulation/pause`

```typescript
connection.on('SimulationPaused', () => {
    // Update UI to show pause state
});
```

### 3. SimulationResumed
**Triggered when:** Simulation resumes via `/api/simulation/resume`

```typescript
connection.on('SimulationResumed', () => {
    // Update UI to show running state
});
```

### 4. SimulationStopped
**Triggered when:** Simulation stops via `/api/simulation/stop`

```typescript
connection.on('SimulationStopped', () => {
    // Update UI to show stopped state
});
```

### 5. SimulationSpeedChanged
**Triggered when:** Simulation speed changes via `/api/simulation/speed`

```typescript
connection.on('SimulationSpeedChanged', (delayMs: number) => {
    // delayMs is the new delay between ticks in milliseconds
    console.log(`New speed: ${delayMs}ms per tick`);
});
```

### 6. SimulationStatus
**Triggered when:** Status is requested via `/api/simulation/status` or periodically (if implemented)

```typescript
connection.on('SimulationStatus', (status: {
    initialized: boolean;
    running: boolean;
    paused: boolean;
    tick: number;
    delay: number;
}) => {
    // Update UI with current status
    console.log(`Tick: ${status.tick}, Running: ${status.running}`);
});
```

## What You Can Do

### 1. **Real-Time Control Feedback**
When a user clicks "Pause" in your frontend:
- Send POST to `/api/simulation/pause`
- Immediately receive `SimulationPaused` event via SignalR
- Update UI instantly without polling

### 2. **Live Status Dashboard**
- Connect to SignalR on page load
- Display current tick, running/paused state
- Update automatically when status changes

### 3. **Speed Control with Visual Feedback**
- User changes speed slider
- Send POST to `/api/simulation/speed`
- Receive `SimulationSpeedChanged` event
- Update UI to show new speed

### 4. **Multi-User Synchronization**
- Multiple users can connect to the same simulation
- All users see the same status updates in real-time
- Perfect for collaborative monitoring

### 5. **Connection Status Monitoring**
```typescript
connection.onclose((error) => {
    console.log('Connection closed', error);
    // Show "Disconnected" indicator
});

connection.onreconnecting((error) => {
    console.log('Reconnecting...', error);
    // Show "Reconnecting" indicator
});

connection.onreconnected((connectionId) => {
    console.log('Reconnected', connectionId);
    // Rejoin group and update UI
    connection.invoke('JoinSimulationGroup');
});
```

## Advanced Usage

### Periodic Status Updates
You can poll the status endpoint and broadcast updates:

```typescript
// In your frontend, poll every 5 seconds
setInterval(async () => {
    const response = await fetch('http://localhost:5000/api/simulation/status');
    const status = await response.json();
    // Status is also broadcast via SignalR automatically
}, 5000);
```

### Custom Event Handling
You can extend the SignalR hub to send custom events based on your needs. For example:
- Tick updates (when each tick occurs)
- Agent activity (when agents send/receive messages)
- Energy transactions
- Auction events

## Troubleshooting

### Connection Issues
- **CORS**: Make sure your frontend origin is in the CORS policy (localhost:3000, 5173, etc.)
- **URL**: Use `http://localhost:5000/simulationHub` (not `ws://`)
- **Reconnection**: Use `withAutomaticReconnect()` for better reliability

### Events Not Received
- Make sure you called `JoinSimulationGroup()` after connecting
- Check browser console for connection errors
- Verify the event names match exactly (case-sensitive)

### Seq Logging (localhost:5341)

**What is Seq?**
Seq is a log aggregation tool that provides a web UI for viewing structured logs. It's optional.

**Why it might not work:**
1. Seq is not installed/running on your machine
2. Seq is running on a different port
3. Network/firewall issues

**Solutions:**
- **Option 1**: Install Seq from https://datalust.co/download
- **Option 2**: Remove Seq sink (logs will still go to console)
- **Option 3**: Ignore it - the app works fine without Seq, logs just go to console

**To disable Seq logging:**
Edit `Program.cs` and remove or comment out:
```csharp
.WriteTo.Seq("http://localhost:5341")
```

The application will continue to work normally - all logs will appear in your terminal console instead.
