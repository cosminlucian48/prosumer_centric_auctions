using DissertationProsumerAuctions.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DissertationProsumerAuctions
{
    public class Utils
    {
        private static IConfigurationService _configurationService;

        public static int NoTurns = 10;
        public static int EnergyLoadRateNumberOfDelays { get; private set; }
        public static int EnergyGenerationRateNumberOfDelays { get; private set; }
        public static int EnergyPriceNumberOfDelays { get; private set; }
        public static int NumberOfProsumers { get; private set; }
        public static int EnergyMarketParticipantsSignUpInterval { get; private set; }

        public static int Delay { get; private set; } = 1500;
        public static Random RandNoGen = new Random();
        public static readonly Guid CorrelationId = Guid.NewGuid();

        /// <summary>
        /// Initializes the Utils class with configuration service.
        /// Must be called before using configuration-dependent properties.
        /// </summary>
        /// <param name="configurationService">The configuration service instance.</param>
        public static void InitializeConfiguration(IConfigurationService configurationService)
        {
            _configurationService = configurationService ?? throw new ArgumentNullException(nameof(configurationService));
            
            EnergyLoadRateNumberOfDelays = _configurationService.EnergyLoadRateNumberOfDelays;
            EnergyGenerationRateNumberOfDelays = _configurationService.EnergyGenerationRateNumberOfDelays;
            EnergyPriceNumberOfDelays = _configurationService.EnergyPriceNumberOfDelays;
            NumberOfProsumers = _configurationService.NumberOfProsumers;
            EnergyMarketParticipantsSignUpInterval = _configurationService.EnergyMarketParticipantsSignUpInterval;
            Delay = _configurationService.Delay;
        }
        

        [Obsolete("Use TryParseMessage for safer parsing with error handling")]
        public static void ParseMessage(string content, out string action, out string parameters)
        {
            if (!TryParseMessage(content, out action, out parameters))
            {
                action = string.Empty;
                parameters = string.Empty;
            }
        }

        public static bool TryParseMessage(string content, out string action, out string parameters)
        {
            action = string.Empty;
            parameters = string.Empty;

            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            try
            {
                string[] parts = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length == 0)
                {
                    return false;
                }

                action = parts[0];
                
                if (parts.Length > 1)
                {
                    parameters = string.Join(" ", parts.Skip(1));
                }

                return true;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error parsing message content: {Content}", content);
                return false;
            }
        }

        /// <summary>
        /// Parses a tick message to extract tick index and simulation time.
        /// Expected format: "tick {tickIndex} {timestamp}"
        /// Example: "tick 15 12:15:00 AM"
        /// </summary>
        /// <param name="content">The tick message content (parameters part after "tick" action).</param>
        /// <param name="tickIndex">The parsed tick index (0-based, represents minutes).</param>
        /// <param name="simulationTime">The parsed simulation time as DateTime.</param>
        /// <returns>True if parsing was successful, false otherwise.</returns>
        public static bool TryParseTickMessage(string content, out int tickIndex, out DateTime simulationTime)
        {
            tickIndex = 0;
            simulationTime = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(content))
            {
                return false;
            }

            try
            {
                // Parse format: "{tickIndex} {timestamp}"
                // Example: "15 12:15:00 AM"
                string[] parts = content.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                
                if (parts.Length < 2)
                {
                    return false;
                }

                // Parse tick index (first part)
                if (!int.TryParse(parts[0], out tickIndex))
                {
                    return false;
                }

                // Parse timestamp (remaining parts - could be "12:15:00 AM" or "12:15:00 PM")
                string timestampStr = string.Join(" ", parts.Skip(1));
                
                // Try parsing with common time formats
                if (DateTime.TryParseExact(timestampStr, "hh:mm:ss tt", System.Globalization.CultureInfo.InvariantCulture, 
                    System.Globalization.DateTimeStyles.None, out simulationTime))
                {
                    return true;
                }

                // Fallback to general parsing
                if (DateTime.TryParse(timestampStr, out simulationTime))
                {
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error parsing tick message content: {Content}", content);
                return false;
            }
        }
        
        public static string Str(object p1, object p2)
        {
            return string.Format("{0} {1}", p1, p2);
        }

        public static string Str(object p1, object p2, object p3)
        {
            return string.Format("{0} {1} {2}", p1, p2, p3);
        }

        public static string Str(object p1, object p2, object p3, object p4)
        {
            return string.Format("{0} {1} {2} {3}", p1, p2, p3, p4);
        }
    }
}
