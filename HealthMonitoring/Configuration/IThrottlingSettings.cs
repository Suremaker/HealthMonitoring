using System.Collections.Generic;

namespace HealthMonitoring.Configuration
{
    public interface IThrottlingSettings
    {
        IReadOnlyDictionary<string, int> Throttling { get; }
    }
}