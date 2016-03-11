using System;

namespace HealthMonitoring
{
    public class UnsupportedMonitorException : InvalidOperationException
    {
        public UnsupportedMonitorException(string monitorType):base($"Unsupported monitor: {monitorType}")
        {
        }
    }
}