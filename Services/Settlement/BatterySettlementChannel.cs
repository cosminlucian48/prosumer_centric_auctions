using ProsumerAuctionPlatform.Constants;
using System;

namespace ProsumerAuctionPlatform.Services.Settlement
{
    /// <summary>
    /// Settlement channel that routes surplus/deficit through the prosumer's battery agent.
    /// Holds back-pressure state (outstanding request counters) so ProsumerAgent does not
    /// need to know about battery-specific in-flight tracking.
    /// </summary>
    internal class BatterySettlementChannel : ISettlementChannel
    {
        private readonly IProsumerSettlementContext _ctx;
        private double _outstandingStoreRequest;
        private double _outstandingConsumeRequest;

        public BatterySettlementChannel(IProsumerSettlementContext ctx)
        {
            _ctx = ctx;
        }

        public string Name => AgentRoles.Battery;
        public bool IsAvailable => _ctx.HasBattery;
        public bool HasPendingSurplusRequest => _outstandingStoreRequest > 0;
        public bool HasPendingDeficitRequest => _outstandingConsumeRequest > 0;

        public double OutstandingStoreRequest   => _outstandingStoreRequest;
        public double OutstandingConsumeRequest => _outstandingConsumeRequest;

        public double TrySettleSurplus(double pending)
        {
            if (pending <= double.Epsilon) return 0;
            _outstandingStoreRequest = pending;
            _ctx.SendToRole(AgentRoles.Battery, $"{MessageTypes.Battery.StoreEnergy} {pending}");
            return pending;
        }

        public double TrySettleDeficit(double pending)
        {
            if (pending <= double.Epsilon) return 0;
            if (_ctx.BatterySOC <= double.Epsilon) return 0; // empty — let chain continue to grid
            _outstandingConsumeRequest = pending;
            _ctx.SendToRole(AgentRoles.Battery, $"{MessageTypes.Battery.ConsumeEnergy} {pending}");
            return pending;
        }

        /// <summary>Called when battery confirms it stored <paramref name="stored"/> units.</summary>
        public void OnSurplusConfirmed(double stored)
        {
            _outstandingStoreRequest = Math.Max(0, _outstandingStoreRequest - stored);
        }

        /// <summary>
        /// Called when battery reports it is full and could not store <paramref name="overflow"/> units.
        /// The outstanding request is cleared because battery has responded definitively.
        /// </summary>
        public void OnSurplusOverflow(double overflow)
        {
            _outstandingStoreRequest = Math.Max(0, _outstandingStoreRequest - overflow);
        }

        /// <summary>Called when battery confirms it consumed <paramref name="consumed"/> units.</summary>
        public void OnDeficitConfirmed(double consumed)
        {
            _outstandingConsumeRequest = Math.Max(0, _outstandingConsumeRequest - consumed);
        }

        /// <summary>Clears the outstanding consume request after a shortfall is handled externally.</summary>
        public void ClearOutstandingDeficit()
        {
            _outstandingConsumeRequest = 0;
        }
    }
}
