using System.Collections.Generic;
using HealthMonitoring.Configuration;

namespace HealthMonitoring.Monitors.Core.Exchange.Entities
{
    internal class ThrottlingSettings : IThrottlingSettings
    {
        public ThrottlingSettings(IReadOnlyDictionary<string, int> throttling)
        {
            Throttling = throttling;
        }

        public IReadOnlyDictionary<string, int> Throttling { get; }
    }
}