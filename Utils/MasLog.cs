using System.Globalization;

namespace ProsumerAuctionPlatform;

using Serilog;
using ActressMas;

public static class MasLog
{
    public static readonly DateTime AppStartTime = DateTime.UtcNow;
    
    private static string ElapsedSinceStart()
    {
        var elapsed = DateTime.UtcNow - AppStartTime;
        return elapsed.ToString(@"hh\:mm\:ss\.fff"); // e.g. 00:01:23.150
    }
    
    public static void Sent(Agent sender, string target, string messageType, object? log = null)
    {
        Log
            .ForContext("Agent", sender.Name)
            .ForContext("Target", target)
            .ForContext("MessageType", messageType)
            .ForContext("Value", log)
            .ForContext("CorrelationId",Utils.CorrelationId)
            .Information("Message sent {Extra}", log);
    }
    

    public static void Received(Agent receiver, Message message, object? log = null)
    {
        var parts = message.Content?.Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    ?? Array.Empty<string>();

        string messageType = parts.Length > 0 ? parts[0] : "";
        string[] parameters = parts.Skip(1).ToArray();

        // Numeric parsing helper
        bool TryParseInvariant(string s, out double value)
        {
            // Normalize comma decimals to dot decimals
            s = s.Replace(",", ".");
            return double.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
        }

        // Extract only numeric parameters
        List<double> numericParams = new();
        foreach (var p in parameters)
        {
            if (TryParseInvariant(p, out double n))
                numericParams.Add(n);
        }

        Log
            .ForContext("Agent", receiver.Name)
            .ForContext("Sender", message.Sender)
            .ForContext("MessageType", messageType)
            .ForContext("Parameters", parameters)            // original parameters (strings)
            .ForContext("NumericParameters", numericParams)  // numeric-only parameters
            .ForContext("Value", log)
            .ForContext("CorrelationId", Utils.CorrelationId)
            .ForContext("Elapsed", ElapsedSinceStart())
            .Information("Message received {Extra}", log);
    }


    public static void Event(Agent agent, string eventType, object? log = null)
    {
        Log
            .ForContext("Agent", agent.Name)
            .ForContext("EventType", eventType)
            .ForContext("Value", log)
            .ForContext("CorrelationId",Utils.CorrelationId)
            .ForContext("Elapsed", ElapsedSinceStart())
            .Information("Event occurred {Extra}", log);
    }
    
    public static void InfoDebug(Agent agent, string eventType, object? log = null)
    {
        Log
            .ForContext("Agent", agent.Name)
            .ForContext("EventType", eventType)
            .ForContext("Value", log)
            .ForContext("CorrelationId",Utils.CorrelationId)
            .ForContext("Elapsed", ElapsedSinceStart())
            .Information("Extra: {Extra}", log);
    }
    public static void Debug(Agent agent, string eventType, object? log = null)
    {
        Log
            .ForContext("Agent", agent.Name)
            .ForContext("EventType", eventType)
            .ForContext("Value", log)
            .ForContext("CorrelationId",Utils.CorrelationId)
            .ForContext("Elapsed", ElapsedSinceStart())
            .Debug("Debug {Extra}", log);
    }

}   
